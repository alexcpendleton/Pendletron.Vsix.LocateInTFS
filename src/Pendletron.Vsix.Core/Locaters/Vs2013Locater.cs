using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.TeamFoundation.VersionControl.Client;
using Pendletron.Vsix.Core.Wrappers;
using Pendletron.Vsix.LocateInTFS;
using System;
using System.Windows.Threading;

namespace Pendletron.Vsix.Core.Locaters
{
    public class Vs2013Locater : ExplorerSccLocater
    {
        public Vs2013Locater(ILocateInTfsVsPackage package) : base(package)
        {
            DispatchPollTime = TimeSpan.FromSeconds(0.5);
            MaxDispatchAttempts = 10;
        }

        public TimeSpan DispatchPollTime { get; set; }
        public int MaxDispatchAttempts { get; set; }

        public override bool IsVersionControlled(string selectedPath)
        {
            try
            {
                // Could be a git context, not currently supported
                bool isTfsContext = HatterasPackage.HatterasService.IsCurrentContextTFVC;
                if (!isTfsContext)
                {
                    return false;
                }
            }
            catch { }
            return base.IsVersionControlled(selectedPath);
        }

        public override void Locate(string localPath)
        {
            dynamic vcs = HatterasPackage.HatterasService.VersionControlServer;
            var workspace = vcs.GetWorkspace(localPath);
            string serverPath = workspace.TryGetServerItemForLocalItem(localPath);
            
            //HatterasPackage._wrapped.OpenSceToPath("$/", workspace);
            //HatterasPackage._wrapped.OpenSceToPath(serverPath, workspace);
            DispatchOpenSceToPath(serverPath, workspace);
        }


        public void DispatchOpenSceToPath(string serverPath, object workspace)
        {
            /* In VS2013 (unlike VS2010) the Source Control Explorer opens asynchronously. This caused an issue that when
             * the SCE was not open and "Locate in TFS" was clicked we would not be able to open the SCE to the correct place
             * because it hadn't finished opening and connecting.
             * 
             * There aren't any obvious events to which to subscribe, thus we do some polling in a separate thread via 
             * the Dispatcher. It has to be a separate thread in order to not block the SCE from loading.
             * */

            // This will make sure the SCE window is open, or will open
            dynamic sccToolWindow = new AccessPrivateWrapper(HatterasPackage._wrapped.GetToolWindowSccExplorer(true));
            dynamic explorer = new AccessPrivateWrapper(sccToolWindow.SccExplorer);
            
            // If it's already open and connected there's no need to poll
            if (!explorer.IsDisconnected)
            {
                OpenSceToPathWithPrecedingCall(serverPath, workspace);
            }
            else
            {
                int attemptsMade = 0;
                DispatcherTimer timer = new DispatcherTimer()
                {
                    Interval = DispatchPollTime,
                    Tag = 0
                };
                timer.Tick += (sender, args) =>
                {
                    // We don't want to loop infinitely, if we've tried a few times and it's still not connected then just give up
                    // By default this will be about five seconds (10 attempts, 0.5 seconds interval)
                    if (attemptsMade == MaxDispatchAttempts)
                    {
                        // Give up, something bad happened
                        timer.Stop();
                    }
                    else
                    {
                        if (!explorer.IsDisconnected)
                        {
                            // The explorer is connected, so we're ready to go
                            timer.Stop();
                            OpenSceToPathWithPrecedingCall(serverPath, workspace);
                        }
                        else
                        {
                            // Keep the timer going and try again a few more times
                            attemptsMade++;
                        }
                    }
                };
                timer.Start();
            }

        }

        public void OpenSceToSinglePath(string serverPath, object workspace)
        {
            HatterasPackage._wrapped.OpenSceToPath(serverPath, workspace);
        }

        public void OpenSceToPathWithPrecedingCall(string serverPath, object workspace)
        {
            // If you call OpenSceToPath to the same directory, it will mess up and won't show any files
            // So we clear whatever was there before navigating to the desired path
            OpenSceToSinglePath("$/", workspace);
            OpenSceToSinglePath(serverPath, workspace);
        }

        public override void ShowInExplorer(string serverItem)
        {
            DispatchOpenSceToPath(serverItem, null);
        }
    }
}