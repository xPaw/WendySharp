using System;
using System.Timers;
using NetIrc2;

namespace WendySharp
{
    class LateModeRequest : IDisposable
    {
        public string Channel { get; set; }
        public string Recipient { get; set; }
        public string Reason { get; set; }
        public DateTime Time { get; set; }
        public string Mode { get; set; }
        private Timer Timer;

        public void Check()
        {
            if (Time == default(DateTime))
            {
                return;
            }

            if (DateTime.UtcNow >= Time)
            {
                StartNormalTimer();

                Execute();

                return;
            }

            if (Timer != null)
            {
                return;
            }

            Timer = new Timer();
            Timer.Interval = Time.Subtract(DateTime.UtcNow).TotalMilliseconds;
            Timer.Elapsed += OnRealTimerElapsed;
            Timer.Start();

            Log.WriteDebug("late mode", "timer for {0} will run in {1}", Recipient, Time.Subtract(DateTime.UtcNow).TotalMinutes);
        }

        public void Dispose()
        {
            if (Timer != null)
            {
                Timer.Dispose();

                Timer = null;
            }
        }

        public void Execute()
        {
            if (!Bootstrap.Client.Client.IsConnected)
            {
                return;
            }

            Bootstrap.Client.Client.Mode(Channel, Mode, new IrcString[1] { Recipient });
        }

        private void StartNormalTimer()
        {
            Timer = new Timer();
            Timer.Interval = TimeSpan.FromMinutes(1).TotalMilliseconds;;
            Timer.Elapsed += OnTimerElapsed;
            Timer.Start();
        }

        private void OnRealTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Timer.Dispose();

            StartNormalTimer();

            Log.WriteDebug("late mode", "timer ran for {0}", Recipient);

            Execute();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Log.WriteDebug("late mode", "minutly timer ran for {0}", Recipient);

            Execute();
        }
    }
}
