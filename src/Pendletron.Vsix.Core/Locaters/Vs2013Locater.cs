using System.Diagnostics;
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
        public static int debugThing = 1;
        public override void Locate(string localPath)
        {
            dynamic vcs = HatterasPackage.HatterasService.VersionControlServer;
            var workspace = vcs.GetWorkspace(localPath);
            string serverPath = workspace.TryGetServerItemForLocalItem(localPath);

            // If you call OpenSceToPath to the same directory, it will mess up and won't show any files
            // So we clear whatever was there before, first
            HatterasPackage._wrapped.OpenSceToPath("$/", workspace);
            HatterasPackage._wrapped.OpenSceToPath(serverPath, workspace);
        }

        public override void ShowInExplorer(string serverItem)
        {
            throw new System.NotImplementedException();
        }
    }
}