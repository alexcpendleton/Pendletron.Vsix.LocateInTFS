namespace Pendletron.Vsix.Core.Commands
{
	public class SolutionExplorerLocateCommand : LocateCommand
	{
		public SolutionExplorerLocateCommand(ITfsLocater locater, ILocateInTfsVsPackage pkg) : base(locater, pkg) { }

		public override int CommandID
		{
			get { return (int)PackageIDs.cmdidLocateInTFS_SolutionExplorer; }
		}

		public override string GetSelectedLocalPath()
		{
			return Locater.GetSelectedPathFromSolutionExplorer();
		}
	}
}