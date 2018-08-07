using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.TTM.Optimised
{
    public class TrimbleTINModel : TriangleMesh
    {
        private string FModelName;
        private TTMEdges FEdges;
        private TTMStartPoints FStartPoints;
        private TTMHeader FHeader;
        private bool FLoading;

        protected short CalcFloatSize(double MaxValue, double Resolution)
        {
            const int SingleMantissaBits = 24; //  Including implied first bit 

            return (short)(Math.Abs(MaxValue) < (1 << SingleMantissaBits) * Resolution ? sizeof(float) : sizeof(double));
        }

        protected short CalcIntSize(int MaxValue)
        {
            return (short)(Math.Abs(MaxValue) <= Consts.MaxSmallIntValue ? sizeof(short) : sizeof(int));
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
            (Vertices as TTMVertices).SnapToOutputResolution(FHeader);
        }

    public TrimbleTINModel() : base()
        {
            FEdges = new TTMEdges();
            FStartPoints = new TTMStartPoints();
            CoordinateResolution = Consts.DefaultCoordinateResolution;
            ElevationResolution = Consts.DefaultElevationResolution;
        }

        public void SetUpSizes()
        {
            double MinElevation = 0, MaxElevation = 0;

            Vertices.GetLimits(ref FHeader.MinimumEasting, ref FHeader.MinimumNorthing, ref MinElevation,
                               ref FHeader.MaximumEasting, ref FHeader.MaximumNorthing, ref MaxElevation);

            FHeader.EastingOffsetValue = (FHeader.MinimumEasting + FHeader.MaximumEasting) / 2;
            FHeader.NorthingOffsetValue = (FHeader.MinimumNorthing + FHeader.MaximumNorthing) / 2;
            FHeader.VertexCoordinateSize = (byte)CalcFloatSize(Math.Max(
                                             FHeader.MaximumEasting - FHeader.EastingOffsetValue,
                                             FHeader.MaximumNorthing - FHeader.NorthingOffsetValue),
                                   CoordinateResolution);
            FHeader.VertexValueSize = (byte)CalcFloatSize(Math.Max(Math.Abs(MinElevation), Math.Abs(MaxElevation)),
                                   ElevationResolution);
            FHeader.VertexNumberSize = (byte)CalcIntSize(Vertices.Count);
            FHeader.TriangleNumberSize = (byte)CalcIntSize(Triangles.Count);

        }

        public void Read(BinaryReader reader)
        {
            string LoadErrMsg = "";

            FLoading = true;
            try
            {
                try
                {
                    LoadErrMsg = "Error reading header";

                    FHeader = TTMHeader.NewHeader();
                    FHeader.Read(reader);

                    // Commented out for now...
                    //if (FileSignatureToANSIString(FHeader.FileSignature) != kTTMFileIdentifier)
                    //{
                    //    Raise ETTMReadError.Create('File is not a Trimble TIN Model.');
                    //}

                    // Check file version
                    if (FHeader.FileMajorVersion != Consts.TTMMajorVersion
                        || FHeader.FileMinorVersion != Consts.TTMMinorVersion)
                    {
                        throw new Exception("Unable to read this version of Trimble TIN Model file.");
                    }

                    Clear();

                    // ModelName = (String)(InternalNameToANSIString(fHeader.DTMModelInternalName));
                    // Not handled for now
                    ModelName = "Reading not implemented";

                    LoadErrMsg = "Error reading vertices";
                    reader.BaseStream.Position = FHeader.StartOffsetOfVertices;
                    (Vertices as TTMVertices).Read(reader, FHeader);

                    LoadErrMsg = "Error reading triangles";
                    reader.BaseStream.Position = FHeader.StartOffsetOfTriangles;
                    (Triangles as TTMTriangles).Read(reader, FHeader, Vertices);

                    LoadErrMsg = "Error reading edges";
                    reader.BaseStream.Position = FHeader.StartOffsetOfEdgeList;
                    Edges.Read(reader, FHeader, Triangles as TTMTriangles);

                    LoadErrMsg = "Error reading start points";
                    reader.BaseStream.Position = FHeader.StartOffsetOfStartPoints;
                    StartPoints.Read(reader, FHeader, Triangles);
                }
                catch (Exception E)
                {
                    Clear();

                    throw new Exception(LoadErrMsg + ": " + E.Message);
                }
            }
            finally
            {
                FLoading = false;
            }
        }

        private void PadToBoundary(BinaryWriter writer)
        {
            const int BlockSize = 8;

            byte[] Zero = new byte[BlockSize]; // Will be initialised to zero...

            writer.Write(Zero, 0, (int)(BlockSize - ((writer.BaseStream.Position - 1) % BlockSize) - 1));
        }

        public void Write(BinaryWriter writer)
        {
            // Write a blank header now, and go back later and fix it up
            long HeaderPos = writer.BaseStream.Position;

            FHeader.FileSignature = ASCIIEncoding.ASCII.GetBytes(Consts.TTMFileIdentifier);
            FHeader.DTMModelInternalName = ASCIIEncoding.ASCII.GetBytes(ModelName ?? "Un-named Model\0");
            if (FHeader.DTMModelInternalName.Length != HeaderConsts.kDTMInternalModelNameSize)
            {
                Array.Resize(ref FHeader.DTMModelInternalName, HeaderConsts.kDTMInternalModelNameSize);
            }

            FHeader.Write(writer);

            PadToBoundary(writer);

            FHeader.FileMajorVersion = Consts.TTMMajorVersion;
            FHeader.FileMinorVersion = Consts.TTMMinorVersion;

            FHeader.CoordinateUnits = 1; // Metres
            FHeader.VertexValueUnits = 1; // Metres
            FHeader.InterpolationMethod = 1; // Linear

            SetUpSizes();

            FHeader.StartOffsetOfVertices = (int)writer.BaseStream.Position;
            FHeader.NumberOfVertices = Vertices.Count;
            FHeader.VertexRecordSize = (short)(2 * FHeader.VertexCoordinateSize + FHeader.VertexValueSize);
            (Vertices as TTMVertices).Write(writer, FHeader);
            PadToBoundary(writer);

            FHeader.StartOffsetOfTriangles = (int)writer.BaseStream.Position;
            FHeader.NumberOfTriangles = Triangles.Count;
            FHeader.TriangleRecordSize = (short)(3 * FHeader.VertexNumberSize + 3 * FHeader.TriangleNumberSize);
            Vertices.NumberVertices();
            (Triangles as TTMTriangles).Write(writer, FHeader);
            PadToBoundary(writer);

            FHeader.StartOffsetOfEdgeList = (int)writer.BaseStream.Position;
            FHeader.NumberOfEdgeRecords = Edges.Count();
            FHeader.EdgeRecordSize = FHeader.TriangleNumberSize;
            Edges.Write(writer, FHeader);
            PadToBoundary(writer);

            FHeader.StartOffsetOfStartPoints = (int)(writer.BaseStream.Position);
            FHeader.NumberOfStartPoints = StartPoints.Count();
            FHeader.StartPointRecordSize = (short)(2 * FHeader.VertexCoordinateSize + FHeader.TriangleNumberSize);
            StartPoints.Write(writer, FHeader);

            // Fix up header
            long EndPos = writer.BaseStream.Position;
            writer.BaseStream.Position = HeaderPos;

            FHeader.Write(writer);

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
            using (FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
               LoadFromStream(fs);
            }

            if (FModelName.Length == 0)
            {
                FModelName = Path.ChangeExtension(Path.GetFileName(FileName), "");
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
            FStartPoints.Clear();

            if (Triangles.Count == 0)
            {
                return;
            }

            // How many start points do we want?
            int NumStartPoints = Math.Min(Math.Max((int)Math.Round(Math.Sqrt(Triangles.Count) / 2), 1), Consts.MaxStartPoints);

            // Use the centre points of this number of triangles evenly spaced throughout the job
            int TriNumOffset = (Triangles.Count / NumStartPoints) / 2;
            for (int StPtNum = 0; StPtNum < NumStartPoints; StPtNum++)
            {
                int TriangleNum = (StPtNum * Triangles.Count) / NumStartPoints + TriNumOffset;

                Debug.Assert(TriangleNum >= 0);

                XYZ Centroid = Triangles[TriangleNum].Centroid();
                FStartPoints.Add(new TTMStartPoint(Centroid.X, Centroid.Y, Triangles[TriangleNum]));
            }
        }


        public override void Clear()
        {
            base.Clear();

            FEdges.Clear();
            FStartPoints.Clear();
            FModelName = "";
        }


        public string ModelName { get { return FModelName; } set { FModelName = value; } }
        public TTMStartPoints StartPoints { get { return FStartPoints; } }
        public TTMEdges Edges { get { return FEdges; } }
        public TTMHeader Header = TTMHeader.NewHeader(); //{ get { return FHeader; } }

        public double CoordinateResolution { get; set; }
        public double ElevationResolution { get; set; }

        public bool Loading { get { return FLoading; } }

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
                            ErrorMsg = "Unable to read this version of Trimble TIN Model file.";
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
