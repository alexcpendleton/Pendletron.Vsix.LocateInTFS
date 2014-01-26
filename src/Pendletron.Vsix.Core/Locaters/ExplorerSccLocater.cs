using System;
using System.CodeDom;
using System.ComponentModel.Design;
using System.Reflection;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Pendletron.Vsix.Core.Commands;
using Pendletron.Vsix.Core.Wrappers;
using System.Collections.Generic;
using Pendletron.Vsix.Core;

namespace Pendletron.Vsix.LocateInTFS
{
	public class ExplorerSccLocater : TfsLocaterBase
	{
        public ExplorerSccLocater(ILocateInTfsVsPackage package):base(package)
	    {
	    }

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

	    private dynamic _explorerScc = null;
	    public dynamic ExplorerScc
	    {
	        get
	        {
	            if (_explorerScc == null)
	            {
                    dynamic explorerSccDiag = new AccessPrivateWrapper(HatterasPackage._wrapped.ExplorerSccDiagProvider);
                    if (explorerSccDiag.m_explorerScc == null)
                    {
                        // if the tool window hasn't been opened yet "explorer" will be null, so we make sure it has opened at least once via ExecuteCommand
                        DTEInstance.ExecuteCommand("View.TfsSourceControlExplorer");
                    }
                    _explorerScc = new AccessPrivateWrapper(explorerSccDiag.m_explorerScc);
	            }
	            return _explorerScc;
	        }
	    }

	    public override void ShowInExplorer(string serverItem)
        {
            if (!String.IsNullOrEmpty(serverItem))
            {
                ExplorerScc.NavigateAsync(serverItem);
                ExplorerScc.UpdateToolbarState();
            }
	    }
    }
}