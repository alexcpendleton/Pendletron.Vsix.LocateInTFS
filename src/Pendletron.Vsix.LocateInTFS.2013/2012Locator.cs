using Pendletron.Vsix.Core;

namespace Pendletron.Vsix.LocateInTFS
{
	public class _2012_DynamicishLocator : ITfsLocater
	{
	    protected ITfsLocater _decorated;
	    public _2012_DynamicishLocator(ILocateInTfsVsPackage package)
	    {
            _decorated = new DynamicishLocator(package);
	    }
        public bool IsVersionControlled(string fileSystemPath)
        {
            return _decorated.IsVersionControlled(fileSystemPath);
        }

        public void Locate(string localPath)
        {
            _decorated.Locate(localPath);
        }

        public void Initialize()
        {
            _decorated.Initialize();
        }

        public string GetSelectedPathFromSolutionExplorer()
        {
            return _decorated.GetSelectedPathFromSolutionExplorer();
        }

        public string GetSelectedPathFromActiveDocument()
        {
            return _decorated.GetSelectedPathFromActiveDocument();
        }

        public int CommandExecute(ICommandExecParams e)
        {
            return _decorated.CommandExecute(e);
        }

        public IQueryStatusResult CommandBeforeQueryStatus(ICommandQueryStatusParams e)
        {
            return _decorated.CommandBeforeQueryStatus(e);
        }
    }
}