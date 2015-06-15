using System;
using System.Windows;
using CameraWizard.ViewModels;
using WIA;

namespace CameraWizard.Windows
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
      var model = DataContext as ConnectToCamera;
      if (model != null) model.Connect(this);
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

    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
      if (ViewModel != null) ViewModel.Cancel(this);
    }

    ConnectToCamera ViewModel { get { return DataContext as ConnectToCamera; } }
  }
}
