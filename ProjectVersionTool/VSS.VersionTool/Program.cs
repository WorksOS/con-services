using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Extensions.CommandLineUtils;

namespace VSS.VersionTool
{
  class Program
  {
    static void Main(string[] args)
    {
      var app = new Microsoft.Extensions.CommandLineUtils.CommandLineApplication
      {
        Name = "VSS C# Project Tool",
        Description = "A tool to help with parsing a collection of C# projects, and filtering based on the contents."
      };
      app.HelpOption("-?|-h|--help");
      var directories = app.Option("-d|--directory", "Directories to search (allows multiple)", CommandOptionType.MultipleValue);
      var isRecursive = app.Option("-r|--recursive", "Recursive search, defaults to false", CommandOptionType.NoValue);

      app.OnExecute(() =>
      {
          app.ShowHelp();
          return 1;
      });



      app.Command("tags", (c) => {
        c.Description = "Find all tags inside the Project Metadata";
        c.HelpOption("-?|-h|--help");
        c.Options.Add(directories);
        c.Options.Add(isRecursive);
 
        c.OnExecute(() => {
          if (!directories.HasValue())
          {
            Console.WriteLine("Needs at least one directory");
            c.ShowHelp();
            return 1;
          }
          var projects = GetProjectFiles(directories.Values, isRecursive.HasValue());
          var tags = projects.SelectMany(p => p.Tags).Distinct();
          foreach (var tag in tags)
          {
            Console.WriteLine(tag);
          }
          return 0;
        });
      });

      app.Command("references", c =>
      {
        c.Description = "Find all Projects with a specific reference";
        c.HelpOption("-?|-h|--help");
        c.Options.Add(directories);
        c.Options.Add(isRecursive);
        var isLocalReference = c.Option("-l|--local", "Local Project References (local or package required)", CommandOptionType.NoValue);
        var isPackageReference = c.Option("-p|--package", "Package References (local or package required)", CommandOptionType.NoValue);
        var names = c.Option("-n|--name", "Reference Name", CommandOptionType.SingleValue);
        var version = c.Option("-pv|--package-version", "Package Reference version, allows wildcard (e.g 1.1.* == 1.1.9)", CommandOptionType.MultipleValue);

        c.OnExecute(() =>
        {
          if (!directories.HasValue())
          {
            Console.WriteLine("Needs at least one directory");
            c.ShowHelp();
            return 1;
          }

          if (!names.HasValue() && !string.IsNullOrEmpty(names.Values[0]))
          {
            Console.WriteLine("A reference name is needed");
            c.ShowHelp();
            return 1;
          }

          if (!isLocalReference.HasValue() && !isPackageReference.HasValue())
          {
            Console.WriteLine("Need at least one reference type");
            c.ShowHelp();
            return 1;
          }

          if (version.HasValue() && (!isPackageReference.HasValue() || isLocalReference.HasValue()))
          {
            Console.WriteLine("Version matching only supports package references");
            c.ShowHelp();
            return 1;
          }

          var referenceName = names.Values[0];


          var projects = GetProjectFiles(directories.Values, isRecursive.HasValue());
          foreach (var projectFile in projects)
          {
            if (isLocalReference.HasValue())
            {
              var projectReference = projectFile.ProjectReferences.FirstOrDefault(r =>
                string.Compare(r, referenceName, StringComparison.InvariantCultureIgnoreCase) == 0);

              if (projectReference != null)
              {
                Console.WriteLine($"{projectFile.PackageName} ({projectFile.Version})");
                Console.WriteLine($"Location: {projectFile.Filename}");
                Console.WriteLine($"Local Reference: {projectReference}");
                Console.WriteLine();
                continue;
              }
            }

            if (isPackageReference.HasValue())
            {
              var reference = projectFile
                .References
                .FirstOrDefault(r => string.Compare(r.Name, referenceName, StringComparison.InvariantCultureIgnoreCase) == 0);

              if (reference == null) continue;

              if (version.HasValue())
              {
                var v = new VersionNumber(version.Values[0]);
                if (!v.Equals(reference.Version))
                  continue;
              }

              Console.WriteLine($"{projectFile.PackageName} ({projectFile.Version})");
              Console.WriteLine($"Location: {projectFile.Filename}");
              Console.WriteLine($"Package Reference: {reference.Name} ({reference.Version})");
              Console.WriteLine();
              continue;
            }
          }

          return 0;
        });

      });

      app.Execute(args);
    }


    private static IEnumerable<ProjectFile> GetProjectFiles(IEnumerable<string> directories, bool recursive)
    {
      foreach (var directory in directories)
      {
        var files = Directory.GetFiles(directory, "*.csproj", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        foreach (var file in files)
        {
          var result = ProjectFile.Create(file);
          if (result != null)
          {
            yield return result;
          }
        }
      }

      
//
//      foreach (var projectFile in results.Where(r => r.References.Any(a => a.Name == "VSS.Authentication.JWT")))
//      {
//        Console.WriteLine(projectFile);
//      }
    }
  }
}
