using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System;
using Microsoft.VisualStudio.Shell;
namespace AlexPendleton.VisualStudio_LocateInSourceControl_VSIP
{
	public class SelectionEvents : IVsSelectionEvents
	{
		private static IVsMonitorSelection SelectionService;
		private static uint ContextCookie = RegisterContext();
		private static uint RegisterContext()
		{
			// Initialize the selection service
			SelectionService = (IVsMonitorSelection)Package.GetGlobalService(typeof(SVsShellMonitorSelection));
			// Get a cookie for our UI Context. This “registers” our
			// UI context with the selection service so we can set it again later.
			uint retVal;
			Guid uiContext = GuidList.guid_UICONTEXT_underSourceControl;
			SelectionService.GetCmdUIContextCookie(ref uiContext, out retVal);
			return retVal;
		}
		private Func<string, bool> _locator = null; 
		public SelectionEvents(Func<string, bool> versionControlLocator)
		{
			_locator = versionControlLocator;
		}
		public int OnCmdUIContextChanged(uint dwCmdUICookie, int fActive)
		{
			return VSConstants.S_OK;
		}

		public int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
		{
			return VSConstants.S_OK;
		}

		public int OnSelectionChanged(IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld, ISelectionContainer pSCOld, IVsHierarchy pHierNew, uint itemidNew, IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew)
		{
			bool isUnderSourceControl = false;
			if (pHierNew != null)
			{
				object fileName;
				pHierNew.GetProperty(itemidNew, (int)__VSHPROPID.VSHPROPID_Name, out fileName);
				if(fileName != null && fileName is String)
				{
					isUnderSourceControl = _locator((string) fileName);
				}
			}
			SelectionService.SetCmdUIContext(ContextCookie, Convert.ToInt32(isUnderSourceControl));
			return VSConstants.S_OK;
		}
	}
}