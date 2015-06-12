using System;
using System.Windows;
using WIA;

namespace CameraWizard
{
  /// <summary>
  /// Interaction logic for BuildImangeListWindow.xaml
  /// </summary>
  public partial class BuildImangeListWindow
  {
    public BuildImangeListWindow()
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

    public Device Device
    {
      get { return (ViewModel == null ? null : ViewModel.Device); }
      set { if (ViewModel != null) ViewModel.Device = value; }
    }
    BuildingImageListViewModel ViewModel { get { return DataContext as BuildingImageListViewModel; } }
  }
}
