namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public enum ProjectConfigurationFileType
  {
    CALIBRATION = 0,         // WM supports .dc .cal
    AVOIDANCE_ZONE = 1,      // WM supports .avoid.svl, .avoid.dxf
    CONTROL_POINTS = 2,      // WM supports .office.csv, .cpz
    GEOID = 3,               // WM supports .ggf 
    FEATURE_CODE = 4,        // WM supports .fxl
    SITE_CONFIGURATION = 5,  // WM supports site.xml
    SITE_MAP = 6,            // WM supports ??
    GCS_CALIBRATION = 7      // WM supports .cfg
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
