// Guids.cs
// MUST match guids.h
using System;

namespace AlexPendleton.VisualStudio_LocateInSourceControl_VSIP
{
    static class GuidList
    {
        public const string guidVisualStudio_LocateInSourceControl_VSIPPkgString = "8e7c7ec6-736b-42a6-8a02-301d0f80a1f3";
        public const string guidVisualStudio_LocateInSourceControl_VSIPCmdSetString = "19dabbc7-d73f-48a0-ad3a-582e0349ce21";

        public static readonly Guid guidVisualStudio_LocateInSourceControl_VSIPCmdSet = new Guid(guidVisualStudio_LocateInSourceControl_VSIPCmdSetString);

        public const string guidDynamicMenuDevelopmentCmdSetPart2_String = "b62785c7-068f-4ff1-b2bf-dee52d3f6a72";
        public static readonly Guid guidDynamicMenuDevelopmentCmdSetPart2  = new Guid(guidDynamicMenuDevelopmentCmdSetPart2_String);

        public const string guidSolutionExplorerGuid_String = "D309F791-903F-11D0-9EFC-00A0C911004F";
        public static readonly Guid guidSolutionExplorer  = new Guid(guidSolutionExplorerGuid_String);

        public const string guidTest_String = "2318f493-7b52-4ed9-b11d-02ad552caa1a";
        public static readonly Guid guidTest = new Guid(guidTest_String);
    };
}