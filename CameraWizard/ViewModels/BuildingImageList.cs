using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using WIA;

namespace CameraWizard.ViewModels
{
  class BuildingImageList : NotifyPropertyChanged
  {
    public async void Start(Window window)
    {
      try
      {
        Window = window;
        if (Device == null)
        {
          window.Close();
          return;
        }
        m_cancellation_token_source = new CancellationTokenSource();
        m_cancellation_token = m_cancellation_token_source.Token;

        await AsyncBuildList(Device.Items, m_cancellation_token);
      }
      catch (Exception e)
      {
        WriteException(e);
        window.Close();
      }
    }

    public void Cancel(Window window)
    {
      if (m_cancellation_token_source != null)
      {
        m_cancellation_token_source.Cancel(false);
        m_cancellation_token_source.Dispose();
        m_cancellation_token_source = null;
      }
      window.Close();
    }
    private CancellationTokenSource m_cancellation_token_source;
    private CancellationToken m_cancellation_token;

    private Task AsyncBuildList(IItems items, CancellationToken token)
    {
      return Task.Run(() =>
      {
        for (var i = 1; i < items.Count + 1; i++)
        {
          if (token.IsCancellationRequested)
            return;
          var item = items[i];
          if (TransferableImageFile.ItemIsImageFile(item))
          {
            var image = new TransferableImageFile(items, item);
            image.InitializeAll();
            ItemText = image.Name;
            PreviewImage = image.Thumbnail;
            ImageList.Add(image);
          }
          if (token.IsCancellationRequested)
            return;
          AsyncBuildList(item.Items, token);
        }
      }, token);
    }

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

    public Device Device { get; set; }
    public readonly List<TransferableImageFile> ImageList = new List<TransferableImageFile>();
    private Window Window { get; set; }
  }
}
