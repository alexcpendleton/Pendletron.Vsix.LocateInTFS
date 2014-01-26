using Pendletron.Vsix.LocateInTFS;

namespace Pendletron.Vsix.Core.Locaters
{
    public class Vs2013Locater : ExplorerSccLocater
    {
        public Vs2013Locater(ILocateInTfsVsPackage package) : base(package) { }
    }
}