using System;
using Pendletron.Vsix.LocateInTFS;
namespace Pendletron.Vsix.Core.Locaters
{
    public class Vs2012DispatchingLocater : ExplorerSccDispatchingLocater
    {
        public Vs2012DispatchingLocater(ILocateInTfsVsPackage package) : base(package) { }
    }
}