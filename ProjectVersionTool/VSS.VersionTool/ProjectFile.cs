using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace VSS.VersionTool
{
  public class ProjectFile
  {
    private ProjectFile()
    {
      References = new List<Reference>();
      ProjectReferences = new List<string>();
      Tags = new List<string>();
    }

    public string PackageName { get; set; }
    public string AssemblyName { get; set; }
    public VersionNumber Version { get; set; }

    public string Filename { get; set; }

    public List<Reference> References { get; }

    public List<string> ProjectReferences { get; }

    public List<string> Tags { get; }

    public override string ToString()
    {
      return $"{AssemblyName} - {PackageName} ({Version})";
    }

    public static ProjectFile Create(string filename)
    {

      var res = new ProjectFile
      {
        Filename = filename
      };

      var xmlDoc = new XmlDocument(); // Create an XML document object
      try
      {
        xmlDoc.Load(filename); // Load the XML document from the specified file
      }
      catch (Exception e)
      {
        //Console.WriteLine($"Failed to parse {filename}, {e}");
        return null;
      }

      var references = xmlDoc.GetElementsByTagName("PackageReference");
      foreach (XmlNode reference in references)
      {
        var name = reference.Attributes["Include"].Value;
        var version = "undefined";
        if (reference.Attributes["Version"] != null)
        {
          version = reference.Attributes["Version"].Value;
        }
        else
        {
          // Do we have a nested version
          var nested = reference["Version"];
          if (nested != null)
            version = nested.InnerText;
        }

        res.References.Add(new Reference(name, version));
      }

      var projectReferences = xmlDoc.GetElementsByTagName("ProjectReference");
      foreach (XmlNode reference in projectReferences)
      {
        var name = reference.Attributes["Include"].Value;
        res.ProjectReferences.Add(Path.GetFileNameWithoutExtension(name));
      }

      var versions = xmlDoc.GetElementsByTagName("VersionPrefix");
      res.Version = new VersionNumber(versions.Count == 1 ? versions[0].InnerText : "undefined");

      res.PackageName = Path.GetFileNameWithoutExtension(filename);

      var assemblyName = xmlDoc.GetElementsByTagName("AssemblyName");
      res.AssemblyName = assemblyName.Count == 1 ? assemblyName[0].InnerText : Path.GetFileNameWithoutExtension(filename);

      var tags = xmlDoc.GetElementsByTagName("PackageTags");
      if (tags.Count == 1)
      {
        foreach (var tag in tags[0].InnerText.Split(";"))
        {
          res.Tags.Add(tag);
        }
      }

      return res;
    }
  }
}