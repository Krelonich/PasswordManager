using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace PasswordManager
{
  public class PasswordBase : IDisposable, INotifyPropertyChanged
  {
    private const int SALT_LENGTH = 16, ENC_KEY_LENGTH = 32;

    private string _filePath;
    public string FilePath
    {
      get => _filePath;
      set
      {
        _filePath = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FilePath"));
      }
    }
    public string FileName => Path.GetFileName(_filePath);
    public PasswordBaseData Data { get; set; }
    private SecureString password;

    public event PropertyChangedEventHandler PropertyChanged;

    public PasswordBase(string _path, SecureString _password, PasswordBaseData _data = null)
    {
      FilePath = _path;
      password = _password;
      Data = _data ?? new PasswordBaseData();
    }

    public static async Task<PasswordBase> Load(string path, SecureString passwordString)
    {
      PasswordBaseData data;
      using (FileStream fsDecrypt = File.OpenRead(path))
      {
        byte[] salt = new byte[SALT_LENGTH];
        if (await fsDecrypt.ReadAsync(salt, 0, SALT_LENGTH) != SALT_LENGTH)
          throw new FileFormatException();

        using (Aes aes = Aes.Create())
        {
          aes.IV = salt;
          aes.Key = GetEncKey(passwordString, salt);
          aes.Mode = CipherMode.CFB;

          using (ICryptoTransform decryptor = aes.CreateDecryptor())
          using (CryptoStream csDecrypt = new CryptoStream(fsDecrypt, decryptor, CryptoStreamMode.Read))
            data = await JsonSerializer.DeserializeAsync<PasswordBaseData>(csDecrypt);
        }
      }
      return new PasswordBase(path, passwordString, data);
    }

    public async Task<bool> Save()
    {
      try
      {
        byte[] salt = new byte[SALT_LENGTH];
        using (RNGCryptoServiceProvider rngCSP = new RNGCryptoServiceProvider())
          rngCSP.GetBytes(salt);

        using (Aes aes = Aes.Create())
        {
          aes.IV = salt;
          aes.Key = GetEncKey(password, salt);
          aes.Mode = CipherMode.CFB;

          using (FileStream fsEncrypt = File.Create(FilePath))
          {
            await fsEncrypt.WriteAsync(salt, 0, SALT_LENGTH);
            using (ICryptoTransform encryptor = aes.CreateEncryptor())
            using (CryptoStream csEncrypt = new CryptoStream(fsEncrypt, encryptor, CryptoStreamMode.Write))
              await JsonSerializer.SerializeAsync(csEncrypt, Data);
          }
        }
        return true;
      }
      catch
      {
        MessageBox.Show($"Не удалось сохранить базу паролей в расположении {FilePath} из-за неизвестной ошибки", "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
        return false;
      }
    }

    public async void ChangePassword(SecureString newPassword)
    {
      password = newPassword;
      await Save();
    }

    private static byte[] GetEncKey(SecureString passwordString, byte[] salt)
    {
      int pwdLength = passwordString.Length * 2;
      byte[] password = new byte[pwdLength];
      GCHandle handle = GCHandle.Alloc(password, GCHandleType.Pinned);
      IntPtr pwdPtr = Marshal.SecureStringToBSTR(passwordString);
      try
      {
        Marshal.Copy(pwdPtr, password, 0, pwdLength);
        using (Rfc2898DeriveBytes pbkdf2 =
          new Rfc2898DeriveBytes(password, salt, 120000, HashAlgorithmName.SHA512))
        {
          return pbkdf2.GetBytes(ENC_KEY_LENGTH);
        }
      }
      finally
      {
        Marshal.ZeroFreeBSTR(pwdPtr);
        Array.Clear(password, 0, pwdLength);
        handle.Free();
      }
    }

    public void Dispose()
    {
      if (Data.Passwords != null)
      {
        foreach (PasswordData data in Data.Passwords)
        {
          data.Name.Clear();
          data.Login.Clear();
          data.Password.Clear();
          data.Comment.Clear();
        }
      }

      if (Data.Notes != null)
      {
        foreach (NoteData data in Data.Notes)
        {
          data.Name.Clear();
          data.Content.Clear();
        }
      }

      password?.Dispose();
    }
  }
}
