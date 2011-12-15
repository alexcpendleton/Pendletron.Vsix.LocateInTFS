using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using EnvDTE;
using EnvDTE80;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.VisualStudio.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.VisualStudio.TeamFoundation;
using System.Reflection;
using AlexPendleton.VisualStudio_LocateInSourceControl_VSIP.Wrappers;

namespace AlexPendleton.VisualStudio_LocateInSourceControl_VSIP
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
	[Guid(GuidList.guidVisualStudio_LocateInSourceControl_VSIPPkgString)]
	public sealed class VisualStudio_LocateInSourceControl_VSIPPackage : Package
	{



		DTE2 _dte = null;
		/// <summary>
		/// Default constructor of the package.
		/// Inside this method you can place any initialization code that does not require 
		/// any Visual Studio service because at this point the package object is created but 
		/// not sited yet inside Visual Studio environment. The place to do all the other 
		/// initialization is the Initialize method.
		/// </summary>
		public VisualStudio_LocateInSourceControl_VSIPPackage()
		{
			Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
		}



		/////////////////////////////////////////////////////////////////////////////
		// Overriden Package Implementation
		#region Package Members

		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initilaization code that rely on services provided by VisualStudio.
		/// </summary>
		protected override void Initialize()
		{
			Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
			base.Initialize();

			// Add our command handlers for menu (commands must exist in the .vsct file)
			OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			if (null != mcs)
			{
				// Create the command for the menu item.
				CommandID menuCommandID = new CommandID(GuidList.guidVisualStudio_LocateInSourceControl_VSIPCmdSet, (int)PkgCmdIDList.cmdidLocateInSourceControl);
				MenuCommand menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
				mcs.AddCommand(menuItem);
			}
		}
		#endregion

		private UIHierarchyItem GetSelectedUIHierarchy(UIHierarchy solutionExplorer)
		{
			object[] objArray = solutionExplorer.SelectedItems as object[];
			if (objArray != null && objArray.Length == 1)
				return objArray[0] as UIHierarchyItem;
			else
				return (UIHierarchyItem)null;
		}

		public T GetService<T>() where T : class
		{
			return GetService(typeof(T)) as T;
		}

		public string GetLocalPath(SelectedItem item)
		{
			string result = "";

			if (item.ProjectItem == null)
			{
				if (item.Project == null)
				{
					// If there's no ProjectItem and no Project then it's (probably?) the solution
					result = _dte.Solution.FullName;
				}
				else
				{
					// If there's no ProjectItem but there is a Project then the Project node is selected
					result = item.Project.FullName;
				}
			}
			else
			{
				//Just selected a file
				result = item.ProjectItem.get_FileNames(0);
			}
			return result;
		}

		public string GetSelectedPathFromSolutionExplorer()
		{
			string localPath = "";
			if (_dte.SelectedItems != null && _dte.SelectedItems.Count > 0)
			{
				foreach (SelectedItem item in _dte.SelectedItems)
				{
					localPath = GetLocalPath(item);
					if (!String.IsNullOrWhiteSpace(localPath))
					{
						break;
					}
				}
			}
			return localPath;
		}

		public void Locate(string localPath)
		{

			// Get the first selected item? _dte.
			if (String.IsNullOrEmpty(localPath)) return; // Throw an exception, log to output?

			HatPackage hat = new HatPackage();
			VersionControlServer vcServer = hat.GetVersionControlServer();
			
			string localFilePath = localPath;

			Workspace workspace = vcServer.GetWorkspace(localFilePath);
			string serverItem = "";
			try
			{
				serverItem = workspace.TryGetServerItemForLocalItem(localFilePath);
			}
			catch (Exception)
			{

			}
			if (!String.IsNullOrEmpty(serverItem))
			{
				Assembly tfsVC = Assembly.Load("Microsoft.VisualStudio.TeamFoundation.VersionControl");
				//Type t = tfsVC.GetType("Microsoft.VisualStudio.TeamFoundation.VersionControl.HatPackage");
				// if the tool window hasn't been opened yet "explorer" will be null, so we make sure it has opened at least once via ExecuteCommand
				_dte.ExecuteCommand("View.TfsSourceControlExplorer");
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

		/// <summary>
		/// This function is the callback used to execute a command when the a menu item is clicked.
		/// See the Initialize method to see how the menu item is associated to this function using
		/// the OleMenuCommandService service and the MenuCommand class.
		/// </summary>
		private void MenuItemCallback(object sender, EventArgs e)
		{
			_dte = (DTE2)this.GetService(typeof(DTE));
			//Locate();
			string selected = GetSelectedPathFromSolutionExplorer();
			Locate(selected);
		}
	}
}
