using System;
using System.Diagnostics;
using UnityEngine;
using VARLab.Analytics;

namespace VARLab.PublicHealth
{
    public static class AnalyticsHelper
    {
        private static Stopwatch _stopwatch = new();
        private const int MaxInterval = 180; // If in 3 minutes no objects have been inspected update total time
        public static void StartTimer()
        {
            _stopwatch.Start();
            UnityEngine.Debug.Log("Analytics timer started.");
        }

        public static void UpdateTimer()
        {
            if (GetTime() >= 180)
            {
                UpdateSessionTotalTimePlayed();
            }
        }

        public static void UpdateSessionTotalTimePlayed()
        {
            UnityEngine.Debug.Log($"Total time triggered: {GetTime()} seconds");
            CoreAnalytics.SendDLXTotalDurationEvent(GetTime());
            _stopwatch.Reset();
            _stopwatch.Start();
            GC.Collect();
        }

        private static int GetTime()
        {
            return (int)_stopwatch.Elapsed.TotalSeconds;
        }
    }
}
