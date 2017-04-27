using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Pendletron.Vsix.Core.Wrappers;
using Pendletron.Vsix.LocateInTFS;
using System;
using System.Windows.Threading;

namespace Pendletron.Vsix.Core.Locaters
{
    public class Vs2013DispatchingLocater : ExplorerSccDispatchingLocater
    {
        public Vs2013DispatchingLocater(ILocateInTfsVsPackage package) : base(package)
        {
        }

        public override bool IsVersionControlled(string selectedPath)
        {
            try
            {
                // Could be a git context, not currently supported
                bool isTfsContext = HatterasPackage.HatterasService.IsCurrentContextTFVC;
                if (isTfsContext)
                {
                    return true;
                }
            }
            catch { }
            return base.IsVersionControlled(selectedPath);
        }

    }
}