using System.Windows;

namespace PasswordManager
{
  /// <summary>
  /// Логика взаимодействия для StartupWindow.xaml
  /// </summary>
  public partial class StartupWindow : Window
  {
    public StartupWindow()
    {
      InitializeComponent();
    }

    private void OpenBase(object sender, RoutedEventArgs e) =>
      App.OpenBase(this, true);
    private void CreateBase(object sender, RoutedEventArgs e) =>
      App.CreateBase(this, true);

    private void OpenGeneratorWindow(object sender, RoutedEventArgs e) =>
      App.OpenPasswordGenerator();

    private void ShowAbout(object sender, RoutedEventArgs e) =>
      App.ShowAbout();
  }
}
