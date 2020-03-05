using System;
using System.Collections.Generic;
using System.Text;

namespace CommonModel.Exceptions
{
	public class ExceptionInfo
	{
		public string Message { get; set; }
		public string StackTrace { get; set; }
		public string InnerException { get; set; }
	}
}
