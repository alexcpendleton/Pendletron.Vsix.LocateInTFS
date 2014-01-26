using Microsoft.TeamFoundation.VersionControl.Client;

namespace Pendletron.Vsix.Core
{
    public class TfsWorkspaceLocatorDecorator : ILocaterWorkspace
    {
        public TfsWorkspaceLocatorDecorator(Workspace toDecorate)
        {
            Decorated = toDecorate;
        }
        internal Workspace Decorated { get; set; }
        public string TryGetServerItemForLocalItem(string localPath)
        {
            return Decorated.TryGetServerItemForLocalItem(localPath);
        }
    }
}