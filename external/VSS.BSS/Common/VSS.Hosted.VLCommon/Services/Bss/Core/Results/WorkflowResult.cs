using System;
using System.Collections.Generic;

namespace VSS.Hosted.VLCommon.Bss
{
  public class WorkflowResult
  {
    private readonly IList<ActivityResult> _activityResults = new List<ActivityResult>();
    private string _summary = string.Empty;

    public bool Success
    {
      get { return ActivityResults.IsSuccessful(); }
    }

    public string Summary 
    { 
      get
      {
        if (Success)
          return _summary;

        return string.Format(CoreConstants.WORKFLOW_FAILED, ActivityResults.GetErrorOrExceptionSummary() ?? _summary);
      }
      set { _summary = value; }
    }

    public IList<ActivityResult> ActivityResults
    {
      get { return _activityResults; }
    }
  }
}