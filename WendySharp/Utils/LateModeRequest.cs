using System;
using System.Timers;

namespace WendySharp
{
    class LateModeRequest : IDisposable
    {
        public string Channel { get; set; }
        public string Recipient { get; set; }
        public string Reason { get; set; }
        public DateTime Time { get; set; }
        public string Mode { get; set; }
        public string Sender { get; set; }
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

            var time = Time.Subtract(DateTime.UtcNow).TotalMilliseconds;

            if (time > int.MaxValue)
            {
                Log.WriteError("Late mode", "TODO: Timers can't have a big interval, timer for {0} not started.", Recipient);

                return;
            }

            Timer = new Timer();
            Timer.Interval = time;
            Timer.Elapsed += OnRealTimerElapsed;
            Timer.Start();

            Log.WriteDebug("late mode", "timer for {0} will run {1}", Recipient, Time.ToRelativeString());
        }

        public void Dispose()
        {
            if (Timer != null)
            {
                Timer.Dispose();

                Timer = null;
            }
        }

        private void Execute()
        {
            if (!Bootstrap.Client.Client.IsConnected)
            {
                return;
            }

            Bootstrap.Client.Client.Mode(Channel, Mode, Recipient);
        }

        private void StartNormalTimer()
        {
            Dispose();

            Timer = new Timer();
            Timer.Interval = TimeSpan.FromMinutes(1).TotalMilliseconds;
            Timer.Elapsed += OnTimerElapsed;
            Timer.Start();
        }

        private void OnRealTimerElapsed(object sender, ElapsedEventArgs e)
        {
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
