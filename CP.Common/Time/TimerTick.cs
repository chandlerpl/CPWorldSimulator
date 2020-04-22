using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CP.Common.Time
{
    public class TimerTick
    {
        long startTime = 0;
        long lastTime = 0;

        bool isPaused = false;
        float speedFactor = 1.0f;

        public TimeSpan ElapsedTime { get; private set; }
        public TimeSpan StartTime { get => Time.ConvertRawToTimestamp(startTime); }
        public TimeSpan TotalTime { get; private set; }
        public float DeltaTime { get; private set; } = 0;
        public float SpeedFactor { get => speedFactor; set => speedFactor = value; }

        public int TickCount { get; private set; }
        public float AvgDeltaTime { get; private set; }

        public TimerTick()
        {
            Restart();
        }

        public void Restart()
        {
            startTime = Stopwatch.GetTimestamp();
            lastTime = startTime;
            isPaused = false;
            TotalTime = TimeSpan.Zero;
            ElapsedTime = TimeSpan.Zero;
            DeltaTime = 0;
        }

        public void Tick()
        {
            if (isPaused)
            {
                ElapsedTime = TimeSpan.Zero;
                DeltaTime = 0;
            }

            long currTime = Stopwatch.GetTimestamp();
            ElapsedTime = new TimeSpan((long)Math.Round(Time.ConvertRawToTimestamp(currTime - lastTime).Ticks * speedFactor));
            TotalTime += ElapsedTime;
            DeltaTime = (float)ElapsedTime.TotalSeconds;
            lastTime = currTime;
            AvgDeltaTime = ((AvgDeltaTime * TickCount) + DeltaTime) / ++TickCount;
        }

        public void Resume()
        {
            if (isPaused)
            {
                isPaused = false;
                lastTime = Stopwatch.GetTimestamp();
            }
        }

        public void Pause()
        {
            isPaused = true;
        }
    }
}