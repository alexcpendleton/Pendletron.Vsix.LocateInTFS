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
}
