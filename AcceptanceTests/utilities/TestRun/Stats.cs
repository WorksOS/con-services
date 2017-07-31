using System;
using System.Diagnostics;

namespace TestRun
{
    public class Stats
    {
        // All test suites
        private int globalFailCount;
        private int globalErrorCount;
        private int globalPassCount;
        private int globalTestCount;

        // One test suite / project / dll
        private int suiteFailCount;
        private int suiteErrorCount;
        private int suitePassCount;
        private int suiteTestCount;

        private int classCount;
        private readonly Stopwatch globalTime;
        private readonly Stopwatch localTime;
        public DateTime StartDateTimeOneTest { get; set; }
        public DateTime EndDateTimeOneTest { get; set; }
        public DateTime StartDateTimeAllTests { get; set; }
        public DateTime EndDateTimeAllTests { get; set; }
        public int ClassCount => classCount;
        public int GlobalFailCount => globalFailCount;
        public int GlobalErrorCount => globalErrorCount;
        public int GlobalPassCount => globalPassCount;
        public int GlobalTestCount => globalTestCount;
        public int SuiteFailCount => suiteFailCount;
        public int SuiteErrorCount => suiteErrorCount;
        public int SuitePassCount => suitePassCount;
        public int SuiteTestCount => suiteTestCount;
        public TimeSpan GlobalTime => globalTime.Elapsed;
        public TimeSpan LocalTime => localTime.Elapsed;

        public Stats()
        {
            globalTime = new Stopwatch();
            localTime = new Stopwatch();

            globalTime.Start();
        }

        public void AddClassCount()
        {
            classCount++;
        }

        public void AddGlobalFailCount()
        {
            globalFailCount++;
        }

        public void AddGlobalErrorCount()
        {
            globalErrorCount++;
        }

        public void AddGlobalPassCount()
        {
            globalPassCount++;
        }

        public void AddGlobalCount()
        {
            globalTestCount++;
        }

         public void AddSuiteFailCount()
        {
            suiteFailCount++;
            AddGlobalFailCount();
        }

        public void AddSuiteErrorCount()
        {
            suiteErrorCount++;
            AddGlobalErrorCount();
        }

        public void AddSuitePassCount()
        {
            suitePassCount++;
            AddGlobalPassCount();
        }

        public void AddSuiteCount()
        {
            suiteTestCount++;
            AddClassCount();
        }

        public void ResetSuiteCount()
        {
            suiteTestCount = 0;
            suitePassCount = 0;
            suiteErrorCount = 0;
            suiteFailCount = 0;
        }

        public void StartLocalTime()
        {
            localTime.Start();
        }

        public void ResetLocalTime()
        {
            localTime.Reset();
        }

        public void GetFinalResult()
        {
            localTime.Stop();
            globalTime.Stop();
        }
    }
}
