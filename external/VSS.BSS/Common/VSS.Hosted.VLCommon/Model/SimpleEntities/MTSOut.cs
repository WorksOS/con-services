//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace VSS.Hosted.VLCommon
{
  public partial class MTSOut
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

    #region Primitive Properties

    public virtual long ID
    {
      get;
      set;
    }

    public virtual System.DateTime InsertUTC
    {
      get;
      set;
    }

    public virtual System.DateTime SentUTC
    {
      get { return _sentUTC; }
      set { _sentUTC = value; }
    }
    private System.DateTime _sentUTC = new DateTime(599266080000000000, DateTimeKind.Unspecified);

    public virtual int Status
    {
      get;
      set;
    }

    public virtual byte[] Payload
    {
      get;
      set;
    }

    public virtual Nullable<long> SequenceID
    {
      get;
      set;
    }

    public virtual bool IsAck
    {
      get;
      set;
    }

    public virtual Nullable<byte> SentCount
    {
      get;
      set;
    }

    public virtual Nullable<int> PacketID
    {
      get;
      set;
    }

    public virtual Nullable<int> TypeID
    {
      get;
      set;
    }

    public virtual string SerialNumber
    {
      get;
      set;
    }

    public virtual Nullable<System.DateTime> DueUTC
    {
      get;
      set;
    }

    public virtual Nullable<int> SubTypeID
    {
      get;
      set;
    }

    public virtual int DeviceType
    {
      get;
      set;
    }

    #endregion

  }
}
