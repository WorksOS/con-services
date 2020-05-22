using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Amazon.S3.Model.Internal.MarshallTransformations;
using VSS.Productivity3D.Models.Models.Files;
using VSS.TRex.Common;
using VSS.TRex.Common.Extensions;
using VSS.TRex.Geometry;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Files.DXF
{
  public class DXFReader
  {
    public const int kSplineFrameControlPointFlag = 16;
    public const int kQuadraticBSplineSmoothing = 5;
    public const int kCubicBSplineSmoothing = 6;

    public const int kPolylineIsClosed = 0x1;
    public const int kPolylineHasCurveFitVertices = 0x2;
    public const int kPolylineHasSplineFitVertices = 0x4;
    public const int kPolylineIs3D = 0x8;
    public const int kPolylineIsPolygonMesh = 0x10;
    public const int kPolygonmeshIsClosedInNDirection = 0x20;
    public const int kPolylineIsPolyfaceMesh = 0x40;
    public const int kPolylineHasContinuoueLinetypePattern = 0x80;

    public const int kVertexCreatedByCurveFitting = 0x1;
    public const int kVertexHasDefinedCurveFitTangent = 0x2;
    public const int kVertexCreatedBySplineFitting = 0x8;
    public const int kVertexIsSplineFrameControlPoint = 0x10;
    public const int kVertexIs3DPolylineVertex = 0x20;
    public const int kVertexIsIn3DPolygonMesh = 0x40;
    public const int kVertexIsPolyfaceMeshVertex = 0x80;

    private const ushort ASCIIDXFRecordsMax = 1071;
    private const ushort BinaryDXFRecordsMax = 1071;

    private const byte ASCIIDXFRecordType_Integer = 0;
    private const byte ASCIIDXFRecordType_Real = 1;
    private const byte ASCIIDXFRecordType_String = 2;
    private const byte ASCIIDXFRecordType_Ignore = 3;

    private const byte BinaryDXFRecordType_Integer1 = 0;
    private const byte BinaryDXFRecordType_Integer2 = 1;
    private const byte BinaryDXFRecordType_Integer4 = 2;
    private const byte BinaryDXFRecordType_Integer8 = 3;
    private const byte BinaryDXFRecordType_Real = 4;
    private const byte BinaryDXFRecordType_String = 5;
    private const byte BinaryDXFRecordType_ExtendedData = 6;
    private const byte BinaryDXFRecordType_Ignore = 7;

    private bool DXFFileIsBinary;

    private readonly StreamReader _aSCIIdxfFile;
    private readonly Stream _binarydxfFile;
    private int _dxfLine;

    private bool _haveFoundEntitiesSection;
    private bool _reuseRecord;
    private DXFRecord _lastRecord;

    private bool _dXFFileIsPostR13c3;
    private static byte[] BinaryACADDXFSignature;

    private static readonly byte[] ASCIIDXFRecordTypeLookUp = new byte [ASCIIDXFRecordsMax + 1];
    private static readonly byte[] BinaryDXFRecordTypeLookUp = new byte[BinaryDXFRecordsMax + 1];

    public double DXFImportConvFactor;
    public DxfUnitsType Units;

    static DXFReader()
    {
      BinaryACADDXFSignature = Encoding.ASCII.GetBytes("AutoCAD Binary DXF" + (char)13 + (char)10 + (char)26 + (char)0);
      InitialiseASCIIDXFRecordLookUpTable();
      InitialiseBinaryDXFRecordLookUpTable();
    }

    /// <summary>
    /// Initialises the maps of record numbers to value types in ASCII DXF files
    /// </summary>
    private static void InitialiseASCIIDXFRecordLookUpTable()
    {
      for (var type = 0; type <= ASCIIDXFRecordsMax; type++)
      {
        if ((type >= 10 && type <= 59) ||
            (type >= 210 && type <= 239) ||
            (type >= 1010 && type <= 1059))
        {
          ASCIIDXFRecordTypeLookUp[type] = ASCIIDXFRecordType_Real;
        }
        else if ((type >= 60 && type <= 79) ||
                 (type >= 90 && type <= 99) ||
                 (type >= 270 && type <= 279) ||
                 (type >= 280 && type <= 289) ||
                 (type >= 370 && type <= 379) ||
                 (type >= 380 && type <= 389) ||
                 (type >= 400 && type <= 409) ||
                 (type >= 1060 && type <= 1070))
        {
          ASCIIDXFRecordTypeLookUp[type] = ASCIIDXFRecordType_Integer;
        }
        else if ((type >= 0 && type <= 9) ||
                 (type == 100 || type == 102 || type == 105) ||
                 (type >= 300 && type <= 309) ||
                 (type >= 310 && type <= 319) ||
                 (type >= 320 && type <= 329) ||
                 (type >= 330 && type <= 369) ||
                 (type >= 400 && type <= 409) ||
                 (type >= 410 && type <= 419) ||
                 (type == 999) ||
                 (type >= 1000 && type <= 1009))
        {
          ASCIIDXFRecordTypeLookUp[type] = ASCIIDXFRecordType_String;
        }
        else if ((type >= 140 && type <= 147) ||
                 (type >= 170 && type <= 178))
        {
          ASCIIDXFRecordTypeLookUp[type] = ASCIIDXFRecordType_Ignore;
        }
        else
        {
          ASCIIDXFRecordTypeLookUp[type] = ASCIIDXFRecordType_Ignore;
        }
      }
    }

    /// <summary>
    /// Initialises the maps of record numbers to value types in ASCII DXF files
    /// </summary>
    private static void InitialiseBinaryDXFRecordLookUpTable()
    {
      for (var type = 0; type <= BinaryDXFRecordsMax; type++)
      {
        if ((type >= 10 && type <= 59) ||
            (type >= 110 && type <= 112) ||
            (type >= 120 && type <= 122) ||
            (type >= 130 && type <= 132) ||
            (type >= 140 && type <= 148) ||
            (type >= 210 && type <= 239) ||
            (type >= 1010 && type <= 1059))
        {
          BinaryDXFRecordTypeLookUp[type] = BinaryDXFRecordType_Real;
        }
        else if (type >= 60 && type <= 79 ||
                 type >= 170 && type <= 179 ||
                 type >= 270 && type <= 279 ||
                 type >= 280 && type <= 289 ||
                 type >= 370 && type <= 379 ||
                 type >= 380 && type <= 389 ||
                 type >= 400 && type <= 409 ||
                 type >= 1060 && type <= 1070)
        {
          BinaryDXFRecordTypeLookUp[type] = BinaryDXFRecordType_Integer2;
        }
        else if (type >= 290 && type <= 299)
        {
          BinaryDXFRecordTypeLookUp[type] = BinaryDXFRecordType_Integer1;
        }
        else if (type >= 90 && type <= 99)
        {
          BinaryDXFRecordTypeLookUp[type] = BinaryDXFRecordType_Integer4;
        }
        else if (type == 1071)
        {
          BinaryDXFRecordTypeLookUp[type] = BinaryDXFRecordType_Integer8;
        }
        else if (type >= 0 && type <= 9 ||
                 type == 100 || type == 102 || type == 105 ||
                 type >= 300 && type <= 309 ||
                 type >= 320 && type <= 329 ||
                 type >= 330 && type <= 369 ||
                 type >= 390 && type <= 399 ||
                 type >= 410 && type <= 419 ||
                 // (type == 999) || Not used in binary DXF
                 type >= 1000 && type <= 1003 ||
                 type >= 1005 && type <= 1009)
        {
        BinaryDXFRecordTypeLookUp[type] = BinaryDXFRecordType_String;
        }
        else if (type >= 310 && type <= 319 ||
                 type == 1004)
        {
          BinaryDXFRecordTypeLookUp[type] = BinaryDXFRecordType_ExtendedData;
        }
        else
        {
          BinaryDXFRecordTypeLookUp[type] = BinaryDXFRecordType_Ignore;
        }
      }
    }

    /// <summary>
    /// Read a single DXF record from an ASCII DXF file consisting of the record type and value
    /// </summary>
    /// <param name="rec"></param>
    /// <param name="lineNumber"></param>
    /// <param name="readingTextEntity"></param>
    /// <returns></returns>
    public bool ReadASCIIDXFRecord(out DXFRecord rec,
      ref int lineNumber,
      bool readingTextEntity = false)
    {
      rec = new DXFRecord();

      var recTypeString = _aSCIIdxfFile.ReadLine();
      rec.recType = Convert.ToUInt16(recTypeString);

      if (_aSCIIdxfFile.EndOfStream)
        return false; // may be ASCII but not DXF format
      lineNumber++;

      if (rec.recType <= ASCIIDXFRecordsMax)
      {
        var line = _aSCIIdxfFile.ReadLine();

        switch (ASCIIDXFRecordTypeLookUp[rec.recType])
        {
          case ASCIIDXFRecordType_Integer:
            rec.i = Convert.ToInt32(line);
            break;
          case ASCIIDXFRecordType_Real:
            rec.r = Convert.ToDouble(line);
            break;
          case ASCIIDXFRecordType_String:
          {
            rec.s = line;

            // We do not want to remove leading or trailing spaces from
            // the text in DXF text entities (TEXT and M TEXT). These
            // group codes 1 (for TEXT) and 1&3 (for M TEXT). If the caller
            // has advised it is reading a DXF text entity we no do not
            // strip the spaces from the string value.
            if (readingTextEntity)
            {
              if (rec.recType != 1 && rec.recType != 3)
                rec.s = rec.s.Trim();
            }
            else
              rec.s = rec.s.Trim();

            break;
          }
        }

        lineNumber++;
      }

      return true;
    }

    /// <summary>
    /// Read a DXF record from a binary DXF file
    /// </summary>
    /// <param name="rec"></param>
    /// <param name="lineNumber"></param>
    /// <param name="readingTextEntity"></param>
    /// <returns></returns>
    public bool ReadBinaryDXFRecord(out DXFRecord rec,
      ref int lineNumber,
      bool readingTextEntity = false)
    {
      rec = new DXFRecord();
      var buffer = new byte[100];

      // Read the record number
      if (_dXFFileIsPostR13c3)
      {
        _binarydxfFile.Read(buffer, 0, 2);
        rec.recType = BitConverter.ToUInt16(buffer);
      }
      else
      {
        var binaryRecType1 = _binarydxfFile.ReadByte();

        if (binaryRecType1 != 255)
          rec.recType = (ushort)binaryRecType1;
        else // escaped record number
        {
          _binarydxfFile.Read(buffer, 0, 2);
          rec.recType = BitConverter.ToUInt16(buffer);
        }
      }

      if (rec.recType >= BinaryDXFRecordTypeLookUp.Length)
      {
        return false;
      }

      switch (BinaryDXFRecordTypeLookUp[rec.recType])
      {
        case BinaryDXFRecordType_String:
          var builder = new StringBuilder();
          // read chars until we find a #0

          byte theByte;
          do
          {
            theByte = (byte)_binarydxfFile.ReadByte();
            if (theByte != 0)
            {
              builder.Append((char) theByte);
            }
          } while  (theByte != 0);

          rec.s = builder.ToString();

          // We do not want to remove leading or trailing spaces from
          // the text in DXF text entities (TEXT and MTEXT). These
          // group codes 1 (for TEXT) and 1&3 (for MTEXT). If the caller
          // has advised it is reading a DXF text entity we no do not
          // strip the spaces from the string value.
          if (readingTextEntity)
          {
            if (!(rec.recType == 1 || rec.recType == 3))
              rec.s = rec.s.Trim();
          }
          else
            rec.s = rec.s.Trim();
          break;

        case BinaryDXFRecordType_Real:
          _binarydxfFile.Read(buffer, 0, 8);
          rec.r = BitConverter.ToDouble(buffer);
          break;

        case BinaryDXFRecordType_Integer1:
          _binarydxfFile.Read(buffer, 0, 1);
          rec.i = buffer[0];
          break;

        case BinaryDXFRecordType_Integer2:
          _binarydxfFile.Read(buffer, 0, 2);
          rec.i = BitConverter.ToInt16(buffer);
          break;

        case BinaryDXFRecordType_Integer4:
          _binarydxfFile.Read(buffer, 0, 4);
          rec.i = BitConverter.ToInt32(buffer);
          break;

        case BinaryDXFRecordType_Integer8:
          _binarydxfFile.Read(buffer, 0, 4);
          rec.i = BitConverter.ToUInt32(buffer);
          break;

        case BinaryDXFRecordType_ExtendedData:
          rec.s = "";
          var recLength = _binarydxfFile.ReadByte();
          _binarydxfFile.Position += recLength;
          break;

        default:
          return false;
      }

      lineNumber++;
      return true;
    }

    /// <summary>
    /// Reads the next record from the DXF file
    /// </summary>
    /// <param name="rec"></param>
    /// <returns></returns>
    public bool ReadDXFRecord(out DXFRecord rec)
    {
      if (_reuseRecord)
      {
        rec = _lastRecord;
        _reuseRecord = false;
        return true;
      }
      else
      {
        var result = DXFFileIsBinary ? ReadBinaryDXFRecord(out rec, ref _dxfLine) : ReadASCIIDXFRecord(out rec, ref _dxfLine);

        _lastRecord = rec;
        return result;
      }
    }

    /// <summary>
    /// Locates a named section in a DXF file to begin reading records from
    /// </summary>
    /// <param name="name0"></param>
    /// <param name="name2"></param>
    /// <returns></returns>
    public bool FindSection(string name0, string name2)
    {
      var TestString = "";
      var rec = new DXFRecord();

      var _haveFoundSection = false;
      while (!_haveFoundSection)
      {
        // Scan for a section start
        var foundSectionStart = false;
        while (!foundSectionStart)
        {
          if (!ReadDXFRecord(out rec))
            return false;

          if (rec.recType != 0)
            continue;

          TestString = rec.s.ToUpper(CultureInfo.InvariantCulture);
          // ReSharper disable once StringLiteralTypo
          foundSectionStart = string.Compare(TestString, name0, StringComparison.InvariantCulture) == 0 || TestString == "ENDSEC" || TestString == "SECTION";
        }

        // ReSharper disable once StringLiteralTypo
        if (TestString != "ENDSEC") // empty section
        {
          if (!ReadDXFRecord(out rec))
            return false;
          TestString = rec.s.ToUpper(CultureInfo.InvariantCulture);
        }

        _haveFoundSection = rec.recType == 2 && string.Compare(TestString, name2, StringComparison.InvariantCulture) == 0;
      }

      return true;
    }

    /// <summary>
    /// Locates the section in the DXF file that contain entities such as poly lines
    /// </summary>
    /// <returns></returns>
    public bool FindEntitiesSection()
    {
      return _haveFoundEntitiesSection || (_haveFoundEntitiesSection = FindSection("SECTION", "ENTITIES"));
    }

    private bool GetStartOfNextEntity(out DXFRecord rec)
    {
      do
      {
        if (!ReadDXFRecord(out rec))
          return false;
      } while (rec.recType != 0);

      // ReSharper disable once StringLiteralTypo
      return rec.s == "ENDSEC";
    }

    /* Todo: Extrusion not supported
    private bool CheckExtrusionRecord(DXFRecord rec)
    {
      // Returns True if the record is a non default extrusion record

      return ((rec.recType == 210) && (rec.r != 0.0)) ||
             ((rec.recType == 220) && (rec.r != 0.0)) ||
             ((rec.recType == 230) && (rec.r != 1.0));
    }
    */

    /// <summary>
    /// Reads the entirety of a polyline from the DXF file
    /// </summary>
    /// <param name="loadError"></param>
    /// <param name="lwPolyLine"></param>
    /// <param name="entity"></param>
    /// <param name="polyLineIsClosed"></param>
    public void ReadPolyLine(out bool loadError,
      bool lwPolyLine,
      PolyLineBoundary entity,
      out bool polyLineIsClosed)
    {
      const double EPSILON = 0.000001;

      int FetchIndex;
      int NumArrayEntries;
      DXFRecord rec;
      XYZ lastPt = XYZ.Null, pt;
      bool PaperSpace;
      // Todo: Extrusion is not taken into account
      // bool extruded;
      long PolyLineFlags;
//      double bulge;
      long NVertices;
      long VertexNum;
      long CurveSmoothing;
      // Todo extrusion not supported XYZ Extrusion;
      bool PolyLineIs3D;
      double DefaultPolyLineHeight;
      var ExtendedAttrName = "";

      var PolyLineRecords = new List<DXFRecord>();

      // This function allows us to reread the first few record in the poly line.
      bool GetDXFRecord(out DXFRecord record)
      {
        if (FetchIndex < NumArrayEntries)
        {
          record = PolyLineRecords[FetchIndex];
          FetchIndex++;
          return true;
        }

        return ReadDXFRecord(out record);
      }

      void LoadSimplePolyLine()
      {
        //  Load all vertices in the poly line 
        var FirstPoint = true;

        while ((rec.s == "VERTEX") || (lwPolyLine && (VertexNum < NVertices)))
        {
//          double nextBulge = 0;
          var VertexFlags = 0;
          var Rec10Count = 0;
          pt.Z = Consts.NullDouble;

          do
          {
            if (!GetDXFRecord(out rec))
              return;

            switch (rec.recType)
            {
              case 10:
                Rec10Count++;
                if (Rec10Count == 1)
                  pt.X = rec.r * DXFImportConvFactor;
                break;

              case 20:
                pt.Y = rec.r * DXFImportConvFactor;
                break;

              case 30:
                if (PolyLineIs3D)
                  pt.Z = rec.r * DXFImportConvFactor;
                break;

//              case 42:
//                nextBulge = rec.r;
//                break;

//              case 62 : ; //SetPen (pen,rec.i);
//                break;
              case 70:
                VertexFlags = (ushort) rec.i;
                break;
            }
          } while (!(rec.recType == 0 || (lwPolyLine && Rec10Count == 2)));

          // If we are reading in a 2D poly line, then the heights of all the
          // vertices in the poly line should be set to the elevation read in from
          // the 38 field in the POLY LINE entity (ie: any elevation read in for
          // the vertex is discarded. This also applies to LW POLY LINE entities.
          // Note: This could have been achieved by initializing
          // the value of pt.z before the repeat loop, but this explicit
          // behaviour is more evident as to its purpose.
          if (!PolyLineIs3D)
            pt.Z = DefaultPolyLineHeight;

          // Todo: Extrusion is not taken into account
          //if (extruded && !PolyLineIs3D)
          //  AdjustForExtrusion(pt.x, pt.y, pt.z, Extrusion);

          if (lwPolyLine)
          {
            FetchIndex--; // Reprocess the last record
            VertexNum++;
          }

          if ((VertexFlags & kSplineFrameControlPointFlag) != kSplineFrameControlPointFlag)
          {
            bool IsDuplicateVertex;
            if (FirstPoint)
              IsDuplicateVertex = false;
            else
              IsDuplicateVertex = (Math.Abs(pt.X - lastPt.X) < EPSILON) &&
                                  (Math.Abs(pt.Y - lastPt.Y) < EPSILON) &&
                                  (!PolyLineIs3D || (Math.Abs(pt.Z - lastPt.Z) < EPSILON));

            // Determine if the vertex we have just read in is the same as the previous vertex.
            // If it is, then don't create entities for it
            if (!IsDuplicateVertex)
            {
              // Add the vertex to the fence
              entity.Boundary.Points.Add(new FencePoint(pt.X, pt.Y));
            }

//            bulge = nextBulge;

            if (!IsDuplicateVertex)
            {
              FirstPoint = false;
              lastPt = pt;
            }
          }
        }
      }

      void ProcessNonVertexRecord()
      {
        switch (rec.recType)
        {
          case 8:
            //load := SetCurrentLayer (rec.s,pen);
            break;

          // 30 record is height for all vertices in a 2D poly line
          case 30:
            DefaultPolyLineHeight = rec.r * DXFImportConvFactor;
            break;

          // 38 record is height for all vertices in a lightweight poly line
          case 38:
            DefaultPolyLineHeight = rec.r * DXFImportConvFactor;
            break;

          case 62:
            //SetPen (pen,rec.i);
            break;
          case 67:
            PaperSpace = rec.i != 0;
            break;
          case 70:
            PolyLineFlags = rec.i;
            break;
          case 75:
            /*Curves and smooth surface type (optional; default = 0); integer codes, not bit-coded:
              0 = No smooth surface fitted
              5 = Quadratic B-spline surface
              6 = Cubic B-spline surface
              8 = Bezier surface */
            CurveSmoothing = rec.i;
            break;
          case 90:
            NVertices = rec.i;
            break;

          /* Todo: Extrusion not supported
          case 210:
            Extrusion.X = rec.r;
            break;
          case 220:
            Extrusion.Y = rec.r;
            break;
          case 230:
            Extrusion.Z = rec.r;
            break;
            */

          case 1001: 
            ExtendedAttrName = rec.s.ToUpper(CultureInfo.InvariantCulture); // Registered application name (up to 31 bytes) in extended data
            break;
          case 1000: // ASCII string in extended attrs (up to 255 bytes)
          case 1070: 
            if (ExtendedAttrName == "TRIMBLEBNDYTYPE")
              entity.Type = (DXFLineWorkBoundaryType)rec.i;
            else if (ExtendedAttrName == "TRIMBLENAME")
              entity.Name = rec.s;
            break;
        }
      }

      bool IsPolyLineClosed()
      {
        XYZ pt_start, pt_end;

        var SavedFetchIndex = FetchIndex;

        // Start vertex...
        pt_start.X = PolyLineRecords[FetchIndex].r * DXFImportConvFactor;
        pt_start.Y = PolyLineRecords[FetchIndex + 1].r * DXFImportConvFactor;

        if (PolyLineIs3D)
          pt_start.Z = PolyLineRecords[FetchIndex + 2].r * DXFImportConvFactor;
        else
          pt_start.Z = Consts.NullDouble;

        do
        {
          if (!GetDXFRecord(out rec))
            return false;
        } while (rec.recType != 0);

        // End vertex...
        if (PolyLineIs3D)
        {
          pt_end.Z = PolyLineRecords[FetchIndex - 2].r * DXFImportConvFactor;
          pt_end.Y = PolyLineRecords[FetchIndex - 3].r * DXFImportConvFactor;
          pt_end.X = PolyLineRecords[FetchIndex - 4].r * DXFImportConvFactor;
        }
        else
        {
          pt_end.Y = PolyLineRecords[FetchIndex - 2].r * DXFImportConvFactor;
          pt_end.X = PolyLineRecords[FetchIndex - 3].r * DXFImportConvFactor;
          pt_end.Z = Consts.NullDouble;
        }

        FetchIndex = SavedFetchIndex;

        return (Math.Abs(pt_start.X - pt_end.X) < EPSILON) &&
               (Math.Abs(pt_start.Y - pt_end.Y) < EPSILON) &&
               (!PolyLineIs3D || (Math.Abs(pt_start.Z - pt_end.Z) < EPSILON));
      }

      loadError = true; 
      PaperSpace = false;
      // Todo extruded = false;
//      bulge = 0;
      NVertices = 0;
      VertexNum = 0;
      PolyLineFlags = 0;
      DefaultPolyLineHeight = Consts.NullDouble;
      CurveSmoothing = 0;
      // Todo Extrusion = DefaultExtrusion;
      pt = XYZ.Null;
      polyLineIsClosed = false;

      //--------------------------------------------
      // This is a bit grubby, but in my defense we had to handle extrusions for
      // LW POLY LINE entities.  In this case we have to read the entire entity looking
      // for extrusion records before we use the point.
      FetchIndex = 0;

      do
      {
        if (!ReadDXFRecord(out rec))
          return;

        ProcessNonVertexRecord();

        PolyLineRecords.Add(rec);
        FetchIndex++;
      } while (rec.recType != 0); // Start of next entity 

      polyLineIsClosed = (PolyLineFlags & kPolylineIsClosed) == kPolylineIsClosed;
      PolyLineIs3D = !lwPolyLine &&
                     (((PolyLineFlags & kPolylineIs3D) == kPolylineIs3D) ||
                      ((PolyLineFlags & kPolylineIsPolyfaceMesh) == kPolylineIsPolyfaceMesh));

      // Todo: extruded = CheckExtrusionRecord(Extrusion);

      NumArrayEntries = FetchIndex;

      if (lwPolyLine)
        _reuseRecord = true; // load_entities needs to read this record 

      //--------------------------------------------
      // Read poly line header values 
      FetchIndex = 0; // Reprocess the records that have already been read.
      do
      {
        if (!GetDXFRecord(out rec))
          return;

        ProcessNonVertexRecord();
      } while (!((rec.recType == 0) || (lwPolyLine && (rec.recType == 10)))); // Start of first vertex 
      //--------------------------------------------

      if (lwPolyLine)
        FetchIndex--; // Reprocess the last record

      // Check whether the poly line is closed if the internal flag indicates it is not...
      polyLineIsClosed = polyLineIsClosed || (lwPolyLine && IsPolyLineClosed());

      if (!PaperSpace)
      {
        if ((PolyLineFlags & kPolylineIsPolyfaceMesh) == kPolylineIsPolyfaceMesh)
        {
          // We no longer load poly face meshes as background line work...
        }
        else // It's a poly line of some form
        {
          if (!lwPolyLine &&
              (CurveSmoothing == kQuadraticBSplineSmoothing) ||
              (CurveSmoothing == kCubicBSplineSmoothing))
          {
            // Spline fit poly lines are not supported here
          }
          else
            LoadSimplePolyLine();
        }
      }
      else
      {
        // ReSharper disable once StringLiteralTypo
        //  Scan past SEQ END for standard 3D poly lines
        if (!lwPolyLine)
          while ((rec.recType != 0) || (rec.s != "SEQEND"))
            if (!ReadDXFRecord(out rec))
              return;
      }

      loadError = false;
    }

    /// <summary>
    /// Reads a poly line from the DXF file and construct a boundary from the geometry and extended attributes
    /// defining the boundary type and name
    /// </summary>
    /// <param name="closedPolyLinesOnly"></param>
    /// <param name="atEof"></param>
    /// <param name="boundary"></param>
    /// <returns></returns>
    public bool GetBoundaryFromPolyLineEntity(bool closedPolyLinesOnly, out bool atEof, out PolyLineBoundary boundary)
    {
      var loadError = false;
      var polyLineIsClosed = false;

      atEof = GetStartOfNextEntity(out var DXFRec);
      boundary = null;

      if (atEof)
        return false;

      boundary = new PolyLineBoundary();

      var testString = DXFRec.s.ToUpper(CultureInfo.InvariantCulture);
      switch (testString)
      {
        // ReSharper disable once StringLiteralTypo
        case "POLYLINE":
          ReadPolyLine(out loadError, false, boundary, out polyLineIsClosed);
          break;
        // ReSharper disable once StringLiteralTypo
        case "LWPOLYLINE":
          ReadPolyLine(out loadError, true, boundary, out polyLineIsClosed);
          break;
      }

      return !loadError && (polyLineIsClosed || !closedPolyLinesOnly);
    }

    // This function assumes that the files exists - we are just determining its type
    private bool IsBinaryDXF(Stream dxfFile, out bool dxfFileIsPostR13C3)
    {
      dxfFileIsPostR13C3 = false;

      var buffer = new byte[BinaryACADDXFSignature.Length];

      try
      {
        dxfFile.Read(buffer, 0, buffer.Length);
      }
      catch
      {
        return false;
      }

      // now check the version of the file
      var result = true;
      buffer.ForEach((b, i) => result = result && (BinaryACADDXFSignature[i] == b));

      if (result)
      {
        dxfFile.Read(buffer, 0, 2);
        dxfFileIsPostR13C3 = BitConverter.ToUInt16(new ReadOnlySpan<byte>(buffer, 0, 2)) == 0;
      }

      return result;
    }

    /// <summary>
    /// Construct a DXFReader given a stream reader representing teh DXF file content and the units to use
    /// </summary>
    /// <param name="dxfFile"></param>
    /// <param name="units"></param>
    public DXFReader(MemoryStream dxfFile, DxfUnitsType units)
    {
      DXFFileIsBinary = IsBinaryDXF(dxfFile, out _dXFFileIsPostR13c3);

      if (DXFFileIsBinary)
      {
        _binarydxfFile = dxfFile;
        _binarydxfFile.Position = BinaryACADDXFSignature.Length; // Skip the sentinel
      }
      else
      {
        dxfFile.Position = 0;
        _aSCIIdxfFile = new StreamReader(dxfFile);
      }

      Units = units;

      DXFImportConvFactor = units switch
      {
        DxfUnitsType.Meters => UnitUtils.DistToMeters(DistanceUnitsType.Meters),
        DxfUnitsType.UsSurveyFeet => UnitUtils.DistToMeters(DistanceUnitsType.US_feet),
        DxfUnitsType.ImperialFeet => UnitUtils.DistToMeters(DistanceUnitsType.Feet),
        _ => 1.0
      };
    }
  }
}
