using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.Hosted.VLCommon.MTSMessages;
using VSS.Hosted.VLCommon;


namespace VSS.Hosted.VLCommon.PLMessages
{
  public abstract class PLMessageBase : PlatformMessage
  {
    protected static readonly DateTime epoch = new DateTime(1980, 01, 06);

    public static string BytesToBinaryString(byte[] asciiBytes)
    {
      string s = Encoding.Unicode.GetString(Encoding.Convert(Encoding.GetEncoding("windows-1252"), Encoding.Unicode, asciiBytes));
      return s;
    }

    public static byte[] BinaryStringtoBytes(string binaryString)
    {
      byte[] bytes = Encoding.Convert(Encoding.Unicode, Encoding.GetEncoding("windows-1252"), Encoding.Unicode.GetBytes(binaryString));
      return bytes;
    }

    public static PLMessageBase HydratePLMessageBase(string body, bool deviceMsg)
    {
      uint bitPosition = 0;
      byte[] raw = null;
      if (deviceMsg)
      {
        raw = Convert.FromBase64String(body);
        return hydratePlatformMessage(raw, ref bitPosition, false, MessageCategory.PLTrackerMessage) as PLMessageBase;
      }
      else
      {
        raw = BinaryStringtoBytes(body);
        return hydratePlatformMessage(raw, ref bitPosition, false, MessageCategory.PLBaseMessage) as PLMessageBase;
      }
    }

    public static PLTransactionTypeEnum GetMessageType(string body, bool deviceMsg)
    {
      if (string.IsNullOrEmpty(body))
        return PLTransactionTypeEnum.Unknown;

      byte[] message = null;
      if (deviceMsg)
        message = Convert.FromBase64String(body);
      else
        message = BinaryStringtoBytes(body);

      PLTransactionTypeEnum transactionType = PLTransactionTypeEnum.Unknown;

      switch (message[0])
      {
        case (byte)PLTransactionTypeEnum.Administration:
          transactionType = PLTransactionTypeEnum.Administration;
          break;
        case (byte)PLTransactionTypeEnum.CumulativesMessage:
          transactionType = PLTransactionTypeEnum.CumulativesMessage;
          break;
        case (byte)PLTransactionTypeEnum.FaultDiagnostic:
          transactionType = PLTransactionTypeEnum.FaultDiagnostic;
          break;
        case (byte)PLTransactionTypeEnum.FaultEvent:
          transactionType = PLTransactionTypeEnum.FaultEvent;
          break;
        case (byte)PLTransactionTypeEnum.FenceConfig:
          transactionType = PLTransactionTypeEnum.FenceConfig;
          break;
        case (byte)PLTransactionTypeEnum.OTAConfigMessages:
          transactionType = PLTransactionTypeEnum.OTAConfigMessages;
          break;
        case (byte)PLTransactionTypeEnum.RegistrationMessage:
          transactionType = PLTransactionTypeEnum.RegistrationMessage;
          break;
        case (byte)PLTransactionTypeEnum.RegistrationMessage0x31:
          transactionType = PLTransactionTypeEnum.RegistrationMessage0x31;
          break;
        case (byte)PLTransactionTypeEnum.Status0x60:
          transactionType = PLTransactionTypeEnum.Status0x60;
          break;
        default:
          transactionType = PLTransactionTypeEnum.Unknown;
          break;
      }
      return transactionType;
    }
  }

  public abstract class PLTrackerMessage : PLMessageBase
  {
    public override MessageCategory Category
    {
      get { return MessageCategory.PLTrackerMessage; }
    }
  }

  public abstract class PLBaseMessage : PLMessageBase
  {
    public override MessageCategory Category
    {
      get { return MessageCategory.PLBaseMessage; }
    }
  }
}
