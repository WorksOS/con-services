using System;
using System.Diagnostics;
using System.IO;
using VSS.TRex.Designs.TTM.Optimised;

namespace VSS.TRex.Tools.TTMToSTLConverter
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length != 2)
      {
        Console.WriteLine("TTM2STL Conversion Usage:");
        Console.WriteLine("dotnet <appname> <text|binary> <TTMFileName>");
        return;
      }

      try
      {
        string type = args[0].ToLower();
        string fileName = args[1];
        string outputFileName = fileName + ".stl";

        TrimbleTINModel tin = new TrimbleTINModel();
        tin.LoadFromFile(fileName);

        Console.WriteLine("Starting conversion...");
        DateTime startTime = DateTime.Now;

        using (FileStream stl = new FileStream(outputFileName, FileMode.CreateNew))
        {
          if (type == "text")
          {
            using (StreamWriter writer = new StreamWriter(stl))
            {
              writer.WriteLine($"solid {fileName}");

              foreach (var tri in tin.Triangles.Items)
              {
                writer.WriteLine("facet normal 0 0 0");
                writer.WriteLine("outer loop");
                foreach (int vertex in new[] {tri.Vertex0, tri.Vertex1, tri.Vertex2})
                  writer.WriteLine($"Vertex {tin.Vertices.Items[vertex].X} {tin.Vertices.Items[vertex].Y} {tin.Vertices.Items[vertex].Z}");
                writer.WriteLine("endloop");
                writer.WriteLine("endfacet");
              }

              writer.WriteLine($"endsolid {fileName}");
            }
          }

          if (type == "binary")
          {
            using (BinaryWriter writer = new BinaryWriter(stl))
            {
              // Header
              byte[] buffer = new byte[80];
              writer.Write(buffer);

              // Number of triangles
              writer.Write((int) tin.Triangles.Items.Length);
              foreach (var tri in tin.Triangles.Items)
              {
                // Normal vector
                writer.Write((int) 0);
                writer.Write((int) 0);
                writer.Write((int) 0);

                // Vertices
                foreach (int vertex in new[] {tri.Vertex0, tri.Vertex1, tri.Vertex2})
                {
                  writer.Write(tin.Vertices.Items[vertex].X);
                  writer.Write(tin.Vertices.Items[vertex].Y);
                  writer.Write(tin.Vertices.Items[vertex].Z);
                }
              }

              // Attribute byte count
              writer.Write((ushort) 0);
            }
          }
        }

        Console.WriteLine($"Conversion complete in {DateTime.Now - startTime}");
        Console.WriteLine($"{fileName} [{new FileInfo(fileName).Length} bytes] -> {outputFileName} [{new FileInfo(outputFileName).Length} bytes]");
      }
      catch (Exception e)
      {
        Console.WriteLine($"Error: {e.Message}");
      }

      Console.WriteLine("Press any key to continue");
      Console.ReadLine();
    }
  }
}
