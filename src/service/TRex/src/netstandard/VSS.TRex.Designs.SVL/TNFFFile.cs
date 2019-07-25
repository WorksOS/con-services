using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace VSS.TRex.Designs.SVL
{
  public class TNFFFile
  {
    public TNFFFileType NFFFileType;
    public TNFFFileVersion FileVersion;

    //FGrids : TNFFGridList;
    //F3DLineDesignGridFile : TNFF3DGridLineworkFile;

    public int GridSize; // Number of metres square the cell is
    public int GridOriginX;
    public int GridOriginY;

    // Can't re-specify Grid Origion once Surfaces/Linework have been added
    //fGridOriginSpecificationAllowed: Boolean;

    public byte Flags;

    //FSurfaces : TNFFSurfaceList;
    //FReferenceSurfaces : TNFFVerticalOffsetList;

    public TNFFNamedGuidanceIDList NamedGuidanceIDs;
    public TNFFGuidableAlignmentEntityList GuidanceAlignments;

    /* TNFFFile makes use of a compound document format to store it's contents.
      Depending upon version either the Microsoft IStorage compound document
      format or our own custom format is used
    */

    //FCompoundStorage : IStorage;
    //FileStorage : TFileStream;

    // Collection of name tags(and in case of our own custom format offsets
    //  and sizes) for logical streams within compound document

    public TNFFStreamInfoList StreamInfoList;

    public bool UnsupportedFileVersion;


    public TNFFErrorStatus ErrorStatus;

    //FXMLDataIslands : TNFFXMLDataIslandList;
    //FGestalt : TNFFGestaltDataIsland;

    //FRelatedStreams : TNFFRelatedStreamList;

    //FFilenameLoadedFrom: TFilename;

    public TNFFFile()
    {
      //FGrids:= TNFFGridList.Create(Self);
      //GridSize := kNFFDefaultDefaultGridSize;
      //GridOriginX:= 0;
      //GridOriginY:= 0;

      //fGridOriginSpecificationAllowed:= True;

      //F3DLineDesignGridFile:= TNFF3DGridLineworkFile.Create(Self);

      //FSurfaces:= TNFFSurfaceList.Create(Self);
      NamedGuidanceIDs = new TNFFNamedGuidanceIDList();
      //FReferenceSurfaces:= TNFFVerticalOffsetList.Create;
      StreamInfoList = new TNFFStreamInfoList();

      UnsupportedFileVersion = false;

      GuidanceAlignments = new TNFFGuidableAlignmentEntityList();
      //FGuidanceAlignments.OnItemAdded := OnGuidanceAlignmentAdded;

      //FXMLDataIslands:= TNFFXMLDataIslandList.Create;
      //FGestalt:= TNFFGestaltDataIsland.Create;
      //FXMLDataIslands.Add(FGestalt);

      //FRelatedStreams:= TNFFRelatedStreamList.Create;

      //FAvoidanceZoneUndergroundServicesRadius:= 0;
      //FAvoidanceZoneType:= nff_aztNone;
    }

    public TNFFFile(TNFFFileType ANFFFileType) : this()
    {
      NFFFileType = ANFFFileType;
    }

    public TNFFFile(TNFFFileType ANFFFileType, TNFFFileVersion ANFFFileVersion) : this(ANFFFileType)
    {
      FileVersion = ANFFFileVersion;
    }

    public TNFFFile(TNFFFileType ANFFFileType, TNFFFileVersion ANFFFileVersion, int AGridSize) : this(ANFFFileType, ANFFFileVersion)
    {
      GridSize = AGridSize;
    }

    //FAvoidanceZoneType : TNFFAvoidanceZoneType;
    //FAvoidanceZoneUndergroundServicesRadius : Double;

    //procedure SetGridSize(const Value: Integer);
    //Function LocateGrid(X, Y : Double) : TNFFGrid;
    //Procedure ComputeGridIndex(X, Y : Double; var IndexX, IndexY : Integer);
    // Procedure CalculateGridCoverage(MinimumEasting, MinimumNorthing, MaximumEasting, MaximumNorthing : Double; var I_MinX, I_MinY, I_MaxX, I_MaxY : Integer);
    // procedure Resize;

    //Function OpenCompoundDocumentReadOnly(FileName : TFileName) : Boolean;
    // Function OpenCompoundDocumentRW(FileName : TFileName) : Boolean;

    //Procedure SaveHeaderToStream(Stream : TStream);
    public bool LoadHeaderFromStream(BinaryReader reader)
    {
      var Header = new TNFFIndexFileHeader();

      var b = reader.ReadBytes(Marshal.SizeOf(Header));

      var handle = GCHandle.Alloc(b, GCHandleType.Pinned);
      Header = (TNFFIndexFileHeader) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(TNFFIndexFileHeader));

      if (NFFUtils.MagicNumberToANSIString(Header.MajicNumber) != NFFConsts.kNFFIndexFileMajicNumber)
        return false;

      if (!NFFUtils.SetFileVersionFromMinorMajorVersionNumbers(Header.MajorVer, Header.MinorVer, out FileVersion))
      {
        UnsupportedFileVersion = true;
        return false;
      }

      // We do not support reading version 1.0 NFF .SVD files as these files
      // used quantised triangle vertex positions which were found to cause
      // numerous precision issues. Version 1.1 and later files do not use quantised
      // vertex coordinates, but use full double precision coordinates instead.

      if (FileVersion == TNFFFileVersion.nffVersion1_0)
      {
        UnsupportedFileVersion = true;
        return false;
      }

      // For a short period in development of v5, version 1.2 NFF files were written out
      // as MS compound documents prior to changing to the Trimble compound document format
      // If we see one of these files, flag it as an unsupported version.
      // Note: This is hard to check in .Net, so we will ignore it...

//      if (FileVersion == nffVersion1_2) && Assigned(FCompoundStorage) then
//          begin
//            FUnsupportedFileVersion= True;
//      Exit;
//      end;

      GridOriginX = Header.GridOrigin.X;
      GridOriginY = Header.GridOrigin.Y;
      GridSize = Header.GridSquareSize;

      if (FileVersion > TNFFFileVersion.nffVersion1_2 && NFFFileType == TNFFFileType.nffSVLFile) // There is a flags byte following the header
        Flags = reader.ReadByte();

      if (FileVersion >= TNFFFileVersion.nffVersion1_6 && NFFFileType == TNFFFileType.nffSVLFile) // There is a double containing the exclusion radius to be used for underground services avoidance zones
      {
        byte AZType = reader.ReadByte(); //Stream.Read(AZType, SizeOf(FAvoidanceZoneType));
        //FAvoidanceZoneType= TNFFAvoidanceZoneType(AZType);
        double AvoidanceZoneUndergroundServicesRadiusStream = reader.ReadDouble(); //.Read(FAvoidanceZoneUndergroundServicesRadius, Sizeof(FAvoidanceZoneUndergroundServicesRadius));
      }

      if (NFFFileType == TNFFFileType.nffSVDFile)
      {
        throw new NotImplementedException("SVD files are not supported");
        //FSurfaces.LoadFromStream(Stream);
        //FReferenceSurfaces.LoadFromStream(Stream);
      }

      if (NFFFileType == TNFFFileType.nffSVLFile || FileVersion >= TNFFFileVersion.nffVersion1_4)
      {
        // NOTE: In the header, it is assumed there is always an entry (if nothing more than
        // a zero count of guidance IDs). This is due to an unfortunate bug in SiteVision versions
        // earlier than v6.0 that expent the SVL file format to have a guidance alignment count
        // in the header stream.
        NamedGuidanceIDs.LoadFromStream(reader);
      }

      return true;
    }

    // Procedure SaveHeaderToCompoundDoc;
    // Function LoadHeaderFromCompoundDoc : Boolean;
    //    procedure SaveVersionToCompoundDoc;

    //Procedure EnsurePolygonsClockwise;

    //function ValidateFileName(Filename : TFileName): Boolean; virtual;

    // Protected interface used exclusively by TNFFFileBuilder
    //function AddDXFEntity(DXFEntity: TDXFEntity): Boolean;
    //function AddGuidanceAlignment(NamedGuidanceID: TNFFNamedGuidanceID;
    //                              GuidableAlignment: TNFFGuidableAlignmentEntity): Boolean;
    //  procedure SetGridOrigin(AOriginX, AOriginY : Double; RoundTo : Integer);

private void ProcessGuidanceAlignments()
    {
// Patch up references for any NamedGuidanceIDs that reference a Guidance alignment
// by ID but not via. <GuidanceAlignment>
      for (int I = 0; I < NamedGuidanceIDs.Count; I++)
      {
        if (NamedGuidanceIDs[I].GuidanceAlignment == null)
          NamedGuidanceIDs[I].GuidanceAlignment = GuidanceAlignments.Locate(NamedGuidanceIDs[I].ID);
      }

      // Remove any named guidance IDs that don't reference a Guidable Alignment or refer to one
      // that contains no guidance geometry
      for (int I = 0; I < NamedGuidanceIDs.Count; I++)
      {
        if (NamedGuidanceIDs[I].GuidanceAlignment == null)
          NamedGuidanceIDs[I] = null;

        else if (NamedGuidanceIDs[I].GuidanceAlignment.Entities.Count == 0)
        {
          GuidanceAlignments[GuidanceAlignments.IndexOf(NamedGuidanceIDs[I].GuidanceAlignment)] = null;
          NamedGuidanceIDs[I] = null;
        }
      }

      // Remove the Nil items in the lists
      for (int I = NamedGuidanceIDs.Count - 1; I >= 0; I--)
        NamedGuidanceIDs.RemoveAt(I);

      for (int I = GuidanceAlignments.Count - 1; I >= 0; I--)
        GuidanceAlignments.RemoveAt(I);

      // Perform initial sort of the guidance alignments into order based on the
      // offset of the first element in the list. This is so SiteVision can display
      // nice ordered lists of alignments without having to sort them...
      NamedGuidanceIDs.SortByOffset();

      // Now renumber all the guidance IDs to fill in the gaps and make them consistent
      for (int I = 0; I < NamedGuidanceIDs.Count; I++)
        NamedGuidanceIDs[I].ID = I;
    }

//    Procedure SaveXMLDataIslandsToCompoundDoc;
//Procedure LoadXMLDataIslandsFromCompoundDoc;

//Procedure SaveRelatedStreamsToCompoundDoc;
//Procedure LoadRelatedStreamsFromCompoundDoc;

    public bool GuidanceAlignmentsSupported()
    {
      // Guidance Alignments added at NFF version 1.2 at which point they were stored in the SVL file.
      return FileVersion >= TNFFFileVersion.nffVersion1_2 && NFFFileType == TNFFFileType.nffSVLFile;
    }

    //  procedure OnGuidanceAlignmentAdded(Sender: TEnhancedObjectList; Item: TObject); virtual;

    // public
    //    property Grids : TNFFGridList read FGrids;
    //    property ThreeDLineDesignGridFile : TNFF3DGridLineworkFile read F3DLineDesignGridFile;

    //    property GridSize : Integer read FGridSize write SetGridSize;

    //    property GridOriginX : Integer read FGridOriginX;
    //    property GridOriginY : Integer read FGridOriginY;

    //    property Surfaces : TNFFSurfaceList read FSurfaces;
    //    property ReferenceSurfaces : TNFFVerticalOffsetList read FReferenceSurfaces;

    //    Property XMLDataIslands : TNFFXMLDataIslandList read FXMLDataIslands;
    //    Property Gestalt : TNFFGestaltDataIsland read FGestalt write FGestalt;

    //    Property RelatedStreams : TNFFRelatedStreamList read FRelatedStreams;

    //    property AvoidanceZoneType : TNFFAvoidanceZoneType read FAvoidanceZoneType write FAvoidanceZoneType;
    //    Property AvoidanceZoneUndergroundServicesRadius : Double read FAvoidanceZoneUndergroundServicesRadius write FAvoidanceZoneUndergroundServicesRadius;

    //    class function CreateFromFile(AFileName : TFileName) : TNFFFile;

    public virtual bool LoadFromCompoundDoc(MemoryStream ms)
    {
      using (var reader = new BinaryReader(ms, Encoding.Default, true))
      {
        if (!LoadHeaderFromCompoundDoc(ms))
          return false;

        /*
          // Load the grids in the location
          FGrids.LoadFromCompoundDoc('');
    
          if (FFileVersion >= nffVersion1_3) then
            // Load the 3DGridLines grid from the location
            if FStreamInfoList.Locate(F3DLineDesignGridFile.GridCellFileName) <> nil then
              F3DLineDesignGridFile.LoadFromCompoundDoc(F3DLineDesignGridFile.GridCellFileName);
        */

        if (GuidanceAlignmentsSupported())
        {
          // Load the list of guidance alignments from the file
          for (int I = 0; I < NamedGuidanceIDs.Count - 1; I++)
          {
            GuidanceAlignments.Add(new TNFFGuidableAlignmentEntity());
            string AlignmentStreamName = $"Alignment-{NamedGuidanceIDs[I].ID}.lwk";
            GuidanceAlignments.Last().LoadFromCompoundDoc(this, reader, AlignmentStreamName);
          }

          // Call ProcessGuidanceAlignments() to patch up references between NamedGuidanceIDs
          // and associated GuidableAlignments
          ProcessGuidanceAlignments();
        }

        /*
        // Load any related streams
          if (FFileVersion >= nffVersion1_5) then
            begin
              LoadRelatedStreamsFromCompoundDoc;
          end;
    
          if (FFileVersion >= nffVersion1_4) then
          begin
            // Load in any XML data islands that have been placed into the file
            LoadXMLDataIslandsFromCompoundDoc;
    
        // Locate and reference the Gestalt data island if present
        FGestalt:= FXMLDataIslands.Locate(kXMLDataIslandGestalt) as TNFFGestaltDataIsland;
        end;
        */

        //fGridOriginSpecificationAllowed:= False;

        // FFilenameLoadedFrom:= Filename;
      }

      return true;
    }

    public virtual bool LoadFromFile(string Filename)
    {
      using (var ms = new MemoryStream(File.ReadAllBytes(Filename)))
      {
        return LoadFromCompoundDoc(ms);
      }
    }

    public virtual bool LoadHeaderFromCompoundDoc(MemoryStream ms)
    {
      using (var reader = new BinaryReader(ms, Encoding.Default, true))
      {
        // Position the FFileStorage at the start of stream list stream and read it
        reader.BaseStream.Position = 0; // Move to the start of the stream
        reader.BaseStream.Position = reader.ReadInt32(); // Move to the start of the stream list

        // Read in the stream list
        StreamInfoList.LoadFromStream(TNFFFileVersion.nffVersion1_2, reader);

        // Locate the Header stream
        var HeaderStreamInfo = StreamInfoList.Locate(NFFConsts.kNFFIndexStorageName);
        if (HeaderStreamInfo != null)
          reader.BaseStream.Position = HeaderStreamInfo.Offset;
        else
          return false;

        // Load the header index stream...
        return LoadHeaderFromStream(reader);
      }
    }

    public virtual bool LoadHeaderFromFile(string Filename)
    {
      using (var ms = new MemoryStream(File.ReadAllBytes(Filename)))
      {
        return LoadHeaderFromCompoundDoc(ms);
      }
    }

  //  Function LoadGestaltFromFile(const Filename : TFilename) : Boolean; virtual;
 //   procedure DumpToText(TextFilename : TFileName;var ErrorStatus : TNFFErrorStatus;const DataOnly: Boolean = False);
  //  Procedure SaveToFile(Filename : TFileName; var ErrorStatus : TNFFErrorStatus); virtual;

   // Procedure SaveSurfacesAsTTMFiles(Location : TFileName; GroupName : String; CreatedFiles : TStringList = Nil);
   // Procedure ConvertSurfaceToTTM(Grids : TNFFGridList; Surface : TNFFSurface; TTM : TTrimbleTINModel);

   // function SaveLineWorkAsDXFFile(FileName : TFileName; OutputUnits : distance_units_type) : Boolean;

   public bool HasData() => false; // This implementation does not support line work or surface grids

   // Function AddTIN(NewTTM : TTrimbleTINModel; SurfaceName : String) : Boolean;

   // function AddDXF(NewDXF: TDXFOutputFile ImportOption: TNFFImportDXFOption): Boolean; virtual;

   // function AddGuidanceAlignment(NamedGuidanceID: TNFFNamedGuidanceID; GuidableAlignment: TNFFGuidableAlignmentEntity): Boolean;
   // procedure ProcessGuidanceAlignments;

//Function CalcGridOrigin(MinX, MinY, MaxX, MaxY : Double) : Boolean; virtual;
  }
}
