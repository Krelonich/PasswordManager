using System.Collections.Generic;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;

namespace PasswordManager
{
  /// <summary>
  /// Логика взаимодействия для PasswordGeneratorWindow.xaml
  /// </summary>
  public partial class PasswordGeneratorWindow : Window
  {
    private readonly bool isDialog;

    private const string SPECIAL_SYMBOLS = "~!@#$%^;:&?*-_=+()[]{}<>|\\/,.",
                         UPPERCASE_LATIN_SYMBOLS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                         LOWERCASE_LATIN_SYMBOLS = "abcdefghijklmnopqrstuvwxyz",
                         UPPERCASE_CYRILLIC_SYMBOLS = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ",
                         LOWERCASE_CYRILLIC_SYMBOLS = "абвгдеёжзийклмнопрстуфхцчшщъыьюэя",
                         DIGITAL_SYMBOLS = "0123456789";
    private const int MIN_PASSWORD_LENGTH = 4, MAX_PASSWORD_LENGTH = 100,
                      BUFFER_SIZE = 16, BYTE_SIZE = 256;

    public PasswordGeneratorWindow(bool _isDialog = false)
    {
      isDialog = _isDialog;
      InitializeComponent();
      ResetButtonClick(null, null);
    }

    private void SuppressPasting(object sender, DataObjectPastingEventArgs e) =>
      e.CancelCommand();

    private void MinusButtonClick(object sender, RoutedEventArgs e) =>
      FixPasswordLength(-1);
    private void PlusButtonClick(object sender, RoutedEventArgs e) =>
      FixPasswordLength(1);

    private void CopyPassword(object sender, RoutedEventArgs e) =>
      Clipboard.SetText(PasswordTextBox.Text);

    private void NumberTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
    {
      if (e.Delta > 0) PlusButtonClick(null, null);
      else if (e.Delta < 0) MinusButtonClick(null, null);
    }
    private void NumberTextBox_Input(object sender, TextCompositionEventArgs e)
    {
      if (e.Text[0] < 48 || e.Text[0] > 57)
        e.Handled = true;
    }
    private void NumberTextBox_LostFocus(object sender, RoutedEventArgs e) =>
      FixPasswordLength();

    private void FixPasswordLength(int addend = 0)
    {
      int.TryParse(PasswordLengthTextBox.Text, out int value);
      value += addend;
      if (value < MIN_PASSWORD_LENGTH)
        value = MIN_PASSWORD_LENGTH;
      else if (value > MAX_PASSWORD_LENGTH)
        value = MAX_PASSWORD_LENGTH;
      PasswordLengthTextBox.Text = value.ToString();
    }

    private void ResetButtonClick(object sender, RoutedEventArgs e) =>
      SpecialSymbolsTextBox.Text = SPECIAL_SYMBOLS;

    private void GeneratePassword(object sender, RoutedEventArgs e)
    {
      // Подготовка алфавита для генерации пароля
      HashSet<char> alphabetSet = new HashSet<char>()
        .AddSymbols(UppercaseLatinCheckBox, UPPERCASE_LATIN_SYMBOLS)
        .AddSymbols(LowercaseLatinCheckBox, LOWERCASE_LATIN_SYMBOLS)
        .AddSymbols(UppercaseCyrillicCheckBox, UPPERCASE_CYRILLIC_SYMBOLS)
        .AddSymbols(LowercaseCyrillicCheckBox, LOWERCASE_CYRILLIC_SYMBOLS)
        .AddSymbols(DigitsCheckBox, DIGITAL_SYMBOLS)
        .AddSymbols(AdditionalCheckBox, SpecialSymbolsTextBox.Text);

      char[] alphabetArray = new char[alphabetSet.Count];
      alphabetSet.CopyTo(alphabetArray);
      int alphabetLength = alphabetArray.Length;

      if (alphabetLength > 256 || alphabetLength < 10)
      {
        MessageBox.Show("Количество символов в алфавите для пароля не может быть больше 256-и символов и меньше 10-и символов", "Неправильный размер алфавита", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }


      // Генерация случайного пароля необходимой длины
      char[] passwordArray = new char[int.Parse(PasswordLengthTextBox.Text)];

      using (RNGCryptoServiceProvider rngProvider = new RNGCryptoServiceProvider())
      {
        byte[] buffer = new byte[BUFFER_SIZE];
        int bufferOffset = BUFFER_SIZE, remainder = BYTE_SIZE % alphabetLength;
        for (int i = 0, l = passwordArray.Length; i < l; i++)
        {
          do
          {
            if (++bufferOffset >= BUFFER_SIZE)
            {
              bufferOffset = 0;
              rngProvider.GetBytes(buffer);
            }
          }
          while (buffer[bufferOffset] >= BYTE_SIZE - remainder);
          passwordArray[i] = alphabetArray[buffer[bufferOffset] % alphabetLength];
        }
      }

      // Работа с пользовательским интерфейсом
      PasswordTextBox.Text = new string(passwordArray);
      PasswordField.Visibility = Visibility.Visible;
      if (isDialog) SaveButton.Visibility = Visibility.Visible;
    }

    private void SavePassword(object sender, RoutedEventArgs e)
    {
      if (isDialog)
        DialogResult = true;
    }
  }
}
