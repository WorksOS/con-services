using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Reflection;

namespace VSS.Hosted.VLCommon
{

  partial class MTSOut
  {
    public static MTSOut CreateMTSOut(long id, global::System.DateTime insertUTC, int status, byte[] payload, bool isAck, string serialNumber, int deviceType)
    {
      MTSOut mTSOut = new MTSOut();
      mTSOut.ID = id;
      mTSOut.InsertUTC = insertUTC;
      mTSOut.Status = status;
      mTSOut.Payload = payload;
      mTSOut.IsAck = isAck;
      mTSOut.SerialNumber = serialNumber;
      mTSOut.DeviceType = deviceType;
      return mTSOut;
    }
  }

  public partial class TTOut
  {
      public static TTOut CreateTTOut(long id, global::System.DateTime insertUTC, short status, string payload, string unitID)
      {
          TTOut tTOut = new TTOut();
          tTOut.ID = id;
          tTOut.InsertUTC = insertUTC;
          tTOut.Status = status;
          tTOut.Payload = payload;
          tTOut.UnitID = unitID;
          return tTOut;
      }
  }
  partial class PLMessage
  {
    public DateTime MessageUTC { get; set; }
  }

}
