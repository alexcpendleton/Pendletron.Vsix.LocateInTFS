// Guids.cs
// MUST match guids.h
using System;

namespace AlexPendleton.ToggleShowDeletedItems_VSIP
{
    static class GuidList
    {
        public const string guidToggleShowDeletedItems_VSIPPkgString = "c7a20a99-596c-4c6a-9485-773a1798560a";
        public const string guidToggleShowDeletedItems_VSIPCmdSetString = "fd1c799c-d7fc-406a-94f5-abf11940884b";

        public static readonly Guid guidToggleShowDeletedItems_VSIPCmdSet = new Guid(guidToggleShowDeletedItems_VSIPCmdSetString);
    };
}