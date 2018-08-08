using System;
using System.IO;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace VSS.TRex.Designs.TTM.Optimised
{
  public class TrimbleTINModel
  {
    public TriVertices Vertices { get; set; } = new TriVertices();
    public Triangles Triangles { get; set; } = new Triangles();

    public TTMEdges Edges { get; } = new TTMEdges();
    public TTMStartPoints StartPoints { get; } = new TTMStartPoints();

    public TTMHeader Header = TTMHeader.NewHeader();

    public string ModelName { get; set; }

    public TrimbleTINModel()
    {
    }

    public void Read(BinaryReader reader)
    {
      string LoadErrMsg = "";

      try
      {
        LoadErrMsg = "Error reading header";

        Header.Read(reader);

        // Commented out for now...
        //if (FileSignatureToANSIString(FHeader.FileSignature) != kTTMFileIdentifier)
        //{
        //    Raise ETTMReadError.Create('File is not a Trimble TIN Model.');
        //}

        // Check file version
        if (Header.FileMajorVersion != Consts.TTMMajorVersion
            || Header.FileMinorVersion != Consts.TTMMinorVersion)
        {
          throw new Exception("Unable to read this version of Trimble TIN Model file.");
        }

        // ModelName = (String)(InternalNameToANSIString(fHeader.DTMModelInternalName));
        // Not handled for now
        ModelName = "Reading not implemented";

        LoadErrMsg = "Error reading vertices";
        reader.BaseStream.Position = Header.StartOffsetOfVertices;
        Vertices.Read(reader, Header);

        LoadErrMsg = "Error reading triangles";
        reader.BaseStream.Position = Header.StartOffsetOfTriangles;
        Triangles.Read(reader, Header);

        LoadErrMsg = "Error reading edges";
        reader.BaseStream.Position = Header.StartOffsetOfEdgeList;
        Edges.Read(reader, Header);

        LoadErrMsg = "Error reading start points";
        reader.BaseStream.Position = Header.StartOffsetOfStartPoints;
        StartPoints.Read(reader, Header);
      }
      catch (Exception E)
      {
        throw new Exception(LoadErrMsg + ": " + E.Message);
      }
    }

    public void LoadFromStream(Stream stream)
    {
      using (BinaryReader reader = new BinaryReader(stream))
      {
        Read(reader);
      }
    }

    public void LoadFromFile(string FileName)
    {
      using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(FileName)))
      {
        LoadFromStream(ms);
      }

      // FYI, This method sucks totally - don't use it
      //using (FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read, 2048))
      //{
      //    LoadFromStream(fs);
      //}

      if (ModelName.Length == 0)
      {
        ModelName = Path.ChangeExtension(Path.GetFileName(FileName), "");
      }
    }

    public static bool ReadHeaderFromFile(string FileName, out TTMHeader Header)
    {
      Header = TTMHeader.NewHeader();

      try
      {
        using (FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
          using (BinaryReader reader = new BinaryReader(fs))
          {
            Header.Read(reader);

            // Check signature
            string signature = ASCIIEncoding.ASCII.GetString(Header.FileSignature).Substring(0, Consts.TTMFileIdentifier.Length);
            if (!Consts.TTMFileIdentifier.Equals(signature))
            {
              return false;
            }

            // Check file version
            if (Header.FileMajorVersion != Consts.TTMMajorVersion || Header.FileMinorVersion != Consts.TTMMinorVersion)
            {
              return false;
            }

            return true;
          }
        }
      }
      catch
      {
        return false;
      }
    }
  }
}
