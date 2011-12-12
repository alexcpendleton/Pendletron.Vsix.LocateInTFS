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
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(GuidList.guidVisualStudio_LocateInSourceControl_VSIPCmdSet, (int)PkgCmdIDList.cmdidLocateInSourceControl);
                MenuCommand menuItem = new MenuCommand(MenuItemCallback, menuCommandID );
                mcs.AddCommand( menuItem );
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

		/// <summary>
		/// This function is the callback used to execute a command when the a menu item is clicked.
		/// See the Initialize method to see how the menu item is associated to this function using
		/// the OleMenuCommandService service and the MenuCommand class.
		/// </summary>
		private void MenuItemCallback(object sender, EventArgs e)
		{

			var dte = (DTE2)this.GetService(typeof(DTE));
			var y = GetSelectedNodes();

			List<string> nodefiles = new List<string>();
			foreach (VSITEMSELECTION vsItemSel in y)
			{
				IVsSccProject2 pscp2 = vsItemSel.pHier as IVsSccProject2;
				nodefiles.AddRange(GetNodeFiles(pscp2, vsItemSel.itemid));

			}
			TeamFoundationServerExt ext = dte.GetObject("Microsoft.VisualStudio.TeamFoundation.TeamFoundationServerExt") as TeamFoundationServerExt;
			string uri = ext.ActiveProjectContext.ProjectUri;
			HatPackage hat = new HatPackage();
			VersionControlServer o = hat.GetVersionControlServer();

			Workspace workspace = o.GetWorkspace(nodefiles[0]);
			string serverItem = "";
			try {
				serverItem = workspace.TryGetServerItemForLocalItem(nodefiles[0]);
			} catch (Exception) {
			
			}

			if (!String.IsNullOrEmpty(serverItem))
			{
				Assembly tfsVC = Assembly.Load("Microsoft.VisualStudio.TeamFoundation.VersionControl");
				//Type t = tfsVC.GetType("Microsoft.VisualStudio.TeamFoundation.VersionControl.HatPackage");
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

		///// <summary>
		///// Gets the list of directly selected VSITEMSELECTION objects
		///// </summary>
		///// <returns>A list of VSITEMSELECTION objects</returns>
		private IList<VSITEMSELECTION> GetSelectedNodes()
		{
			// Retrieve shell interface in order to get current selection
			IVsMonitorSelection monitorSelection = this.GetService(typeof(IVsMonitorSelection)) as IVsMonitorSelection;
			Debug.Assert(monitorSelection != null, "Could not get the IVsMonitorSelection object from the services exposed by this project");
			if (monitorSelection == null)
			{
				throw new InvalidOperationException();
			}

			List<VSITEMSELECTION> selectedNodes = new List<VSITEMSELECTION>();
			IntPtr hierarchyPtr = IntPtr.Zero;
			IntPtr selectionContainer = IntPtr.Zero;
			try
			{
				// Get the current project hierarchy, project item, and selection container for the current selection
				// If the selection spans multiple hierachies, hierarchyPtr is Zero
				uint itemid;
				IVsMultiItemSelect multiItemSelect = null;
				ErrorHandler.ThrowOnFailure(monitorSelection.GetCurrentSelection(out hierarchyPtr, out itemid, out multiItemSelect, out selectionContainer));

				if (itemid != VSConstants.VSITEMID_SELECTION)
				{
					// We only care if there are nodes selected in the tree
					if (itemid != VSConstants.VSITEMID_NIL)
					{
						if (hierarchyPtr == IntPtr.Zero)
						{
							// Solution is selected
							VSITEMSELECTION vsItemSelection;
							vsItemSelection.pHier = null;
							vsItemSelection.itemid = itemid;
							selectedNodes.Add(vsItemSelection);
						}
						else
						{
							IVsHierarchy hierarchy = (IVsHierarchy)Marshal.GetObjectForIUnknown(hierarchyPtr);
							// Single item selection
							VSITEMSELECTION vsItemSelection;
							vsItemSelection.pHier = hierarchy;
							vsItemSelection.itemid = itemid;
							selectedNodes.Add(vsItemSelection);
						}
					}
				}
				else
				{
					if (multiItemSelect != null)
					{
						// This is a multiple item selection.

						//Get number of items selected and also determine if the items are located in more than one hierarchy
						uint numberOfSelectedItems;
						int isSingleHierarchyInt;
						ErrorHandler.ThrowOnFailure(multiItemSelect.GetSelectionInfo(out numberOfSelectedItems, out isSingleHierarchyInt));
						bool isSingleHierarchy = (isSingleHierarchyInt != 0);

						// Now loop all selected items and add them to the list 
						Debug.Assert(numberOfSelectedItems > 0, "Bad number of selected itemd");
						if (numberOfSelectedItems > 0)
						{
							VSITEMSELECTION[] vsItemSelections = new VSITEMSELECTION[numberOfSelectedItems];
							ErrorHandler.ThrowOnFailure(multiItemSelect.GetSelectedItems(0, numberOfSelectedItems, vsItemSelections));
							foreach (VSITEMSELECTION vsItemSelection in vsItemSelections)
							{
								selectedNodes.Add(vsItemSelection);
							}
						}
					}
				}
			}
			finally
			{
				if (hierarchyPtr != IntPtr.Zero)
				{
					Marshal.Release(hierarchyPtr);
				}
				if (selectionContainer != IntPtr.Zero)
				{
					Marshal.Release(selectionContainer);
				}
			}

			return selectedNodes;
		}

		/// <summary>
		/// Returns a list of source controllable files associated with the specified node
		/// </summary>
		private IList<string> GetNodeFiles(IVsSccProject2 pscp2, uint itemid)
		{
			// NOTE: the function returns only a list of files, containing both regular files and special files
			// If you want to hide the special files (similar with solution explorer), you may need to return 
			// the special files in a hastable (key=master_file, values=special_file_list)

			// Initialize output parameters
			IList<string> sccFiles = new List<string>();
			if (pscp2 != null)
			{
				CALPOLESTR[] pathStr = new CALPOLESTR[1];
				CADWORD[] flags = new CADWORD[1];

				if (pscp2.GetSccFiles(itemid, pathStr, flags) == VSConstants.S_OK)
				{
					for (int elemIndex = 0; elemIndex < pathStr[0].cElems; elemIndex++)
					{
						IntPtr pathIntPtr = Marshal.ReadIntPtr(pathStr[0].pElems, elemIndex * IntPtr.Size);
						String path = Marshal.PtrToStringAuto(pathIntPtr);

						sccFiles.Add(path);

						// See if there are special files
						if (flags.Length > 0 && flags[0].cElems > 0)
						{
							int flag = Marshal.ReadInt32(flags[0].pElems, elemIndex * IntPtr.Size);

							if (flag != 0)
							{
								// We have special files
								CALPOLESTR[] specialFiles = new CALPOLESTR[1];
								CADWORD[] specialFlags = new CADWORD[1];

								if (pscp2.GetSccSpecialFiles(itemid, path, specialFiles, specialFlags) == VSConstants.S_OK)
								{
									for (int i = 0; i < specialFiles[0].cElems; i++)
									{
										IntPtr specialPathIntPtr = Marshal.ReadIntPtr(specialFiles[0].pElems, i * IntPtr.Size);
										String specialPath = Marshal.PtrToStringAuto(specialPathIntPtr);

										sccFiles.Add(specialPath);
										Marshal.FreeCoTaskMem(specialPathIntPtr);
									}

									if (specialFiles[0].cElems > 0)
									{
										Marshal.FreeCoTaskMem(specialFiles[0].pElems);
									}
								}
							}
						}

						Marshal.FreeCoTaskMem(pathIntPtr);
					}
					if (pathStr[0].cElems > 0)
					{
						Marshal.FreeCoTaskMem(pathStr[0].pElems);
					}
				}
			}

			return sccFiles;
		}

    }
}
