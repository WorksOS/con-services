using System;
using System.IO;
using System.Reflection;

namespace TestRun
{
  /// <summary>
  /// Build the trx file that holds the test results
  /// </summary>
  public class ResultsMstest
    {
        #region public properties
        public string wholeTrxFile { get; set; }
        public string allTestRun  { get; set; }
        public string allTestDefinitions { get; set; }
        public string allTestLists { get; set; }
        public string allTestEntries { get; set; }
        public string allTestResults { get; set; }        
        public string testId { get; set; }
        public string executionId { get; set; }        
        public string testTypeId { get; set; }
        #endregion

        #region private properties
        private string skelTestRun  { get; set; }
        private string skelTestDefWrapper { get; set; }
        private string skelTestDefinition { get; set; }
        private string skelTestList { get; set; }
        private string skelTestEntryWrapper { get; set; }
        private string skelTestEntry { get; set; }
        private string skelTestResultWrapper { get; set; }
        private string skelTestResult { get; set; }
        private string skelErrorInfo { get; set; }
        #endregion
        private Object thisLock = new Object();


        public ResultsMstest()
        {
            InitializeSkeletons();
        }

        /// <summary>
        /// Set the skeleton strings to an initial value
        /// </summary>
        public void InitializeSkeletons()
        {
            skelTestRun = 
                @"<?xml version=""1.0"" encoding=""UTF-8""?>" +
                @"<TestRun id=""0b060eab-e107-4f4b-a02f-207f6c5fc482"" name=""Automated tests XMLCLSSTART"" runUser=""jenkins"" " +
                @"xmlns=""http://microsoft.com/schemas/VisualStudio/TeamTest/2010""><TestSettings name=""Default Test Settings"" " + 
                @"id=""16218e59-4b3b-4bfa-8ff8-38fa51eea6f7""></TestSettings><Times creation=""XMLCLSSTART"" queuing=""XMLCLSSTART"" "+
                @"start=""XMLCLSSTART"" finish=""XMLCLSFINISH"" /><ResultSummary outcome=""XMLTOTALOUTCOME""><Counters total=""XMLTOTALCNT"" "+
                @"executed=""XMLTOTALCNT"" passed=""XMLTOTALPASSCNT"" failed=""XMLTOTALFAILCNT"" timeout=""0"" aborted=""0"" inconclusive=""0"" " + 
                @"passedButRunAborted=""0"" notRunnable=""0"" notExecuted=""0"" disconnected=""0"" warning=""0"" completed=""0"" inProgress=""0"" " + 
                @"pending=""0"" /></ResultSummary>XMLRESTOFSTRINGS</TestRun>";

            skelTestDefWrapper = @"<TestDefinitions>XMLALLTESTDEFINITIONS</TestDefinitions>";

            skelTestDefinition = 
                @"<UnitTest name=""XMLTESTNAME"" id=""XMLTESTID""><Description>XMLTESTNAME</Description><Execution id=""XMLTESTEXEC"" />" +
                @"<Properties><Property><Key>FeatureTitle</Key><Value>XMLCLSNAME</Value></Property></Properties><TestMethod className=""" +
                @"XMLFULLCLSNAME"" name=""XMLTESTNAME"" codeBase=""XMLDLLNAME"" adapterTypeName" +
                @"=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, " + 
                @"Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" /></UnitTest>";

            skelTestList =   @"<TestLists><TestList name=""ListofTests"" id=""8c84fa94-04c1-424b-9868-57a2d4851a1d"" /></TestLists>";

            skelTestEntryWrapper = @"<TestEntries>XMLTESTENTRIES</TestEntries>";

            skelTestEntry = @"<TestEntry testId=""XMLTESTID"" executionId=""XMLTESTEXEC"" testListId=""8c84fa94-04c1-424b-9868-57a2d4851a1d"" />";

            skelTestResultWrapper = @"<Results>XMLALLRESULTS</Results>";

            skelTestResult = 
                @"<UnitTestResult executionId=""XMLTESTEXEC"" testId=""XMLTESTID"" testName=""XMLTESTNAME"" computerName=""XMLCOMPUTER"" duration=""XMLDURATION"" " +
                @"startTime=""XMLSTARTTIME"" endTime=""XMLENDTIME"" testType=""XMLTESTTYPE"" outcome=""XMLOUTCOME"" testListId=""8c84fa94-04c1-424b-9868-57a2d4851a1d"">" +
                @"<Output><StdOut>""XMLSTDOUT""</StdOut>XMLERROR</Output></UnitTestResult>";

            skelErrorInfo = @"<ErrorInfo><Message>XMLMESSAGE</Message><StackTrace>XMLSTACKTRACE</StackTrace></ErrorInfo>";
        }

        /// <summary>
        /// Replace the token in the skeleton with the new value and return the new skeleton
        /// </summary>
        /// <param name="oldValue">Tag value in skeleton</param>
        /// <param name="newValue">New value to replace the tag</param>
        /// <param name="skeleton">The contents of the skeleton</param>
        /// <returns>Skeleton with tag replaced</returns>
        public string ReplaceTag(string oldValue, string newValue, string skeleton)
        {
            var replacedString = skeleton.Replace(oldValue, newValue);
            return replacedString;

        }

        /// <summary>
        /// Set the test run totals for the overall test run 
        /// </summary>
        /// <param name="stats">Stats with all the timings</param>
        public void SetTestRunTotals(Stats stats)
        {
            lock (thisLock)
            {
                // Set wrappers first
                var allTestDefinitionsWrapped = ReplaceTag("XMLALLTESTDEFINITIONS", allTestDefinitions,
                    skelTestDefWrapper);
                var allTestEntryWrapped = ReplaceTag("XMLTESTENTRIES", allTestEntries, skelTestEntryWrapper);
                var allTestResultWrapped = ReplaceTag("XMLALLRESULTS", allTestResults, skelTestResultWrapper);

                var completeTestRun = ReplaceTag("XMLCLSSTART",
                    stats.StartDateTimeAllTests.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz"), skelTestRun);
                completeTestRun = ReplaceTag("XMLCLSFINISH",
                    stats.EndDateTimeAllTests.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz"), completeTestRun);
                completeTestRun = ReplaceTag("XMLTOTALCNT", stats.GlobalTestCount.ToString(), completeTestRun);
                completeTestRun = ReplaceTag("XMLTOTALPASSCNT", stats.GlobalPassCount.ToString(), completeTestRun);
                completeTestRun = ReplaceTag("XMLTOTALFAILCNT", stats.GlobalFailCount.ToString(), completeTestRun);
                completeTestRun = ReplaceTag("XMLTOTALOUTCOME", stats.GlobalFailCount == 0 ? "Passed" : "Failed", completeTestRun);

                // Now combine all the other strings 
                var combinedString = allTestDefinitionsWrapped + skelTestList + allTestEntryWrapped + allTestResultWrapped;
                wholeTrxFile = ReplaceTag("XMLRESTOFSTRINGS", combinedString, completeTestRun);
            }
        }

        /// <summary>
        /// Set up XML for a single passed result
        /// </summary>
        /// <param name="method">method details</param>
        /// <param name="stats">stats for current test</param>
        /// <param name="stdout"></param>
        public void SetTestPassed(MethodInfo method, Stats stats, StringWriter stdout)
        {
            lock (thisLock)
            {
                var testResult = ReplaceTag("XMLTESTNAME", method.Name, skelTestResult);
                testResult = ReplaceTag("XMLTESTID", testId, testResult);
                testResult = ReplaceTag("XMLTESTEXEC", executionId, testResult);
                testResult = ReplaceTag("XMLTESTTYPE", testTypeId, testResult);
                testResult = ReplaceTag("XMLDURATION", stats.LocalTime.ToString(), testResult);
                testResult = ReplaceTag("XMLSTARTTIME",
                    stats.StartDateTimeOneTest.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz"), testResult);
                testResult = ReplaceTag("XMLENDTIME",
                    stats.EndDateTimeOneTest.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz"), testResult);
                testResult = ReplaceTag("XMLOUTCOME", "Passed", testResult);
                testResult = ReplaceTag("XMLCOMPUTER", Environment.MachineName, testResult);
                testResult = ReplaceTag("XMLSTDOUT", stdout.ToString(), testResult);
                testResult = ReplaceTag("XMLERROR", "", testResult);
                allTestResults = allTestResults + testResult;
            }
        }

        /// <summary>
        /// Set up XML for a failed test result
        /// </summary>
        /// <param name="method">method details</param>
        /// <param name="stats">stats for current test</param>
        /// <param name="ex">Exception that happen in the </param>
        public void SetTestFailed(MethodInfo method, Stats stats, Exception ex, StringWriter stdout)
        {
            lock (thisLock)
            {
              var testResult = ReplaceTag("XMLTESTNAME", method.Name, skelTestResult);
              testResult = ReplaceTag("XMLTESTID", testId, testResult);
              testResult = ReplaceTag("XMLTESTEXEC", executionId, testResult);
              testResult = ReplaceTag("XMLTESTTYPE", testTypeId, testResult);
              testResult = ReplaceTag("XMLDURATION", stats.LocalTime.ToString(), testResult);
              testResult = ReplaceTag("XMLSTARTTIME",
                stats.StartDateTimeOneTest.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz"), testResult);
              testResult = ReplaceTag("XMLENDTIME",
                stats.EndDateTimeOneTest.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz"), testResult);
              testResult = ReplaceTag("XMLOUTCOME", "Failed", testResult);
              testResult = ReplaceTag("XMLCOMPUTER", Environment.MachineName, testResult);
              testResult = ReplaceTag("XMLSTDOUT", stdout.ToString(), testResult);
              var excep = ex.GetBaseException().Message.Replace('<', ' ');
              excep = excep.Replace('>', ' ');
              excep = excep.Replace('"', ' ');
              var exceptionDetails = "Test method " + method.Name + " threw exception: " + Environment.NewLine +
                                     ex.Message +
                                     Environment.NewLine + excep;
              var errorResult = ReplaceTag("XMLMESSAGE", exceptionDetails, skelErrorInfo);
              var stackTrace = ex.StackTrace.Replace('<', ' ');
              stackTrace = stackTrace.Replace('>', ' ');
              stackTrace = stackTrace.Replace('"', ' ');
              errorResult = ReplaceTag("XMLSTACKTRACE", stackTrace, errorResult);
              testResult = ReplaceTag("XMLERROR", errorResult, testResult);
              allTestResults = allTestResults + testResult;
            }
        }


        public void SetTestInitialize(MethodInfo method)
        {
            lock (thisLock)
            {
                // Create guids
                testId = Guid.NewGuid().ToString();
                executionId = Guid.NewGuid().ToString();
                testTypeId = "13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b";

                // Create test entry
                var testEntry = ReplaceTag("XMLTESTID", testId, skelTestEntry);
                testEntry = ReplaceTag("XMLTESTEXEC", executionId, testEntry);
                allTestEntries = allTestEntries + testEntry;

                // Create test definition
                var testDefinition = ReplaceTag("XMLTESTID", testId, skelTestDefinition);
                testDefinition = ReplaceTag("XMLTESTNAME", method.Name, testDefinition);
                testDefinition = ReplaceTag("XMLTESTEXEC", executionId, testDefinition);
                testDefinition = ReplaceTag("XMLCLSNAME", method.DeclaringType.Name, testDefinition);
                testDefinition = ReplaceTag("XMLFULLCLSNAME", method.DeclaringType.FullName, testDefinition);
                testDefinition = ReplaceTag("XMLDLLNAME", method.DeclaringType.AssemblyQualifiedName, testDefinition);
                allTestDefinitions = allTestDefinitions + testDefinition;
            }
        }
    }
}
