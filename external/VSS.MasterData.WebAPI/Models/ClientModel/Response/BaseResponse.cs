using CommonModel.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ClientModel.Response
{
	[ExcludeFromCodeCoverage]
    public abstract class BaseResponse<TLists, TErrorInfo>
    {
		private ExceptionInfo _exception;

		[JsonProperty("errors", NullValueHandling = NullValueHandling.Ignore)]
        public IList<TErrorInfo> Errors { get; set; }

		[JsonProperty("exceptionInfo", NullValueHandling = NullValueHandling.Ignore)]
		public ExceptionInfo ExceptionInfo
		{
			get { return _exception; }
		}

		[JsonIgnore]
		public Exception Exception { 
			set 
			{
				this._exception = new ExceptionInfo
				{
					InnerException = JsonConvert.SerializeObject(value.InnerException),
					Message = value.Message,
					StackTrace = value.StackTrace
				};		
			}
		}

		public abstract IList<TLists> Lists { get; set; }

        public BaseResponse() { }

        public BaseResponse(IList<TErrorInfo> errors)
        {
            this.Errors = errors;
        }

        public BaseResponse(TErrorInfo error)
        {
            this.Errors = new List<TErrorInfo> { error };
        }

		public BaseResponse(Exception exception)
		{
			this._exception = new ExceptionInfo
			{
				InnerException = JsonConvert.SerializeObject(exception.InnerException),
				Message = exception.Message,
				StackTrace = exception.StackTrace
			};
		}

		public BaseResponse(IList<TLists> lists)
        {
            this.Lists = lists;
        }

		public BaseResponse(IList<TLists> lists, IList<TErrorInfo> errors = null)
		{
			this.Lists = lists;
			if (errors != null && errors.Count > 0)
			{
				this.Errors = errors;
			}
		}
	}
}
