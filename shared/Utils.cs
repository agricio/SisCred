using System;
using System.Diagnostics;
using System.IO;

namespace CrudApp.Shared
{
    public static class Utils
    {
        public static void OpenFile(string path)
        {
            if (!File.Exists(path)) return;
            var psi = new ProcessStartInfo(path) { UseShellExecute = true };
            Process.Start(psi);
        }
    }
}
