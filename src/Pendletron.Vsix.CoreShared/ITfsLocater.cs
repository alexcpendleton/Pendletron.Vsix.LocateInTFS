using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pendletron.Vsix.Core
{
	public interface ITfsLocater
	{
		bool IsVersionControlled(string fileSystemPath);
		void Locate(string localPath);
		void Initialize();
		string GetSelectedPathFromSolutionExplorer();
        string GetSelectedPathFromActiveDocument();
        int CommandExecute(ICommandExecParams e);
        IQueryStatusResult CommandBeforeQueryStatus(ICommandQueryStatusParams e);
    }
}
