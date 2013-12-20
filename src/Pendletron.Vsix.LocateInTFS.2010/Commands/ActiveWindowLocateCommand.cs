using Pendletron.Vsix.Core;
namespace Pendletron.Vsix.LocateInTFS.Commands
{
	public class ActiveWindowLocateCommand : LocateCommand
	{
		public ActiveWindowLocateCommand(ITfsLocater locater, ILocateInTfsVsPackage pkg) : base(locater, pkg) { }
		protected override int CommandID
		{
			get { return (int)PackageIDs.cmdidLocateInTFS_CodeWindow; }
		}

		public override string GetSelectedLocalPath()
		{
			return Locater.GetSelectedPathFromActiveDocument();
		}
	}
}