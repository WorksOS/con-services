using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TestRun
{
  public class TestControl
  {
    private string currentClassName = string.Empty;
    private string currentPath = string.Empty;
    private string dllpath = string.Empty;
    private string resultsFileName = string.Empty;
    private string singleClassName = string.Empty;
    private string projectName = string.Empty;
    private string dllName = string.Empty;
    private readonly Stats stats = new Stats();
    private int parallelTasks = 1;
    private bool isStdoutCaptured = true;
    private bool isParallel;
    private ResultsMstest resultsMsTest;
    private ResultsJunit resultsJunit;


    /// <summary>
    /// Runs the tests.
    /// </summary>
    public bool RunAllTests(string[] arguments)
    {
      try
      {
        if (!ParseArguments(arguments))
        {
          Console.WriteLine("Failed to parse arguments");
          return false;
        }

        SetFullPathsToDlls();

        var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dllpath);
        var classes = assembly.GetTypes();
        if (!classes.Any())
          return true;
        resultsMsTest = new ResultsMstest();
        resultsJunit = new ResultsJunit();

        var isAllPassed = GetAllClassesInAssemblyAndRunTests(assembly, classes);

        WriteResultFile();
        return isAllPassed;
      }
      catch (Exception ex)
      {
        Console.WriteLine("  An unexpected error occured: {0}", ex.Message);
        if (ex.InnerException != null)
        {
          Console.WriteLine("  Reason: {0}", ex.InnerException.Message);
        }
        WriteDllException(ex);
        return false;
      }
    }

    /// <summary>
    /// Set up the full path names to the dll's
    /// </summary>
    private void SetFullPathsToDlls()
    {
      currentPath = Directory.GetCurrentDirectory();
      if (Debugger.IsAttached)
      {
        currentPath =
            currentPath.Substring(0, currentPath.IndexOf(@"utilities", StringComparison.Ordinal)) +
            @"deploy";
      }

      resultsFileName = currentPath + resultsFileName;
      var currentDir = currentPath + projectName;
      Directory.SetCurrentDirectory(currentDir);
      LoadNecessaryDlls();
      dllpath = $"{currentDir}{Path.DirectorySeparatorChar}{dllName}";
    }

    /// <summary>
    /// Parse all the arugments 
    /// </summary>
    /// <param name="arguments"></param>
    private bool ParseArguments(string[] arguments)
    {
      try
      {
        foreach (var param in arguments)
        {
          Console.WriteLine(param);
          var lowparam = param.ToLower();
          if (lowparam.Contains("results="))
          {
            resultsFileName = lowparam.Substring(8).Trim();
          }
          if (lowparam.Contains("messages=false"))
          {
            isStdoutCaptured = false;
          }

          if (lowparam.Contains("threads="))
          {
            parallelTasks = Convert.ToInt32(lowparam.Substring(8));
            if (parallelTasks > 20)
            {
              parallelTasks = 1;
            }
            isParallel = true;
            Console.WriteLine("Number of tests in parallel: " + parallelTasks + "  is parallel run " + isParallel);
          }
          if (lowparam.Contains("class="))
          {
            singleClassName = lowparam.Substring(6);
          }
          if (lowparam.Contains("project="))
          {
            projectName = param.Substring(8).Trim();
            dllName = projectName + ".dll";
            projectName = '/' + projectName;
          }
        }
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }

    /// <summary>
    /// Write a results file when the DLL won't load
    /// </summary>
    private void WriteDllException(Exception ex)
    {
      if (this.resultsMsTest == null || this.resultsJunit == null)
      {
        return;
      }

      resultsMsTest.InitializeSkeletons();
      resultsJunit.InitializeSkeletons();
      resultsJunit.SetDllException(dllpath, stats, ex);
      resultsJunit.SetTestSuiteDllException(stats, dllpath);
      resultsJunit.SetTestRunTotals(stats);
      WriteResultFile();
    }
    /// <summary>
    /// Write the results file to the 
    /// </summary>
    private void WriteResultFile()
    {
      Write(resultsJunit.wholeJunitFile, resultsFileName + ".xml");
      // Only going to view this in windows so replace lf with cr lf
      var wholeTrxFile = resultsMsTest.wholeTrxFile.Replace("\n", "\r\n");
      Write(wholeTrxFile, resultsFileName + ".trx");
    }

    /// <summary>
    /// Load any dll's that are used by the testing classes
    /// </summary>
    private void LoadNecessaryDlls()
    {
      // Invoke any of the dll's used by the testing framework.
      Thread.Sleep(1);
      _ = DateTime.ParseExact("09:00:00", "HH:mm:ss", CultureInfo.InvariantCulture).TimeOfDay;
      Debug.WriteLine("In Diagnostics");
      _ = new GuidAttribute(Guid.NewGuid().ToString());
      _ = new IPAddress(0x2414188f);
      _ = new Regex("aaa!");
      _ = WebRequest.Create("http://www.google.com");
      _ = new BooleanConverter();
    }

    /// <summary>
    /// Loop through all the classes in the DLL
    /// </summary>
    /// <returns>bool if run successfully</returns>
    private bool GetAllClassesInAssemblyAndRunTests(Assembly assembly, Type[] classes)
    {
      stats.StartDateTimeAllTests = DateTime.Now;
      foreach (var current in classes)
      {
        if (!string.IsNullOrEmpty(singleClassName))
        {
          if (singleClassName != current.Name.ToLower())
          {
            continue;
          }
        }

        try
        {
          currentClassName = current.Name;
          Console.WriteLine(@"#################################################################################################");
          Console.WriteLine(@"########################       Test Class : " + currentClassName + "      #########################");
          Console.WriteLine(@"#################################################################################################");
          var methods = current.GetMethods()
              .Where(m => m.GetCustomAttributes(typeof(TestMethodAttribute)).Count() != 0)
              .ToList();

          if (!methods.Any())          // No methods --- Have they been tagged with TestMethod
          {
            Console.WriteLine(currentClassName + " - No Test Methods");
            continue;
          }

          var instance = assembly.CreateInstance(current.FullName);
          RunAnyTestInitializeMethods(current, instance);
          RunEachTestInTheClass(methods, instance);
          stats.GetFinalResult();
          stats.EndDateTimeAllTests = DateTime.Now;
          resultsJunit.SetTestSuiteTotals(stats, current);
          stats.ResetSuiteCount();
          stats.AddClassCount();
        }
        catch (Exception ex)
        {
          var consoleOut = new StringWriter();
          if (isStdoutCaptured)
          { Console.SetOut(consoleOut); }
          Console.WriteLine("An unexpected error occured: " + ex.Message);
          if (ex.InnerException != null)
          { Console.WriteLine("Reason: " + ex.InnerException.Message); }
          resultsJunit.SetTestFailedException(current, stats, ex, consoleOut);
          resultsJunit.SetTestSuiteTotals(stats, current);

        }
      }
      stats.EndDateTimeAllTests = DateTime.Now;
      resultsMsTest.SetTestRunTotals(stats);
      resultsJunit.SetTestRunTotals(stats);
      var stdOut = Console.Out;
      if (isStdoutCaptured)
      {
        Console.SetOut(stdOut);
        stdOut.Flush();
      }

      Console.WriteLine("Total tests: " + stats.GlobalTestCount + "   Tests Passed: " + stats.GlobalPassCount + "   Tests Failed: " + stats.GlobalFailCount);
      return stats.GlobalFailCount == 0;
    }

    /// <summary>
    /// If there are any TestInitialize then do these
    /// </summary>
    /// <param name="current"></param>
    /// <param name="instance"></param>
    private void RunAnyTestInitializeMethods(Type current, object instance)
    {
      var initializeMethod = current.GetMethods().FirstOrDefault(m => m.GetCustomAttributes(typeof(TestInitializeAttribute)).Any());
      if (initializeMethod != null)
      {
        try
        {
          Console.WriteLine("TestInitialize method: " + initializeMethod.Name);
          initializeMethod.Invoke(instance, null);
          Console.WriteLine("TestInitialize method: " + initializeMethod.Name + " - Successfully completed ");
        }
        catch (Exception ex)
        {
          Console.WriteLine("TestInitialize method: " + initializeMethod.Name + " - Exception:" + ex.InnerException);
        }
      }
    }

    /// <summary>
    /// Run the unit test / method for all the methods in the class
    /// </summary>
    /// <param name="methods">All the methods in the class and their info</param>
    /// <param name="instance">Instance of the class/dll</param>
    private void RunEachTestInTheClass(List<MethodInfo> methods, object instance)
    {
      if (isParallel)
      {

        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = parallelTasks };
        Parallel.ForEach(methods, parallelOptions, method =>
        {
          RunOneMethodAndOutputResults(instance, method);
        });
      }
      else
      {
        foreach (var method in methods)
        {
          RunOneMethodAndOutputResults(instance, method);
        }
      }
    }

    /// <summary>
    /// Run one test method in the class and write results to the string.
    /// </summary>
    private void RunOneMethodAndOutputResults(object instance, MethodInfo method)
    {
      if (method.GetCustomAttributes(typeof(IgnoreAttribute)).Count() != 0)
      {
        Console.WriteLine("IGNORE TestMethod: " + method.Name);
        return;
      }
      // Set the console messages to be in the file
      Console.WriteLine(@"=================================================================================================");
      Console.WriteLine("Test: " + method.Name);
      Console.WriteLine(@"=================================================================================================");
      var consoleOut = new StringWriter();
      if (isStdoutCaptured)
      { Console.SetOut(consoleOut); }

      try
      {
        var dataRowAttributes = method.GetCustomAttributes(typeof(DataRowAttribute)).ToList();

        if (dataRowAttributes.Any())
        {
          foreach (var attribute in dataRowAttributes)
          {
            consoleOut = ExecuteMethodTest(instance, method, ((DataRowAttribute)attribute).Data);
          }
        }
        else
        {
          consoleOut = ExecuteMethodTest(instance, method, null);
        }
      }
      catch (Exception ex)
      {
        stats.EndDateTimeOneTest = DateTime.Now;
        // If assertion the it is a failure otherwise an error
        if (ex.InnerException != null && ex.InnerException.GetType() == typeof(AssertFailedException))
        {
          stats.AddSuiteFailCount();
        }
        else
        {
          stats.AddSuiteErrorCount();
        }
        resultsMsTest.SetTestFailed(method, stats, ex, consoleOut);
        resultsJunit.SetTestFailed(method, stats, ex, consoleOut);
        consoleOut.Flush();

        var stdOut = Console.Out;
        if (isStdoutCaptured)
        {
          Console.SetOut(stdOut);
        }

        Console.WriteLine(@"=================================================================================================");
        Console.WriteLine("Test : " + method.Name + " ***********  FAILED ************ ");
        Console.WriteLine(@"=================================================================================================");
      }
      finally
      {
        stats.ResetLocalTime();
      }
    }

    private StringWriter ExecuteMethodTest(object instance, MethodInfo method, object[] methodParams)
    {
      // Set the console messages to be in the file
      Console.WriteLine(@"=================================================================================================");
      Console.WriteLine("Test: " + method.Name);
      Console.WriteLine(@"=================================================================================================");
      var consoleOut = new StringWriter();
      if (isStdoutCaptured)
      { Console.SetOut(consoleOut); }

      stats.StartDateTimeOneTest = DateTime.Now;
      resultsMsTest.SetTestInitialize(method);
      stats.AddSuiteCount();
      stats.AddGlobalCount();
      stats.StartLocalTime();
      method.Invoke(instance, methodParams);
      stats.AddSuitePassCount();
      stats.EndDateTimeOneTest = DateTime.Now;

      resultsMsTest.SetTestPassed(method, stats, consoleOut);
      resultsJunit.SetTestPassed(method, stats, consoleOut);
      var stdOut = Console.Out;
      Console.SetOut(stdOut);
      Console.WriteLine(@"=================================================================================================");
      Console.WriteLine("Test : " + method.Name + " - PASSED");
      Console.WriteLine(@"=================================================================================================");

      return consoleOut;
    }

    /// <summary>
    /// Write to a test results file
    /// </summary>
    /// <param name="message">test results to be logged</param>
    /// <param name="logfile">log file name</param>
    private void Write(string message, string logfile)
    {
      using (var w = File.AppendText(logfile))
      {
        w.Write(message);
      }
    }
  }
}
