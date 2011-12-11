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
    };
}