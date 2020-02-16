using System;
using VSS.Hosted.VLCommon;
using System.Linq;

namespace VSS.Hosted.VLCommon
{
  public class BssDataGenerator
  {
    public static string GenerateBssId(string ucidOrDealerCode)
    {
      return GenerateNegativeKey(TrimAndRaiseText(ucidOrDealerCode)).ToString();
    }

    public static long GenerateControlNumber(string threeCharMakeCode, string serialNumber)
    {
      return GenerateNegativeKey(string.Format("{0}_{1}", TrimAndRaiseText(threeCharMakeCode), TrimAndRaiseText(serialNumber)));
    }

    public static long GenerateIBKey(string equipmentSerialNumber)
    {
      return GenerateNegativeKey(TrimAndRaiseText(equipmentSerialNumber));
    }

    private static string TrimAndRaiseText(string textValue)
    {
      return textValue.RemoveAllWhitespace().ToUpper();
    }

    private static long GenerateNegativeKey(string keyBase)
    {
      long installBaseKey = EncryptionUtils.TextToLong(keyBase);

      return installBaseKey > 0 ? (-installBaseKey) : installBaseKey;
    }

    public static string GenerateServicePlanlineID(string threeCharMakeCode, string serialNumber, string servicePlan, string ucidOrDealerCode, DateTime startUTC)
    {
      return
        GenerateNegativeKey(
          string.Format(
            "{0}_{1}_{2}_{3}_{4}",
            TrimAndRaiseText(threeCharMakeCode),
            TrimAndRaiseText(serialNumber),
            TrimAndRaiseText(servicePlan),
            TrimAndRaiseText(ucidOrDealerCode),
            TrimAndRaiseText(startUTC.ToString("yyyy-MM-ddTHH:mm:ss")))).ToString();
    }
  }
  
  public interface IKeyGen
  {
    string GenerateValidBssID(SessionContext session);
    string GenerateValidRelationshipID(SessionContext session);
    string GenerateValidPlanLineID(SessionContext session);
    string GenerateValidIBKey(SessionContext session);
    long GenerateValidSeqenceNumber(SessionContext session);
  }

  public class PositiveKeyGen : IKeyGen
  {
    public static Lazy<IKeyGen> Instance = new Lazy<IKeyGen>(() => new PositiveKeyGen());

    #region Implementation of IKeyGen

    public string GenerateValidBssID(SessionContext session)
    {
      string bssID;
      do
      {
        bssID = BssDataGenerator.GenerateBssId(Guid.NewGuid().ToString()).TrimStart('-');
      } while (session.NHOpContext.CustomerReadOnly.Any(x => x.BSSID == bssID));

      return bssID;
    }

    public string GenerateValidRelationshipID(SessionContext session)
    {
      string relationShipID;
      do
      {
        relationShipID = BssDataGenerator.GenerateBssId(Guid.NewGuid().ToString()).TrimStart('-');
      } while (session.NHOpContext.CustomerRelationshipReadOnly.Any(x => x.BSSRelationshipID == relationShipID));
      return relationShipID;
    }

    public string GenerateValidIBKey(SessionContext session)
    {
      string ibKey;
      do
      {
        ibKey = BssDataGenerator.GenerateIBKey(Guid.NewGuid().ToString()).ToString().TrimStart('-');

      } while (session.NHOpContext.DeviceReadOnly.Any(x => x.IBKey == ibKey));
      return ibKey;
    }

    public string GenerateValidPlanLineID(SessionContext session)
    {
      string planLineID;
      do
      {
        planLineID = BssDataGenerator.GenerateServicePlanlineID("CAT", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), DateTime.UtcNow).ToString().TrimStart('-');
      } while (session.NHOpContext.ServiceReadOnly.Any(x => x.BSSLineID == planLineID));
      return planLineID;
    }

    public long GenerateValidSeqenceNumber(SessionContext session)
    {
      try
      {
        // Base Sequence Number for Billable Provisioning = Rounding 2^53(9,007,199,254,740,992) to zeros = 9007199000000000
        // Note: Precision is lost for numbers greater than 2^53 in AMF/Javascript.
        var result = session.NHOpContext.BSSProvisioningMsgReadOnly.Where(x => x.SequenceNumber >= 9007199000000000 && x.SequenceNumber < 9007199254740992).Max(x => x.SequenceNumber);
        return (result + 1);
      }
      catch (InvalidOperationException)
      {
        return 9007199000000000;
      }
      catch (Exception)
      {
        throw;
      }
    }

    #endregion
  }

  public class NegativeKeyGen : IKeyGen
  {
    public static Lazy<IKeyGen> Instance = new Lazy<IKeyGen>(() => new NegativeKeyGen());

    #region Implementation of IKeyGen

    public string GenerateValidBssID(SessionContext session)
    {
      string bssID;
      do
      {
        bssID = BssDataGenerator.GenerateBssId(Guid.NewGuid().ToString());
      } while (session.NHOpContext.CustomerReadOnly.Any(x => x.BSSID == bssID));

      return bssID;
    }

    public string GenerateValidRelationshipID(SessionContext session)
    {
      string relationShipID;
      do
      {
        relationShipID = BssDataGenerator.GenerateBssId(Guid.NewGuid().ToString());
      } while (session.NHOpContext.CustomerRelationshipReadOnly.Any(x => x.BSSRelationshipID == relationShipID));
      return relationShipID;
    }

    public string GenerateValidIBKey(SessionContext session)
    {
      string ibKey;
      do
      {
        ibKey = BssDataGenerator.GenerateIBKey(Guid.NewGuid().ToString()).ToString();

      } while (session.NHOpContext.DeviceReadOnly.Any(x => x.IBKey == ibKey));
      return ibKey;
    }

    public string GenerateValidPlanLineID(SessionContext session)
    {
      string planLineID;
      do
      {
        planLineID = BssDataGenerator.GenerateServicePlanlineID("CAT", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), DateTime.UtcNow).ToString();
      } while (session.NHOpContext.ServiceReadOnly.Any(x => x.BSSLineID == planLineID));
      return planLineID;
    }

    public long GenerateValidSeqenceNumber(SessionContext session)
    {
      try
      {
        var result = session.NHOpContext.BSSProvisioningMsgReadOnly.Where(x => x.SequenceNumber < 0).Min(x => x.SequenceNumber);
        return (result - 1);
      }
      catch (InvalidOperationException)
      {
        return -1;
      }
      catch (Exception)
      {
        throw;
      }
    }

    #endregion
  }

}
