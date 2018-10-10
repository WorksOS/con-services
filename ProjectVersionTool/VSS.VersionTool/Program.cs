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
      
      TagsCommand(app, directories, isRecursive);

      ReferencesCommand(app, directories, isRecursive);

      app.Execute(args);
    }

    private static void TagsCommand(CommandLineApplication app, CommandOption directories, CommandOption isRecursive)
    {
      app.Command("tags", (c) =>
      {
        c.Description = "Find all tags inside the Project Metadata";
        c.HelpOption("-?|-h|--help");
        c.Options.Add(directories);
        c.Options.Add(isRecursive);
        var searchReferences = c.Option("--references-directory",
          "Search for any projects under this directory that reference a found project using the -d parameters (useful for building a dependency graph)",
          CommandOptionType.SingleValue);
        var searchUp = c.Option("--search-up-directory",
          "Search up directories until a project is found. Useful for when the directory changed is a folder under the project.",
          CommandOptionType.NoValue);

        c.OnExecute(() =>
        {
          if (!directories.HasValue())
          {
            Console.WriteLine("Needs at least one directory");
            c.ShowHelp();
            return 1;
          }
          
          var projects = GetProjectFiles(directories.Values, isRecursive.HasValue(), searchUp.HasValue()).ToList();
          var tags = projects.SelectMany(p => p.Tags).Distinct().ToList();
          // check for references if we need
          // This involves searching over all of the projects inside the repo, and seeing if they use any references
          // Updating the list of references and repeating
          // Until we eventually get to the projects that aren't referenced (the services)
          if (searchReferences.HasValue())
          {
            var referencesToSearch = projects.Select(p => p.PackageName).ToList();
            var allProjects = GetProjectFiles(searchReferences.Values, true, false).ToList();
            var completedReferences = new List<string>();
            while (referencesToSearch.Count > 0)
            {
              var newReferences = new List<string>();
              foreach (var project in allProjects)
              {
                // check for any references
                if (project.References.Any(r => referencesToSearch.Contains(r.Name)) || project.ProjectReferences.Any(r => referencesToSearch.Contains(r)))
                {
                  tags.AddRange(project.Tags);
                  if (!completedReferences.Contains(project.PackageName))
                  {
                    newReferences.Add(project.PackageName);
                    completedReferences.Add(project.PackageName);
                  }
                }
              }
              referencesToSearch.Clear();
              referencesToSearch.AddRange(newReferences);
            }
          }

          foreach (var tag in tags.Distinct())
          {
            Console.WriteLine(tag);
          }
          return 0;
        });
      });
    }

    private static void ReferencesCommand(CommandLineApplication app, CommandOption directories, CommandOption isRecursive)
    {
      app.Command("references", c =>
      {
        c.Description = "Find all Projects with a specific reference";
        c.HelpOption("-?|-h|--help");
        c.Options.Add(directories);
        c.Options.Add(isRecursive);
        var isLocalReference = c.Option("-l|--local", "Local Project References (local or package required)",
          CommandOptionType.NoValue);
        var isPackageReference = c.Option("-p|--package", "Package References (local or package required)",
          CommandOptionType.NoValue);
        var names = c.Option("-n|--name", "Reference Name", CommandOptionType.SingleValue);
        var version = c.Option("-pv|--package-version", "Package Reference version, allows wildcard (e.g 1.1.* == 1.1.9)",
          CommandOptionType.MultipleValue);

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
                .FirstOrDefault(
                  r => string.Compare(r.Name, referenceName, StringComparison.InvariantCultureIgnoreCase) == 0);

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
    }


    private static IEnumerable<ProjectFile> GetProjectFiles(IEnumerable<string> directories, bool recursive, bool upSearch = false)
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

        if (upSearch && !files.Any())
        {
          var path = Directory.GetParent(directory);
          if (path != null) // Root for drive if null
          {
            // Create a new list of directories to check containing just our 'up path'.
            foreach (var project in GetProjectFiles(new List<string> {path.FullName}, recursive, upSearch))
            {
              yield return project;
            }
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
