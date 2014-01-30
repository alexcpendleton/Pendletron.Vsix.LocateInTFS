using System;
using Pendletron.Vsix.LocateInTFS;
namespace Pendletron.Vsix.Core.Locaters
{
    public class Vs2012Locater : ExplorerSccLocater
    {
        public Vs2012Locater(ILocateInTfsVsPackage package) : base(package) { }
        public override void ShowInExplorer(string serverItem)
        {
            // If you call OpenSceToPath to the same directory, it will mess up and won't show any files
            // So we clear whatever was there before, first
            HatterasPackage._wrapped.OpenSceToPath("$/");
            HatterasPackage._wrapped.OpenSceToPath(serverItem);
        }
    }
}