using System.Windows;

namespace CameraWizard.Windows
{
  /// <summary>
  /// Interaction logic for ImportOptions.xaml
  /// </summary>
  public partial class ImportOptions
  {
    public ImportOptions()
    {
      InitializeComponent();
      TextBoxDescription.Text = Description;
    }

    public string Description
    {
      get { return m_description; }
      set { m_description = value; }
    }
    private static string m_description;
    private void OkButtonClick(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
      Description = TextBoxDescription.Text;
      Close();
    }

    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
      DialogResult = false;
      Close();
    }
  }
}
