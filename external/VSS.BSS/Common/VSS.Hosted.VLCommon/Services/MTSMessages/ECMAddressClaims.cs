using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon.MTSMessages
{
  public static class ECMAddressClaims
  {
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private static readonly Dictionary<string, Dictionary<byte, string>> ecmAddressesCache = new Dictionary<string,Dictionary<byte,string>>();
    private static readonly object cacheLock = new object();

    public static void Init()
    {
      ecmAddressesCache.Clear();
    }

    public static void RemoveEcmAddressClaimsFromCache(string serialNumber)
    {
      lock (cacheLock)
      {
        if (ecmAddressesCache.ContainsKey(serialNumber))
          ecmAddressesCache.Remove(serialNumber);
      }
    }

    public static string GetECMIDFromSourceAddress(string serialNumber, byte sourceAddress)
    {
      lock (cacheLock)
      {
        if (ecmAddressesCache != null)
        {
          string ecmID = null;
          if (ecmAddressesCache.ContainsKey(serialNumber))
          {
            Dictionary<byte, string> ecms = ecmAddressesCache[serialNumber];
            if (ecms.ContainsKey(sourceAddress))
              ecmID = ecms[sourceAddress];
          }
          return ecmID;
        }
      }
      return null;
    }

    public static Dictionary<byte, string> GetAddressClaimsFromSerialNumber(string serialNumber)
    {
      lock (cacheLock)
      {
        if (ecmAddressesCache != null)
        {
          if (ecmAddressesCache.ContainsKey(serialNumber))
          {
            return ecmAddressesCache[serialNumber];
          }
        }
        return null;
      }
    }

    public static void LogInvalidSourceAddressLookup(string serialNumber, byte ecmSourceAddress)
    {      
      log.IfWarnFormat("{0}: Could not find ECM Number for Source Address: {1}", serialNumber, ecmSourceAddress);

      lock (cacheLock)
      {
        if (!ecmAddressesCache.ContainsKey(serialNumber) || ecmAddressesCache[serialNumber].Count == 0)
        {
          log.IfDebugFormat("{0}: This device currently has no AddressClaims for Source Address: {1}", serialNumber, ecmSourceAddress);
        }
        else
        {
          StringBuilder builder = new StringBuilder();
          builder.Append("\n Current Address Claims:");          
          foreach (var claims in ecmAddressesCache[serialNumber])
          {
            builder.AppendFormat("\nSource Address: {0} ECMID: {1}", claims.Key, claims.Value);
          }
          log.IfDebugFormat(builder.ToString());
        }
      }
      
    }

    private static void UpdateCache(string serialNumber, byte sourceAddress, string ecmID)
    {
      if (!ecmAddressesCache.ContainsKey(serialNumber))
      {
        ecmAddressesCache.Add(serialNumber, new Dictionary<byte, string>());
        ecmAddressesCache[serialNumber].Add(sourceAddress, ecmID);
      }
      else if(ecmAddressesCache.ContainsKey(serialNumber) && !ecmAddressesCache[serialNumber].ContainsKey(sourceAddress))
        ecmAddressesCache[serialNumber].Add(sourceAddress, ecmID);
      else if(ecmAddressesCache.ContainsKey(serialNumber) && ecmAddressesCache[serialNumber].ContainsKey(sourceAddress))
        ecmAddressesCache[serialNumber][sourceAddress] = ecmID;
    }
  }
}
