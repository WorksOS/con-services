using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using System.Threading;

namespace Punisher
{
  class Program
  {
    static int requestNumber = 0;

    class RequestExecutor
    {
      static public void executeRequest(RestClient client, int id, string url, string body)
      {
        if (String.IsNullOrEmpty(body))
        {
          var uri = new Uri(url);
          var request = new RestRequest(uri.AbsolutePath, Method.GET);
          client.Execute(request);
        }
        else
        {
          var uri = new Uri(url);
          if (uri.AbsolutePath.Contains("/tiles") ||  //uri.AbsolutePath.Contains("tiles/png") || - Tiles/png redundant given check for 'tiles'
              uri.AbsolutePath.Contains("cells/passes") ||
              uri.AbsolutePath.Contains("productiondata/patches") ||
              uri.AbsolutePath.Contains("volumes/summary") ||
              uri.AbsolutePath.Contains("projects/statistics") ||
              uri.AbsolutePath.Contains("compaction/cmv/summary") ||
              uri.AbsolutePath.Contains("compaction/cmv/detailed") ||
              uri.AbsolutePath.Contains("compaction/passcounts/detailed") ||
              uri.AbsolutePath.Contains("compaction/passcounts/summary") ||
              uri.AbsolutePath.Contains("productiondata/edit") ||
              uri.AbsolutePath.Contains("productiondata/getedits") ||
              // uri.AbsolutePath.Contains("designcache/delete") || Let's keep the design files around for now...
              uri.AbsolutePath.Contains("projectextents")
              )
          {
            var request = new RestRequest(uri.AbsolutePath, Method.POST);
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            client.Execute(request);
          }
          else
          {
            Console.WriteLine("Request with path {0} not sent to service", uri.AbsolutePath);
          }
        }

        int number = Interlocked.Increment(ref requestNumber);
        if (number % 500 == 0)
        {
          Console.WriteLine("[{0}]: Submitted request #{1}", id, number);
        }
      }

      static public void addRequest(RestClient client, int id, string url, string body)
      {
        executeRequest(client, id, url, body);
      }
    }

    static void doSubmissionOnThread(int id, string serverURL, string logFileName)
    {
      var client = new RestClient(serverURL); // "http://localhost:60012"
      var linkParser = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

      int processedLines = 0;
      int processedRequests = 0;

      using (var sr = new StreamReader(logFileName))
      {
        while (!sr.EndOfStream)
        {
          var line = sr.ReadLine();

          processedLines++;
          if (processedLines % 10000 == 0) // Only write every 10000th line
            Console.Write("\r[{0}]: Lines read: {1}", id, processedLines);

          if (String.IsNullOrEmpty(line))
            continue;
          if (line.Contains("TRUNCATED"))
            continue;

          //GET
          if (line.IndexOf(":GET http:", StringComparison.InvariantCulture) > 0)
            foreach (Match m in linkParser.Matches(line))
            {
              RequestExecutor.addRequest(client, id, m.ToString(), null);
              processedRequests++;
            }

          //POST
          if (line.IndexOf(":POST http:", StringComparison.InvariantCulture) > 0)
          {
            string url = "";
            foreach (Match m in linkParser.Matches(line))
              url = m.ToString();

            string body = "";
            int numOfOpenBrackets = 0;
            int numOfCloseBrackets = 0;

            var idxCurlyOpen = line.IndexOf("{", StringComparison.InvariantCulture);
            if (idxCurlyOpen < 0) continue;

            numOfOpenBrackets += line.Count(x => x == '{');
            numOfCloseBrackets += line.Count(x => x == '}');
            body += line.Substring(idxCurlyOpen);

            while (numOfCloseBrackets != numOfOpenBrackets)
            {
              line = sr.ReadLine();

              numOfOpenBrackets += line.Count(x => x == '{');
              numOfCloseBrackets += line.Count(x => x == '}');
              body += line;
            }

            if (body.Contains("success"))
              continue;

            RequestExecutor.addRequest(client, id, url, body);
            processedRequests++;
          }
        }
      }

      Console.Write("\r[{0}]: Lines read: {1}", id, processedLines);
      Console.WriteLine("[{0}]: Requests processed: {1}", id, processedRequests);

      //      File.WriteAllText("punish.json", JsonConvert.SerializeObject(requests));        
    }

    static void doWork(Object o)
    {
      var args = (Tuple<int, string, string[]>)o;

      foreach (string filename in args.Item3)
      {
        Console.WriteLine("[{0}]: Submitting requests from file {1}", args.Item1, filename);

        doSubmissionOnThread(args.Item1, args.Item2, filename);
      }
    }

    static void submitRequestsFromFileList(string serverURL, string[] files, int instanceCount)
    {
      var threads = Enumerable.Range(1, instanceCount).Select(x => { var y = new Thread(doWork); y.Start(new Tuple<int, string, string[]>(x, serverURL, files)); return y; }).ToArray();

      foreach (var thread in threads)
        thread.Join();
    }

    static void Main(string[] args)
    {
      if (args.Length < 3)
      {
        Console.WriteLine("Usage: Punisher <serverURL> <logFileName> <instanceCount>");
        return;
      }
      string serverURL = args[0];
      string logFileName = args[1];

      int instanceCount = 1;
      if (!Int32.TryParse(args[2], out instanceCount))
        instanceCount = 1;

      //            Console.Write("Hit enter key to start...");
      //            var keyword = Console.ReadLine() ?? "";

      Console.WriteLine("\rStarting....");

      // Get lists of log files from productionDataLogFileName argument and pass them to submitter method
      string filePath = String.Join("\\", logFileName.Split('\\').Take(logFileName.Split('\\').Length - 1));
      string fileName = logFileName.Split('\\').LastOrDefault();

      submitRequestsFromFileList(serverURL, Directory.GetFiles(filePath, fileName), instanceCount);
    }
  }
}
