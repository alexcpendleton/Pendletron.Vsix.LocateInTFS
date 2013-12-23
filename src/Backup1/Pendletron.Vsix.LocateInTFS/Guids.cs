// Guids.cs
// MUST match guids.h
using System;

namespace Pendletron.Vsix.LocateInTFS
{
    static class GuidList
    {
        public const string guidVisualStudio_LocateInTFS_VSIPPkgString = "8e7c7ec6-736b-42a6-8a02-301d0f80a1f3";
        public const string guidVisualStudio_LocateInTFS_VSIPCmdSetString = "19dabbc7-d73f-48a0-ad3a-582e0349ce21";

        public static readonly Guid guidVisualStudio_LocateInTFS_VSIPCmdSet = new Guid(guidVisualStudio_LocateInTFS_VSIPCmdSetString);
		
        public const string guidSolutionExplorerGuid_String = "D309F791-903F-11D0-9EFC-00A0C911004F";
        public static readonly Guid guidSolutionExplorer  = new Guid(guidSolutionExplorerGuid_String);

    };
}