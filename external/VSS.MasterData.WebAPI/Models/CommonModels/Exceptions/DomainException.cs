using CommonModel.Error;
using System;
using System.Collections.Generic;

namespace CommonModel.Exceptions
{
	public class DomainException : Exception
    {
        public IEnumerable<IErrorInfo> Errors { get; set; }
        public IErrorInfo Error { get; set; }
    }
}
