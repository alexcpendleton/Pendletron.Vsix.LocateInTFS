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
	public abstract class ExplorerSccLocater : TfsLocaterBase
	{
        public ExplorerSccLocater(ILocateInTfsVsPackage package):base(package)
	    {
	    }
        
	    protected dynamic _explorerScc = null;
	    virtual public dynamic ExplorerScc
	    {
	        get
	        {
	            if (_explorerScc == null)
	            {
                    dynamic explorerSccDiag = new AccessPrivateWrapper(HatterasPackage._wrapped.ExplorerSccDiagProvider);
                    if (explorerSccDiag.m_explorerScc == null)
                    {
                        // if the tool window hasn't been opened yet "explorer" will be null, so we make sure it has opened at least once via ExecuteCommand
                        //HatterasPackage._wrapped.OpenSCE();
                    }
                    _explorerScc = new AccessPrivateWrapper(explorerSccDiag.m_explorerScc);
	            }
	            return _explorerScc;
	        }
	    }
    }
}