using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.TeamFoundation.VersionControl.Client;
using Pendletron.Vsix.Core.Wrappers;
using Pendletron.Vsix.LocateInTFS;
using System;

namespace Pendletron.Vsix.Core.Locaters
{
    public class Vs2013Locater : ExplorerSccLocater
    {
        public Vs2013Locater(ILocateInTfsVsPackage package) : base(package) { }
        public override bool IsVersionControlled(string selectedPath)
        {
            try
            {
                // Could be a git context, not currently supported
                bool isTfsContext = HatterasPackage.HatterasService.IsCurrentContextTFVC;
                if (!isTfsContext)
                {
                    return false;
                }
            }
            catch { }
            return base.IsVersionControlled(selectedPath);
        }

        public override void Locate(string localPath)
        {
            //var _vcAssembly = Assembly.Load("Microsoft.VisualStudio.TeamFoundation.VersionControl");
            //Type t = _vcAssembly.GetType("Microsoft.VisualStudio.TeamFoundation.VersionControl.HatPackage");

            dynamic vcs = HatterasPackage.HatterasService.VersionControlServer;
            //var wsInstance = Activator.CreateInstance(wsType);
            var workspace = vcs.GetWorkspace(localPath);

            //var prop = t.GetProperty("Instance", BindingFlags.NonPublic | BindingFlags.Static);
            //object instance = prop.GetValue(null, null);

            //var openSce = t.GetMethod("OpenSceToPath", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
            string serverPath = workspace.TryGetServerItemForLocalItem(localPath);
            //openSce.Invoke(instance, new object[] { serverPath, workspace });

            HatterasPackage._wrapped.OpenSceToPath(serverPath, workspace);
        }

        public override void ShowInExplorer(string serverItem)
        {
            throw new System.NotImplementedException();
        }
    }
}