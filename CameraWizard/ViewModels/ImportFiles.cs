using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using WIA;

namespace CameraWizard.ViewModels
{
  class ImportFiles : NotifyPropertyChanged
  {
    public async void Start(Window window)
    {
      try
      {
        Window = window;
        m_cancellation_token = new CancellationTokenSource();
        var progress = new Progress<TransferableImageFile>();
        progress.ProgressChanged += (sender, file) =>
        {
          ItemText = file.Name;
          PreviewImage = file.Thumbnail;
        };
        await AsyncLoadFiles(progress);
        if (DeleteFile != null && FilesToDelete != null)
        {
          Title = "Deleting";
          Thread.Sleep(1000);
          var deleted = new Progress<TransferableImageFile>();
          deleted.ProgressChanged += (sender, file) =>
          {
            if (FileDeleted != null)
              FileDeleted(file);
          };
          await AsyncDeleteFiles(progress, deleted);
        }
      }
      catch (OperationCanceledException)
      {
        // AsyncBuildPictureListTask was canceled for some reason
      }
      catch (Exception e)
      {
        WriteException(e);
        m_display_message_box = true;
        MessageBox.Show(
          Window,
          string.Format("{0} Files imported\n\nError exception caught:\n{1}", FilesWritten, e.Message),
          "Import Images Exception",
          MessageBoxButton.OK,
          MessageBoxImage.Exclamation
          );
      }
      finally
      {
        AsyncOnEnd();
        if (m_display_message_box && FilesWritten < 1)
          MessageBox.Show(
            Window,
            "No images imported",
            "Import Images",
            MessageBoxButton.OK,
            MessageBoxImage.Exclamation
            );
        else if (m_display_message_box)
          MessageBox.Show(
            Window,
            string.Format("{0} Files imported.", FilesWritten),
            "Import Images",
            MessageBoxButton.OK,
            MessageBoxImage.Exclamation
            );
        if (Window.IsVisible)
          Window.Close();
      }
    }

    private Task AsyncLoadFiles(IProgress<TransferableImageFile> progress)
    {
      return Task.Run(() => LoadFiles(progress));
    }

    private bool m_display_message_box = true;

    private void LoadFiles(IProgress<TransferableImageFile> progress)
    {
      foreach (var image in ImageCollection)
      {
        if (IsCancellationRequested) return;
        if (!image.Include) continue;
        progress.Report(image);
        if (IsCancellationRequested) return;
        FileWriter(image);
        FilesWritten++;
      }
    }
    private Task AsyncDeleteFiles(IProgress<TransferableImageFile> progress, IProgress<TransferableImageFile> deletedProgress)
    {
      return Task.Run(() => DeleteFiles(progress, deletedProgress));
    }

    private void DeleteFiles(IProgress<TransferableImageFile> progress, IProgress<TransferableImageFile> deletedProgress)
    {
      var list = FilesToDelete();
      var deleted = new List<TransferableImageFile>();
      foreach (var image in list)
      {
        if (IsCancellationRequested) return;
        if (!image.Include) continue;
        progress.Report(image);
        if (IsCancellationRequested) return;
        if (DeleteFile(image))
          deleted.Add(image);
      }
      if (deleted.Count < 1) return;
      Camera.ExecuteCommand(CommandID.wiaCommandSynchronize);
      foreach (var item in deleted)
        deletedProgress.Report(item);
    }

    private bool IsCancellationRequested { get { return (m_cancellation_token != null && m_cancellation_token.IsCancellationRequested); } }

    public void Cancel(Window window)
    {
      m_cancellation_token.Cancel();
    }

    private void AsyncOnEnd()
    {
      if (m_cancellation_token != null)
        m_cancellation_token.Dispose();
      m_cancellation_token = null;
    }
    private CancellationTokenSource m_cancellation_token;

    public BitmapImage PreviewImage
    {
      get { return m_preview_image; }
      set
      {
        if (m_preview_image == value) return;
        m_preview_image = value;
        RaisePropertyChanged("PreviewImage");
      }
    }
    private BitmapImage m_preview_image;

    public string ItemText
    {
      get { return (m_item_text ?? string.Empty); }
      set { SetProperty(value, ref m_item_text); }
    }
    private string m_item_text = "item_text";

    public string Title
    {
      get { return (m_title ?? string.Empty); }
      set { SetProperty(value, ref m_title); }
    }
    private string m_title = "Importing";

    public int FilesWritten { get; private set; }

    // Must be assigned before using
    public delegate void WriteFileFunction(TransferableImageFile image);
    public WriteFileFunction FileWriter { get; set; }
    public delegate bool DeleteFileFunction(TransferableImageFile image);
    public DeleteFileFunction DeleteFile { get; set; }
    public WriteFileFunction FileDeleted { get; set; }
    public delegate List<TransferableImageFile> FilesToDeleteFunction();
    public FilesToDeleteFunction FilesToDelete { get; set; }
    public Device Camera { get; set; }
    public ObservableCollection<TransferableImageFile> ImageCollection { get; set; }
    private Window Window { get; set; }
  }
}
