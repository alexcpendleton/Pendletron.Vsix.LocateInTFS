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
				var ws = GetWorkspaceForPath(selectedPath);
				if (ws != null)
				{
					isVersionControlled = true;
				}
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
				result = item.ProjectItem.get_FileNames(0);
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

		protected Workspace GetWorkspaceForPath(string localFilePath)
		{
			Workspace workspace = GetWorkspaceForNonSolutionPath(localFilePath);
			if (workspace == null)
			{
				// Not a file from this solution, get it another way
				workspace = GetWorkspaceForNonSolutionPath(localFilePath);
			}
			return workspace;
		}

		protected Workspace GetWorkspaceForSolutionPath(string localFilePath)
		{
			HatPackage hat = new HatPackage();
			VersionControlServer vcServer = hat.GetVersionControlServer();
			Workspace workspace = vcServer.GetWorkspace(localFilePath);
			return workspace;
		}

		protected Workspace GetWorkspaceForNonSolutionPath(string localFilePath)
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
					}
				}
			}
			return result;
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
			try
			{
				var workspace = GetWorkspaceForPath(localFilePath);
				serverItem = workspace.TryGetServerItemForLocalItem(localFilePath);
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