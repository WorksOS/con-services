using System;
using VSS.UnitTest.Common.Contexts;
using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common.EntityBuilder
{
  public class ECMInfoBuilder
  {
    private long _id = IdGen.GetId();
    private bool _isSyncClockMaster = false;
    private bool _hasSMUClock = false;
    private string _engine1SN = IdGen.StringId().TruncateToLength(8);
    private string _transmission1SN = IdGen.StringId().TruncateToLength(8);
    private string _engine2SN;
    private string _transmission2SN;
    private bool _eventProtocolVer = false;
    private bool _diagnosticProtocolVer = false;
    private string _softwarePartNumber = "SOFTWAREPARTNUMBER_" + IdGen.StringId();
    private string _serialNumber = "SERIALNUMBER_" + IdGen.StringId();
    private Device _device = new Device();
    private string _softwareDescription;
    private string _softwareReleaseDate;
    private string _partNumber;

    public virtual ECMInfoBuilder IsSyncClockMaster(bool isSyncClockMaster)
    {
      _isSyncClockMaster = isSyncClockMaster;
      return this;
    }

    public virtual ECMInfoBuilder HasSMUClock(bool hasSMUClock)
    {
      _hasSMUClock = hasSMUClock;
      return this;
    }

    public virtual ECMInfoBuilder Engine1SN(string engine1SN)
    {
      _engine1SN = engine1SN;
      return this;
    }

    public virtual ECMInfoBuilder Transmission1SN(string transmission1SN)
    {
      _transmission1SN = transmission1SN;
      return this;
    }
    
    public virtual ECMInfoBuilder Engine2SN(string engine2SN)
    {
      _engine2SN = engine2SN;
      return this;
    }
    
    public virtual ECMInfoBuilder Transmission2SN(string transmission2SN)
    {
      _transmission2SN = transmission2SN;
      return this;
    }
    
    public virtual ECMInfoBuilder EventProtocolVer(bool eventProtocolVer)
    {
      _eventProtocolVer = eventProtocolVer;
      return this;
    }

    public virtual ECMInfoBuilder DiagnosticProtocolVer(bool diagnosticProtocolVer)
    {
      _diagnosticProtocolVer = diagnosticProtocolVer;
      return this;
    }

    public virtual ECMInfoBuilder SofwarePartNumber(string softwarePartNumber)
    {
      _softwarePartNumber = softwarePartNumber;
      return this;
    }

    public virtual ECMInfoBuilder SerialNumber(string serialNumber)
    {
      _serialNumber = serialNumber;
      return this;
    }

    public virtual ECMInfoBuilder Device(Device device)
    {
      _device = device;
      return this;
    }

    public virtual ECMInfoBuilder SoftwareDescription(string softwareDescription)
    {
      _softwareDescription = softwareDescription;
      return this;
    }

    public virtual ECMInfoBuilder SoftwareReleaseDate(string softwareReleaseDate)
    {
      _softwareReleaseDate = softwareReleaseDate;
      return this;
    }

    public virtual ECMInfoBuilder PartNumber(string partNumber)
    {
      _partNumber = partNumber;
      return this;
    }

    public ECMInfo Build()
    {
      ECMInfo ecmInfo = new ECMInfo();

      ecmInfo.Device = _device;
      ecmInfo.DiagnosticProtocolVer = _diagnosticProtocolVer;
      ecmInfo.Engine1SN = _engine1SN;
      ecmInfo.Engine2SN = _engine2SN ?? String.Empty;
      ecmInfo.EventProtocolVer = _eventProtocolVer;
      ecmInfo.HasSMUClock = _hasSMUClock;
      ecmInfo.ID = _id;
      ecmInfo.IsSyncClockMaster = _isSyncClockMaster;
      ecmInfo.PartNumber = _partNumber ?? String.Empty;
      ecmInfo.SerialNumber = _serialNumber;
      ecmInfo.SoftwareDescription = _softwareDescription ?? String.Empty;
      ecmInfo.SoftwarePartNumber = _softwarePartNumber;
      ecmInfo.SoftwareReleaseDate = _softwareReleaseDate ?? String.Empty;
      ecmInfo.Transmission1SN = _transmission1SN;
      ecmInfo.Transmission2SN = _transmission2SN ?? String.Empty;

      return ecmInfo;
    }

    public virtual ECMInfo Save()
    {
      ECMInfo ecmInfo = Build();

      ContextContainer.Current.OpContext.ECMInfo.AddObject(ecmInfo);
      ContextContainer.Current.OpContext.SaveChanges(); 

      return ecmInfo;
    }

  }
}
