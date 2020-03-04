using System;
using System.Collections.Generic;
using System.Text;

namespace Data.MySql.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class DBColumnNameAttribute : Attribute
	{
		public string Name { get; set; }
	}
}
