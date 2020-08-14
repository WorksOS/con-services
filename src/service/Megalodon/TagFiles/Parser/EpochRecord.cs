
/******************************************
 Adding a new tag? update these units and follow the pattern

 TagConstants
    public const string CCV = "CCV";

 TagFile.cs
   private void CreateTagfileDictionary()

 EpochRecord.cs

 AscciiParser.cs

 DictionaryItem.cs

 CreateTagfileDictionary

 TagfileTest.cs
   TestTagFileCreation for testing new tag
 *****************************************/
using TagFiles.Common;

/// <summary>
/// Acts like a state machine for each epoch
/// </summary>
namespace TagFiles.Parser
{

  public class EpochRecord
  {

    public bool HasHeader = false;

    private uint _Time = uint.MaxValue;
    public uint Time
    {
      get => _Time;
      set
      {
        _Time = value;
        // workout time delta
        if (HasPrevTime)
        {
          DeltaTime = value - PrevTime;
          if (DeltaTime < 0)
            DeltaTime = 0;
        }
        PrevTime = value;
      }
    }

    public bool HasTime => _Time != uint.MaxValue;

    public uint DeltaTime { get; set; } = uint.MaxValue;
    public bool HasDeltaTime => DeltaTime != uint.MaxValue;

    public uint PrevTime { get; set; } = uint.MaxValue;
    public bool HasPrevTime => PrevTime != uint.MaxValue;

    public uint Week { get; set; } = uint.MaxValue;
    public bool HasWeek => Week != uint.MaxValue;

    // Left Easting Blade
    public double LEB { get; set; } = double.MaxValue;
    public bool HasLEB => LEB != double.MaxValue;

    // Left Northing Blade
    public double LNB { get; set; } = double.MaxValue;
    public bool HasLNB => LNB != double.MaxValue;

    // Left Elevation Blade
    public double LHB { get; set; } = double.MaxValue;
    public bool HasLHB => LHB != double.MaxValue;

    // Right Easting Blade
    public double REB { get; set; } = double.MaxValue;
    public bool HasREB => REB != double.MaxValue;

    // Right Northing Blade
    public double RNB { get; set; } = double.MaxValue;
    public bool HasRNB => RNB != double.MaxValue;

    // Right Elevation Blade
    public double RHB { get; set; } = double.MaxValue;
    public bool HasRHB => RHB != double.MaxValue;

    /// GPS Mode
    public ushort GPM { get; set; } = ushort.MaxValue;
    public bool HasGPM => GPM != ushort.MaxValue;

    /// Blade On Ground
    public ushort BOG { get; set; } = ushort.MaxValue;
    public bool HasBOG => BOG != ushort.MaxValue;

    public string Design { get; set; } = "";
    public bool HasDesign => Design != "";

    /// Latitude
    public double LAT { get; set; } = double.MaxValue;
    public bool HasLAT => LAT != double.MaxValue;

    /// Longitude
    public double LON { get; set; } = double.MaxValue;
    public bool HasLON => LON != double.MaxValue;

    /// Seed Height
    public double HGT { get; set; } = double.MaxValue;
    public bool HasHGT => HGT != double.MaxValue;

    /// Machine Speed
    public double MSD { get; set; } = double.MaxValue;
    public bool HasMSD => MSD != double.MaxValue;

    /// Machine Type
    public byte MTP { get; set; } = byte.MaxValue;
    public bool HasMTP => HGT != byte.MaxValue;

    /// Heading
    public double HDG { get; set; } = double.MaxValue;
    public bool HasHDG => HDG != double.MaxValue;

    public string RadioType { get; set; } = "";
    public bool HasRadioType => RadioType != "";

    /// Coordinate System
    public ushort CoordSys { get; set; } = ushort.MaxValue;
    public bool HasCoordSys => CoordSys != ushort.MaxValue;

    public ushort ValidPosition { get; set; } = ushort.MaxValue;
    public bool HasValidPosition => ValidPosition != ushort.MaxValue;

    public byte UTM { get; set; } = byte.MaxValue;
    public bool HasUTM => UTM != byte.MaxValue;

    public ushort MappingMode { get; set; } = ushort.MaxValue;
    public bool HasMappingMode => MappingMode != ushort.MaxValue;

    public string AppVersion { get; set; } = "";
    public bool HasAppVersion => AppVersion != "";


    /// Machine ID
    public string LastMID { get; set; } = "";
    private string _MID = "";
    public string MID
    {
      get => _MID;
      set
      {
        _MID = value;
        if (_MID != "")
          LastMID = value; // keep last known value
      }
    }
    public bool HasMID => _MID != "";

    public string LastSerial { get; set; } = "";
    private string _Serial = "";
    public string Serial
    {
      get => _Serial;
      set
      {
        _Serial = value;
        if (_Serial != "")
          LastSerial = value; // keep last known value
      }
    }
    public bool HasSerial => _Serial != "";

    public string RadioSerial { get; set; } = "";
    public bool HasRadioSerial => RadioSerial != "";

    public uint CCV { get; set; } = uint.MaxValue;
    public bool HasCCV => CCV != uint.MaxValue;

    public uint TargetCCV { get; set; } = uint.MaxValue;
    public bool HasTargetCCV => TargetCCV != uint.MaxValue;

    public uint MDP { get; set; } = uint.MaxValue;
    public bool HasMDP => MDP != uint.MaxValue;

    public uint TargetMDP { get; set; } = uint.MaxValue; // 12 bit
    public bool HasTargetMDP => TargetMDP != uint.MaxValue;

    /// Compaction Sensor Type
    public uint CST { get; set; } = uint.MaxValue;
    public bool HasCST => CST != uint.MaxValue;

    public uint Temperature { get; set; } = uint.MaxValue;
    public bool HasTemperature => Temperature != uint.MaxValue;

    /// Machine Direction
    public uint Direction { get; set; } = uint.MaxValue; // 4 bit absolute 1 = forward, 2=Reverse,3=unknown
    public bool HasDirection => Direction != uint.MaxValue;

    /// Contains info like type of compaction and vibe state
    public uint Flags { get; set; } = uint.MaxValue;
    public bool HasFlags => Flags != uint.MaxValue;

    public uint TargetPasses { get; set; } = uint.MaxValue; // 12 bit
    public bool HasTargetPasses => TargetPasses != uint.MaxValue;

    public uint TargetThickness { get; set; } = uint.MaxValue; // 16 bit
    public bool HasTargetThickness => TargetThickness != uint.MaxValue;

    /// Min target temperature
    public uint TempMin { get; set; } = uint.MaxValue; // 12 bit
    public bool HasTempMin => TempMin != uint.MaxValue;

    /// Max target temperature
    public uint TempMax { get; set; } = uint.MaxValue; // 12 bit
    public bool HasTempMax => TempMax != uint.MaxValue;

    public EpochRecord()
    {
    }


    /// <summary>
    /// Clear data but not static header info
    /// </summary>
    public void ClearEpoch()
    {
      HasHeader = false;
      Time = uint.MaxValue;
      DeltaTime = uint.MaxValue;
      PrevTime = uint.MaxValue;
      Week = uint.MaxValue;
      CoordSys = ushort.MaxValue;
      LEB = double.MaxValue;
      LNB = double.MaxValue;
      LHB = double.MaxValue;
      REB = double.MaxValue;
      RNB = double.MaxValue;
      RHB = double.MaxValue;
      GPM = ushort.MaxValue;
      BOG = ushort.MaxValue;
      LAT = double.MaxValue;
      LON = double.MaxValue;
      HGT = double.MaxValue;
      MID = "";
      MSD = double.MaxValue;
      MTP = byte.MaxValue;
      HDG = double.MaxValue;
      Serial = "";
      UTM = byte.MaxValue;
      RadioSerial = "";
      RadioType = "";
      MappingMode = ushort.MaxValue;
      AppVersion = "";
      ValidPosition = ushort.MaxValue;
      CCV = uint.MaxValue;
      MDP = uint.MaxValue;
      CST = uint.MaxValue;
      TargetCCV = uint.MaxValue;
      TargetMDP = uint.MaxValue;
      Flags = uint.MaxValue;
      Temperature = uint.MaxValue;
      Direction = uint.MaxValue;
      TargetThickness = uint.MaxValue;
      TargetPasses = uint.MaxValue;
      TempMin = uint.MaxValue;
      TempMax = uint.MaxValue;
      LastMID = "";
      LastSerial = "";
    }

    public bool IsFullPositionEpoch()
    {
      return HasLEB & HasLHB & HasLNB & HasREB & HasRHB & HasRNB;
    }

    public bool IsHeaderEpoch()
    {
      return HasTime & HasWeek & HasCoordSys & HasAppVersion;
    }

    /// <summary>
    /// Used for an update Epoch
    /// </summary>
    /// <param name="eRec"></param>
    public void EpochCopy(ref EpochRecord eRec)
    {
      Time = eRec.Time;
      LEB = eRec.LEB;
      LNB = eRec.LNB;
      LHB = eRec.LHB;
      REB = eRec.REB;
      RNB = eRec.RNB;
      RHB = eRec.RHB;
      BOG = eRec.BOG;
      HDG = eRec.HDG;
      MSD = eRec.MSD;
      LAT = eRec.LAT;
      LON = eRec.LON;
      HGT = eRec.HGT;
      GPM = eRec.GPM;
      UTM = eRec.UTM;
      MTP = eRec.MTP;
      MID = eRec.MID;
      Serial = eRec.Serial;
      Design = eRec.Design;
      RadioSerial = eRec.RadioSerial;
      RadioType = eRec.RadioType;
      HasHeader = eRec.HasHeader;
      CCV = eRec.CCV;
      CST = eRec.CST;
      TargetCCV = eRec.TargetCCV;
      Flags = eRec.Flags;
      Temperature = eRec.Temperature;
      Direction = eRec.Direction;
      TargetMDP = eRec.TargetMDP;
      MDP = eRec.MDP;
      TargetThickness = eRec.TargetThickness;
      TargetPasses = eRec.TargetPasses;
      TempMin = eRec.TempMin;
      TempMax = eRec.TempMax;
    }

    /// <summary>
    /// Special copy version that keeps track of last known valid values
    /// </summary>
    public void EpochCopyLatestValues(ref EpochRecord eRec)
    {
      HasHeader = eRec.HasHeader;
      if (eRec.Time != uint.MaxValue)
        Time = eRec.Time;
      if (eRec.LEB != double.MaxValue)
        LEB = eRec.LEB;
      if (eRec.LNB != double.MaxValue)
        LNB = eRec.LNB;
      if (eRec.LHB != double.MaxValue)
        LHB = eRec.LHB;
      if (eRec.REB != double.MaxValue)
        REB = eRec.REB;
      if (eRec.RNB != double.MaxValue)
        RNB = eRec.RNB;
      if (eRec.RHB != double.MaxValue)
        RHB = eRec.RHB;
      if (eRec.BOG != ushort.MaxValue)
        BOG = eRec.BOG;
      if (eRec.HDG != double.MaxValue)
        HDG = eRec.HDG;
      if (eRec.MSD != double.MaxValue)
        MSD = eRec.MSD;
      if (eRec.LAT != double.MaxValue)
        LAT = eRec.LAT;
      if (eRec.LON != double.MaxValue)
        LON = eRec.LON;
      if (eRec.HGT != double.MaxValue)
        HGT = eRec.HGT;
      if (eRec.GPM != ushort.MaxValue)
        GPM = eRec.GPM;
      if (eRec.UTM != byte.MaxValue)
        UTM = eRec.UTM;
      if (eRec.MTP != byte.MaxValue)
        MTP = eRec.MTP;
      if (eRec.MID != "")
        MID = eRec.MID;
      if (eRec.Serial != "")
        Serial = eRec.Serial;
      if (eRec.Design != "")
        Design = eRec.Design;
      if (eRec.RadioSerial != "")
        RadioSerial = eRec.RadioSerial;
      if (eRec.RadioType != "")
        RadioType = eRec.RadioType;
      if (eRec.CCV != uint.MaxValue)
        CCV = eRec.CCV;
      if (eRec.CST != uint.MaxValue)
        CST = eRec.CST;
      if (eRec.CCV != uint.MaxValue)
        TargetCCV = eRec.TargetCCV;
      if (eRec.Flags != uint.MaxValue)
        Flags = eRec.Flags;
      if (eRec.Temperature != uint.MaxValue)
        Temperature = eRec.Temperature;
      if (eRec.Direction != uint.MaxValue)
        Direction = eRec.Direction;
      if (eRec.TargetMDP != uint.MaxValue)
        TargetMDP = eRec.TargetMDP;
      if (eRec.MDP != uint.MaxValue)
        MDP = eRec.MDP;
      if (eRec.TargetThickness != uint.MaxValue)
        TargetThickness = eRec.TargetThickness;
      if (eRec.TargetPasses != uint.MaxValue)
        TargetPasses = eRec.TargetPasses;
      if (eRec.TempMin != uint.MaxValue)
        TempMin = eRec.TempMin;
      if (eRec.TempMax != uint.MaxValue)
        TempMax = eRec.TempMax;
    }



    /// <summary>
    /// Has the machine state changed from the ref epoch
    /// </summary>
    public bool MachineStateDifferent(ref EpochRecord eRec)
    {
      if (IsFullPositionEpoch() & eRec.IsFullPositionEpoch())
        return !(LEB == eRec.LEB & LNB == eRec.LNB & LHB == eRec.LHB & REB == eRec.REB & RNB == eRec.RNB & RHB == eRec.RHB & CCV == eRec.CCV & MDP == eRec.MDP & Temperature == eRec.Temperature & BOG == eRec.BOG & ValidPosition == eRec.ValidPosition);
      else
        return true;

    }

    /// <summary>
    /// Does epoch contain any change data
    /// </summary>
    public bool HasUpdateData()
    {
      return HasLEB || HasLHB || HasLNB || HasREB || HasRHB || HasRNB || HasGPM || HasBOG || HasDesign || HasLAT || HasLON ||
             HasHGT || HasMSD || HasHDG || HasCCV || HasCST || HasTargetCCV || HasFlags || HasDirection || HasTemperature ||
             HasMDP || HasTargetMDP || HasTargetPasses || HasTargetThickness || HasTempMin || HasTempMax;

    }

  }
}
