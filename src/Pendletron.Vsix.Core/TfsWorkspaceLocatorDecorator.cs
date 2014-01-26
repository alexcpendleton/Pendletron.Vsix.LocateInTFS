using Microsoft.TeamFoundation.VersionControl.Client;

namespace Pendletron.Vsix.Core
{
    public class TfsWorkspaceLocatorDecorator : ILocaterWorkspace
    {
        public TfsWorkspaceLocatorDecorator(object toDecorate)
        {
            Decorated = toDecorate;
        }
        internal dynamic Decorated { get; set; }
        public string TryGetServerItemForLocalItem(string localPath)
        {
            return Decorated.TryGetServerItemForLocalItem(localPath);
        }
    }
}