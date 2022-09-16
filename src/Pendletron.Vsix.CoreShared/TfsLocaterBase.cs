using System;
using System.CodeDom;
using System.ComponentModel.Design;
using System.Reflection;
using System.Windows.Threading;
using EnvDTE;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using EnvDTE80;
using Pendletron.Vsix.Core.Commands;
using Pendletron.Vsix.Core.Wrappers;
using Pendletron.Vsix.LocateInTFS;

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
	        ScrollToDispatchLagTime = TimeSpan.FromSeconds(0.6);
		}

        public ILocateInTfsVsPackage Package { get; set; }
        public TimeSpan ScrollToDispatchLagTime { get; set; }

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
		        string serverPath = GetServerPathFromLocal(selectedPath);
		        isVersionControlled = !String.IsNullOrWhiteSpace(serverPath);
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

        public abstract string GetServerPathFromLocal(string localFilePath);

		virtual public void Locate(string localPath)
		{
			// Get the first selected item? _dte.
			if (String.IsNullOrEmpty(localPath)) return; // Throw an exception, log to output?
            
			string localFilePath = localPath;
			string serverItem = "";
			try
			{
			    serverItem = GetServerPathFromLocal(localFilePath);
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
        
        virtual public void DispatchScrollToSccExplorerSelection(TimeSpan interval, dynamic explorer)
        {
            dynamic listView = explorer.listViewExplorer;
            if (listView == null) return;

            Func<bool> condition = () => listView.SelectedIndices.Count > 0;
            Action todo = () =>
            {
                int selectedIndex = listView.SelectedIndices[0];
                listView.EnsureVisible(selectedIndex);
            };

            var poller = new DispatchedPoller(10, TimeSpan.FromSeconds(0.25), condition, todo);
            poller.Go();
        }
    }
}
