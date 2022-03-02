using System.Collections.ObjectModel;

namespace PasswordManager
{
  public class PasswordBaseData
  {
    public ObservableCollection<PasswordData> Passwords { get; set; } = new ObservableCollection<PasswordData>();
    public ObservableCollection<NoteData> Notes { get; set; } = new ObservableCollection<NoteData>();
  }
  public class RecordData
  {
    public string Name { get; set; }
  }
  public class PasswordData : RecordData
  {
    public string Login { get; set; }
    public string Password { get; set; }
    public string Comment { get; set; }
  }
  public class NoteData : RecordData
  {
    public string Content { get; set; }
  }
}
