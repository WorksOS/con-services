using System.Configuration;
using System.IO;
using System.Text;
using log4net.Appender;
using log4net.Layout;

namespace VSS.VisionLink.Landfill.Common.Utilities
{
  public class LogAppender : RollingFileAppender
  {
    public LogAppender()
    {
      AppendToFile = true;
      MaxSizeRollBackups = 100;
      MaximumFileSize = "1000000";
      RollingStyle = RollingMode.Date;
      DatePattern = "yyyyMMdd";
      StaticLogFileName = true;

      var lo = new PatternLayout("%utcdate{ISO8601} [%t] {%method} %-5p %m%n");
      Layout = lo;
      Encoding = Encoding.UTF8;
      ImmediateFlush = true;
    }

    public override string File
    {
      get { return base.File; }

      set
      {
        var logRoot = ConfigurationManager.AppSettings["VisionLinkLogRoot"];
        base.File = Path.Combine(logRoot, value);
      }
    }
  }
}