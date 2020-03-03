using System;
using System.Collections.Generic;
using System.Text;

namespace Data.MySql.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class DBColumnIgnoreAttribute : Attribute
	{
		private bool _ignore = true;
		public bool Ignore
		{
			get { return _ignore; }
			set { this._ignore = value; }
		}
	}
}
