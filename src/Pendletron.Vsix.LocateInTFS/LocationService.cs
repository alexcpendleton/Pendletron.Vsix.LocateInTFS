using System;
using System.ComponentModel.Design;
using System.Reflection;
using EnvDTE;
using EnvDTE80;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.VisualStudio.Shell;
using Pendletron.Vsix.Core.Wrappers;
using Pendletron.Vsix.LocateInTFS.Commands;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Client;

namespace Pendletron.Vsix.LocateInTFS
{
	public class LocationService
	{
		public LocationService(VisualStudio_LocateInTFS_VSIPPackage pkg)
		{
			Package = pkg;
		}

		public VisualStudio_LocateInTFS_VSIPPackage Package { get; set; }

		public void Initialize()
		{
			var solutionExplorerCommand = new SolutionExplorerLocateCommand(this);
			var activeWindowCommand = new ActiveWindowLocateCommand(this);
			solutionExplorerCommand.RegisterCommand();
			activeWindowCommand.RegisterCommand();
		}

		public bool IsVersionControlled(string selectedPath)
		{
			bool isVersionControlled = false;
			try
			{
				GetWorkspaceForPath(selectedPath, ws =>
				{
					if (ws != null) {
						isVersionControlled = true;
					}
				});
			}
			catch (Exception)
			{
				isVersionControlled = false;
			}
			return isVersionControlled;
		}

		private DTE2 _dteInstance = null;
		public DTE2 DTEInstance
		{
			get
			{
				if (_dteInstance == null)
				{
					_dteInstance = GetDTEService();
				}
				return _dteInstance;
			}
			set { _dteInstance = value; }
		}

		private DTE2 GetDTEService()
		{
			return Package.GetService<DTE2>(typeof(DTE));
		}

		public UIHierarchyItem GetSelectedUIHierarchy(UIHierarchy solutionExplorer)
		{
			object[] objArray = solutionExplorer.SelectedItems as object[];
			if (objArray != null && objArray.Length == 1)
				return objArray[0] as UIHierarchyItem;
			else
				return (UIHierarchyItem)null;
		}

		public string GetLocalPath(SelectedItem item)
		{
			string result = "";

			if (item.ProjectItem == null)
			{
				if (item.Project == null)
				{
					// If there's no ProjectItem and no Project then it's (probably?) the solution
					result = DTEInstance.Solution.FullName;
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
				// Regular items in a project seem to be zero-based
				// Items inside of solution folders seem to be one-based...
				try
				{
					result = item.ProjectItem.get_FileNames(0);
				}
				catch(ArgumentException)
				{
					result = item.ProjectItem.get_FileNames(1);
				}
			}
			return result;
		}

		public string GetSelectedPathFromActiveDocument()
		{
			if (DTEInstance.ActiveDocument != null)
			{
				return DTEInstance.ActiveDocument.Path;
			}
			return "";
		}


		public string GetSelectedPathFromSolutionExplorer()
		{
			string localPath = "";
			if (DTEInstance.SelectedItems != null && DTEInstance.SelectedItems.Count > 0)
			{
				foreach (SelectedItem item in DTEInstance.SelectedItems)
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

		protected void GetWorkspaceForPath(string localFilePath, Action<Workspace> ifFound)
		{
			// Needed to change this to use a callback because if it's a non-solution 
			// path then the TFS collection gets disposed
			Workspace workspace = null;
			try
			{
				// TODO: Should probably check if solution is connected to TFS instead of wrapping
				// this in a try/catch
				workspace = GetWorkspaceForSolutionPath(localFilePath);
			} catch(Exception) { }

			if (workspace == null)
			{
				// Not a file from  this solution, get it another way
				GetWorkspaceForNonSolutionPath(localFilePath, ifFound);
			}
			else
			{
				ifFound(workspace);
			}
		}

		protected Workspace GetWorkspaceForSolutionPath(string localFilePath)
		{
			HatPackage hat = new HatPackage();
			VersionControlServer vcServer = hat.GetVersionControlServer();
			Workspace workspace = vcServer.GetWorkspace(localFilePath);
			return workspace;
		}

		protected void GetWorkspaceForNonSolutionPath(string localFilePath, Action<Workspace> ifFound)
		{
			Workstation station = Workstation.Current;
			Workspace result = null;
			if (station != null)
			{
				var wsInfo = station.GetLocalWorkspaceInfo(localFilePath);
				using(var collection = new TfsTeamProjectCollection(wsInfo.ServerUri))
				{
					if (collection != null)
					{
						result = wsInfo.GetWorkspace(collection);
						if (result != null)
						{
							ifFound(result);
						}
					}
				}
			}
		}

		private Assembly _tfsVersionControlAssembly = null;
		public Assembly TfsVersionControlAssembly
		{
			get { return (_tfsVersionControlAssembly ?? (_tfsVersionControlAssembly = LoadTfsVersionControlAssembly())); }
		}
		protected Assembly LoadTfsVersionControlAssembly()
		{
			return Assembly.Load("Microsoft.VisualStudio.TeamFoundation.VersionControl");
		}

		public void Locate(string localPath)
		{
			// Get the first selected item? _dte.
			if (String.IsNullOrEmpty(localPath)) return; // Throw an exception, log to output?

			HatPackage hat = new HatPackage();

			string localFilePath = localPath;
			string serverItem = "";
			try {
				GetWorkspaceForPath(localFilePath, workspace => {
					serverItem = workspace.TryGetServerItemForLocalItem(localFilePath);
				});
			}
			catch (Exception) { }

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