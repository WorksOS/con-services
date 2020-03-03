using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Diagnostics.CodeAnalysis;

namespace VSS.MasterData.WebAPI.Asset.Filters
{
	[ExcludeFromCodeCoverage]
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class ChunkedEncodingFilterAttribute : ActionFilterAttribute
	{

		#region Declarations

		//private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly string _parameterName;
		private readonly Type _deserializerType;

		#endregion Declarations

		#region Constructors

		/// <summary>
		/// 
		/// </summary>
		/// <param name="deserializerType"></param>
		/// <param name="parameterName"></param>
		public ChunkedEncodingFilterAttribute(Type deserializerType, string parameterName)
		{
			_parameterName = parameterName;
			_deserializerType = deserializerType;
		}

		#endregion Constructors

		#region Public Methods

		/// <summary>
		/// 
		/// </summary>
		/// <param name="actionContext"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public override void OnActionExecuting(ActionExecutingContext actionContext)
		{
			if (_parameterName == null)
			{
				throw new ArgumentNullException(
				$"{actionContext.ActionDescriptor.DisplayName} Paramter's name is empty");
			}
			if (_deserializerType == null)
			{
				throw new ArgumentNullException(string.Format("Deserializer type is empty"));
			}
		}
		#endregion Public Methods
	}
}