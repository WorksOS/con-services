using System.Collections.Generic;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class SummaryReportDataModel : MandatoryReportData
  {
    public ProjectSettings ProjectCustomSettings { get; set; }
    public ColorPalettes ColorPalette { get; set; }
    public MachineDesignData DesignData { get; set; }
    //public Preferences UserPreference { get; set; } // todoJeannie map from ccss?
    public List<SummaryDataBase> Data { get; set; }
  }
}
