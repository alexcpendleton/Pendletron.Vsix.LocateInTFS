namespace Pendletron.Vsix.Core
{
    public interface ILocaterWorkspace
    {
        string TryGetServerItemForLocalItem(string localPath);
    }
}