using System;
using System.ComponentModel.Design;
using System.Reflection;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Pendletron.Vsix.Core.Wrappers;
using System.Collections.Generic;
using Pendletron.Vsix.Core;
using Pendletron.Vsix.Core.Commands;

namespace Pendletron.Vsix.LocateInTFS
{
	public class Vs2010Locater : TfsLocaterBase
	{
	    public Vs2010Locater(ILocateInTfsVsPackage pkg) : base(pkg) { }

	    public override void ShowInExplorer(string serverItem)
        {
            if (!String.IsNullOrEmpty(serverItem))
            {
                Assembly tfsVC = TfsVersionControlAssembly;

                // if the tool window hasn't been opened yet "explorer" will be null, so we make sure it has opened at least once via ExecuteCommand
                DTEInstance.ExecuteCommand("View.TfsSourceControlExplorer");
                Type explorer = tfsVC.GetType("Microsoft.VisualStudio.TeamFoundation.VersionControl.ToolWindowSccExplorer");

                var prop = explorer.GetProperty("Instance", BindingFlags.NonPublic | BindingFlags.Static);
                object toolWindowSccExplorerInstance = prop.GetValue(null, null);
                if (toolWindowSccExplorerInstance != null)
                {
                    var navMethod = toolWindowSccExplorerInstance.GetType().GetMethod("Navigate", BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic);
                    if (navMethod != null)
                    {
                        navMethod.Invoke(toolWindowSccExplorerInstance, new object[] { serverItem });
                        dynamic dynEx = new AccessPrivateWrapper(toolWindowSccExplorerInstance);
                        dynamic x2 = new AccessPrivateWrapper(dynEx.SccExplorer);
                        DispatchScrollToSccExplorerSelection(ScrollToDispatchLagTime, x2);
                    }
                }
            }
	    }

        public override string GetServerPathFromLocal(string localFilePath)
        {
            string serverPath = "";
            try
            {
                dynamic vcs = HatterasPackage.HatterasService.VersionControlServer;
                dynamic workspace = vcs.GetWorkspace(localFilePath);
                serverPath = workspace.TryGetServerItemForLocalItem(localFilePath);
            }
            catch
            {
                serverPath = "";
            }
            return serverPath;
        }
    }
}