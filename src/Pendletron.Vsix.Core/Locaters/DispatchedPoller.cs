using System;
using System.Windows.Threading;

namespace Pendletron.Vsix.LocateInTFS
{
    public class DispatchedPoller
    {
        public DispatchedPoller(int maximumNumberOfAttempts, TimeSpan frequency, Func<bool> condition, Action toDo)
        {
            MaximumNumberOfAttempts = maximumNumberOfAttempts;
            Condition = condition;
            ToDo = toDo;
            Frequency = frequency;
        }

        public int MaximumNumberOfAttempts { get; protected set; }
        public Func<bool> Condition { get; protected set; }
        public Action ToDo { get; protected set; }
        public TimeSpan Frequency { get; protected set; }

        public void Go()
        {
            Loop();
        }

        protected void Loop()
        {
            if (Condition())
            {
                ToDo();
            }
            else
            {
                int attemptsMade = 0;
                var timer = new DispatcherTimer()
                {
                    Interval = Frequency,
                    Tag = 0
                };
                timer.Tick += (sender, args) =>
                {
                    if (attemptsMade == MaximumNumberOfAttempts)
                    {
                        // Give up, we've tried enough times, no point in continuing
                        timer.Stop();
                    }
                    else
                    {
                        if (Condition())
                        {
                            timer.Stop();
                            ToDo();
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
    }
}