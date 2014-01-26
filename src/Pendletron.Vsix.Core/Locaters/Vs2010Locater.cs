using System;
using System.ComponentModel.Design;
using System.Reflection;
using EnvDTE;
using EnvDTE80;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Pendletron.Vsix.Core.Wrappers;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Client;
using Pendletron.Vsix.Core;
using Pendletron.Vsix.Core.Commands;

namespace Pendletron.Vsix.LocateInTFS
{
	public class Vs2010Locater : TfsLocaterBase
	{
	    public Vs2010Locater(ILocateInTfsVsPackage pkg) : base(pkg) { }

		private Assembly _tfsVersionControlAssembly = null;
		public Assembly TfsVersionControlAssembly
		{
			get { return (_tfsVersionControlAssembly ?? (_tfsVersionControlAssembly = LoadTfsVersionControlAssembly())); }
		}
		protected Assembly LoadTfsVersionControlAssembly()
		{
			return Assembly.Load("Microsoft.VisualStudio.TeamFoundation.VersionControl");
		}

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
                    }
                }
            }
	    }
    }
}