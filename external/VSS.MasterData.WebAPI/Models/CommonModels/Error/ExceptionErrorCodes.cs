using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CommonModel.Error
{
	public enum ExceptionErrorCodes
	{
		#region InternalServerError - Starts with 500

		#region Unexpected Error - Postfixed with 100

		[Description("An Unexpected Error has occurred")]
		UnexpectedError = 5001001,

		#endregion

		#endregion

		#region Bad Request - Starts with 400

		#region PayloadModel Validator - Postfixed with - 001

		[Description("Invalid Request")]
		InvalidRequest = 4000010,

		[Description("{0}")]
		InvalidRequestPropertyMissing = 4000011,

		#endregion

		#endregion
	}
}
