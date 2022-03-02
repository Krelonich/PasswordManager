using Microsoft.Shell;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace PasswordManager
{
  /// <summary>
  /// Логика взаимодействия для App.xaml
  /// </summary>
  public partial class App : Application, ISingleInstanceApp
  {
    public static readonly Dictionary<string, Window> windowDictionary = new Dictionary<string, Window>();
    private static PasswordGeneratorWindow generatorWindow;

    public const string ProductName = "Менеджер паролей", ProductVersion = "0.1.1";
    private const string AppInstanceName = @"Local\PasswordManager_Instance";

    private const string FILE_FILTER = "Файл с базой паролей|*.pbase";

    private void OnStartup(object sender, StartupEventArgs e)
    {
      if (!SingleInstance<App>.Initialize(AppInstanceName, e.Args))
        Shutdown();
    }
    private void OnExit(object sender, ExitEventArgs e) =>
      SingleInstance<App>.Cleanup();

    public async void ProcessCommandLineArgs(string[] args)
    {
      if (args.Length > 0 && System.IO.File.Exists(args[0]))
      {
        Window owner = Windows.Count > 0 ? Windows[0] : null;

        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        if (!await LoadBase(args[0], owner) && owner != null)
          Shutdown();
        else
          ShutdownMode = ShutdownMode.OnLastWindowClose;
      }
      else if (Windows.Count > 0)
      {
        Windows[0].Normalize();
      }
      else
      {
        new StartupWindow().Show();
      }
    }

    public static void OpenPasswordGenerator()
    {
      if (generatorWindow == null || !generatorWindow.IsLoaded)
        (generatorWindow = new PasswordGeneratorWindow()).Show();
      else
        generatorWindow.Activate();
    }

    public static void OpenBase(Window owner, bool autoClose = false) =>
      PrepareBase(new OpenFileDialog { Title = "Открытие базы паролей" }, owner, autoClose);
    public static void CreateBase(Window owner, bool autoClose = false) =>
      PrepareBase(new SaveFileDialog { Title = "Создание базы паролей", FileName = "passwords" }, owner, autoClose);
    public static async void SaveBase(Window owner, PasswordBase passwordBase)
    {
      SaveFileDialog fileDialog = new SaveFileDialog { Title = "Сохранение базы паролей", FileName = "passwords", Filter = FILE_FILTER };
      owner.IsEnabled = false;
      if (fileDialog.ShowDialog() == true)
      {
        string currentPath = passwordBase.FilePath, newPath = fileDialog.FileName;
        if (currentPath != newPath && windowDictionary.ContainsKey(newPath))
        {
          ShowRewriteError(newPath);
        }
        else
        {
          passwordBase.FilePath = newPath;
          if (await passwordBase.Save())
          {
            windowDictionary.Remove(currentPath);
            windowDictionary[newPath] = owner;
          }
          else
          {
            passwordBase.FilePath = currentPath;
          }
        }
      }
      owner.IsEnabled = true;
    }

    private static async void PrepareBase(FileDialog fileDialog, Window owner, bool autoClose)
    {
      owner.IsEnabled = false;
      fileDialog.Filter = FILE_FILTER;
      if (
        fileDialog.ShowDialog() == true &&
        await LoadBase(fileDialog.FileName, owner, fileDialog is SaveFileDialog) &&
        autoClose
      )
      {
        owner.Close();
      }
      else
      {
        owner.IsEnabled = true;
      }
    }


    private static async Task<bool> LoadBase(string filePath, Window owner, bool isNewBase = false)
    {
      if (windowDictionary.TryGetValue(filePath, out Window window))
      {
        if (isNewBase)
        {
          ShowRewriteError(filePath);
          return false;
        }
        else
        {
          window.Normalize();
          return true;
        }
      }

      while (true)
      {
        if (owner != null) owner.Normalize();
        PasswordWindow passwordWindow = new PasswordWindow(isNewBase, System.IO.Path.GetFileName(filePath)) { Owner = owner };
        windowDictionary[filePath] = passwordWindow;

        if (passwordWindow.ShowDialog() == true)
        {
          try
          {
            PasswordBase passwordBase = isNewBase
              ? new PasswordBase(filePath, passwordWindow.PasswordBox.SecurePassword)
              : await PasswordBase.Load(filePath, passwordWindow.PasswordBox.SecurePassword);

            MainWindow newWindow = new MainWindow(passwordBase);
            windowDictionary[filePath] = newWindow;
            newWindow.Show();

            return true;
          }
          catch
          {
            MessageBox.Show("Введён неправильный мастер-пароль или база повреждена", "Неверный мастер-пароль", MessageBoxButton.OK, MessageBoxImage.Error);
          }
        }
        else
        {
          windowDictionary.Remove(filePath);
          return false;
        }
      }
    }

    private static void ShowRewriteError(string filePath) =>
      MessageBox.Show($"Файл с базой паролей в расположении {filePath} в данный момент используется", "Невозможно перезаписать базу паролей", MessageBoxButton.OK, MessageBoxImage.Error);

    public static void ShowAbout() =>
      MessageBox.Show(
        $"Версия программы: {ProductVersion}\r\n\r\n" +
        "Данная программа предназначена для безопасного хранения паролей и заметок и включает в себя функцию генерации стойких паролей.\r\n\r\n" +
        "Программа разработана в рамках выполнения студенческой проектной практики.\r\n\r\n" +
        "ВНИМАНИЕ: автор не предоставляет каких-либо гарантий корректной работы данной программы. Её использование возможно только на свой страх и риск.", $"О программе - {ProductName}", MessageBoxButton.OK, MessageBoxImage.Information);
  }
}
