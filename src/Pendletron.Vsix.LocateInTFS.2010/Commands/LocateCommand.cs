using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Pendletron.Vsix.Core;
namespace Pendletron.Vsix.LocateInTFS.Commands
{
	public abstract class LocateCommand
	{
		public LocateCommand(ITfsLocater locater, ILocateInTfsVsPackage pkg)
		{
			Locater = locater;
			Package = pkg;
		}

		public ITfsLocater Locater { get; set; }
		public ILocateInTfsVsPackage Package { get; set; }
		protected abstract int CommandID { get; }

		public IVsPackageIdentifiers PackageIDs { get { return Package.PackageIDs; } }

		protected virtual void AddToMenuCommandService(OleMenuCommand cmd)
		{
			IMenuCommandService mcs = (IMenuCommandService)Package.GetServiceAsDynamic(typeof(IMenuCommandService));
			var found = mcs.FindCommand(cmd.CommandID);
			if (found == null)
			{
				mcs.AddCommand(cmd);
			}
		}

        public virtual Microsoft.VisualStudio.Shell.OleMenuCommand RegisterCommand()
        {
            // Create the command for the menu item.
            CommandID menuCommandID = new CommandID(PackageIDs.guidVisualStudio_LocateInTFS_VSIPCmdSet, CommandID);
            var menuItem = new Microsoft.VisualStudio.Shell.OleMenuCommand(Execute, menuCommandID);
            //menuItem.BeforeQueryStatus += new EventHandler(BeforeQueryStatus);
            AddToMenuCommandService(menuItem);
            return menuItem;
        }

		public abstract string GetSelectedLocalPath();

        public virtual bool BeforeQueryStatus(object sender, EventArgs e)
        {
            var menuCommand = sender as MenuCommand;
            string selectedPath = GetSelectedLocalPath();
            bool isVersionControlled = Locater.IsVersionControlled(selectedPath);
            return isVersionControlled;
        }

        public virtual void Execute(object sender, EventArgs e)
        {
            string selectedPath = GetSelectedLocalPath();
            Locater.Locate(selectedPath);
        }
	}
}