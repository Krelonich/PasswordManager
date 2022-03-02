using System.Windows;
using System.Windows.Controls;

namespace PasswordManager
{
  /// <summary>
  /// Логика взаимодействия для PasswordBlock.xaml
  /// </summary>
  public partial class PasswordBlock : UserControl
  {
    public string Password
    {
      get => (string)GetValue(PasswordProperty);
      set => SetValue(PasswordProperty, value);
    }
    public static readonly DependencyProperty PasswordProperty =
      DependencyProperty.Register("Password", typeof(string), typeof(PasswordBlock), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public PasswordBlock()
    {
      InitializeComponent();
    }

    private void CopyPassword(object sender, RoutedEventArgs e) =>
      Clipboard.SetText(PasswordTextBox.Text);

    private void OpenGeneratorWindow(object sender, RoutedEventArgs e)
    {
      PasswordGeneratorWindow generatorWindow = new PasswordGeneratorWindow(true);
      if (generatorWindow.ShowDialog() == true)
        Password = generatorWindow.PasswordTextBox.Text;
    }

    private void PasswordTextBox_GotFocus(object sender, RoutedEventArgs e) =>
      PasswordBarrier.Visibility = Visibility.Hidden;

    private void PasswordTextBox_LostFocus(object sender, RoutedEventArgs e) =>
      PasswordBarrier.Visibility = Visibility.Visible;
  }
}
