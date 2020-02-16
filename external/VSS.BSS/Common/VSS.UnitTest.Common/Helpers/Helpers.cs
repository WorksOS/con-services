using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.UnitTest.Common.Contexts;
using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common 
{
  public enum Database
  {
    NH_OP,
    NH_RPT,
    NH_RAW,
    NH_DATA
  }

  public class Helpers 
  {
    public static bool IsMockedContext()
    {
      return (ContextContainer.Current.OpContext is NH_OPMock);
    }

    public static void ExecuteStoredProcedure(Database database, string storedProcedureName)
    {
      StoredProcDefinition storedProcedure = new StoredProcDefinition(database.ToString(), storedProcedureName);
      SqlAccessMethods.ExecuteNonQuery(storedProcedure);
    }

    public static NHOpHelper NHOp
    {
      get
      {
        return new NHOpHelper();
      }
    }

    public static NHRawHelper NHRaw
    {
      get
      {
        return new NHRawHelper();
      }
    }

    public static WorkingSetHelper WorkingSet
    {
      get
      {
        return new WorkingSetHelper();
      }
    }
    
    public static SessionHelper Sessions
    {
      get
      {
        return new SessionHelper();
      }
    }

    public static SiteHelper Sites
    {
      get
      {
        return new SiteHelper();
      }
    }
  }
}
