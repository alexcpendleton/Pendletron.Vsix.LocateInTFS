using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Pendletron.Vsix.Core;
using Pendletron.Vsix.Core.Locaters;

namespace Pendletron.Vsix.LocateInTFS
{
	/// <summary>
	/// This is the class that implements the package exposed by this assembly.
	///
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
	/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
	/// to do it: it derives from the Package class that provides the implementation of the 
	/// IVsPackage interface and uses the registration attributes defined in the framework to 
	/// register itself and its components with the shell.
	/// </summary>
	// This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
	// a package.
	[PackageRegistration(UseManagedResourcesOnly = true)]
	// This attribute is used to register the informations needed to show the this package
	// in the Help/About dialog of Visual Studio.
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
	// This attribute is needed to let the shell know that this package exposes some menus.
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(GuidList.guidVisualStudio_LocateInTFS_VSIPPkgString)]
    [ProvideAutoLoad("{f1536ef8-92ec-443c-9ed7-fdadf150da82}")]
	[ProvideAutoLoad("{8fe2df1d-e0da-4ebe-9d5c-415d40e487b5}")]
	public sealed class VisualStudio_LocateInTFS_VSIPPackage : Package, IOleCommandTarget, ILocateInTfsVsPackage
	{
		public static bool _initialized = false;
		/// <summary>
		/// Default constructor of the package.
		/// Inside this method you can place any initialization code that does not require 
		/// any Visual Studio service because at this point the package object is created but 
		/// not sited yet inside Visual Studio environment. The place to do all the other 
		/// initialization is the Initialize method.
		/// </summary>
		public VisualStudio_LocateInTFS_VSIPPackage()
		{
			Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
		}

        private ITfsLocater LocaterPackage { get; set; }

		/////////////////////////////////////////////////////////////////////////////
		// Overriden Package Implementation

		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initilaization code that rely on services provided by VisualStudio.
		/// </summary>
		protected override void Initialize()
		{
			_initialized = true;
			Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
			base.Initialize();

            LocaterPackage = DerivePackageByVisualStudioVersion();
            if (LocaterPackage != null)
			{
                LocaterPackage.Initialize();
			}
		}

        private ITfsLocater DerivePackageByVisualStudioVersion()
        {
            ITfsLocater results = null;
            int version = DetermineVisualStudioVersionNumber();
            switch (version)
            {
                case 10:
                    results = new Vs2010Locater(this);
                    break;
                case 11:
                    results = new Vs2012Locater(this);
                    break;
                case 12:
                    results = new Vs2013Locater(this);
                    break;
            }
            return results;
        }

	    public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
	    {
	        var p = new CommandQueryStatusParams();
	        p.CmdGroup = pguidCmdGroup;
	        p.Cmds = cCmds;
	        p.PrgCmds = prgCmds;
	        p.CmdText = pCmdText;

	        var result = LocaterPackage.CommandBeforeQueryStatus(p);
	        prgCmds[0].cmdf |= result.PrgCmdsValue;
	        return (int) result.ReturnValue;
	    }

	    public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	    {
            var p = new CommandExecParams();
            p.CmdGroup = pguidCmdGroup;
            p.CommandExecOpt = nCmdexecopt;
            p.CommandID = nCmdID;
            p.In = pvaIn;
            p.Out = pvaOut;

            return LocaterPackage.CommandExecute(p);
	    }

	    public int DetermineVisualStudioVersionNumber()
		{
			var d = GetDteAsDynamic();
			string version = d.Version;
	        double result = 0;
	        Double.TryParse(version, out result);
	        return Convert.ToInt32(Math.Floor(result));
		}

		public dynamic GetServiceAsDynamic(Type serviceInterfaceType)
		{
			return GetService(serviceInterfaceType);
		}

		public dynamic GetDteAsDynamic()
		{
			return GetService(typeof (EnvDTE.DTE));
		}

		private VsPackageIdentifiers packageIDs = null;
		public IVsPackageIdentifiers PackageIDs
		{
			get
			{
				if (packageIDs == null)
				{
					packageIDs = new VsPackageIdentifiers();
					packageIDs.guidSolutionExplorer = GuidList.guidSolutionExplorer;
					packageIDs.guidSolutionExplorerGuid_String = GuidList.guidSolutionExplorerGuid_String;
					packageIDs.guidVisualStudio_LocateInTFS_VSIPCmdSet = GuidList.guidVisualStudio_LocateInTFS_VSIPCmdSet;
					packageIDs.guidVisualStudio_LocateInTFS_VSIPCmdSetString = GuidList.guidVisualStudio_LocateInTFS_VSIPCmdSetString;
					packageIDs.guidVisualStudio_LocateInTFS_VSIPPkgString = GuidList.guidVisualStudio_LocateInTFS_VSIPPkgString;
				    packageIDs.cmdidLocateInTFS_CodeWindow = 0x0110;
				    packageIDs.cmdidLocateInTFS_SolutionExplorer = 0x0100;
				}
				return packageIDs;
			}
		}
    }
}
