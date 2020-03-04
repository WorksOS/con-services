using CommonModel.Error;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ClientModel.Response
{
	public class ExceptionResponse : BaseResponse<int, AssetErrorInfo> //Int a dummy type, we can refactor
	{
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public override IList<int> Lists { get; set; }
		public ExceptionResponse(IList<AssetErrorInfo> errors) : base(errors) { }
		public ExceptionResponse(AssetErrorInfo error) : base(error) { }
		public ExceptionResponse(Exception exception) : base(exception) { }
	}
}
