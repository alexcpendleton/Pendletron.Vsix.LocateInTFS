namespace Pendletron.Vsix.LocateInTFS.Commands
{
	public class SolutionExplorerLocateCommand : LocateCommand
	{
		public SolutionExplorerLocateCommand(LocationService service) : base(service) { }

		protected override int CommandID
		{
			get { return (int) PkgCmdIDList.cmdidLocateInTFS; }
		}
		public override string GetSelectedLocalPath()
		{
			return Service.GetSelectedPathFromSolutionExplorer();
		}
	}
}