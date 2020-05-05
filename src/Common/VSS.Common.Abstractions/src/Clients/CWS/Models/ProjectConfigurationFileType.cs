namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public enum ProjectConfigurationFileType
  {
    CALIBRATION = 0,     // WM supports .dc .cal
    AVOIDANCE_ZONE,      // WM supports .avoid.svl, .avoid.dxf
    CONTROL_POINTS,      // WM supports .office.csv, .cpz
    GEOID,               // WM supports .ggf 
    FEATURE_CODE,        // WM supports .fxl
    SITE_CONFIGURATION,  // WM supports site.xml
    SITE_MAP,            // WM supports ??
    GCS_CALIBRATION      // WM supports .cfg
  }

  // also design files:
  //  Machine Control  
  //       Surface:         // WM supports .xml .dsz .svd
  //       Linework:        // WM supports .xml .dsz .svl
  //  Data Collectors
  //       Surface:         // WM supports .ttm
  //       Linework:        // WM supports .dxf .dwg
  //       Stakeout Points: // WM supports .csv
  //       Corridor:        // WM supports .pro
}
