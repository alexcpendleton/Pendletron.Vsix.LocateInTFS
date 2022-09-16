using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pendletron.Vsix.Core
{
	public interface ILocateInTfsVsPackage
	{
		IVsPackageIdentifiers PackageIDs { get; }
		dynamic GetServiceAsDynamic(Type serviceInterfaceType);
	}

    public interface IQueryStatusResult
    {
        bool IsVersionControlled { get; }
        uint PrgCmdsValue { get; }
        uint ReturnValue { get; }
    }
    public class QueryStatusResult : IQueryStatusResult
    {
        public QueryStatusResult()
        {
        }

        public QueryStatusResult(bool isVersionControlled, uint prgCmdsValue, uint returnValue)
        {
            IsVersionControlled = isVersionControlled;
            PrgCmdsValue = prgCmdsValue;
            ReturnValue = returnValue;
        }

        virtual public bool IsVersionControlled { get; set; }
        virtual public uint PrgCmdsValue { get; set; }
        virtual public uint ReturnValue { get; set; }
    }



    public interface ICommandQueryStatusParams
    {
        //ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText
        Guid CmdGroup { get; }
        uint Cmds { get; }
        object PrgCmds { get; }
        IntPtr CmdText { get; }
    }

    public class CommandQueryStatusParams : ICommandQueryStatusParams
    {
        virtual public Guid CmdGroup { get; set; }
        virtual public uint Cmds { get; set; }
        virtual public object PrgCmds { get; set; }
        virtual public IntPtr CmdText { get; set; }
    }

    public interface ICommandExecParams
    {
        //ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut
        Guid CmdGroup { get; }
        uint CommandID { get; }
        uint CommandExecOpt { get; }
        IntPtr In { get; }
        IntPtr Out { get; }
    }

    public class CommandExecParams : ICommandExecParams
    {
        virtual public Guid CmdGroup { get; set; }
        virtual public uint CommandID { get; set; }
        virtual public uint CommandExecOpt { get; set; }
        virtual public IntPtr In { get; set; }
        virtual public IntPtr Out { get; set; }
    }
}
