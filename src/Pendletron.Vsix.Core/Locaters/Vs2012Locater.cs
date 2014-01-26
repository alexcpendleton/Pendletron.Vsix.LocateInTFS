using Pendletron.Vsix.LocateInTFS;
namespace Pendletron.Vsix.Core.Locaters
{
    public class Vs2012Locater : ExplorerSccLocater
    {
        public Vs2012Locater(ILocateInTfsVsPackage package) : base(package) { }
    }
}