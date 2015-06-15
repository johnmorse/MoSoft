using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using CameraWizard.Windows;
using WIA;

// https://wiadotnet.codeplex.com/
// http://www.codeproject.com/Articles/30726/Transferring-D-images-with-WIA

namespace CameraWizard.ViewModels
{
  class WizardViewModel : NotifyPropertyChanged
  {
    public WizardViewModel()
    {
      m_device_manager.RegisterEvent(EventID.wiaEventDeviceConnected, "*");
      m_device_manager.RegisterEvent(EventID.wiaEventDeviceDisconnected, "*");
      m_device_manager.OnEvent += DeviceManagerOnEvent;

      BindingOperations.EnableCollectionSynchronization(ImageCollection, m_lock);

      if (!InDesignMode)
        RefreshCameraCollection();
    }


    #region private properties
    /// <summary>
    /// Used to make ObservableCollection thread safe
    /// </summary>
    private readonly object m_lock = new object();
    /// <summary>
    /// Provides access to WIA
    /// </summary>
    private readonly DeviceManager m_device_manager = new DeviceManager();
    #endregion private properties

    #region public auto properties
    /// <summary>
    /// The code behind file sets this in the constructor
    /// </summary>
    public Window Window { get; set; }
    #endregion public auto properties

    /// <summary>
    /// Check to see if we are in design mode to keep WIA from getting called
    /// bringing the designer and dev studio to its knees
    /// </summary>
    private static bool InDesignMode
    {
      get
      {
        var dep = new DependencyObject();
        return DesignerProperties.GetIsInDesignMode(dep);
      }
    }

    #region WIA events
    void DeviceManagerOnEvent(string eventId, string deviceId, string itemId)
    {
      switch (eventId)
      {
        // A new device was connected
        case EventID.wiaEventDeviceConnected:
          {
            // Don't care if it is not a camera
            var device_info = WiaGetDeviceInfo(deviceId);
            if (device_info == null || device_info.Type != WiaDeviceType.CameraDeviceType)
              break;
            // It is a camera so add it to our device collection if it is not
            // already there.
            if (FromCameraCollection(deviceId) == null)
              CameraCollection.Add(new Camera(device_info));
          }
          break;
        // A device was disconnected
        case EventID.wiaEventDeviceDisconnected:
          {
            // If it is not in our camera collection then bail
            var device = FromCameraCollection(deviceId);
            if (device == null)
              break;
            // It is in our camera collection
            // If it is the selected camera set the selection to none
            if (SelectedDevice == device)
              SelectedDevice = null;
            // Remove the camera from the camera collection
            CameraCollection.Remove(device);
          }
          break;
      }
    }
    #endregion WIA events

    #region Camera collection access

    /// <summary>
    /// Clear the camera collection and use WIA to rebuild the collection by
    /// enumerating the currently connected device list
    /// </summary>
    public void RefreshCameraCollection()
    {
      CameraCollection.Clear();
      foreach (var device in m_device_manager.DeviceInfos.Cast<DeviceInfo>().Where(device => device.Type == WiaDeviceType.CameraDeviceType))
        CameraCollection.Add(new Camera(device));
    }
    /// <summary>
    /// Lookup a camera Id in the camera collection
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private Camera FromCameraCollection(string id)
    {
      return (string.IsNullOrWhiteSpace(id)
        ? null
        : CameraCollection.FirstOrDefault(device => id.Equals(device.Id, StringComparison.Ordinal)));
    }
    /// <summary>
    /// Get a device from the WIA.DeviceManager device list
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private DeviceInfo WiaGetDeviceInfo(string id)
    {
      return (string.IsNullOrWhiteSpace(id)
        ? null
        : m_device_manager.DeviceInfos.Cast<DeviceInfo>()
          .FirstOrDefault(device => id.Equals(device.DeviceID, StringComparison.Ordinal)));
    }
    /// <summary>
    /// Display a modal Window while connecting to a camera and close the
    /// window when done.  Uses a worker thread to connect to the camera and
    /// provides a cancel button to abort.
    /// </summary>
    /// <param name="camera"></param>
    /// <returns></returns>
    private Device ConnectToDevice(Camera camera)
    {
      if (InDesignMode || camera == null) return null;

      var device_info = WiaGetDeviceInfo(camera.Id);
      if (device_info == null) return null;

      var window = new ConnectToCameraWindow { Camera = camera, DeviceManager = m_device_manager, Owner = Window };

      try
      {
        window.ShowDialog();
        var device = window.Device;
        return device;
      }
      catch (Exception e)
      {
        WriteException(e);
        if (window.IsVisible)
          window.Close();
        return null;
      }

    }

    #endregion Camera collection access

    #region async helpers
    private void AsyncOnEnd()
    {
      LoadingGridVisibility = "Collapsed";
      if (m_cancellation_token != null)
        m_cancellation_token.Dispose();
      m_cancellation_token = null;
    }
    private CancellationTokenSource m_cancellation_token;
    #endregion

    #region Image list methods
    /// <summary>
    /// Refresh the image collection by connecting to the currently selected
    /// camera then extracting the image file list complete with preview images
    /// </summary>
    private async void RefreshImageCollection()
    {
      // Should only be non null if there is a cancelable async call running,
      // if so then cancel the call and clean up.
      if (m_cancellation_token != null)
      {
        try
        {
          // Kill the currently running async operation
          throw new OperationCanceledException();
        }
        catch (OperationCanceledException)
        {
        }
        // The exception handler used to execute the asycn function may have
        // cleaned this up since the last time we checked
        if (m_cancellation_token == null)
          m_cancellation_token.Dispose();
        m_cancellation_token = null;
      }
      // Used by the async function to update the progress bar
      var progress = new Progress<CacheItem>();
      // Will eventually contain the camera connection
      Device camera_connection = null;
      // Disable the import button
      IsImportButtonEnabled = false;
      // Disable most of the controls while this is happening, the progress bar
      // and cancel button should be visible 
      EnableEditControls = false;
      // Clear the current image collection
      ImageCollection.Clear();
      // Clear the IImage cache
      m_cache_item_list.Clear();
      try
      {
        var camera = SelectedDevice;
        if (camera == null)
        {
          // No camera is selected
          EnableEditControls = true;
          return;
        }

        // Try to connect to the currently selected camera
        camera_connection = ConnectToDevice(camera);
        if (camera_connection == null)
        {
          // Unable to connect to the current camera
          EnableEditControls = true;
          return;
        }
      }
      catch (Exception e)
      {
        WriteException(e);
        EnableEditControls = true;
        return;
      }

      try
      {
        // Asynchronously scan the camera for images to import, just builds an
        // image list, does not add anything to the ImageCollection yet
        m_cancellation_token = new CancellationTokenSource();
        progress.ProgressChanged += delegate(object sender, CacheItem item)
        {
          try
          {
            m_cache_item_list.Add(item);
            var count = m_cache_item_list.Count;
            LoadProgressText = "Image " + m_cache_item_list.Count;
            ProgressValue = count - (count/100*100);
          }
          catch (Exception exception)
          {
            WriteException(exception);
          }
        };
        // Display the progress bar and cancel button
        LoadingGridVisibility = "Visible";
        // Populate the m_cache_item_list
        StatusBarText = "Scanning the camera for images to import";
        await AsyncBuildPictureListTask(camera_connection, progress);
        StatusBarText = "Images found, building thumbnail list";
      }
      catch (OperationCanceledException)
      {
        // AsyncBuildPictureListTask was canceled for some reason
        StatusBarText = "Image scan canceled";
      }
      catch (Exception e)
      {
        StatusBarText = "Encountered an error while scanning the camera for images";
        WriteException(e);
      }
      finally
      {
        AsyncOnEnd();
      }

      try
      {
        // Display the progress bar and cancel button
        LoadingGridVisibility = "Visible";
/*
        // Add empty items to the image collection to populate the LiseView
        // using the m_cache_item_list before adding new items
        foreach (var item in m_cache_item_list)
          ImageCollection.Add(new TransferableImageFile(item));
        // Initialize the items above using the cached IItems list
        m_cancellation_token = new CancellationTokenSource();
        var init_progress = new Progress<TransferableImageFile>();
        init_progress.ProgressChanged += (sender, file) =>
        {
          if (file == null) return;
          file.InitializeAll();
          file.Include = true;
        };
        StatusBarText = "Importing thumbnail images";
        await AsyncInitializeCollectionUsingCache(init_progress);
        StatusBarText = "Thumbnails successfully imported, check items to import then click on the Import button";
 */
        m_cancellation_token = new CancellationTokenSource();
        var init_progress = new Progress<CacheItem>();
        init_progress.ProgressChanged += (sender, cacheItem) =>
        {
          if (cacheItem == null) return;
          var image = new TransferableImageFile(cacheItem.Parent, cacheItem.Item);
          image.InitializeAll();
          image.Include = true;
          ImageCollection.Add(image);
        };
        StatusBarText = "Importing thumbnail images";
        await AsyncPopulateItemsCollection(init_progress);
        StatusBarText = string.Format("{0} Thumbnails successfully imported, check items to import then click on the Import button", ImageCollection.Count);
      }
      catch (OperationCanceledException)
      {
        // AsyncBuildPictureListTask was canceled for some reason
        StatusBarText = string.Format("Thumbnail import canceled before it could finish, {0} items were found", ImageCollection.Count);
      }
      catch (Exception e)
      {
        WriteException(e);
        StatusBarText = "Encountered an error while importing thumbnail images";
      }
      finally
      {
        EnableEditControls = true;
        AsyncOnEnd();
        IsImportButtonEnabled = (ImageCollection.Count > 0);
      }
    }

    class CacheItem
    {
      public CacheItem(IItems parent, IItem item)
      {
        Parent = parent;
        Item = item;
      }
      public IItems Parent { get; private set; }
      public IItem Item { get; private set; }
    }
    private readonly List<CacheItem> m_cache_item_list = new List<CacheItem>();
    private readonly List<TransferableImageFile>m_files_to_delete = new List<TransferableImageFile>(); 
    /// <summary>
    /// Asynchronous call to InitializeCollectionUsingCache(IProgress)
    /// Uses the progress object to initialize each item to make sure the
    /// thumbnail image gets created in the UI thread.
    /// </summary>
    /// <param name="progress"></param>
    /// <returns></returns>
    private Task AsyncInitializeCollectionUsingCache(IProgress<TransferableImageFile> progress)
    {
      return Task.Run(() => InitializeCollectionUsingCache(progress));
    }
    /// <summary>
    /// Uses the progress object to initialize each item to make sure the
    /// thumbnail image gets created in the UI thread.
    /// </summary>
    /// <param name="progress"></param>
    private void InitializeCollectionUsingCache(IProgress<TransferableImageFile> progress)
    {
      foreach (var item in ImageCollection)
        progress.Report(item);
    }
    /// <summary>
    /// Testing asynchronously adding items to the ItemCollection
    /// </summary>
    /// <param name="progress"></param>
    /// <returns></returns>
    private Task AsyncPopulateItemsCollection(IProgress<CacheItem> progress)
    {
      return Task.Run(() => AsyncItemsCollection(progress));
    }

    private void AsyncItemsCollection(IProgress<CacheItem> progress)
    {
      foreach (var item in m_cache_item_list)
        progress.Report(item);
    }

    private Task AsyncBuildPictureListTask(IDevice device, IProgress<CacheItem> progress)
    {
      m_cache_item_list.Clear();
      return Task.Run(() => BuildPictureItemList(device.Items, progress));
    }

    public string LoadProgressText
    {
      get { return (m_load_progress_text ?? string.Empty); }
      set { SetProperty(value, ref m_load_progress_text); }
    }
    private string m_load_progress_text = "Scanning camera for pictures";

    public int ProgressValue
    {
      get { return m_progress_value; }
      set { SetProperty(value, ref m_progress_value); }
    }
    private int m_progress_value;

    private void BuildPictureItemList(IItems items, IProgress<CacheItem> progress)
    {
      for (var i = 1; i < items.Count + 1; i++)
      {
        if (m_cancellation_token != null && m_cancellation_token.IsCancellationRequested)
          return;
        var item = items[i];
        if (TransferableImageFile.ItemIsImageFile(item))
          progress.Report(new CacheItem(items, item));
        if (m_cancellation_token != null && m_cancellation_token.IsCancellationRequested)
          return;
        BuildPictureItemList(item.Items, progress);
      }
    }

    private void AddItem(object item)
    {
      var cache_item = item as CacheItem;
      if (cache_item == null) return;
      var image = new TransferableImageFile(cache_item.Parent, cache_item.Item);
      image.InitializeAll();
      ImageCollection.Add(image);
    }
    #endregion Image list methods

    private void WriteFile(TransferableImageFile image)
    {
      ImageFile image_file = null;
      try
      {
        image_file = image.Item.Transfer(FormatID.wiaFormatJPEG) as ImageFile;
        if (image_file == null) return;
        var extension = (image_file.FileExtension ?? string.Empty).ToLower().Trim();
        if (string.IsNullOrWhiteSpace(extension))
          extension = ".jpg";
        else if (!extension.StartsWith("."))
          extension = "." + extension;
        var file_name = GetNextFileName(image.Name, extension);
        image_file.SaveFile(file_name);
        if (image.IsTimeStampSet && File.Exists(file_name))
        {
          try
          {
            File.SetCreationTime(file_name, image.TimeStamp);
            File.SetLastWriteTime(file_name, image.TimeStamp);
          }
          catch (Exception ex)
          {
            WriteException(ex);
          }
        }
        if (DeleteWhenDone)
          m_files_to_delete.Add(image);
      }
      catch (Exception e)
      {
        WriteException(e);
      }
      finally
      {
        if (image_file != null) Marshal.ReleaseComObject(image_file);
      }
    }

    string GetNextFileName(string prefix, string extension)
    {
      var index = 0;
      var name = string.IsNullOrWhiteSpace(prefix) ? m_date_string : prefix.Trim();
      if (!string.IsNullOrWhiteSpace(FolderSuffix))
        name += ("-" + FolderSuffix.Trim());
      var folder = Path.Combine(m_input_folder, name);
      var file_name = string.Format("{0}{1}", folder, extension);
      while (File.Exists(file_name))
        file_name = string.Format("{0}-{1:d3}{2}", folder, ++index, extension);
      return file_name;
    }

    private string m_date_string = string.Empty;
    private string m_input_folder = string.Empty;
    private bool CreateImportFolder(out bool errorDisplayed)
    {
      m_input_folder = string.Empty;
      m_date_string = string.Empty;
      errorDisplayed = false;
      try
      {
        var today = DateTime.Now;
        m_date_string = today.ToString("yyyy-MM-dd");
        m_input_folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), m_date_string);
        if (!string.IsNullOrWhiteSpace(FolderSuffix.Trim()))
          m_input_folder += "-" + FolderSuffix.Trim();
        if (!Directory.Exists(m_input_folder))
          Directory.CreateDirectory(m_input_folder);
        return Directory.Exists(m_input_folder);
      }
      catch (Exception e)
      {
        WriteException(e);
        errorDisplayed = true;
        return ErrorMessage(
          "Error creating import folder: " + m_input_folder
            + Environment.NewLine + "Exception:" + Environment.NewLine + e.Message);
      }
    }

    private bool ErrorMessage(string message)
    {
      MessageBox.Show(
        Window,
        message,
        "Import Wizard Error",
        MessageBoxButton.OK,
        MessageBoxImage.Asterisk,
        MessageBoxResult.OK,
        MessageBoxOptions.ServiceNotification);
      return false;
    }

    #region Import photos

    public void ImportPhotos()
    {
      IsImportButtonEnabled = false;
      EnableEditControls = false;
      m_files_to_delete.Clear();
      var import_list = ImageCollection.Where(image => image.Include).ToList();
      if (import_list.Count < 1)
        ErrorMessage("You must select one or more images to import");
      var selected = SelectedDevice;
      if (selected == null)
      {
        ErrorMessage("No camera selected");
        IsImportButtonEnabled = true;
        EnableEditControls = true;
        return;
      }
      var device_name = selected.Name;
      var id = selected.Id;
      var device_info = WiaGetDeviceInfo(id);
      if (device_info == null)
      {
        ErrorMessage(string.Format("Camera \"{0}\" is no longer available", device_name));
        IsImportButtonEnabled = true;
        EnableEditControls = true;
        return;
      }
      Device camera = null;
      try
      {
        camera = device_info.Connect();
        if (camera == null)
          ErrorMessage(string.Format("Unable to connect to device \"{0}\"", device_name));

        bool error_displayed;
        if (!CreateImportFolder(out error_displayed))
        {
          if (!error_displayed)
            ErrorMessage(string.Format("Unable to create import folder \"{0}\"", m_input_folder));
          return;
        }
      }
      catch (COMException ce)
      {
        var error_code = ce.GetWiaErrorCode();
        var description = WiaHelper.GetErrorCodeDescription(error_code);
        System.Diagnostics.Debug.WriteLine("WIA Error description: " + description);
        WriteException(ce);
        ErrorMessage("COM Error: " + description);
        return;
      }
      catch (Exception e)
      {
        WriteException(e);
        return;
      }
      finally
      {
        DoneImporting();
      }

      try
      {
        // Display a progress window and write the files
        var window = new Windows.ImportFilesWindow
        {
          ImageCollection = ImageCollection,
          FileWriter = WriteFile,
          Camera = camera,
          Owner = Window
        };
        if (DeleteWhenDone)
        {
          window.DeleteFile = DeleteFile;
          window.FileDeleted = FileDeleted;
          window.FilesToDelete = () => m_files_to_delete;
        }
        window.ShowDialog();
      }
      catch (Exception e)
      {
        WriteException(e);
      }
      finally
      {
        DoneImporting();
      }
    }

    private bool DeleteFile(TransferableImageFile file)
    {
      if (file == null) return false;
      var index = 1;
      var id = file.Item.ItemID;
      while (index <= file.Parent.Count && !id.Equals(file.Parent[index].ItemID, StringComparison.Ordinal))
        index++;
      if (index > file.Parent.Count)
        return false;
      file.Parent.Remove(index);
      return true;
    }

    private void FileDeleted(TransferableImageFile file)
    {
      if (ImageCollection.Contains(file))
        ImageCollection.Remove(file);
    }

    private void DoneImporting()
    {
      IsImportButtonEnabled = true;
      EnableEditControls = true;
    }
    #endregion Import photos

    #region Window callbacks

    public void WindowClosing(object sender, CancelEventArgs e)
    {
      if (m_cancellation_token == null) return;

      if (m_cancellation_token.IsCancellationRequested)
        e.Cancel = true;
      else
        m_cancellation_token.Cancel();
    }

    public void StopLoadingButtonClick()
    {
      if (m_cancellation_token != null && !m_cancellation_token.IsCancellationRequested)
        m_cancellation_token.Cancel();
    }

    public void SetIncludeForAll(bool include)
    {
      if (m_cancellation_token != null) return;
      var import_enabled = IsImportButtonEnabled;
      IsImportButtonEnabled = false;
      var edit_enabled = EnableEditControls;
      EnableEditControls = false;
      foreach (var image in ImageCollection)
        image.Include = include;
      IsImportButtonEnabled = import_enabled;
      EnableEditControls = edit_enabled;
    }

    #endregion Window callbacks

    #region Binable properties
    public ObservableCollection<Camera> CameraCollection
    {
      get { return m_camera_collection; }
    }
    private readonly ObservableCollection<Camera> m_camera_collection = new ObservableCollection<Camera>();

    public Camera SelectedDevice
    {
      get { return m_selected_device; }
      set
      {
        if (value == m_selected_device) return;
        m_selected_device = value;
        RaisePropertyChanged("SelectedDevice");
        if (SelectedDeviceChanged == null)
          SelectedDeviceChanged += (sender, args) => RefreshImageCollection();
        SelectedDeviceChanged.Invoke(this, EventArgs.Empty);
      }
    }
    private Camera m_selected_device;
    private event EventHandler SelectedDeviceChanged;

    public bool IsImportButtonEnabled
    {
      get { return m_is_import_button_enabled; }
      set { SetProperty(value, ref m_is_import_button_enabled); }
    }
    private bool m_is_import_button_enabled;

    public bool EnableEditControls
    {
      get { return m_enable_edit_controls; }
      set { SetProperty(value, ref m_enable_edit_controls); }
    }
    private bool m_enable_edit_controls = true;

    public bool DeleteWhenDone
    {
      get { return m_delete_when_done; }
      set { SetProperty(value, ref m_delete_when_done); }
    }
    private bool m_delete_when_done = true;

    public string LoadingGridVisibility
    {
      get { return m_loading_grid_visibility; }
      set { SetProperty(value, ref m_loading_grid_visibility); }
    }
    private string m_loading_grid_visibility = "Collapsed";

    public string FolderSuffix
    {
      get { return (m_folder_suffix ?? String.Empty); }
      set { SetProperty(value, ref m_folder_suffix); }
    }
    private string m_folder_suffix;

    public ObservableCollection<TransferableImageFile> ImageCollection
    {
      get { return m_image_list; }
      set
      {
        if (m_image_list == value) return;
        m_image_list = value;
        RaisePropertyChanged("ImageCollection");
      }
    }
    ObservableCollection<TransferableImageFile> m_image_list = new ObservableCollection<TransferableImageFile>();

    public string StatusBarText
    {
      get { return m_status_bar_text; }
      set { SetProperty(value, ref m_status_bar_text); }
    }
    private string m_status_bar_text;

    #endregion Binable properties
  }
}
