using System;
using System.Collections.Generic;

namespace VSS.MasterData.WebAPI.Utilities.Helpers
{
	public class TypeHelper
	{
		#region Declaration

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

		#endregion Declaration

		#region Public Methods

		public static bool IsNumeric(Type myType)
		{
			return NumericTypes.Contains(Nullable.GetUnderlyingType(myType) ?? myType);
		}

		#endregion Public Methods

	}
}
