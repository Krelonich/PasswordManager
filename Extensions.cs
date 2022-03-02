using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace PasswordManager
{
  public static class Extensions
  {
    public static HashSet<char> AddSymbols(this HashSet<char> symbols, CheckBox checkBox, string newSymbols)
    {
      if (checkBox.IsChecked == true)
        symbols.UnionWith(newSymbols);

      return symbols;
    }

    public static unsafe void Clear(this string str)
    {
      if (str == null) return;

      GCHandle handle = GCHandle.Alloc(str, GCHandleType.Pinned);
      if (string.IsInterned(str) == null)
      {
        fixed (char* c = str)
        {
          for (int i = 0; i < str.Length; i++)
            c[i] = '\0';
        }
      }
      handle.Free();
    }

    public static void Normalize(this Window window)
    {
      window.WindowState = WindowState.Normal;
      window.Activate();
    }
  }
}
