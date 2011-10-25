// Guids.cs
// MUST match guids.h
using System;

namespace Microsoft.LocateInSourceControl
{
    static class GuidList
    {
        public const string guidLocateInSourceControlPkgString = "d761d564-dc8d-43a7-a0c3-c71894961051";
        public const string guidLocateInSourceControlCmdSetString = "5c0b933c-48f8-43fc-a1e7-ddfd801135e7";

        public static readonly Guid guidLocateInSourceControlCmdSet = new Guid(guidLocateInSourceControlCmdSetString);
    };
}