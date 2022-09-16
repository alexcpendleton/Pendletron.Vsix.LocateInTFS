using System;

namespace Pendletron.Vsix.Core.Commands
{
	public class ActiveWindowLocateCommand : LocateCommand
	{
		public ActiveWindowLocateCommand(ITfsLocater locater, ILocateInTfsVsPackage pkg) : base(locater, pkg) { }
		public override int CommandID
		{
			get { return (int)PackageIDs.cmdidLocateInTFS_CodeWindow; }
		}

		public override string GetSelectedLocalPath()
		{
			return Locater.GetSelectedPathFromActiveDocument();
		}

	    public override bool BeforeQueryStatus(object sender, EventArgs e)
	    {
	        return base.BeforeQueryStatus(sender, e);
	    }
	}
}