using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace LandfillService.AcceptanceTests.Utils
{
    public class LandfillCommonUtils
    {
        public readonly static Random Random = new Random((int)(DateTime.Now.Ticks % 1000000));

        public static void UpdateAppSetting(string key, string value)
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings[key].Value = value;
            configuration.Save();

            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
