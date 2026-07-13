using Microsoft.Win32;

namespace Necroperator.Services
{
    internal class UbisoftService : IUbisoftService
    {
        public bool TryGetInstallationPath(out string installationPath)
        {
            installationPath = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Ubisoft\Launcher", false)?.GetValue("InstallDir")?.ToString() ?? string.Empty;
            return !string.IsNullOrEmpty(installationPath);
        }
    }
}
