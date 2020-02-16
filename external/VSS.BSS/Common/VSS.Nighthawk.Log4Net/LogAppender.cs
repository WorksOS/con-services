using System.Text;
using log4net.Appender;
using log4net.Layout;
using System.Configuration;
using System.IO;

namespace VSS.Nighthawk.Log4Net
{
  public class LogAppender : RollingFileAppender
  {
    public LogAppender()
    {
      base.AppendToFile = true;
      base.MaxSizeRollBackups = 100;
      base.MaximumFileSize = "1000000";
      base.RollingStyle = RollingFileAppender.RollingMode.Date;
      base.DatePattern = "yyyyMMdd";
      base.StaticLogFileName = true;

      PatternLayout lo = new PatternLayout("%utcdate{ISO8601} [%t] %-5p %m%n");
      base.Layout = lo;
      base.Encoding = Encoding.UTF8;
      base.ImmediateFlush = true;
    }

    public override string File
    {
      get { return base.File; }
      
      set 
      {
        string logRoot = ConfigurationManager.AppSettings["NightHawkLogRoot"];
        base.File = Path.Combine( logRoot, value ); 
      }
    }
  }
}
