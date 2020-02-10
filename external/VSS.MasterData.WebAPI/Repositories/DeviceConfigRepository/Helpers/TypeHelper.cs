using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceConfigRepository.Helpers
{
  public class TypeHelper
  {
    private static readonly HashSet<Type> NumericTypes = new HashSet<Type>{
            typeof(int),
            typeof(double),
            typeof(decimal),
				    typeof(long),
				    typeof(short),
				    typeof(sbyte),
				    typeof(byte),
				    typeof(ulong),
				    typeof(ushort),
				    typeof(uint),
				    typeof(float)
        };

    public static bool IsNumeric(Type myType)
    {
      return NumericTypes.Contains(Nullable.GetUnderlyingType(myType) ?? myType);
    }
  }
}