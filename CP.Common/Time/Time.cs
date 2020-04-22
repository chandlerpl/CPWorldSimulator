using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CP.Common.Time
{
    public class Time
    {
        private Time() { }

        private static TimerTick gameTime = new TimerTick();

        public static float SpeedFactor { get => gameTime.SpeedFactor; set => gameTime.SpeedFactor = value; }
        public static TimeSpan ElapsedTime { get => gameTime.ElapsedTime; }
        public static TimeSpan StartTime { get => gameTime.StartTime; }
        public static TimeSpan TotalTime { get => gameTime.TotalTime; }
        public static float DeltaTime { get => gameTime.DeltaTime; }
        public static float AvgDeltaTime { get => gameTime.AvgDeltaTime; }
        public static int FrameCount { get => gameTime.TickCount; }
        public static int FrameRate { get => (int)(1f / gameTime.DeltaTime); }
        public static int AvgFrameRate { get => (int)(1f / gameTime.AvgDeltaTime); }

        public static void Update()
        {
            gameTime.Tick();
        }

        public static TimeSpan ConvertRawToTimestamp(long delta)
        {
            return new TimeSpan(delta == 0 ? 0 : (delta * TimeSpan.TicksPerSecond) / Stopwatch.Frequency);
        }
    }
}
