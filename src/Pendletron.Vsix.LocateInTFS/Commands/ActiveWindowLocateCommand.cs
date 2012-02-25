namespace Pendletron.Vsix.LocateInTFS.Commands
{
	public class ActiveWindowLocateCommand : LocateCommand
	{
		public ActiveWindowLocateCommand(LocationService service) :base(service) { }
		protected override int CommandID
		{
			get { return (int) PkgCmdIDList.cmdidLocateInTFS_CodeWindow; }
		}

		public override string GetSelectedLocalPath()
		{
			return Service.GetSelectedPathFromActiveDocument();
		}
	}
}