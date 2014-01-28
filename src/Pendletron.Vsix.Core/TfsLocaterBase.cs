using System;
using System.CodeDom;
using System.ComponentModel.Design;
using System.Reflection;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.Client;
using Pendletron.Vsix.Core.Commands;
using Pendletron.Vsix.Core.Wrappers;

namespace Pendletron.Vsix.Core
{
    abstract public class TfsLocaterBase : ITfsLocater
    {

        private Assembly _tfsVersionControlAssembly = null;
        public Assembly TfsVersionControlAssembly
        {
            get { return (_tfsVersionControlAssembly ?? (_tfsVersionControlAssembly = LoadTfsVersionControlAssembly())); }
        }
        protected Assembly LoadTfsVersionControlAssembly()
        {
            return Assembly.Load("Microsoft.VisualStudio.TeamFoundation.VersionControl");
        }

	    private HatPackage _hat = null;
	    virtual public HatPackage HatterasPackage
	    {
	        get
	        {
	            if (_hat == null)
	            {
	                _hat = new HatPackage();
	            }
	            return _hat;
	        }
	    }

	    public TfsLocaterBase(ILocateInTfsVsPackage pkg)
		{
			Package = pkg;
		}

		public ILocateInTfsVsPackage Package { get; set; }

		public void Initialize()
		{
            RegisterCommands();
		}

        public Dictionary<int, CommandItem> CommandMap { get; set; }

	    public class CommandItem
	    {
	        public CommandItem()
	        {
	            
	        }

	        public CommandItem(LocateCommand baseCommand, MenuCommand menuCommand)
	        {
	            BaseCommand = baseCommand;
	            MenuCommand = menuCommand;
	        }

	        public LocateCommand BaseCommand { get; set; }
            public MenuCommand MenuCommand { get; set; }
	    }

	    public void RegisterCommands()
	    {
	        var commandService = Package.GetServiceAsDynamic(typeof (IMenuCommandService)) as IMenuCommandService;
	        if (commandService != null)
	        {
                CommandMap = new Dictionary<int, CommandItem>();
	            var activeWindow = new ActiveWindowLocateCommand(this, Package);
                MenuCommand cmd = activeWindow.RegisterCommand();
	            CommandMap[activeWindow.CommandID] = new CommandItem(activeWindow, cmd);

	            
	            var solutionExplorer = new SolutionExplorerLocateCommand(this, Package);
	            cmd = solutionExplorer.RegisterCommand();
                CommandMap[solutionExplorer.CommandID] = new CommandItem(solutionExplorer, cmd);
	        }
	    }


		virtual public bool IsVersionControlled(string selectedPath)
		{
		    bool isVersionControlled = false;
		    try
			{
				GetWorkspaceForPath(selectedPath, ws =>
				{
					if (ws != null) 
                    {
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

		virtual protected DTE2 GetDTEService()
		{
			return Package.GetServiceAsDynamic(typeof(DTE)) as DTE2;
		}

		virtual public UIHierarchyItem GetSelectedUIHierarchy(UIHierarchy solutionExplorer)
		{
			object[] objArray = solutionExplorer.SelectedItems as object[];
			if (objArray != null && objArray.Length == 1)
				return objArray[0] as UIHierarchyItem;
			else
				return (UIHierarchyItem)null;
		}

		virtual public string GetLocalPath(SelectedItem item)
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

		virtual public string GetSelectedPathFromActiveDocument()
		{
			if (DTEInstance.ActiveDocument != null)
			{
				return DTEInstance.ActiveDocument.FullName;
			}
			return "";
		}


		virtual public string GetSelectedPathFromSolutionExplorer()
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

		virtual protected void GetWorkspaceForPath(string localFilePath, Action<ILocaterWorkspace> ifFound)
		{
			// Needed to change this to use a callback because if it's a non-solution 
			// path then the TFS collection gets disposed
			ILocaterWorkspace workspace = null;
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

        virtual public ILocaterWorkspace GetWorkspaceForSolutionPath(string localFilePath)
		{
			dynamic vcServer = HatterasPackage.GetVersionControlServer();
			dynamic workspace = vcServer.GetWorkspace(localFilePath);
            return new TfsWorkspaceLocatorDecorator(workspace);
		}

        virtual public void GetWorkspaceForNonSolutionPath(string localFilePath, Action<ILocaterWorkspace> ifFound)
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
							ifFound(new TfsWorkspaceLocatorDecorator(result));
						}
					}
				}
			}
		}

		virtual public void Locate(string localPath)
		{
			// Get the first selected item? _dte.
			if (String.IsNullOrEmpty(localPath)) return; // Throw an exception, log to output?
            
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
                ShowInExplorer(serverItem);
		    }
		}

        abstract public void ShowInExplorer(string serverItem);

        virtual public int CommandExecute(ICommandExecParams e)
        {
            int commandID = Convert.ToInt32(e.CommandID);
            if (CommandMap.ContainsKey(commandID))
            {
                var mappedCommand = CommandMap[commandID];
                //var x = Microsoft.VisualStudio.Shell.Interop.Constants.OLECMDERR_E_UNKNOWNGROUP;
                mappedCommand.BaseCommand.Execute(mappedCommand.MenuCommand, new EventArgs());
            }
            return 0;
        }

        virtual public IQueryStatusResult CommandBeforeQueryStatus(ICommandQueryStatusParams e)
        {
            var result = new QueryStatusResult();
            var prgcmds = (OLECMD[])e.PrgCmds;
            uint wtfisthis = prgcmds[0].cmdf;
            int commandID = Convert.ToInt32(prgcmds[0].cmdID);

            if (CommandMap.ContainsKey(commandID))
            {
                var mappedCommand = CommandMap[commandID];
                bool isVisible = mappedCommand.BaseCommand.BeforeQueryStatus(mappedCommand.MenuCommand, new EventArgs());
                wtfisthis |= (uint) OLECMDF.OLECMDF_SUPPORTED | (uint) OLECMDF.OLECMDF_ENABLED;
                if (!isVisible)
                {
                    wtfisthis = (uint)OLECMDF.OLECMDF_DEFHIDEONCTXTMENU | (uint)OLECMDF.OLECMDF_SUPPORTED | (uint)OLECMDF.OLECMDF_INVISIBLE;
                }
                result.IsVersionControlled = isVisible;
            }
            else
            {
            }
            result.PrgCmdsValue = wtfisthis;
            result.ReturnValue = 0;
            return result;
        }
    }
}
