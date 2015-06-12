using WIA;

namespace CameraWizard
{
  public class Camera : NotifyPropertyChanged
  {
    public Camera(IDeviceInfo info)
    {
      Id = info.DeviceID;
      Manufacturer = string.Empty;
      Name = info.Properties.GetProperty(7) as string;
      Manufacturer = info.Properties.GetProperty(3) as string;
      //Type = (int)info.Properties.GetProperty(5);
      Type = info.Type;
    }

    public override string ToString()
    {
      var value = Name;
      if (string.IsNullOrWhiteSpace(value))
        value = Manufacturer;
      if (string.IsNullOrWhiteSpace(value))
        value = Id;
      return (string.IsNullOrWhiteSpace(value) ? "(unknown)" : value);
    }

    public string Id { get; private set; }
    public string Manufacturer { get; private set; }
    public string Name { get; private set; }
    public WiaDeviceType Type { get; private set; }
  }
}
