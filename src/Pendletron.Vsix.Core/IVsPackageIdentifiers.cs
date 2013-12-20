using System;

namespace Pendletron.Vsix.Core
{
	public interface IVsPackageIdentifiers
	{
		string guidVisualStudio_LocateInTFS_VSIPPkgString { get; }
		string guidVisualStudio_LocateInTFS_VSIPCmdSetString { get; }
		Guid guidVisualStudio_LocateInTFS_VSIPCmdSet { get; }
		string guidSolutionExplorerGuid_String { get; }
		Guid guidSolutionExplorer { get; }

		uint cmdidLocateInTFS_SolutionExplorer { get; }
		uint cmdidLocateInTFS_CodeWindow { get; }
		uint cmdidQueryStatus { get; }
		uint cmdidSolutionExplorer { get; }
	}

	public class VsPackageIdentifiers : IVsPackageIdentifiers
	{
		virtual public string guidVisualStudio_LocateInTFS_VSIPPkgString { get; set; }
		virtual public string guidVisualStudio_LocateInTFS_VSIPCmdSetString { get; set; }
		virtual public Guid guidVisualStudio_LocateInTFS_VSIPCmdSet { get; set; }
		virtual public string guidSolutionExplorerGuid_String { get; set; }
		virtual public Guid guidSolutionExplorer { get; set; }

		virtual public uint cmdidLocateInTFS_SolutionExplorer { get; set; }
		virtual public uint cmdidLocateInTFS_CodeWindow { get; set; }
		virtual public uint cmdidQueryStatus { get; set; }
		virtual public uint cmdidSolutionExplorer { get; set; }
	}
}