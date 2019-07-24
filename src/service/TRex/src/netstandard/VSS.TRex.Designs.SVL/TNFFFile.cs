using System;
using System.IO;
using System.Runtime.InteropServices;

namespace VSS.TRex.Designs.SVL
{
  public class TNFFFile
  {
    //private
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


      // protected
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
    //procedure ProcessGuidanceAlignments;

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

    // **FIX ME** Only used by NFFLinework.pas but globally visible
 //   property CompoundStorage : IStorage read FCompoundStorage;
//    property FileStorage : TFileStream read FFileStorage;


//    Property XMLDataIslands : TNFFXMLDataIslandList read FXMLDataIslands;
//    Property Gestalt : TNFFGestaltDataIsland read FGestalt write FGestalt;

//    Property RelatedStreams : TNFFRelatedStreamList read FRelatedStreams;

//    property AvoidanceZoneType : TNFFAvoidanceZoneType read FAvoidanceZoneType write FAvoidanceZoneType;
//    Property AvoidanceZoneUndergroundServicesRadius : Double read FAvoidanceZoneUndergroundServicesRadius write FAvoidanceZoneUndergroundServicesRadius;

//    class function CreateFromFile(AFileName : TFileName) : TNFFFile;

    public virtual bool LoadFromFile(string Filename)
    {
      return false;
    }

    public virtual bool LoadHeaderFromFile(string Filename)
    {
      return false;
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
