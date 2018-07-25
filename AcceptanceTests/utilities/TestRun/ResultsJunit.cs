using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestRun
{
    public class ResultsJunit
    {
        #region public properties
        public string wholeJunitFile { get; set; }
        public string allTestSuites  { get; set; }
        public string allTestCase { get; set; }      
        #endregion

        #region private properties
        private string skelTestSuites  { get; set; }
        private string skelTestSuiteWrapper { get; set; }
        private string skelTestCase { get; set; }
        private string skelError { get; set; }
        private string skelFailure { get; set; }
        private string skelPassed { get; set; }

        private readonly object thisLock = new object();
        #endregion


        public ResultsJunit()
        {
            InitializeSkeletons();
        }
        
        /// <summary>
        /// Set the skeleton strings to an initial value
        /// </summary>
        public void InitializeSkeletons()
        {
            skelTestSuites = 
                @"<?xml version=""1.0"" encoding=""UTF-8""?>" +
                @"<testsuites errors=""XMLTOTALERRORCNT"" failures=""XMLTOTALFAILCNT"" tests=""XMLTOTALCNT"" time=""XMLTOTALTIME"">" +
                @"XMLRESTOFSTRINGS</testsuites>";

            skelTestSuiteWrapper = 
                @"<testsuite errors=""XMLCLSERRORCNT"" failures=""XMLCLSFAILCNT"" hostname=""XMLCOMPUTER"" " +
                @"id=""XMLCLSCNT"" name=""XMLCLSNAME"" package=""XMLFULLCLSNAME"" tests=""XMLTOTALCNT"" time=""XMLTOTALTIME"" timestamp=""XMLCLSSTART"" >" + 
                @"<properties><property name=""assert-passed"" value=""XMLTOTALPASSCNT""/></properties>XMLALLTESTCASES</testsuite>";

            skelTestCase = @"<testcase classname=""XMLCLSNAME"" name=""XMLTESTNAME"" time=""XMLTIMETAKEN""XMLERRORORFAILURE";
            skelPassed = @"><system-out>XMLSTDOUT</system-out></testcase>";
            skelFailure = @"><failure message=""XMLMESSAGE"" type=""failure""></failure><system-out>XMLSTDOUT</system-out></testcase>";
            skelError = @"><error message=""XMLMESSAGE"" type=""error""></error><system-out>XMLSTDOUT</system-out></testcase>";
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
                var completeTestRun = ReplaceTag("XMLTOTALTIME", stats.GlobalTime.TotalSeconds.ToString(CultureInfo.InvariantCulture), skelTestSuites);
                completeTestRun = ReplaceTag("XMLTOTALCNT", stats.GlobalTestCount.ToString(), completeTestRun);
                completeTestRun = ReplaceTag("XMLTOTALERRORCNT", stats.GlobalErrorCount.ToString(), completeTestRun);
                completeTestRun = ReplaceTag("XMLTOTALFAILCNT", stats.GlobalFailCount.ToString(), completeTestRun);

                // Add in all the test suites
                completeTestRun = ReplaceTag("XMLRESTOFSTRINGS", allTestSuites, completeTestRun);
                wholeJunitFile = completeTestRun;
                allTestSuites = string.Empty;
            }
        }

        /// <summary>
        /// Set the testsuite totals 
        /// </summary>
        /// <param name="stats"></param>
        /// <param name="currentClass"></param>
        public void SetTestSuiteTotals(Stats stats, Type currentClass)
        {
            lock (thisLock)
            {
                var completeTestSuite = ReplaceTag("XMLCOMPUTER", Environment.MachineName, skelTestSuiteWrapper);
                completeTestSuite = ReplaceTag("XMLCLSERRORCNT", stats.SuiteErrorCount.ToString(), completeTestSuite);
                completeTestSuite = ReplaceTag("XMLCLSFAILCNT", stats.SuiteFailCount.ToString(), completeTestSuite);
                completeTestSuite = ReplaceTag("XMLCLSCNT", stats.ClassCount.ToString(), completeTestSuite);
                completeTestSuite = ReplaceTag("XMLTOTALCNT", stats.GlobalPassCount.ToString(), completeTestSuite);
                completeTestSuite = ReplaceTag("XMLCLSNAME", currentClass.Name, completeTestSuite);
                completeTestSuite = ReplaceTag("XMLFULLCLSNAME", currentClass.FullName, completeTestSuite);
                completeTestSuite = ReplaceTag("XMLTOTALCNT", stats.SuiteTestCount.ToString(), completeTestSuite);
                completeTestSuite = ReplaceTag("XMLTOTALPASSCNT", stats.SuitePassCount.ToString(), completeTestSuite);
                completeTestSuite = ReplaceTag("XMLCLSSTART", stats.StartDateTimeAllTests.ToString(CultureInfo.InvariantCulture), completeTestSuite);
                completeTestSuite = ReplaceTag("XMLTOTALTIME", stats.GlobalTime.TotalSeconds.ToString(CultureInfo.InvariantCulture), completeTestSuite);

                // Add in all test cases
                completeTestSuite = ReplaceTag("XMLALLTESTCASES", allTestCase, completeTestSuite);
                allTestSuites = allTestSuites + completeTestSuite;
                allTestCase = string.Empty;
            }
        }

        public void SetTestSuiteDllException(Stats stats,string dllname)
        {
            var completeTestSuite = ReplaceTag("XMLCOMPUTER", Environment.MachineName, skelTestSuiteWrapper);  
            completeTestSuite = ReplaceTag("XMLCLSERRORCNT", stats.SuiteErrorCount.ToString(), completeTestSuite);
            completeTestSuite = ReplaceTag("XMLCLSFAILCNT", stats.SuiteFailCount.ToString(), completeTestSuite);
            completeTestSuite = ReplaceTag("XMLCLSCNT", stats.ClassCount.ToString(), completeTestSuite);
            completeTestSuite = ReplaceTag("XMLTOTALCNT", stats.GlobalPassCount.ToString(), completeTestSuite);
            completeTestSuite = ReplaceTag("XMLCLSNAME", dllname, completeTestSuite);
            completeTestSuite = ReplaceTag("XMLFULLCLSNAME", dllname, completeTestSuite);
            completeTestSuite = ReplaceTag("XMLTOTALCNT", stats.SuiteTestCount.ToString(), completeTestSuite);
            completeTestSuite = ReplaceTag("XMLTOTALPASSCNT", stats.SuitePassCount.ToString(), completeTestSuite);
            completeTestSuite = ReplaceTag("XMLCLSSTART", stats.StartDateTimeAllTests.ToString(CultureInfo.InvariantCulture), completeTestSuite);
            completeTestSuite = ReplaceTag("XMLTOTALTIME", stats.GlobalTime.TotalSeconds.ToString(CultureInfo.InvariantCulture), completeTestSuite);  

            // Add in all test cases
            completeTestSuite = ReplaceTag("XMLALLTESTCASES",allTestCase,completeTestSuite);
            allTestSuites = allTestSuites + completeTestSuite;
            allTestCase = string.Empty;
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
                var testResult = ReplaceTag("XMLTESTNAME", method.Name, skelTestCase);
                testResult = ReplaceTag("XMLTIMETAKEN", stats.LocalTime.TotalSeconds.ToString(CultureInfo.InvariantCulture), testResult);
                testResult = ReplaceTag("XMLCLSNAME", method.DeclaringType.FullName, testResult);
                testResult = ReplaceTag("XMLERRORORFAILURE", skelPassed, testResult);
                testResult = ReplaceTag("XMLSTDOUT", stdout.ToString(), testResult);
                allTestCase = allTestCase + testResult;
            }
        }

        /// <summary>
        /// Set up XML for a failed test result
        /// </summary>
        /// <param name="method">method details</param>
        /// <param name="stats">stats for current test</param>
        /// <param name="ex">Exception that happen in the </param>
        /// <param name="stdout"></param>
        public void SetTestFailed(MethodInfo method, Stats stats, Exception ex, StringWriter stdout)
        {
            lock (thisLock)
            {
                var testResult = ReplaceTag("XMLTESTNAME", method.Name, skelTestCase);
                testResult = ReplaceTag("XMLTIMETAKEN", stats.LocalTime.TotalSeconds.ToString(CultureInfo.InvariantCulture), testResult);
                testResult = ReplaceTag("XMLCLSNAME", method.DeclaringType.FullName, testResult);
                SetFailureMessage(ex, stdout, testResult);
            }
        }

        /// <summary>
        /// Set the test failed when the class has thrown exception
        /// </summary>
        /// <param name="currentClass"></param>
        /// <param name="stats"></param>
        /// <param name="ex">exception from class</param>
        /// <param name="stdout"></param>
        public void SetTestFailedException(Type currentClass, Stats stats, Exception ex, StringWriter stdout)
        {
            lock (thisLock)
            {
                var testResult = ReplaceTag("XMLTESTNAME", currentClass.Name, skelTestCase);
                testResult = ReplaceTag("XMLTIMETAKEN", stats.LocalTime.TotalSeconds.ToString(CultureInfo.InvariantCulture), testResult);
                testResult = ReplaceTag("XMLCLSNAME", currentClass.FullName, testResult);
                SetFailureMessage(ex, stdout, testResult);
            }
        }

        public void SetDllException(string dllname,Stats stats, Exception ex)
        {
            lock (thisLock)
            {
                StringWriter stdout = new StringWriter();
                var testResult = ReplaceTag("XMLTESTNAME", dllname, skelTestCase);
                testResult = ReplaceTag("XMLTIMETAKEN", stats.LocalTime.TotalSeconds.ToString(CultureInfo.InvariantCulture), testResult);
                testResult = ReplaceTag("XMLCLSNAME", dllname, testResult);
                SetFailureMessage(ex, stdout, testResult);
            }
        }


        /// <summary>
        /// Set the exception message
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="stdout"></param>
        /// <param name="testResult"></param>
        private void SetFailureMessage(Exception ex, StringWriter stdout, string testResult)
        {
            lock (thisLock)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.GetType() == typeof(AssertFailedException))
                    {
                        var excep = ex.InnerException.Message.Replace('<', ' ');
                        excep = excep.Replace('>', ' ');
                        excep = excep.Replace('"', ' ');
                        var failureResult = ReplaceTag("XMLMESSAGE", excep, skelFailure);
                        failureResult = ReplaceTag("XMLSTACKTRACE", "", failureResult);
                        failureResult = ReplaceTag("XMLSTDOUT", stdout.ToString(), failureResult);
                        stdout.Flush();
                        testResult = ReplaceTag("XMLERRORORFAILURE", failureResult, testResult);
                    }
                    else
                    {
                        var excep = ex.InnerException.Message.Replace('<', ' ');
                        excep = excep.Replace('>', ' ');
                        excep = excep.Replace('"', ' ');
                        var errorResult = ReplaceTag("XMLMESSAGE", excep, skelFailure);
                        var stackTrace = ex.StackTrace.Replace('<', ' ');
                        stackTrace = stackTrace.Replace('>', ' ');
                        stackTrace = stackTrace.Replace('"', ' ');
                        errorResult = ReplaceTag("XMLSTACKTRACE", stackTrace, errorResult);
                        errorResult = ReplaceTag("XMLSTDOUT", stdout.ToString(), errorResult);
                        stdout.Flush();
                        testResult = ReplaceTag("XMLERRORORFAILURE", errorResult, testResult);
                    }
                }
                else
                {
                    var excep = ex.Message.Replace('<', ' ');
                    excep = excep.Replace('>', ' ');
                    excep = excep.Replace('"', ' ');
                    var errorResult = ReplaceTag("XMLMESSAGE", excep, skelFailure);
                    var stackTrace = ex.StackTrace.Replace('<', ' ');
                    stackTrace = stackTrace.Replace('>', ' ');
                    stackTrace = stackTrace.Replace('"', ' ');
                    errorResult = ReplaceTag("XMLSTACKTRACE", stackTrace, errorResult);
                    errorResult = ReplaceTag("XMLSTDOUT", stdout.ToString(), errorResult);
                    stdout.Flush();
                    testResult = ReplaceTag("XMLERRORORFAILURE", errorResult, testResult);
                }

                allTestCase = allTestCase + testResult;
            }
        }
    }
}
