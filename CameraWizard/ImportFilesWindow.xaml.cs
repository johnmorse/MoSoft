using System;
using System.Collections.ObjectModel;
using System.Windows;
using WIA;

namespace CameraWizard
{
  /// <summary>
  /// Interaction logic for ImportFilesWindow.xaml
  /// </summary>
  partial class ImportFilesWindow
  {
    public ImportFilesWindow()
    {
      InitializeComponent();
    }

    protected override void OnContentRendered(EventArgs e)
    {
      base.OnContentRendered(e);
      if (m_started) return;
      m_started = false;
      if (ViewModel != null) ViewModel.Start(this);
    }
    private bool m_started;

    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
      if (ViewModel != null) ViewModel.Cancel(this);
    }

    internal ImportFilesViewModel.WriteFileFunction FileWriter
    {
      get { return (ViewModel == null ? null : ViewModel.FileWriter); }
      set { if (ViewModel != null) ViewModel.FileWriter = value; }
    }

    internal ImportFilesViewModel.DeleteFileFunction DeleteFile
    {
      get { return (ViewModel == null ? null : ViewModel.DeleteFile); }
      set { if (ViewModel != null) ViewModel.DeleteFile = value; }
    }

    internal ImportFilesViewModel.WriteFileFunction FileDeleted
    {
      get { return (ViewModel == null ? null : ViewModel.FileDeleted); }
      set { if (ViewModel != null) ViewModel.FileDeleted = value; }
    }

    internal ImportFilesViewModel.FilesToDeleteFunction FilesToDelete
    {
      get { return (ViewModel == null ? null : ViewModel.FilesToDelete); }
      set { if (ViewModel != null) ViewModel.FilesToDelete = value; }
    }

    internal Device Camera
    {
      get { return (ViewModel == null ? null : ViewModel.Camera); }
      set { if (ViewModel != null) ViewModel.Camera = value; }
    }

    internal ObservableCollection<TransferableImageFile> ImageCollection
    {
      get { return (ViewModel == null ? null : ViewModel.ImageCollection); }
      set { if (ViewModel != null) ViewModel.ImageCollection = value; }
    }
    ImportFilesViewModel ViewModel { get { return DataContext as ImportFilesViewModel; } }
  }
}
