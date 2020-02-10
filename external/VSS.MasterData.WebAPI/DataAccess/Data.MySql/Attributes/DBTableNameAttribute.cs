using System;
using System.Collections.Generic;
using System.Text;

namespace Data.MySql.Attributes
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class DBTableNameAttribute : Attribute
	{
		public string Name { get; set; }
	}
}
