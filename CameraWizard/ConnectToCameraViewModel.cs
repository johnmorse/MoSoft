using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WIA;

namespace CameraWizard
{
  class ConnectToCameraViewModel : NotifyPropertyChanged
  {
    public void Cancel(Window window)
    {

      window.Close();
    }

    public async void ConnectToCamera(Window window)
    {
      try
      {
        m_cancellation_token = new CancellationTokenSource();
        await AsyncConnect(window);
      }
      catch (TaskCanceledException)
      {
        if (Device == null)
          m_error = ErrorCode.Cancled;
      }
      catch (Exception e)
      {
        WriteException(e);
        m_exception = e;
      }
      finally
      {
        if (m_cancellation_token != null)
          m_cancellation_token.Dispose();
        m_cancellation_token = null;
        if (Device == null)
        {
          if (m_exception == null)
            switch (m_error)
            {
              case ErrorCode.Error:
                MessageBox.Show(
                  window,
                  "Error connecting to camera",
                  ErrorMessageCaption,
                  MessageBoxButton.OK,
                  MessageBoxImage.Error
                  );
                break;
              case ErrorCode.CameraNotFound:
                MessageBox.Show(
                  window,
                  "The camera no longer appears to be connected",
                  ErrorMessageCaption,
                  MessageBoxButton.OK,
                  MessageBoxImage.Error
                  );
                break;
            }
          else
            MessageBox.Show(
              window,
              "Error connecting to camera: " + Environment.NewLine + Environment.NewLine + m_exception.Message,
              ErrorMessageCaption,
              MessageBoxButton.OK,
              MessageBoxImage.Error
              );
          if (window.IsVisible)
            window.Close();
        }
      }
    }
    private CancellationTokenSource m_cancellation_token;
    private static string ErrorMessageCaption { get { return "Camera Connection Error"; } }
    private Task AsyncConnect(Window window)
    {
      return Task.Run(() => Connect(window));
    }

    enum ErrorCode
    {
      None,
      CameraNotFound,
      Cancled,
      Error,
    }
    private ErrorCode m_error = ErrorCode.None;
    private Exception m_exception;

    private void Connect(Window window)
    {
      // Make sure the camera is still in the device list before trying to
      // connect to it
      var id = Camera.Id;
      var device_info = DeviceManager.DeviceInfos.Cast<DeviceInfo>().FirstOrDefault(device => id.Equals(device.DeviceID, StringComparison.Ordinal));
      if (device_info == null)
      {
        m_error = ErrorCode.CameraNotFound;
        return;
      }
      try
      {
        m_async_connect_window = window;
        var device = device_info.Connect();
        m_synchronization_context.Send(SetDevice, device);
      }
      catch (Exception e)
      {
        m_error = ErrorCode.Error;
        m_exception = e;
        WriteException(e);
      }
    }

    private void SetDevice(object device)
    {
      Device = device as Device;
      // https://social.msdn.microsoft.com/Forums/vstudio/en-US/49c5a702-0cb8-45b4-ad36-5122827b2348/wia-20-image-deletion?forum=csharpgeneral
      // Check to see if the camera supports deleting
      //if (Device != null)
      //{
      //  foreach (DeviceCommand command in Device.Commands)
      //  {
      //    var msg = string.Format("Command: {0}  Id: {1}  Description: {2}", command.Name, command.CommandID,
      //      command.Description);
      //    System.Diagnostics.Debug.WriteLine(msg);
      //  }
      //}

      m_error = Device == null ? ErrorCode.Error : ErrorCode.None;
      if (m_async_connect_window != null)
        m_async_connect_window.Close();
      m_async_connect_window = null;
    }

    private Window m_async_connect_window;
    private readonly SynchronizationContext m_synchronization_context = SynchronizationContext.Current;

    public Camera Camera
    {
      get { return m_camera; }
      set
      {
        if (m_camera == value) return;
        m_camera = value;
        ConnectingString = m_camera == null ? string.Empty : string.Format("Connecting to camera {0}", m_camera.Name);
        Name = m_camera == null ? string.Empty : m_camera.Name;
        Manufacturer = m_camera == null ? string.Empty : m_camera.Manufacturer;
      }
    }
    private Camera m_camera;

    public DeviceManager DeviceManager { get; set; }

    public Device Device { get; set; }

    public string ConnectingString
    {
      get { return (m_connecting_string ?? string.Empty); }
      set { SetProperty(value, ref m_connecting_string); }
    }
    private string m_connecting_string = "Connecting to camera <name>";

    public string Name
    {
      get { return (m_name ?? string.Empty); }
      set { SetProperty(value, ref m_name); }
    }
    private string m_name = "name";

    public string Manufacturer
    {
      get { return (m_manufacturer ?? string.Empty); }
      set { SetProperty(value, ref m_manufacturer); }
    }
    private string m_manufacturer = "manufacturer";

  }
}
