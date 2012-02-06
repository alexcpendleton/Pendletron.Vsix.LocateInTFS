using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Pendletron.Vsix.Core.Wrappers
{
	public class HatPackage 
	{
		private dynamic _wrapped = null;
		private Assembly _vcAssembly = null;

		public HatPackage() {
			_vcAssembly = Assembly.Load("Microsoft.VisualStudio.TeamFoundation.VersionControl");
			Type t = _vcAssembly.GetType("Microsoft.VisualStudio.TeamFoundation.VersionControl.HatPackage");
			var prop = t.GetProperty("Instance", BindingFlags.NonPublic | BindingFlags.Static);
			object instance = prop.GetValue(null, null);
			_wrapped = new AccessPrivateWrapper(instance);
		}

		private dynamic _hatterasService = null;
		public dynamic HatterasService
		{
			get {
				if (_hatterasService == null)
				{
					_hatterasService = new AccessPrivateWrapper(_wrapped.HatterasService);
				}
				return _hatterasService; }
			set { _hatterasService = value; }
		}

		public VersionControlServer GetVersionControlServer()
		{
			return HatterasService.VersionControlServer;
		}
	}
}
