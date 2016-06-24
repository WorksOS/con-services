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

        /// <summary>
        /// Test whether two lists are equivalent.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listA"></param>
        /// <param name="listB"></param>
        /// <returns></returns>
        public static bool ListsAreEqual<T>(List<T> listA, List<T> listB)
        {
            if (listA == null && listB == null)
                return true;
            else if (listA == null || listB == null)
                return false;
            else
            {
                if (listA.Count != listB.Count)
                    return false;

                for (int i = 0; i < listA.Count; ++i)
                {
                    if (!listB.Exists(item => item.Equals(listA[i])))
                        return false;
                }

                return true;
            }
        }
        public static void UpdateAppSetting(string key, string value)
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings[key].Value = value;
            configuration.Save();

            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
