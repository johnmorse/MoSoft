using System;
using System.Windows;
using WIA;

namespace CameraWizard
{
  /// <summary>
  /// Interaction logic for ConnectToCameraWindow.xaml
  /// </summary>
  public partial class ConnectToCameraWindow
  {
    public ConnectToCameraWindow()
    {
      InitializeComponent();
    }

    protected override void OnContentRendered(EventArgs e)
    {
      base.OnContentRendered(e);
      if (!m_connect_to_camera) return;
      m_connect_to_camera = false;
      var model = DataContext as ConnectToCameraViewModel;
      if (model != null) model.ConnectToCamera(this);
    }

    private bool m_connect_to_camera = true;

    public Camera Camera
    {
      get { return (ViewModel == null ? null : ViewModel.Camera); }
      set
      {
        if (ViewModel != null) ViewModel.Camera = value;
      }
    }

    public DeviceManager DeviceManager
    {
      get { return (ViewModel == null ? null : ViewModel.DeviceManager); }
      set
      {
        if (ViewModel != null) ViewModel.DeviceManager = value;
      }
    }

    public Device Device
    {
      get
      {
        return (ViewModel == null ? null : ViewModel.Device);
      }
    }

    ConnectToCameraViewModel ViewModel { get { return DataContext as ConnectToCameraViewModel; } }

    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
      var model = DataContext as ConnectToCameraViewModel;
      if (model != null) model.Cancel(this);
    }
  }
}
