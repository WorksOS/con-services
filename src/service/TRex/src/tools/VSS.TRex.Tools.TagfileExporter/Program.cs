using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.Logging;
using VSS.TRex.TAGFiles.Classes;
using VSS.TRex.TAGFiles.Classes.Processors;
using VSS.TRex.TAGFiles.Classes.Sinks;
using VSS.TRex.TAGFiles.Classes.States;

namespace VSS.TRex.Tools.TagfileExporter
{
  class Program
  {
    private static readonly string[] DEFAULT_DATA_LIST = { "DataTime", "DataLeft", "DataRight" };
    private static ILogger Log;

    static void Main(string[] args)
    {
      Log = Logger.CreateLogger<Program>();
      CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
      CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
      DIBuilder.New().AddLogging().Complete();
      string[] dataList = null;
      try
      {
        if (0 < args.Length)
        {
          if (args[0].Equals("-a", StringComparison.InvariantCultureIgnoreCase))
          {
            PrintAttributes();
          }
          else if (Directory.Exists(args[0]))
          {
            var directoryToProcess = args[0];
            if (1 < args.Length)
            {
              dataList = new ArraySegment<string>(args, 1, args.Length - 1).ToArray();
            }

            ProcessTagFiles(directoryToProcess, dataList);

          }
          else
          {
            PrintHelp();
          }
        }
        else
        {
          PrintHelp();
        }
      }
      catch (Exception e)
      {
        Log.LogCritical(e.ToString());
        Console.WriteLine("An error occured processing input please refer to log file for more details");
        PrintHelp();
      }
    }

    private static void ProcessTagFiles(string directoryToProcess, string[] dataList)
    {
      if (dataList == null)
      {
        Console.WriteLine($"No specific tag attributes requested, using defaults of {DEFAULT_DATA_LIST}");
      }
      var files = new List<string>();
      CollectTagFilesInFolder(directoryToProcess, files);

      TAGReader reader = null;
      FileStream fs = null;
      CSVTAGProcessor stateBase = null;
      foreach (var filename in files)
      {
        // Create the TAG file and reader classes
        var file = new TAGFile();
        
        try
        {
          fs = new FileStream(Path.GetFullPath(filename), FileMode.Open, FileAccess.Read);
          reader = new TAGReader(fs);

          // Create the state and sink
          stateBase = new CSVTAGProcessor(dataList ?? DEFAULT_DATA_LIST, Path.GetFullPath(filename));
          var sink = new TAGValueSink(stateBase);

          //Read the TAG file
          var result = file.Read(reader, sink);
          Console.WriteLine($"File {filename} processed with result {result}");
        }
        finally
        {
          fs?.Dispose();
          reader?.Dispose();
          stateBase?.Dispose();
        }

      }

      Console.WriteLine("Complete");
    }

    private static void PrintHelp()
    {
      var helpText = "Usage: TagFileExporter TAG_FOLDER [ATTRIBUTES] \n" +
                     "To display available attributes use TagFileExporter -a ";
      Console.Write(helpText);
    }

    private static void PrintAttributes()
    {
      var builder = new StringBuilder();
      builder.AppendLine("*********** Available attributes ************** ");
      foreach (var property in typeof(TAGProcessorStateBase).GetProperties())
      {
        builder.AppendLine(property.Name);
      }

      foreach (var field in typeof(TAGProcessorStateBase).GetFields())
      {
        builder.AppendLine(field.Name);
      }
      Console.WriteLine(builder);
    }

    private static void CollectTagFilesInFolder(string path, List<string> fileNamesFromFolders)
    {
      // If it is a single file, just include it
      if (File.Exists(path))
      {
        fileNamesFromFolders.Add(path);
      }
      else
      {
        foreach (var f in Directory.GetDirectories(path))
          CollectTagFilesInFolder(f, fileNamesFromFolders);

        fileNamesFromFolders.AddRange(Directory.GetFiles(path, "*.tag"));
      }
    }

  }
}
