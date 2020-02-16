using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon
{
  public static class SiteAccess
  {
    /// <summary>
    /// Get the database to populate the site information, including bounding box, area, and polygon representation for a specified site
    /// </summary>
    /// <param name="siteID"></param>
    public static bool PopulateGeometry(long siteID)
    {
      try
      {
        StoredProcDefinition sp = new StoredProcDefinition("NH_OP", "uspPub_Site_PopulateGeometry");
        sp.AddInput("@SiteID", siteID);
        SqlAccessMethods.ExecuteNonQuery(sp);
      }
      catch
      {
        return false;
      }

      return true;
    }
  }
}
