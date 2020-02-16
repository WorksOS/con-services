using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace VSS.Hosted.VLCommon.TrimTracMessages
{
  // Position:
  // <tt><Raw><![CDATA[>RTKP309C1100F1454187793000000100000+398979513-1051126911+05515000000;ID=CS000020;*30<]]></Raw>
  //     <Sequence>309C</Sequence><Trigger>1</Trigger><BatteryLevel>100</BatteryLevel><GPSStatusCode>0</GPSStatusCode><PositionAge>0</PositionAge>
  // </tt>
  // Status:
  // <tt><Raw><![CDATA[>RTKS304E5100F1454177045701000100000;ID=CS000020;*1B<]]></Raw>
  //     <Sequence>304E</Sequence><Trigger>5</Trigger><BatteryLevel>100</BatteryLevel><GPSStatusCode>7</GPSStatusCode><PositionAge>1</PositionAge>
  // </tt>
  // Runtime:
  // <tt><Raw><![CDATA[>RTKM0000021420330000000000;ID=CS000020;*33<]]></Raw><MotionReset>0</MotionReset><LPAReset>0</LPAReset><MotionRuntime>0002142033</MotionRuntime><LPARuntime>0000000000</LPARuntime></tt>
  // Start Motion:
  // <tt><Raw><![CDATA[>RTKS2FDB1100F1454161082701000100000;ID=CS000020;*13<]]></Raw><Sequence>2FDB</Sequence><Trigger>1</Trigger><BatteryLevel>100</BatteryLevel><GPSStatusCode>7</GPSStatusCode><PositionAge>1</PositionAge></tt>
  // Stop Motion:
  // <tt><Raw><![CDATA[>RTKS2FDB1100F1454161082701000100000;ID=CS000020;*13<]]></Raw><Sequence>2FDB</Sequence><Trigger>1</Trigger><BatteryLevel>100</BatteryLevel><GPSStatusCode>7</GPSStatusCode><PositionAge>1</PositionAge></tt>
  public class TrimTracData
  {

    public TrimTracData()
    {
    }
    public TrimTracData(TT tt)
    {
      TTEvent = tt;
      Raw = tt.OriginalParseData;

      if (TTEvent is TT_RTKM)
        Runtime = TTEvent as TT_RTKM;
      else if (TTEvent is TT_RTKS)
        Status = (TTEvent as TT_RTKS).Data;
      else if (TTEvent is TT_RTKP)
        Status = (TTEvent as TT_RTKP).Data;
    }

    public STATUS_MSG Status
    {
      set
      {
        this.Sequence = value.ProtocolSequenceNumber.ToString( "X" );
        this.Trigger = value.TriggerType.ToString();
        this.BatteryLevel = value.BatteryLevel.ToString();
        this.GPSStatusCode = ( (int)value.GPSStatusCode ).ToString();
        this.PositionAge = ( (int)value.PositionAge ).ToString();
        this.MPAStatus = value.MPAStatus.ToString();
        this.LPAStatus = value.LPAStatus.ToString();
      }
    }

    public TT_RTKM Runtime
    {
      set
      {
        this.LPAReset = value.RuntimeLPABasedResetConfirmation.ToString();
        this.MotionReset = value.RuntimeMotionBasedResetConfirmation.ToString();
        this.LPARuntime = value.RuntimeLPABasedReading.ToString();
        this.MotionRuntime = value.RuntimeMotionBasedReading.ToString();
      }
    }
    
    [XmlIgnore]
    public TT TTEvent = null;

    public string Raw = null;

    public string Sequence = null;
    public string Trigger = null;
    public string BatteryLevel = null;
    public string GPSStatusCode = null;
    public string PositionAge = null;
    public string MPAStatus = null;
    public string LPAStatus = null;

    public string MotionReset = null;
    public string LPAReset = null;
    public string MotionRuntime = null;
    public string LPARuntime = null;
  }

}
