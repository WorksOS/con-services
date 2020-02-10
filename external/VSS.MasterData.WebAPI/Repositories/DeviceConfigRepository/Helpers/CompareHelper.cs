using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DeviceConfigRepository.Helpers
{
    public class CompareHelper
    {
        public static bool AreObjectsEqual(object objectA, object objectB, params string[] ignoreList)
        {
            //bool result = false;
            Type objectTypeA, objectTypeB;

            objectTypeA = objectA.GetType();
            objectTypeB = objectB.GetType();

            List<string> valueA = new List<string>();
            List<string> valueB = new List<string>();

            foreach (PropertyInfo propertyInfo in objectTypeA.GetProperties().Where(
                          p => p.CanRead && !ignoreList.Contains(p.Name, StringComparer.OrdinalIgnoreCase)))
                valueA.Add(propertyInfo.GetValue(objectA)?.ToString());

            foreach (PropertyInfo propertyInfo in objectTypeB.GetProperties().Where(
                          p => p.CanRead && !ignoreList.Contains(p.Name, StringComparer.OrdinalIgnoreCase)))
                valueB.Add(propertyInfo.GetValue(objectB)?.ToString());

            for (var i = 0; i < valueA.Count(); i++)
            {
                if (valueA[i] != valueB[i])
                {
                    return false;
                }
                else
                    continue;
            }

            //result = valueA.Where(x => !valueB.Contains(x)).Count() == 0;

            return true;
        }

        public static DateTime ConvertDateTimeForComparison(DateTime dateTimeVal)
        {
            return dateTimeVal = Convert.ToDateTime(dateTimeVal.ToString("yyyy-MM-dd hh:mm:ss"));
        }
    }
}
