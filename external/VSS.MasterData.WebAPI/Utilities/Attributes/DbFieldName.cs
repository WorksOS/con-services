using System;

namespace VSS.MasterData.WebAPI.Utilities.Attributes
{
	public class DbFieldNameAttribute : Attribute
	{
		public readonly string fieldName;
		public readonly Type expectedType;

		public DbFieldNameAttribute(string fieldName, Type expectedType = null)
		{
			this.fieldName = fieldName;
			this.expectedType = expectedType;
		}
	}
}