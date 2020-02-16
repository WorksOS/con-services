using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon.Services.Types
{
  public class AlertDetail
  {
    public long assetID;
    public long alertIncidentID;
    public long alertID;
    public string customerName; 
    public string owner; 
    public string title;
    public string emailIds;    
    public DateTime incidentTimeUTC;
    public DateTime sentTimeUTC;
    public int alertTypeID;
    public bool isActive;

  }
}
