using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.TTM
{
  public class TrimbleTINModel : TriangleMesh
  {
    public string ModelName { get; set; }

    public TTMStartPoints StartPoints { get; private set; }

    public TTMEdges Edges { get; private set; }

    public TTMHeader Header = TTMHeader.NewHeader(); 

    public double CoordinateResolution { get; set; }
    public double ElevationResolution { get; set; }

    public bool Loading { get; private set; }


    protected short CalcFloatSize(double MaxValue, double Resolution)
    {
      const int SingleMantissaBits = 24; //  Including implied first bit 

      return (short) (Math.Abs(MaxValue) < (1 << SingleMantissaBits) * Resolution ? sizeof(float) : sizeof(double));
    }

    protected short CalcIntSize(int MaxValue)
    {
      return (short) (Math.Abs(MaxValue) <= Consts.MaxSmallIntValue ? sizeof(short) : sizeof(int));
    }

    protected override void CreateLists(out TriVertices vertices, out Triangles triangles)
    {
      // Note : Do NOT call inherited here, doing so will cause a memory leak as the
      //        Vertices and Triangles lists create in the inherited method are then lost
      //        by being overwritten here.  
      vertices = new TTMVertices();
      triangles = new TTMTriangles();
    }

    protected override void SnapToOutputResolution()
    {
      SetUpSizes();
      (Vertices as TTMVertices).SnapToOutputResolution(Header);
    }

    public TrimbleTINModel() : base()
    {
      Edges = new TTMEdges();
      StartPoints = new TTMStartPoints();
      CoordinateResolution = Consts.DefaultCoordinateResolution;
      ElevationResolution = Consts.DefaultElevationResolution;
    }

    private void SetUpSizes()
    {
      double MinElevation = 0, MaxElevation = 0;

      Vertices.GetLimits(ref Header.MinimumEasting, ref Header.MinimumNorthing, ref MinElevation,
        ref Header.MaximumEasting, ref Header.MaximumNorthing, ref MaxElevation);

      Header.EastingOffsetValue = (Header.MinimumEasting + Header.MaximumEasting) / 2;
      Header.NorthingOffsetValue = (Header.MinimumNorthing + Header.MaximumNorthing) / 2;
      Header.VertexCoordinateSize = (byte) CalcFloatSize(Math.Max(
          Header.MaximumEasting - Header.EastingOffsetValue,
          Header.MaximumNorthing - Header.NorthingOffsetValue),
        CoordinateResolution);
      Header.VertexValueSize = (byte) CalcFloatSize(Math.Max(Math.Abs(MinElevation), Math.Abs(MaxElevation)),
        ElevationResolution);
      Header.VertexNumberSize = (byte) CalcIntSize(Vertices.Count);
      Header.TriangleNumberSize = (byte) CalcIntSize(Triangles.Count);

    }

    public void Read(BinaryReader reader)
    {
      string LoadErrMsg = "";

      Loading = true;
      try
      {
        try
        {
          LoadErrMsg = "Error reading header";

          Header = TTMHeader.NewHeader();
          Header.Read(reader);

          // Commented out for now...
          //if (FileSignatureToANSIString(Header.FileSignature) != kTTMFileIdentifier)
          //{
          //    Raise ETTMReadError.Create('File is not a Trimble TIN Model.');
          //}

          // Check file version
          if (Header.FileMajorVersion != Consts.TTMMajorVersion
              || Header.FileMinorVersion != Consts.TTMMinorVersion)
          {
            throw new Exception($"TTM.Read(): Unable to read this version {Header.FileMajorVersion}: {Header.FileMinorVersion} of Trimble TIN Model file. Expected version: { Consts.TTMMajorVersion}: {Consts.TTMMinorVersion}");
          }

          Clear();

          // ModelName = (String)(InternalNameToANSIString(Header.DTMModelInternalName));
          // Not handled for now
          ModelName = "Reading not implemented";

          LoadErrMsg = "Error reading vertices";
          reader.BaseStream.Position = Header.StartOffsetOfVertices;
          (Vertices as TTMVertices).Read(reader, Header);

          LoadErrMsg = "Error reading triangles";
          reader.BaseStream.Position = Header.StartOffsetOfTriangles;
          (Triangles as TTMTriangles).Read(reader, Header, Vertices);

          LoadErrMsg = "Error reading edges";
          reader.BaseStream.Position = Header.StartOffsetOfEdgeList;
          Edges.Read(reader, Header, Triangles as TTMTriangles);

          LoadErrMsg = "Error reading start points";
          reader.BaseStream.Position = Header.StartOffsetOfStartPoints;
          StartPoints.Read(reader, Header, Triangles);
        }
        catch (Exception E)
        {
          Clear();

          throw new Exception(LoadErrMsg + ": " + E.Message);
        }
      }
      finally
      {
        Loading = false;
      }
    }

    private void PadToBoundary(BinaryWriter writer)
    {
      const int BlockSize = 8;

      byte[] Zero = new byte[BlockSize]; // Will be initialised to zero...

      writer.Write(Zero, 0, (int) (BlockSize - ((writer.BaseStream.Position - 1) % BlockSize) - 1));
    }

    public void Write(BinaryWriter writer)
    {
      // Write a blank header now, and go back later and fix it up
      long HeaderPos = writer.BaseStream.Position;

      Header.FileSignature = ASCIIEncoding.ASCII.GetBytes(Consts.TTMFileIdentifier);
      Header.DTMModelInternalName = ASCIIEncoding.ASCII.GetBytes(ModelName ?? "Un-named Model\0");
      if (Header.DTMModelInternalName.Length != HeaderConsts.kDTMInternalModelNameSize)
      {
        Array.Resize(ref Header.DTMModelInternalName, HeaderConsts.kDTMInternalModelNameSize);
      }

      Header.Write(writer);

      PadToBoundary(writer);

      Header.FileMajorVersion = Consts.TTMMajorVersion;
      Header.FileMinorVersion = Consts.TTMMinorVersion;

      Header.CoordinateUnits = 1; // Metres
      Header.VertexValueUnits = 1; // Metres
      Header.InterpolationMethod = 1; // Linear

      SetUpSizes();

      Header.StartOffsetOfVertices = (int) writer.BaseStream.Position;
      Header.NumberOfVertices = Vertices.Count;
      Header.VertexRecordSize = (short) (2 * Header.VertexCoordinateSize + Header.VertexValueSize);
      (Vertices as TTMVertices).Write(writer, Header);
      PadToBoundary(writer);

      Header.StartOffsetOfTriangles = (int) writer.BaseStream.Position;
      Header.NumberOfTriangles = Triangles.Count;
      Header.TriangleRecordSize = (short) (3 * Header.VertexNumberSize + 3 * Header.TriangleNumberSize);
      Vertices.NumberVertices();
      (Triangles as TTMTriangles).Write(writer, Header);
      PadToBoundary(writer);

      Header.StartOffsetOfEdgeList = (int) writer.BaseStream.Position;
      Header.NumberOfEdgeRecords = Edges.Count();
      Header.EdgeRecordSize = Header.TriangleNumberSize;
      Edges.Write(writer, Header);
      PadToBoundary(writer);

      Header.StartOffsetOfStartPoints = (int) (writer.BaseStream.Position);
      Header.NumberOfStartPoints = StartPoints.Count();
      Header.StartPointRecordSize = (short) (2 * Header.VertexCoordinateSize + Header.TriangleNumberSize);
      StartPoints.Write(writer, Header);

      // Fix up header
      long EndPos = writer.BaseStream.Position;
      writer.BaseStream.Position = HeaderPos;

      Header.Write(writer);

      writer.BaseStream.Position = EndPos;
    }

    public void WriteDefault(BinaryWriter writer)
    {
      if (Triangles.Count > 0)
      {
        BuildTriangleLinks();
        BuildEdgeList();
        BuildStartPointList();
      }

      CoordinateResolution = Consts.DefaultCoordinateResolution;
      ElevationResolution = Consts.DefaultElevationResolution;

      Write(writer);
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

      if (ModelName.Length == 0)
      {
        ModelName = Path.ChangeExtension(Path.GetFileName(FileName), "");
      }
    }

    public void SaveToStream(double CoordinateResolution,
      double ElevationResolution,
      bool BuildEdgeListEtAl,
      Stream stream)
    {
      if (BuildEdgeListEtAl && Triangles.Count > 0)
      {
        BuildTriangleLinks();
        BuildEdgeList();
        BuildStartPointList();
      }

      this.CoordinateResolution = CoordinateResolution;
      this.ElevationResolution = ElevationResolution;

      using (BinaryWriter writer = new BinaryWriter(stream))
      {
        Write(writer);
      }
    }

    public void SaveToFile(string FileName,
      double CoordinateResolution = Consts.DefaultCoordinateResolution,
      double ElevationResolution = Consts.DefaultElevationResolution,
      bool BuildEdgeListEtAl = true)
    {
      using (FileStream fs = new FileStream(FileName, FileMode.CreateNew, FileAccess.Write))
      {
        SaveToStream(CoordinateResolution, ElevationResolution, BuildEdgeListEtAl, fs);
      }
    }

    public void SaveToFile(string FileName,
      bool BuildEdgeListEtAl)
    {
      SaveToFile(FileName,
        Consts.DefaultCoordinateResolution,
        Consts.DefaultElevationResolution,
        BuildEdgeListEtAl);
    }

    public void BuildEdgeList()
    {
      Edges.Clear();
      for (int TriNumber = 0; TriNumber < Triangles.Count; TriNumber++)
      {
        if (Triangles[TriNumber].IsEdgeTriangle())
        {
          Edges.AddTriangle(Triangles[TriNumber] as TTMTriangle);
        }
      }
    }

    public void BuildStartPointList()
    {
      StartPoints.Clear();

      if (Triangles.Count == 0)
      {
        return;
      }

      // How many start points do we want?
      int NumStartPoints = Math.Min(Math.Max((int) Math.Round(Math.Sqrt(Triangles.Count) / 2), 1), Consts.MaxStartPoints);

      // Use the centre points of this number of triangles evenly spaced throughout the job
      int TriNumOffset = (Triangles.Count / NumStartPoints) / 2;
      for (int StPtNum = 0; StPtNum < NumStartPoints; StPtNum++)
      {
        int TriangleNum = (StPtNum * Triangles.Count) / NumStartPoints + TriNumOffset;

        Debug.Assert(TriangleNum >= 0);

        XYZ Centroid = Triangles[TriangleNum].Centroid();
        StartPoints.Add(new TTMStartPoint(Centroid.X, Centroid.Y, Triangles[TriangleNum]));
      }
    }


    public override void Clear()
    {
      base.Clear();

      Edges.Clear();
      StartPoints.Clear();
      ModelName = "";
    }

    // procedure RemoveDuplicateTriangles;

    public static bool IsTTMFile(string FileName, out string ErrorMsg)
    {
      ErrorMsg = "Error reading header";
      try
      {
        TTMHeader Header = TTMHeader.NewHeader();

        using (FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
          using (BinaryReader reader = new BinaryReader(fs))
          {
            Header.Read(reader);

            // Check signature
            string signature = ASCIIEncoding.ASCII.GetString(Header.FileSignature).Substring(0, Consts.TTMFileIdentifier.Length);
            if (!Consts.TTMFileIdentifier.Equals(signature))
            {
              ErrorMsg = "File is not a Trimble TIN Model.";
              return false;
            }

            // Check file version
            if (Header.FileMajorVersion != Consts.TTMMajorVersion || Header.FileMinorVersion != Consts.TTMMinorVersion)
            {
              ErrorMsg = $"TTM.IsTTMFile(): Unable to read this version {Header.FileMajorVersion}: {Header.FileMinorVersion} of Trimble TIN Model file. Expected version: { Consts.TTMMajorVersion}: {Consts.TTMMinorVersion}";
              return false;
            }

            return true;
          }
        }
      }
      catch (Exception E)
      {
        ErrorMsg = ErrorMsg + "\n" + E.Message;
        return false;
      }
    }


    // The combine methods take a list of TTM files/surfaces and merge them all into a
    // single coherent surface
    //    procedure Combine(const TTMSurfaces : TOwnedObjectList); overload;
    //    procedure Combine(const TTMFiles : TStringList); overload;

    public static bool ReadHeaderFromFile(string FileName,
      out TTMHeader Header)
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

    //    class Function MemorySizeInKB(const AFileName: TFileName) : Integer;

    public void GetElevationRange(out double Min, out double Max)
    {
      Min = 1E100;
      Max = -1E100;

      foreach (TriVertex vertex in Vertices)
      {
        if (Min > vertex.Z) Min = vertex.Z;
        if (Max < vertex.Z) Max = vertex.Z;
      }
    }
  }
}
