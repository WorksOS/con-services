using System;
/// <summary>
/// Acts like a state machine for each epoch
/// </summary>
namespace TagFiles.Parser
{

  public class EpochRecord
  {

    public bool HasHeader = false;
    public bool HasTime = false;
    public bool HasDeltaTime = false;
    public bool HasPrevTime = false;
    public bool HasWeek = false;
    public bool HasCoordSys = false;
    public bool HasUTMZone = false;
    public bool HasLEB = false;
    public bool HasLNB = false;
    public bool HasLHB = false;
    public bool HasREB = false;
    public bool HasRNB = false;
    public bool HasRHB = false;
    public bool HasGPM = false;
    public bool HasBOG = false;
    public bool HasDES = false;
    public bool HasLAT = false;
    public bool HasLON = false;
    public bool HasHGT = false;
    public bool HasMID = false;
    public bool HasMSD = false;
    public bool HasMTP = false;
    public bool HasHDG = false;
    public bool HasSER = false;
    public bool HasUTM = false;
    public bool HasRadioSerial = false;
    public bool HasRadioType = false;
    public bool HasMappingMode = false;
    public bool HasAppVersion = false;

    // Fields

    /// <summary>
    /// Time
    /// </summary>
    private uint _Time = 0;
    public uint Time
    {
      get => _Time;
      set
      {
        _Time = value;
        // workout time delta
        if (HasPrevTime)
          DeltaTime = value - _PrevTime;
        PrevTime = value;
        HasTime = true;
      }
    }

    private uint _DeltaTime = 0;
    public uint DeltaTime
    {
      get => _DeltaTime;
      set
      {
        _DeltaTime = value;
        HasDeltaTime = true;
      }
    }

    /// <summary>
    /// PrevTime
    /// </summary>
    private uint _PrevTime = 0;
    public uint PrevTime
    {
      get => _PrevTime;
      set
      {
        _PrevTime = value;
        HasPrevTime = true;
      }
    }

    /// <summary>
    /// Week
    /// </summary>
    private uint _Week = 0;
    public uint Week
    {
      get => _Week;
      set
      {
        _Week = value;
        HasWeek = true;
      }
    }

    /// <summary>
    /// Left Easting Blade
    /// </summary>
    private double _LEB = 0;
    public double LEB
    {
      get => _LEB;
      set
      {
        _LEB = value;
        HasLEB = true;
      }
    }

    /// <summary>
    /// Left Northing Blade
    /// </summary>
    private double _LNB = 0;
    public double LNB
    {
      get => _LNB;
      set
      {
        _LNB = value;
        HasLNB = true;
      }
    }

    /// <summary>
    /// Left Height Blade
    /// </summary>
    private double _LHB = 0;
    public double LHB
    {
      get => _LHB;
      set
      {
        _LHB = value;
        HasLHB = true;
      }
    }

    /// <summary>
    /// Right Easting Blade
    /// </summary>
    private double _REB = 0;
    public double REB
    {
      get => _REB;
      set
      {
        _REB = value;
        HasREB = true;
      }
    }

    /// <summary>
    /// Righ Northing Blade
    /// </summary>
    private double _RNB = 0;
    public double RNB
    {
      get => _RNB;
      set
      {
        _RNB = value;
        HasRNB = true;
      }
    }

    /// <summary>
    /// Right Height Blade
    /// </summary>
    private double _RHB = 0;
    public double RHB
    {
      get => _RHB;
      set
      {
        _RHB = value;
        HasRHB = true;
      }
    }

    /// <summary>
    /// GPS Mode
    /// </summary>
    private ushort _GPM = 0;
    public ushort GPM
    {
      get => _GPM;
      set
      {
        _GPM = value;
        HasGPM = true;
      }
    }

    /// <summary>
    /// Blade On Ground
    /// </summary>
    private ushort _BOG = 0;
    public ushort BOG
    {
      get => _BOG;
      set
      {
        _BOG = value;
        HasBOG = true;
      }
    }

    /// <summary>
    /// Design
    /// </summary>
    private string _Design = "";
    public string Design
    {
      get => _Design;
      set
      {
        _Design = value;
        HasDES = true;
      }
    }

    /// <summary>
    /// Latitude
    /// </summary>
    private double _LAT = 0;
    public double LAT
    {
      get => _LAT;
      set
      {
        _LAT = value;
        HasLAT = true;
      }
    }

    /// <summary>
    /// Longitude
    /// </summary>
    private double _LON = 0;
    public double LON
    {
      get => _LON;
      set
      {
        _LON = value;
        HasLON = true;
      }
    }

    /// <summary>
    /// Height
    /// </summary>
    private double _HGT = 0;
    public double HGT
    {
      get => _HGT;
      set
      {
        _HGT = value;
        HasHGT = true;
      }
    }

    /// <summary>
    /// Machine ID
    /// </summary>
    private string _MID = "";
    public string MID
    {
      get => _MID;
      set
      {
        _MID = value;
        HasMID = true;
      }
    }

    /// <summary>
    /// Machine Speed
    /// </summary>
    private double _MSD = 0;
    public double MSD
    {
      get => _MSD;
      set
      {
        _MSD = value;
        HasMSD = true;
      }
    }

    /// <summary>
    /// Machine Type
    /// </summary>
    private byte _MTP = 0;
    public byte MTP
    {
      get => _MTP;
      set
      {
        _MTP = value;
        HasMTP = true;
      }
    }

    /// <summary>
    /// Heading
    /// </summary>
    private double _HDG = 0;
    public double HDG
    {
      get => _HDG;
      set
      {
        _HDG = value;
        HasHDG = true;
      }
    }

    /// <summary>
    /// Radio Serial
    /// </summary>
    private string _RadioSerial = "";
    public string RadioSerial
    {
      get => _RadioSerial;
      set
      {
        _RadioSerial = value;
        HasRadioSerial = true;
      }
    }

    /// <summary>
    /// Radio Type
    /// </summary>
    private string _RadioType = "torch";
    public string RadioType
    {
      get => _RadioType;
      set
      {
        _RadioType = value;
        HasRadioType = true;
      }
    }

    /// <summary>
    /// Coordinate System
    /// </summary>
    private ushort _CoordSys = 0;
    public ushort CoordSys
    {
      get => _CoordSys;
      set
      {
        _CoordSys = value;
        HasCoordSys = true;
      }
    }

    /// <summary>
    /// UTM Zone
    /// </summary>
    private byte _UTM = 0;
    public byte UTM
    {
      get => _UTM;
      set
      {
        _UTM = value;
        HasUTMZone = true;
      }
    }


    /// <summary>
    /// Mapping Mode
    /// </summary>
    private ushort _MappingMode = 0;
    public ushort MappingMode
    {
      get => _MappingMode;
      set
      {
        _MappingMode = value;
        HasMappingMode = true;
      }
    }


    /// <summary>
    /// Application Version
    /// </summary>
    private string _AppVersion = "";
    public string AppVersion
    {
      get => _AppVersion;
      set
      {
        _AppVersion = value;
        HasAppVersion = true;
      }
    }

    /// <summary>
    /// Serial
    /// </summary>
    private string _Serial = "";
    public string Serial
    {
      get => _Serial;
      set
      {
        _Serial = value;
        HasSER = true;
      }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public EpochRecord()
    {
      ClearEpoch();
    }

    /// <summary>
    /// Clear data but not static header info
    /// </summary>
    public void ClearEpoch()
    {
      HasHeader = false;
      HasTime = false;
      HasDeltaTime = false;
      HasPrevTime = false;
      HasWeek = false;
      HasCoordSys = false;
      HasUTMZone = false;
      HasLEB = false;
      HasLNB = false;
      HasLHB = false;
      HasREB = false;
      HasRNB = false;
      HasRHB = false;
      HasGPM = false;
      HasBOG = false;
      HasLAT = false;
      HasLON = false;
      HasHGT = false;
      HasMID = false;
      HasMSD = false;
      HasMTP = false;
      HasHDG = false;
      HasSER = false;
      HasUTM = false;
      HasRadioSerial = false;
      HasRadioType = false;
      HasMappingMode = false;
      HasAppVersion = false;
    }

  }
}
