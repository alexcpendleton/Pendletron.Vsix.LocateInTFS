// Guids.cs
// MUST match guids.h
using System;

namespace Genco.VSPackage1
{
    static class GuidList
    {
        public const string guidVSPackage1PkgString = "8c5666d6-b02c-4ee9-baaa-f041a7003cc3";
        public const string guidVSPackage1CmdSetString = "c9d4b3c4-e3bc-4a1c-9e09-f7df5a819fd8";

        public static readonly Guid guidVSPackage1CmdSet = new Guid(guidVSPackage1CmdSetString);
    };
}