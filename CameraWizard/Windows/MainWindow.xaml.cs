using System.Windows;
using CameraWizard.ViewModels;

namespace CameraWizard.Windows
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow
  {
    public MainWindow()
    {
      InitializeComponent();
      if (ViewModel != null) ViewModel.Window = this;
    }

    private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      if (DialogResult != true)
        return;
      if (ViewModel != null) ViewModel.WindowClosing(sender, e);
    }

    private void UnselectAllButtonClick(object sender, RoutedEventArgs e)
    {
      if (ViewModel != null) ViewModel.SetIncludeForAll(false);
    }

    private void SelectAllButtonClick(object sender, RoutedEventArgs e)
    {
      if (ViewModel != null) ViewModel.SetIncludeForAll(true);
    }

    private void ImportButtonClick(object sender, RoutedEventArgs e)
    {
      if (ViewModel != null) ViewModel.ImportPhotos();
    }

    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void StopLoadingButtonClick(object sender, RoutedEventArgs e)
    {
      if (ViewModel != null) ViewModel.StopLoadingButtonClick();
    }

    WizardViewModel ViewModel { get { return DataContext as WizardViewModel; } }
  }
}
