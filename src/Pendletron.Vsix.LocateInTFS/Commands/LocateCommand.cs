using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
namespace Pendletron.Vsix.LocateInTFS.Commands
{
	public abstract class LocateCommand
	{
		public LocateCommand(LocationService service)
		{
			Service = service;
		}

		public LocationService Service { get; set; }
		protected abstract int CommandID { get; }

		protected virtual void AddToMenuCommandService(OleMenuCommand cmd)
		{
			OleMenuCommandService mcs = Service.Package.GetService<OleMenuCommandService>(typeof(IMenuCommandService));
			mcs.AddCommand(cmd);
		}

		public virtual void RegisterCommand()
		{
			// Create the command for the menu item.
			CommandID menuCommandID = new CommandID(GuidList.guidVisualStudio_LocateInTFS_VSIPCmdSet, CommandID);
			OleMenuCommand menuItem = new OleMenuCommand(Execute, menuCommandID);
			menuItem.BeforeQueryStatus += new EventHandler(BeforeQueryStatus);
			AddToMenuCommandService(menuItem);
		}

		public abstract string GetSelectedLocalPath();

		public virtual void BeforeQueryStatus(object sender, EventArgs e)
		{
			var menuCommand = sender as MenuCommand;
			string selectedPath = GetSelectedLocalPath();
			bool isVersionControlled = Service.IsVersionControlled(selectedPath);
			menuCommand.Visible = isVersionControlled;
		}

		public virtual void Execute(object sender, EventArgs e)
		{
			string selectedPath = GetSelectedLocalPath();
			Service.Locate(selectedPath);
		}
	}
}