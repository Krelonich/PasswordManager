using System.Windows;
using System.Windows.Controls;

namespace PasswordManager
{
  /// <summary>
  /// Логика взаимодействия для PasswordWindow.xaml
  /// </summary>
  public partial class PasswordWindow : Window
  {
    private readonly bool isCreating;
    public Visibility NewPasswordElementsVisibility => isCreating ? Visibility.Visible : Visibility.Collapsed;

    public PasswordWindow(bool _isCreating, string fileName)
    {
      isCreating = _isCreating;
      DataContext = NewPasswordElementsVisibility;
      InitializeComponent();
      DescriptionTextBlock.Text = isCreating
        ? $"Придумайте и введите надёжный мастер-пароль.\r\nВ дальнейшем он понадобится для открытия файла с базой паролей."
        : $"Введите мастер-пароль к открываемой базе ({fileName}).";
    }

    private void Confirm(object sender, RoutedEventArgs e)
    {
      if (VerifyPassword())
        DialogResult = true;
    }

    private void OpenHelp(object sender, RoutedEventArgs e) =>
      MessageBox.Show(
        "* Постарайтесь придумать пароль подлиннее. Чем длиннее пароль, тем сложнее его подобрать. Однако, не стоит его делать слишком длинным - в таком случае, Вам будет трудно его запомнить и набирать при каждом открытии базы. Длина вполне хорошего мастер-пароля - 16 символов.\r\n" +
        "* Не используйте простые последовательности (например, «12345678», «password», «qwertyui»). Они все уже давно известны злоумышленникам и, скорее всего, будут перебираться в первую очередь.\r\n" +
        "* Разнообразьте состав символов в Вашем мастер-пароле. Помните, что в его составе можно использовать заглавные и строчные буквы (как латиницы, так и кириллицы), специальные символы (*, #, ?, @ и т.д.), цифры и даже смайлы 😉 (как и другие символы UTF-8). Чем больше алфавит Вашего пароля, тем сложнее его подобрать.\r\n" +
        "* Попробуйте придумать в качестве основы для мастер-пароля необычную и нелогичную фразу, которую Вам будет легко запомнить. Она окажется понятна Вам, но абсолютно неожиданной при машинном переборе. (Например, «3Поезда поплыли в Космос». Почему бы и нет?)\r\n" +
        "* По возможности придумайте дополнительное правило, по которому Вы исказите основную фразу. Данное правило не обязательно должно быть уникальным для каждого пароля. (Например, каждый третий символ фразы прописной, вместо пробелов специальный символ и т.д.)\r\n" +
        "* Мастер-пароль должен быть уникальным. Если Вы уже где-либо использовали такой пароль, то его использование в качестве мастер-пароля крайне не рекомендуется.\r\n" +
        "* Используйте В МЕРУ сложный мастер-пароль, так как если Вы его забудете, то потеряете доступ ко всей базе паролей.",
        "Как придумать надёжный мастер-пароль?", MessageBoxButton.OK, MessageBoxImage.Information);

    private bool VerifyPassword() =>
      PasswordBox.Password.Length >= 8 &&
      (!isCreating || PasswordBox.Password == PasswordBox2.Password);

    private void OnPasswordChanged(object sender, RoutedEventArgs e) =>
      ConfirmButton.IsEnabled = VerifyPassword();
  }
}
