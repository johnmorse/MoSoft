using System;
using System.IO;
using System.Windows.Media.Imaging;
using WIA;

namespace CameraWizard
{
  class TransferableImageFile : NotifyPropertyChanged
  {
    public TransferableImageFile(IItems parent, IItem item)
    {
      Item = item;
      Parent = parent;
    }

    public void InitializeAll()
    {
      InitializeName();
      InitializeTimeStamp();
      InitalizeThumbNail();
    }

    public void InitializeName()
    {
      var prop = Item.Properties["Item Name"];
      if (prop == null) return;
      var value = prop.get_Value() as string;
      Name = value;
    }

    public void InitializeTimeStamp()
    {
      var prop = Item.Properties["Item Time Stamp"];
      var value = (prop == null ? null : prop.get_Value() as Vector);
      TimeStamp = (value == null ? DateTime.MinValue : value.Date);
    }

    public void InitalizeThumbNail()
    {
      var prop = Item.Properties["Thumbnail Data"];
      if (prop == null) return;
      var thumbnail = prop.get_Value() as WIA.Vector;
      if (thumbnail == null)
      {
        Thumbnail = null;
        return;
      }
      var width_prop = Item.Properties["Thumbnail Width"];
      var width = width_prop == null ? 0 : (int)width_prop.get_Value();
      if (width == 0)
      {
        Thumbnail = null;
        return;
      }
      var height_prop = Item.Properties["Thumbnail Height"];
      var height = height_prop == null ? 0 : (int)height_prop.get_Value();
      if (height == 0)
      {
        Thumbnail = null;
        return;
      }
      var image_file = thumbnail.get_ImageFile(width, height);
      if (image_file == null)
      {
        Thumbnail = null;
        return;
      }
      var data = (byte[])image_file.FileData.get_BinaryData();
      using (var stream = new MemoryStream(data))
      {
        stream.Seek(0, SeekOrigin.Begin);
        var image = new BitmapImage();
        image.BeginInit();
        image.StreamSource = stream;
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.EndInit();
        Thumbnail = image;
      }
    }

    public void FromItem(IItem item, bool extractThumbNail)
    {
      Item = item;
      Include = true;
      InitializeName();
      InitializeTimeStamp();
      if (extractThumbNail)
        InitalizeThumbNail();
    }

    public static bool ItemIsImageFile(IItem item)
    {
      try
      {
        if (item == null) return false;

        var flags_object = item.Properties.Exists("Item Flags") ? item.Properties.GetProperty("Item Flags") : null;
        if (flags_object == null) return false;

        var flags = (int)flags_object;
        const int include_flags = ((int)WiaItemFlag.FileItemFlag | (int)WiaItemFlag.ImageItemFlag | (int)WiaItemFlag.TransferItemFlag);
        return ((include_flags & flags) == include_flags);
      }
      catch (Exception e)
      {
        WriteException(e);
        return false;
      }
    }

    public IItem Item { get; private set; }
    public IItems Parent { get; private set; }

    public string Name
    {
      get { return m_name; }
      private set { SetProperty(value, ref m_name); }
    }
    private string m_name;

    public bool IsTimeStampSet { get { return (TimeStamp != DateTime.MinValue); } }
    public DateTime TimeStamp
    {
      get { return m_time_stamp; }
      private set { SetProperty(value, ref m_time_stamp); }
    }
    private DateTime m_time_stamp = DateTime.MinValue;

    public BitmapImage Thumbnail
    {
      get { return m_thumbnail; }
      set
      {
        if (value == m_thumbnail) return;
        m_thumbnail = value;
        RaisePropertyChanged("Thumbnail");
      }
    }
    private BitmapImage m_thumbnail;

    public bool Include
    {
      get { return m_include; }
      set { SetProperty(value, ref m_include); }
    }
    private bool m_include;
  }
}
