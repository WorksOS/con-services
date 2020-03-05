using System;
using System.Diagnostics.CodeAnalysis;

namespace VSS.MasterData.WebAPI.Asset.Helpers
{
	[ExcludeFromCodeCoverage]
	public class ConfigParams
	{
		#region Public Properties

		/// <summary>
		/// 
		/// </summary>
		public Uri SearchAndQuerySvcUri { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int? ServiceClientTriesMax { get; set; }

		#endregion Public Properties
	}
}
