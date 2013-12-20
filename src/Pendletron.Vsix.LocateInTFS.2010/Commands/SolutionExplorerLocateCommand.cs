using Pendletron.Vsix.Core;
namespace Pendletron.Vsix.LocateInTFS.Commands
{
	public class SolutionExplorerLocateCommand : LocateCommand
	{
		public SolutionExplorerLocateCommand(ITfsLocater locater, ILocateInTfsVsPackage pkg) : base(locater, pkg) { }

		protected override int CommandID
		{
			get { return (int)PackageIDs.cmdidLocateInTFS_SolutionExplorer; }
		}

		public override string GetSelectedLocalPath()
		{
			return Locater.GetSelectedPathFromSolutionExplorer();
		}
	}
}