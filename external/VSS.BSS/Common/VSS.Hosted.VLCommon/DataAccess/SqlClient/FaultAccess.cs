using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using log4net;

namespace VSS.Hosted.VLCommon
{
  public static class FaultAccess
  {
    public static bool SaveFaults(string faultSetRecords, int totalFaultRecords)
    {
      try
      {
        var procDefinition = new StoredProcDefinition("NH_OP", "uspPub_Fault_Update");

        procDefinition.AddInputXml("@Faults", faultSetRecords);

        int mergedRecordCount = SqlAccessMethods.ExecuteReaderForMerge(procDefinition);

        log.IfDebugFormat("FaultAccess.SaveFaults: uspPub_Fault_Update: records in={0}, records merged={1}.", totalFaultRecords, mergedRecordCount);

        return true;
      }
      catch(SqlException exception)
      {
        log.IfErrorFormat(exception, "uspPub_Fault_Update merge failed. {0}Input XML:{1}",
            Environment.NewLine,
            faultSetRecords);

        return false;
      }
    }

    public static bool SaveFaultParameters(string faultParameterSetRecords, int totalFaultParameterRecords)
    {
      try
      {
        var procDefinition = new StoredProcDefinition("NH_OP", "uspPub_FaultParameter_Update");

        procDefinition.AddInputXml("@FaultParameters", faultParameterSetRecords);

        int mergedRecordCount = SqlAccessMethods.ExecuteReaderForMerge(procDefinition);

        log.IfDebugFormat("FaultAccess.SaveFaultParameters: uspPub_FaultParameter_Update: records in={0}, records merged={1}.", totalFaultParameterRecords, mergedRecordCount);

        return true;
      }
      catch (SqlException exception)
      {
        log.IfErrorFormat(exception, "uspPub_FaultParameter_Update merge failed. {0}Input XML:{1}",
                          Environment.NewLine,
                          faultParameterSetRecords);

        return false;
      }
    }

    public static bool SaveFaultDescriptions(string faultDescriptionSetRecords, int totalFaultDescriptionRecords)
    {
      try
      {
        var procDefinition = new StoredProcDefinition("NH_OP", "uspPub_FaultDescription_Update");

        procDefinition.AddInputXml("@FaultDescriptions", faultDescriptionSetRecords);

        int mergedRecordCount = SqlAccessMethods.ExecuteReaderForMerge(procDefinition);

        log.IfDebugFormat("FaultAccess.SaveFaultDescriptions: uspPub_FaultDescription_Update: records in={0}, records merged={1}.", totalFaultDescriptionRecords, mergedRecordCount);

        return true;
      }
      catch (SqlException exception)
      {
        log.IfErrorFormat(exception, "uspPub_FaultDescription_Update merge failed. {0}Input XML:{1}",
            Environment.NewLine,
            faultDescriptionSetRecords);

        return false;
      }
    }

    public static long GenerateSignatureID(string codedDescription)
    {
      try
      {
        var procDefinition = new StoredProcDefinition("NH_OP", "uspPub_FaultSignatureID");

        procDefinition.AddInput("@CodedDesc", codedDescription);

        return (long)SqlAccessMethods.ExecuteScalar(procDefinition);
      }
      catch (SqlException exception)
      {
        log.IfErrorFormat(exception, "uspPub_FaultSignatureID_Hash failed to calculate SignatureID for codedDescription:{0}",
            codedDescription);
      }

      return -1;
    }

    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);
  }
}
