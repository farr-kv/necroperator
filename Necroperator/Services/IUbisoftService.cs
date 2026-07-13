namespace Necroperator.Services
{
    public interface IUbisoftService
    {
        bool TryGetInstallationPath(out string installationPath);
    }
}