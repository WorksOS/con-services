using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using VSS.Hosted.VLCommon;
using Microsoft.SqlServer.Server;
using System.Data;

namespace VSS.Hosted.VLCommon
{
  public partial class CAT_SMULoc : INHOEMDataObject, ICloneable
  {
    private static SqlMetaData[] metadata = new SqlMetaData[] { 
        new SqlMetaData("MessageID", SqlDbType.BigInt),
        new SqlMetaData("MasterMsgID", SqlDbType.BigInt),
        new SqlMetaData("EventUTC", SqlDbType.DateTime2),
        new SqlMetaData("SerialNumber", SqlDbType.NVarChar, 24),
        new SqlMetaData("MakeCode", SqlDbType.NVarChar, 3),
        new SqlMetaData("GpsDeviceID", SqlDbType.NVarChar, 50),
        new SqlMetaData("DeviceTypeID", SqlDbType.Int), 
        new SqlMetaData("SMU", SqlDbType.Float),
        new SqlMetaData("Latitude", SqlDbType.Float),
        new SqlMetaData("Longitude", SqlDbType.Float),
        new SqlMetaData("LocIsValid", SqlDbType.Bit),
        new SqlMetaData("RecordID", SqlDbType.BigInt),
      };

    public Microsoft.SqlServer.Server.SqlDataRecord ToSqlDataRecord()
    {
      SqlDataRecord record = new SqlDataRecord(metadata);

      record.SetValue(record.GetOrdinal("MessageID"), this.MessageID);
      record.SetValue(record.GetOrdinal("MasterMsgID"), this.MasterMsgID);
      record.SetValue(record.GetOrdinal("EventUTC"), this.EventUTC);
      record.SetValue(record.GetOrdinal("SerialNumber"), this.SerialNumber);
      record.SetValue(record.GetOrdinal("MakeCode"), this.MakeCode);
      record.SetValue(record.GetOrdinal("GpsDeviceID"), this.GpsDeviceID);
      record.SetValue(record.GetOrdinal("DeviceTypeID"), this.DeviceTypeID);
      record.SetValue(record.GetOrdinal("SMU"), this.SMU);
      record.SetValue(record.GetOrdinal("Latitude"), this.Latitude);
      record.SetValue(record.GetOrdinal("Longitude"), this.Longitude);
      record.SetValue(record.GetOrdinal("LocIsValid"), this.LocIsValid);
      record.SetValue(record.GetOrdinal("RecordID"), 0L);

      return record;
    }

    public XElement ToXElement(string oemName)
    {
      XElement itemXML = new XElement("CAT_SMULoc");
      itemXML.SetAttributeValue("InsertUTC", InsertUTC);
      itemXML.SetAttributeValue("MessageID", MessageID);
      itemXML.SetAttributeValue("MasterMsgID", MasterMsgID);
      itemXML.SetAttributeValue("EventUTC", EventUTC);
      itemXML.SetAttributeValue("SerialNumber", SerialNumber);
      itemXML.SetAttributeValue("MakeCode", MakeCode);
      itemXML.SetAttributeValue("GpsDeviceID", GpsDeviceID);
      itemXML.SetAttributeValue("DeviceTypeID", DeviceTypeID);
      itemXML.SetAttributeValue("SMU", SMU);
      itemXML.SetAttributeValue("Latitude", Latitude);
      itemXML.SetAttributeValue("Longitude", Longitude);
      itemXML.SetAttributeValue("LocIsValid", LocIsValid);
      itemXML.SetAttributeValue("RecordID", 0L);

      return itemXML;
    }

    public object Clone()
    {
      return this.MemberwiseClone() as INHOEMDataObject;
    }
  }

  public partial class CAT_StartStop : INHOEMDataObject, ICloneable
  {
    private static SqlMetaData[] metadata = new SqlMetaData[] { 
        new SqlMetaData("MessageID", SqlDbType.BigInt),
        new SqlMetaData("MasterMsgID", SqlDbType.BigInt),
        new SqlMetaData("EventUTC", SqlDbType.DateTime2),
        new SqlMetaData("SerialNumber", SqlDbType.NVarChar, 24),
        new SqlMetaData("MakeCode", SqlDbType.NVarChar, 3),
        new SqlMetaData("GpsDeviceID", SqlDbType.NVarChar, 50),
        new SqlMetaData("DeviceTypeID", SqlDbType.Int), 
        new SqlMetaData("StartUTC", SqlDbType.DateTime2),
        new SqlMetaData("StopUTC", SqlDbType.DateTime2),
        new SqlMetaData("RecordID", SqlDbType.BigInt),
      };

    public Microsoft.SqlServer.Server.SqlDataRecord ToSqlDataRecord()
    {
      SqlDataRecord record = new SqlDataRecord(metadata);

      record.SetValue(record.GetOrdinal("MessageID"), this.MessageID);
      record.SetValue(record.GetOrdinal("MasterMsgID"), this.MasterMsgID);
      record.SetValue(record.GetOrdinal("EventUTC"), this.EventUTC);
      record.SetValue(record.GetOrdinal("SerialNumber"), this.SerialNumber);
      record.SetValue(record.GetOrdinal("MakeCode"), this.MakeCode);
      record.SetValue(record.GetOrdinal("GpsDeviceID"), this.GpsDeviceID);
      record.SetValue(record.GetOrdinal("DeviceTypeID"), this.DeviceTypeID);
      record.SetValue(record.GetOrdinal("StartUTC"), this.StartUTC);
      record.SetValue(record.GetOrdinal("StopUTC"), this.StopUTC);
      record.SetValue(record.GetOrdinal("RecordID"), 0L);

      return record;
    }

    public XElement ToXElement(string oemName)
    {
      XElement itemXML = new XElement("CAT_StartStop");
      itemXML.SetAttributeValue("InsertUTC", InsertUTC);
      itemXML.SetAttributeValue("MessageID", MessageID);
      itemXML.SetAttributeValue("MasterMsgID", MasterMsgID);
      itemXML.SetAttributeValue("EventUTC", EventUTC);
      itemXML.SetAttributeValue("SerialNumber", SerialNumber);
      itemXML.SetAttributeValue("MakeCode", MakeCode);
      itemXML.SetAttributeValue("GpsDeviceID", GpsDeviceID);
      itemXML.SetAttributeValue("DeviceTypeID", DeviceTypeID);
      itemXML.SetAttributeValue("StartUTC", StartUTC);
      itemXML.SetAttributeValue("StopUTC", StopUTC);
      itemXML.SetAttributeValue("RecordID", 0L);
      return itemXML;
    }

    public object Clone()
    {
      return this.MemberwiseClone() as INHOEMDataObject;
    }
  }

  public partial class CAT_Fuel : INHOEMDataObject, ICloneable
  {
    private static SqlMetaData[] metadata = new SqlMetaData[] { 
        new SqlMetaData("MessageID", SqlDbType.BigInt),
        new SqlMetaData("MasterMsgID", SqlDbType.BigInt),
        new SqlMetaData("EventUTC", SqlDbType.DateTime2),
        new SqlMetaData("SerialNumber", SqlDbType.NVarChar, 24),
        new SqlMetaData("MakeCode", SqlDbType.NVarChar, 3),
        new SqlMetaData("GpsDeviceID", SqlDbType.NVarChar, 50),
        new SqlMetaData("DeviceTypeID", SqlDbType.Int), 
        new SqlMetaData("ConsumptionOneEighthGallons", SqlDbType.Float),
        new SqlMetaData("LevelPercent", SqlDbType.Float),
        new SqlMetaData("RecordID", SqlDbType.BigInt),
      };

    public Microsoft.SqlServer.Server.SqlDataRecord ToSqlDataRecord()
    {
      SqlDataRecord record = new SqlDataRecord(metadata);

      record.SetValue(record.GetOrdinal("MessageID"), this.MessageID);
      record.SetValue(record.GetOrdinal("MasterMsgID"), this.MasterMsgID);
      record.SetValue(record.GetOrdinal("EventUTC"), this.EventUTC);
      record.SetValue(record.GetOrdinal("SerialNumber"), this.SerialNumber);
      record.SetValue(record.GetOrdinal("MakeCode"), this.MakeCode);
      record.SetValue(record.GetOrdinal("GpsDeviceID"), this.GpsDeviceID);
      record.SetValue(record.GetOrdinal("DeviceTypeID"), this.DeviceTypeID);
      record.SetValue(record.GetOrdinal("ConsumptionOneEighthGallons"), this.ConsumptionOneEighthGallons);
      record.SetValue(record.GetOrdinal("LevelPercent"), this.LevelPercent);
      record.SetValue(record.GetOrdinal("RecordID"), 0L);

      return record;
    }


    public XElement ToXElement(string oemName)
    {
      XElement itemXML = new XElement("CAT_Fuel");
      itemXML.SetAttributeValue("InsertUTC", InsertUTC);
      itemXML.SetAttributeValue("MessageID", MessageID);
      itemXML.SetAttributeValue("MasterMsgID", MasterMsgID);
      itemXML.SetAttributeValue("EventUTC", EventUTC);
      itemXML.SetAttributeValue("SerialNumber", SerialNumber);
      itemXML.SetAttributeValue("MakeCode", MakeCode);
      itemXML.SetAttributeValue("GpsDeviceID", GpsDeviceID);
      itemXML.SetAttributeValue("DeviceTypeID", DeviceTypeID);
      itemXML.SetAttributeValue("ConsumptionOneEighthGallons", ConsumptionOneEighthGallons);
      itemXML.SetAttributeValue("LevelPercent", LevelPercent);
      itemXML.SetAttributeValue("RecordID", 0L);

      return itemXML;
    }

    public object Clone()
    {
      return this.MemberwiseClone() as INHOEMDataObject;
    }
  }

  public partial class CAT_Engine : INHOEMDataObject, ICloneable
  {
    private static SqlMetaData[] metadata = new SqlMetaData[] { 
        new SqlMetaData("MessageID", SqlDbType.BigInt),
        new SqlMetaData("MasterMsgID", SqlDbType.BigInt),
        new SqlMetaData("EventUTC", SqlDbType.DateTime2),
        new SqlMetaData("SerialNumber", SqlDbType.NVarChar, 24),
        new SqlMetaData("MakeCode", SqlDbType.NVarChar, 3),
        new SqlMetaData("GpsDeviceID", SqlDbType.NVarChar, 50),
        new SqlMetaData("DeviceTypeID", SqlDbType.Int), 
        new SqlMetaData("MaxFuelOneEighthGallons", SqlDbType.Float),
        new SqlMetaData("IdleFuelOneEighthGallons", SqlDbType.Float),
        new SqlMetaData("EngineIdleOneTwentiethHours", SqlDbType.Float),
        new SqlMetaData("Starts", SqlDbType.Int),
        new SqlMetaData("Revolutions", SqlDbType.BigInt),
        new SqlMetaData("RecordID", SqlDbType.BigInt),
      };

    public Microsoft.SqlServer.Server.SqlDataRecord ToSqlDataRecord()
    {
      SqlDataRecord record = new SqlDataRecord(metadata);

      record.SetValue(record.GetOrdinal("MessageID"), this.MessageID);
      record.SetValue(record.GetOrdinal("MasterMsgID"), this.MasterMsgID);
      record.SetValue(record.GetOrdinal("EventUTC"), this.EventUTC);
      record.SetValue(record.GetOrdinal("SerialNumber"), this.SerialNumber);
      record.SetValue(record.GetOrdinal("MakeCode"), this.MakeCode);
      record.SetValue(record.GetOrdinal("GpsDeviceID"), this.GpsDeviceID);
      record.SetValue(record.GetOrdinal("DeviceTypeID"), this.DeviceTypeID);
      record.SetValue(record.GetOrdinal("MaxFuelOneEighthGallons"), this.MaxFuelOneEighthGallons);
      record.SetValue(record.GetOrdinal("IdleFuelOneEighthGallons"), this.IdleFuelOneEighthGallons);
      record.SetValue(record.GetOrdinal("EngineIdleOneTwentiethHours"), this.EngineIdleOneTwentiethHours);
      record.SetValue(record.GetOrdinal("Starts"), this.Starts);
      record.SetValue(record.GetOrdinal("Revolutions"), this.Revolutions);
      record.SetValue(record.GetOrdinal("RecordID"), 0L);

      return record;
    }

    public XElement ToXElement(string oemName)
    {
      XElement itemXML = new XElement("CAT_Engine");
      itemXML.SetAttributeValue("InsertUTC", InsertUTC);
      itemXML.SetAttributeValue("MessageID", MessageID);
      itemXML.SetAttributeValue("MasterMsgID", MasterMsgID);
      itemXML.SetAttributeValue("EventUTC", EventUTC);
      itemXML.SetAttributeValue("SerialNumber", SerialNumber);
      itemXML.SetAttributeValue("MakeCode", MakeCode);
      itemXML.SetAttributeValue("GpsDeviceID", GpsDeviceID);
      itemXML.SetAttributeValue("DeviceTypeID", DeviceTypeID);
      itemXML.SetAttributeValue("MaxFuelOneEighthGallons", MaxFuelOneEighthGallons);
      itemXML.SetAttributeValue("IdleFuelOneEighthGallons", IdleFuelOneEighthGallons);
      itemXML.SetAttributeValue("EngineIdleOneTwentiethHours", EngineIdleOneTwentiethHours);
      itemXML.SetAttributeValue("Starts", Starts);
      itemXML.SetAttributeValue("Revolutions", Revolutions);
      itemXML.SetAttributeValue("RecordID", 0L);

      return itemXML;
    }

    public object Clone()
    {
      return this.MemberwiseClone() as INHOEMDataObject;
    }
  }

  public partial class CAT_PayloadCycle : INHOEMDataObject, ICloneable
  {
    private static SqlMetaData[] metadata = new SqlMetaData[] { 
        new SqlMetaData("MessageID", SqlDbType.BigInt),
        new SqlMetaData("MasterMsgID", SqlDbType.BigInt),
        new SqlMetaData("EventUTC", SqlDbType.DateTime2),
        new SqlMetaData("SerialNumber", SqlDbType.NVarChar, 24),
        new SqlMetaData("MakeCode", SqlDbType.NVarChar, 3),
        new SqlMetaData("GpsDeviceID", SqlDbType.NVarChar, 50),
        new SqlMetaData("DeviceTypeID", SqlDbType.Int), 
        new SqlMetaData("Payload", SqlDbType.BigInt),
        new SqlMetaData("Utilization", SqlDbType.BigInt),
        new SqlMetaData("Cycle", SqlDbType.BigInt),        
      };

    public Microsoft.SqlServer.Server.SqlDataRecord ToSqlDataRecord()
    {
      SqlDataRecord record = new SqlDataRecord(metadata);

      record.SetValue(record.GetOrdinal("MessageID"), this.MessageID);
      record.SetValue(record.GetOrdinal("MasterMsgID"), this.MasterMsgID);
      record.SetValue(record.GetOrdinal("EventUTC"), this.EventUTC);
      record.SetValue(record.GetOrdinal("SerialNumber"), this.SerialNumber);
      record.SetValue(record.GetOrdinal("MakeCode"), this.MakeCode);
      record.SetValue(record.GetOrdinal("GpsDeviceID"), this.GpsDeviceID);
      record.SetValue(record.GetOrdinal("DeviceTypeID"), this.DeviceTypeID);
      record.SetValue(record.GetOrdinal("Payload"), this.Payload);
      record.SetValue(record.GetOrdinal("Utilization"), this.Utilization);
      record.SetValue(record.GetOrdinal("Cycle"), this.Cycle);     

      return record;
    }

    public XElement ToXElement(string oemName)
    {
      XElement itemXML = new XElement("CAT_Engine");
      itemXML.SetAttributeValue("InsertUTC", InsertUTC);
      itemXML.SetAttributeValue("MessageID", MessageID);
      itemXML.SetAttributeValue("MasterMsgID", MasterMsgID);
      itemXML.SetAttributeValue("EventUTC", EventUTC);
      itemXML.SetAttributeValue("SerialNumber", SerialNumber);
      itemXML.SetAttributeValue("MakeCode", MakeCode);
      itemXML.SetAttributeValue("GpsDeviceID", GpsDeviceID);
      itemXML.SetAttributeValue("DeviceTypeID", DeviceTypeID);
      itemXML.SetAttributeValue("Payload", Payload);
      itemXML.SetAttributeValue("Utilization", Utilization);      
      itemXML.SetAttributeValue("Cycle", Cycle);      

      return itemXML;
    }

    public object Clone()
    {
      return this.MemberwiseClone() as INHOEMDataObject;
    }
  }

  public partial class CAT_Diagnostic : INHOEMDataObject, ICloneable
  {
    private static SqlMetaData[] metadata = new SqlMetaData[] { 
        new SqlMetaData("MessageID", SqlDbType.BigInt),
        new SqlMetaData("MasterMsgID", SqlDbType.BigInt),
        new SqlMetaData("EventUTC", SqlDbType.DateTime2),
        new SqlMetaData("SerialNumber", SqlDbType.NVarChar, 24),
        new SqlMetaData("MakeCode", SqlDbType.NVarChar, 3),
        new SqlMetaData("GpsDeviceID", SqlDbType.NVarChar, 50),
        new SqlMetaData("DeviceTypeID", SqlDbType.Int), 
        new SqlMetaData("DatalinkID", SqlDbType.Int),
        new SqlMetaData("MID", SqlDbType.BigInt),
        new SqlMetaData("CID", SqlDbType.Int),
        new SqlMetaData("FMI", SqlDbType.Int),
        new SqlMetaData("Occurrences", SqlDbType.TinyInt),
        new SqlMetaData("SeverityLevelID", SqlDbType.Int),
        new SqlMetaData("RecordID", SqlDbType.BigInt),
      };

    public Microsoft.SqlServer.Server.SqlDataRecord ToSqlDataRecord()
    {
      SqlDataRecord record = new SqlDataRecord(metadata);

      record.SetValue(record.GetOrdinal("MessageID"), this.MessageID);
      record.SetValue(record.GetOrdinal("MasterMsgID"), this.MasterMsgID);
      record.SetValue(record.GetOrdinal("EventUTC"), this.EventUTC);
      record.SetValue(record.GetOrdinal("SerialNumber"), this.SerialNumber);
      record.SetValue(record.GetOrdinal("MakeCode"), this.MakeCode);
      record.SetValue(record.GetOrdinal("GpsDeviceID"), this.GpsDeviceID);
      record.SetValue(record.GetOrdinal("DeviceTypeID"), this.DeviceTypeID);
      record.SetValue(record.GetOrdinal("DatalinkID"), this.DatalinkID);
      record.SetValue(record.GetOrdinal("MID"), this.MID);
      record.SetValue(record.GetOrdinal("CID"), this.CID);
      record.SetValue(record.GetOrdinal("FMI"), this.FMI);
      record.SetValue(record.GetOrdinal("Occurrences"), this.Occurrences);
      record.SetValue(record.GetOrdinal("SeverityLevelID"), this.SeverityLevelID);
      record.SetValue(record.GetOrdinal("RecordID"), 0L);

      return record;
    }

    public XElement ToXElement(string oemName)
    {
      XElement itemXML = new XElement("CAT_Diagnostic");
      itemXML.SetAttributeValue("InsertUTC", InsertUTC);
      itemXML.SetAttributeValue("MessageID", MessageID);
      itemXML.SetAttributeValue("MasterMsgID", MasterMsgID);
      itemXML.SetAttributeValue("EventUTC", EventUTC);
      itemXML.SetAttributeValue("SerialNumber", SerialNumber);
      itemXML.SetAttributeValue("MakeCode", MakeCode);
      itemXML.SetAttributeValue("GpsDeviceID", GpsDeviceID);
      itemXML.SetAttributeValue("DeviceTypeID", DeviceTypeID);
      itemXML.SetAttributeValue("DatalinkID", DatalinkID);
      itemXML.SetAttributeValue("MID", MID);
      itemXML.SetAttributeValue("CID", CID);
      itemXML.SetAttributeValue("FMI", FMI);
      itemXML.SetAttributeValue("Occurrences", Occurrences);
      itemXML.SetAttributeValue("SeverityLevelID", SeverityLevelID);
      itemXML.SetAttributeValue("RecordID", 0L);
      return itemXML;
    }

    public object Clone()
    {
      return this.MemberwiseClone() as INHOEMDataObject;
    }
  }

  public partial class CAT_Event : INHOEMDataObject, ICloneable
  {
    private static SqlMetaData[] metadata = new SqlMetaData[] { 
        new SqlMetaData("MessageID", SqlDbType.BigInt),
        new SqlMetaData("MasterMsgID", SqlDbType.BigInt),
        new SqlMetaData("EventUTC", SqlDbType.DateTime2),
        new SqlMetaData("SerialNumber", SqlDbType.NVarChar, 24),
        new SqlMetaData("MakeCode", SqlDbType.NVarChar, 3),
        new SqlMetaData("GpsDeviceID", SqlDbType.NVarChar, 50),
        new SqlMetaData("DeviceTypeID", SqlDbType.Int), 
        new SqlMetaData("DatalinkID", SqlDbType.Int),
        new SqlMetaData("MID", SqlDbType.BigInt),
        new SqlMetaData("EID", SqlDbType.Int),
        new SqlMetaData("Occurrences", SqlDbType.TinyInt),
        new SqlMetaData("SeverityLevelID", SqlDbType.Int),
        new SqlMetaData("RecordID", SqlDbType.BigInt),
      };

    public Microsoft.SqlServer.Server.SqlDataRecord ToSqlDataRecord()
    {
      SqlDataRecord record = new SqlDataRecord(metadata);

      record.SetValue(record.GetOrdinal("MessageID"), this.MessageID);
      record.SetValue(record.GetOrdinal("MasterMsgID"), this.MasterMsgID);
      record.SetValue(record.GetOrdinal("EventUTC"), this.EventUTC);
      record.SetValue(record.GetOrdinal("SerialNumber"), this.SerialNumber);
      record.SetValue(record.GetOrdinal("MakeCode"), this.MakeCode);
      record.SetValue(record.GetOrdinal("GpsDeviceID"), this.GpsDeviceID);
      record.SetValue(record.GetOrdinal("DeviceTypeID"), this.DeviceTypeID);
      record.SetValue(record.GetOrdinal("DatalinkID"), this.DatalinkID);
      record.SetValue(record.GetOrdinal("MID"), this.MID);
      record.SetValue(record.GetOrdinal("EID"), this.EID);
      record.SetValue(record.GetOrdinal("Occurrences"), this.Occurrences);
      record.SetValue(record.GetOrdinal("SeverityLevelID"), this.SeverityLevelID);
      record.SetValue(record.GetOrdinal("RecordID"), 0L);

      return record;
    }

    public XElement ToXElement(string oemName)
    {
      XElement itemXML = new XElement("CAT_Event");
      itemXML.SetAttributeValue("InsertUTC", InsertUTC);
      itemXML.SetAttributeValue("MessageID", MessageID);
      itemXML.SetAttributeValue("MasterMsgID", MasterMsgID);
      itemXML.SetAttributeValue("EventUTC", EventUTC);
      itemXML.SetAttributeValue("SerialNumber", SerialNumber);
      itemXML.SetAttributeValue("MakeCode", MakeCode);
      itemXML.SetAttributeValue("GpsDeviceID", GpsDeviceID);
      itemXML.SetAttributeValue("DeviceTypeID", DeviceTypeID);
      itemXML.SetAttributeValue("DatalinkID", DatalinkID);
      itemXML.SetAttributeValue("MID", MID);
      itemXML.SetAttributeValue("EID", EID);
      itemXML.SetAttributeValue("Occurrences", Occurrences);
      itemXML.SetAttributeValue("SeverityLevelID", SeverityLevelID);
      itemXML.SetAttributeValue("RecordID", 0L);
      return itemXML;
    }

    public object Clone()
    {
      return this.MemberwiseClone() as INHOEMDataObject;
    }
  }

  public partial class CAT_FenceAlert : INHOEMDataObject, ICloneable
  {
    private static SqlMetaData[] metadata = new SqlMetaData[] { 
        new SqlMetaData("MessageID", SqlDbType.BigInt),
        new SqlMetaData("MasterMsgID", SqlDbType.BigInt),
        new SqlMetaData("EventUTC", SqlDbType.DateTime2),
        new SqlMetaData("SerialNumber", SqlDbType.NVarChar, 24),
        new SqlMetaData("MakeCode", SqlDbType.NVarChar, 3),
        new SqlMetaData("GpsDeviceID", SqlDbType.NVarChar, 50),
        new SqlMetaData("DeviceTypeID", SqlDbType.Int), 
        new SqlMetaData("TimeWatchActive", SqlDbType.Bit),
        new SqlMetaData("ExclusiveWatchActive", SqlDbType.Bit),
        new SqlMetaData("InclusiveWatchActive", SqlDbType.Bit),
        new SqlMetaData("TimeWatchAlarm", SqlDbType.Bit),
        new SqlMetaData("ExclusiveWatchAlarm", SqlDbType.Bit),
        new SqlMetaData("InclusiveWatchAlarm", SqlDbType.Bit),
        new SqlMetaData("SatelliteBlockage", SqlDbType.Bit),
        new SqlMetaData("DisconnectSwitchUsed", SqlDbType.Bit),
        new SqlMetaData("RecordID", SqlDbType.BigInt),
      };

    public Microsoft.SqlServer.Server.SqlDataRecord ToSqlDataRecord()
    {
      SqlDataRecord record = new SqlDataRecord(metadata);

      record.SetValue(record.GetOrdinal("MessageID"), this.MessageID);
      record.SetValue(record.GetOrdinal("MasterMsgID"), this.MasterMsgID);
      record.SetValue(record.GetOrdinal("EventUTC"), this.EventUTC);
      record.SetValue(record.GetOrdinal("SerialNumber"), this.SerialNumber);
      record.SetValue(record.GetOrdinal("MakeCode"), this.MakeCode);
      record.SetValue(record.GetOrdinal("GpsDeviceID"), this.GpsDeviceID);
      record.SetValue(record.GetOrdinal("DeviceTypeID"), this.DeviceTypeID);
      record.SetValue(record.GetOrdinal("TimeWatchActive"), this.TimeWatchActive);
      record.SetValue(record.GetOrdinal("ExclusiveWatchActive"), this.ExclusiveWatchActive);
      record.SetValue(record.GetOrdinal("InclusiveWatchActive"), this.InclusiveWatchActive);
      record.SetValue(record.GetOrdinal("TimeWatchAlarm"), this.TimeWatchAlarm);
      record.SetValue(record.GetOrdinal("ExclusiveWatchAlarm"), this.ExclusiveWatchAlarm);
      record.SetValue(record.GetOrdinal("InclusiveWatchAlarm"), this.InclusiveWatchAlarm);
      record.SetValue(record.GetOrdinal("SatelliteBlockage"), this.SatelliteBlockage);
      record.SetValue(record.GetOrdinal("DisconnectSwitchUsed"), this.DisconnectSwitchUsed);
      record.SetValue(record.GetOrdinal("RecordID"), 0L);

      return record;
    }

    public XElement ToXElement(string oemName)
    {
      XElement itemXML = new XElement("CAT_FenceAlert");
      itemXML.SetAttributeValue("InsertUTC", InsertUTC);
      itemXML.SetAttributeValue("MessageID", MessageID);
      itemXML.SetAttributeValue("MasterMsgID", MasterMsgID);
      itemXML.SetAttributeValue("EventUTC", EventUTC);
      itemXML.SetAttributeValue("SerialNumber", SerialNumber);
      itemXML.SetAttributeValue("MakeCode", MakeCode);
      itemXML.SetAttributeValue("GpsDeviceID", GpsDeviceID);
      itemXML.SetAttributeValue("DeviceTypeID", DeviceTypeID);
      itemXML.SetAttributeValue("TimeWatchActive", TimeWatchActive);
      itemXML.SetAttributeValue("ExclusiveWatchActive", ExclusiveWatchActive);
      itemXML.SetAttributeValue("InclusiveWatchActive", InclusiveWatchActive);
      itemXML.SetAttributeValue("TimeWatchAlarm", TimeWatchAlarm);
      itemXML.SetAttributeValue("ExclusiveWatchAlarm", ExclusiveWatchAlarm);
      itemXML.SetAttributeValue("InclusiveWatchAlarm", InclusiveWatchAlarm);
      itemXML.SetAttributeValue("SatelliteBlockage", SatelliteBlockage);
      itemXML.SetAttributeValue("DisconnectSwitchUsed", DisconnectSwitchUsed);
      itemXML.SetAttributeValue("RecordID", 0L);

      return itemXML;
    }

    public object Clone()
    {
      return this.MemberwiseClone() as INHOEMDataObject;
    }
  }

  public partial class CAT_ManualSMU : INHOEMDataObject, ICloneable
  {
    private static SqlMetaData[] metadata = new SqlMetaData[] { 
        new SqlMetaData("MessageID", SqlDbType.BigInt),
        new SqlMetaData("MasterMsgID", SqlDbType.BigInt),
        new SqlMetaData("EventUTC", SqlDbType.DateTime2),
        new SqlMetaData("SerialNumber", SqlDbType.NVarChar, 24),
        new SqlMetaData("MakeCode", SqlDbType.NVarChar, 3),
        new SqlMetaData("GpsDeviceID", SqlDbType.NVarChar, 50),
        new SqlMetaData("DeviceTypeID", SqlDbType.Int), 
        new SqlMetaData("RuntimeBeforeHours", SqlDbType.Float),
        new SqlMetaData("RuntimeAfterHours", SqlDbType.Float),
        new SqlMetaData("RecordID", SqlDbType.BigInt),
      };

    public Microsoft.SqlServer.Server.SqlDataRecord ToSqlDataRecord()
    {
      SqlDataRecord record = new SqlDataRecord(metadata);

      record.SetValue(record.GetOrdinal("MessageID"), this.MessageID);
      record.SetValue(record.GetOrdinal("MasterMsgID"), this.MasterMsgID);
      record.SetValue(record.GetOrdinal("EventUTC"), this.EventUTC);
      record.SetValue(record.GetOrdinal("SerialNumber"), this.SerialNumber);
      record.SetValue(record.GetOrdinal("MakeCode"), this.MakeCode);
      record.SetValue(record.GetOrdinal("GpsDeviceID"), this.GpsDeviceID);
      record.SetValue(record.GetOrdinal("DeviceTypeID"), this.DeviceTypeID);
      record.SetValue(record.GetOrdinal("RuntimeBeforeHours"), this.RuntimeBeforeHours);
      record.SetValue(record.GetOrdinal("RuntimeAfterHours"), this.RuntimeAfterHours);
      record.SetValue(record.GetOrdinal("RecordID"), 0L);

      return record;
    }

    public XElement ToXElement(string oemName)
    {
      XElement itemXML = new XElement("CAT_ManualSMU");
      itemXML.SetAttributeValue("InsertUTC", InsertUTC);
      itemXML.SetAttributeValue("MessageID", MessageID);
      itemXML.SetAttributeValue("MasterMsgID", MasterMsgID);
      itemXML.SetAttributeValue("EventUTC", EventUTC);
      itemXML.SetAttributeValue("SerialNumber", SerialNumber);
      itemXML.SetAttributeValue("MakeCode", MakeCode);
      itemXML.SetAttributeValue("GpsDeviceID", GpsDeviceID);
      itemXML.SetAttributeValue("DeviceTypeID", DeviceTypeID);
      itemXML.SetAttributeValue("RuntimeBeforeHours", RuntimeBeforeHours);
      itemXML.SetAttributeValue("RuntimeAfterHours", RuntimeAfterHours);
      itemXML.SetAttributeValue("RecordID", 0L);

      return itemXML;
    }

    public object Clone()
    {
      return this.MemberwiseClone() as INHOEMDataObject;
    }
  }

  public partial class CAT_DigStatus : INHOEMDataObject, ICloneable
  {
    private static SqlMetaData[] metadata = new SqlMetaData[] { 
        new SqlMetaData("MessageID", SqlDbType.BigInt),
        new SqlMetaData("MasterMsgID", SqlDbType.BigInt),
        new SqlMetaData("EventUTC", SqlDbType.DateTime2),
        new SqlMetaData("SerialNumber", SqlDbType.NVarChar, 24),
        new SqlMetaData("MakeCode", SqlDbType.NVarChar, 3),
        new SqlMetaData("GpsDeviceID", SqlDbType.NVarChar, 50),
        new SqlMetaData("DeviceTypeID", SqlDbType.Int), 
        new SqlMetaData("PowerMode", SqlDbType.TinyInt),
        new SqlMetaData("Pending", SqlDbType.Bit),
        new SqlMetaData("Switch1Active", SqlDbType.Bit),
        new SqlMetaData("Switch2Active", SqlDbType.Bit),
        new SqlMetaData("Switch3Active", SqlDbType.Bit),
        new SqlMetaData("Switch4Active", SqlDbType.Bit),
        new SqlMetaData("RecordID", SqlDbType.BigInt),
      };

    public Microsoft.SqlServer.Server.SqlDataRecord ToSqlDataRecord()
    {
      SqlDataRecord record = new SqlDataRecord(metadata);

      record.SetValue(record.GetOrdinal("MessageID"), this.MessageID);
      record.SetValue(record.GetOrdinal("MasterMsgID"), this.MasterMsgID);
      record.SetValue(record.GetOrdinal("EventUTC"), this.EventUTC);
      record.SetValue(record.GetOrdinal("SerialNumber"), this.SerialNumber);
      record.SetValue(record.GetOrdinal("MakeCode"), this.MakeCode);
      record.SetValue(record.GetOrdinal("GpsDeviceID"), this.GpsDeviceID);
      record.SetValue(record.GetOrdinal("DeviceTypeID"), this.DeviceTypeID);
      record.SetValue(record.GetOrdinal("PowerMode"), this.PowerMode);
      record.SetValue(record.GetOrdinal("Pending"), this.Pending);
      record.SetValue(record.GetOrdinal("Switch1Active"), this.Switch1Active);
      record.SetValue(record.GetOrdinal("Switch2Active"), this.Switch2Active);
      record.SetValue(record.GetOrdinal("Switch3Active"), this.Switch3Active);
      record.SetValue(record.GetOrdinal("Switch4Active"), this.Switch4Active);
      record.SetValue(record.GetOrdinal("RecordID"), 0L);

      return record;
    }

    public XElement ToXElement(string oemName)
    {
      XElement itemXML = new XElement("CAT_DigStatus");
      itemXML.SetAttributeValue("InsertUTC", InsertUTC);
      itemXML.SetAttributeValue("MessageID", MessageID);
      itemXML.SetAttributeValue("MasterMsgID", MasterMsgID);
      itemXML.SetAttributeValue("EventUTC", EventUTC);
      itemXML.SetAttributeValue("SerialNumber", SerialNumber);
      itemXML.SetAttributeValue("MakeCode", MakeCode);
      itemXML.SetAttributeValue("GpsDeviceID", GpsDeviceID);
      itemXML.SetAttributeValue("DeviceTypeID", DeviceTypeID);
      itemXML.SetAttributeValue("PowerMode", PowerMode);
      itemXML.SetAttributeValue("Pending", Pending);
      itemXML.SetAttributeValue("Switch1Active", Switch1Active);
      itemXML.SetAttributeValue("Switch2Active", Switch2Active);
      itemXML.SetAttributeValue("Switch3Active", Switch3Active);
      itemXML.SetAttributeValue("Switch4Active", Switch4Active);
      itemXML.SetAttributeValue("RecordID", 0L);

      return itemXML;
    }

    public object Clone()
    {
      return this.MemberwiseClone() as INHOEMDataObject;
    }
  }

  public partial class CAT_Fluid : INHOEMDataObject, ICloneable
  {
    private static SqlMetaData[] metadata = new SqlMetaData[] { 
        new SqlMetaData("MessageID", SqlDbType.BigInt),
        new SqlMetaData("MasterMsgID", SqlDbType.BigInt),
        new SqlMetaData("EventUTC", SqlDbType.DateTime2),
        new SqlMetaData("SerialNumber", SqlDbType.NVarChar,24),
        new SqlMetaData("MakeCode", SqlDbType.NVarChar,3),
        new SqlMetaData("GpsDeviceID", SqlDbType.NVarChar,50),
        new SqlMetaData("DeviceTypeID", SqlDbType.Int),
        new SqlMetaData("SampleNumber", SqlDbType.BigInt),
        new SqlMetaData("TextID", SqlDbType.NVarChar,50),
        new SqlMetaData("Description", SqlDbType.NVarChar,500),
        new SqlMetaData("SampleTakenDate", SqlDbType.DateTime2),
        new SqlMetaData("SampleConfirmedUTC", SqlDbType.DateTime2),
        new SqlMetaData("CompartmentName", SqlDbType.NVarChar,256),
        new SqlMetaData("CompartmentID", SqlDbType.NVarChar,50),
        new SqlMetaData("MeterValue", SqlDbType.Float),
        new SqlMetaData("MeterValueUnit", SqlDbType.Char,1),
        new SqlMetaData("OverallEvaluation", SqlDbType.VarChar,3),
        new SqlMetaData("Status", SqlDbType.Char,1),
        new SqlMetaData("ActionNumber", SqlDbType.BigInt),
        new SqlMetaData("ActionUTC", SqlDbType.DateTime2),
        new SqlMetaData("ActionDescription", SqlDbType.NVarChar,500),
        new SqlMetaData("ActionedByID", SqlDbType.VarChar,50),
        new SqlMetaData("ActionedByFirstName", SqlDbType.NVarChar,100),
        new SqlMetaData("ActionedByLastName", SqlDbType.NVarChar,100),
      };

    public Microsoft.SqlServer.Server.SqlDataRecord ToSqlDataRecord()
    {
      SqlDataRecord record = new SqlDataRecord(metadata);

      record.SetValue(record.GetOrdinal("MessageID"), this.MessageID);
      record.SetValue(record.GetOrdinal("MasterMsgID"), this.MasterMsgID);
      record.SetValue(record.GetOrdinal("EventUTC"), this.EventUTC);
      record.SetValue(record.GetOrdinal("SerialNumber"), this.SerialNumber);
      record.SetValue(record.GetOrdinal("MakeCode"), this.MakeCode);
      record.SetValue(record.GetOrdinal("GpsDeviceID"), this.GpsDeviceID);
      record.SetValue(record.GetOrdinal("DeviceTypeID"), this.DeviceTypeID);
      record.SetValue(record.GetOrdinal("SampleNumber"), this.SampleNumber);
      record.SetValue(record.GetOrdinal("TextID"), this.TextID);
      record.SetValue(record.GetOrdinal("Description"), this.Description);
      record.SetValue(record.GetOrdinal("SampleTakenDate"), this.SampleTakenDate);
      record.SetValue(record.GetOrdinal("SampleConfirmedUTC"), this.SampleConfirmedUTC);
      record.SetValue(record.GetOrdinal("CompartmentName"), this.CompartmentName);
      record.SetValue(record.GetOrdinal("CompartmentID"), this.CompartmentID);
      record.SetValue(record.GetOrdinal("MeterValue"), this.MeterValue);
      record.SetValue(record.GetOrdinal("MeterValueUnit"), this.MeterValueUnit);
      record.SetValue(record.GetOrdinal("OverallEvaluation"), this.OverallEvaluation);
      record.SetValue(record.GetOrdinal("Status"), this.Status);
      record.SetValue(record.GetOrdinal("ActionNumber"), this.ActionNumber);
      record.SetValue(record.GetOrdinal("ActionUTC"), this.ActionUTC);
      record.SetValue(record.GetOrdinal("ActionDescription"), this.ActionDescription);
      record.SetValue(record.GetOrdinal("ActionedByID"), this.ActionedByID);
      record.SetValue(record.GetOrdinal("ActionedByFirstName"), this.ActionedByFirstName);
      record.SetValue(record.GetOrdinal("ActionedByLastName"), this.ActionedByLastName);

      return record;
    }

    public XElement ToXElement(string oemName)
    {
      XElement itemXML = new XElement("CAT_Fluid");
      itemXML.SetAttributeValue("InsertUTC", InsertUTC);
      itemXML.SetAttributeValue("MessageID", MessageID);
      itemXML.SetAttributeValue("MasterMsgID", MasterMsgID);
      itemXML.SetAttributeValue("EventUTC", EventUTC);
      itemXML.SetAttributeValue("SerialNumber", SerialNumber);
      itemXML.SetAttributeValue("MakeCode", MakeCode);
      itemXML.SetAttributeValue("GpsDeviceID", GpsDeviceID);
      itemXML.SetAttributeValue("DeviceTypeID", DeviceTypeID);
      itemXML.SetAttributeValue("SampleNumber", SampleNumber);
      itemXML.SetAttributeValue("TextID", TextID);
      itemXML.SetAttributeValue("Description", Description);
      itemXML.SetAttributeValue("SampleTakenDate", SampleTakenDate);
      itemXML.SetAttributeValue("SampleConfirmedUTC", SampleConfirmedUTC);
      itemXML.SetAttributeValue("CompartmentName", CompartmentName);
      itemXML.SetAttributeValue("CompartmentID", CompartmentID);
      itemXML.SetAttributeValue("MeterValue", MeterValue);
      itemXML.SetAttributeValue("MeterValueUnit", MeterValueUnit);
      itemXML.SetAttributeValue("OverallEvaluation", OverallEvaluation);
      itemXML.SetAttributeValue("Status", Status);
      itemXML.SetAttributeValue("ActionNumber", ActionNumber);
      itemXML.SetAttributeValue("ActionUTC", ActionUTC);
      itemXML.SetAttributeValue("ActionDescription", ActionDescription);
      itemXML.SetAttributeValue("ActionedByID", ActionedByID);
      itemXML.SetAttributeValue("ActionedByFirstName", ActionedByFirstName);
      itemXML.SetAttributeValue("ActionedByLastName", ActionedByLastName);


      return itemXML;
    }

    public object Clone()
    {
      return this.MemberwiseClone() as INHOEMDataObject;
    }
  }
}
