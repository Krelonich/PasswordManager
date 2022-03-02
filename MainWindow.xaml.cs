using System.Threading.Tasks;
using System.Windows;

namespace PasswordManager
{
  /// <summary>
  /// Логика взаимодействия для MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private readonly PasswordBase passwordBase;
    private bool isDirty = false;

    private const string NEW_NOTE = "Новая заметка", NEW_PASSWORD = "Новая запись";

    public MainWindow(PasswordBase _passwordBase)
    {
      passwordBase = _passwordBase;
      DataContext = passwordBase;
      UpdateTitle();
      passwordBase.PropertyChanged += (o, e) => UpdateTitle();
      InitializeComponent();
      NoteSelectionChanged(null, null);
      PasswordSelectionChanged(null, null);
    }
    private void UpdateTitle() =>
      Title = $"{passwordBase.FileName} - {App.ProductName}";
    private async void ChangeBase(object sender, RoutedEventArgs e)
    {
      if (await ConfirmExit())
      {
        new StartupWindow().Show();
        isDirty = false;
        Close();
      }
    }

    private void OpenGeneratorWindow(object sender, RoutedEventArgs e) =>
      App.OpenPasswordGenerator();

    private void ShowAbout(object sender, RoutedEventArgs e) =>
      App.ShowAbout();

    private void CreateElement<T>(
      System.Collections.ObjectModel.ObservableCollection<T> collection,
      System.Windows.Controls.ListBox listBox,
      string defaultName
    ) where T : RecordData, new()
    {
      string newElementName = defaultName;
      for (int i = 1; true; i++)
      {
        bool isFound = false;
        foreach (T element in collection)
        {
          if (element.Name == newElementName)
          {
            isFound = true;
            break;
          }
        }
        if (!isFound) break;
        else newElementName = $"{defaultName} {i}";
      }

      T newElement = new T { Name = newElementName };
      collection.Add(newElement);
      listBox.SelectedItem = newElement;
      listBox.ScrollIntoView(newElement);

      isDirty = true;
    }
    private void CreateNote(object sender, RoutedEventArgs e) =>
      CreateElement(passwordBase.Data.Notes, NotesListBox, NEW_NOTE);
    private void CreatePassword(object sender, RoutedEventArgs e) =>
      CreateElement(passwordBase.Data.Passwords, PasswordsListBox, NEW_PASSWORD);

    private void RemoveNote(object sender, RoutedEventArgs e)
    {
      if (
        NotesListBox.SelectedIndex >= 0 &&
        MessageBox.Show(
          $"Вы уверены, что хотите безвозвратно удалить заметку «{((NoteData)NotesListBox.SelectedItem).Name}»?",
          "Удаление заметки", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes
      )
        passwordBase.Data.Notes.RemoveAt(NotesListBox.SelectedIndex);
      isDirty = true;
    }
    private void NoteSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
      if (RemoveNoteButton != null && NoteContentControl != null)
        RemoveNoteButton.IsEnabled = NoteContentControl.IsEnabled = NotesListBox.SelectedIndex >= 0;
    }

    private void RemovePassword(object sender, RoutedEventArgs e)
    {
      if (
        PasswordsListBox.SelectedIndex >= 0 &&
        MessageBox.Show(
          $"Вы уверены, что хотите безвозвратно удалить запись «{((PasswordData)PasswordsListBox.SelectedItem).Name}»?",
          "Удаление записи", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes
      )
        passwordBase.Data.Passwords.RemoveAt(PasswordsListBox.SelectedIndex);
      isDirty = true;
    }
    private void PasswordSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
      if (RemovePasswordButton != null && PasswordContentControl != null)
        RemovePasswordButton.IsEnabled = PasswordContentControl.IsEnabled = PasswordsListBox.SelectedIndex >= 0;
    }

    private void DataChanged(object sender, System.Windows.Data.DataTransferEventArgs e) =>
      isDirty = true;

    private async Task<bool> ConfirmExit()
    {
      if (!isDirty) return true;
      MessageBoxResult result = MessageBox.Show($"Вы хотите сохранить изменения в базе {passwordBase.FileName}?", "Закрытие базы", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
      if (result == MessageBoxResult.Yes)
        return await passwordBase.Save();
      else if (result == MessageBoxResult.Cancel)
        return false;
      return true;
    }
    private async void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      e.Cancel = !await ConfirmExit();
      if (!e.Cancel)
      {
        App.windowDictionary.Remove(passwordBase.FilePath);
        passwordBase.Dispose();
      }
    }

    private async void SaveBase(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
    {
      if (await passwordBase.Save())
        isDirty = false;
    }
    private void SaveBaseAs(object sender, System.Windows.Input.ExecutedRoutedEventArgs e) =>
      App.SaveBase(this, passwordBase);
    private void OpenBase(object sender, System.Windows.Input.ExecutedRoutedEventArgs e) =>
      App.OpenBase(this);
    private void NewBase(object sender, System.Windows.Input.ExecutedRoutedEventArgs e) =>
      App.CreateBase(this);

    private void Quit(object sender, System.Windows.Input.ExecutedRoutedEventArgs e) =>
      Close();

    private void ChangePassword(object sender, RoutedEventArgs e)
    {
      PasswordWindow passwordWindow = new PasswordWindow(true, passwordBase.FileName) { Owner = this };
      if (passwordWindow.ShowDialog() == true)
        passwordBase.ChangePassword(passwordWindow.PasswordBox.SecurePassword);
    }
  }
}
