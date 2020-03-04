using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class MultipleResponse
	{
		/// <summary>
		/// 
		/// </summary>
		public class Assets
		{
			/// <summary>
			/// 
			/// </summary>
			public bool IsLastPage { get; set; }
			/// <summary>
			/// 
			/// </summary>
			public List<Asset> AssetRecords { get; set; }
		}

		/// <summary>
		/// 
		/// </summary>
		[XmlRoot(ElementName = "Asset")]
		public class Asset
		{
			/// <summary>
			/// 
			/// </summary>
			[XmlAttribute(AttributeName = "url")]
			public string Url { get; set; }

			/// <summary>
			/// 
			/// </summary>
			public string VisionLinkIdentifier { get; set; }

			/// <summary>
			/// 
			/// </summary>
			public string MakeCode { get; set; }

			/// <summary>
			/// 
			/// </summary>
			public string MakeName { get; set; }

			/// <summary>
			/// 
			/// </summary>
			public string SerialNumber { get; set; }

			/// <summary>
			/// 
			/// </summary>
			public string AssetID { get; set; }

			/// <summary>
			/// 
			/// </summary>
			public string EquipmentVIN { get; set; }

			/// <summary>
			/// 
			/// </summary>
			public string Model { get; set; }

			/// <summary>
			/// 
			/// </summary>
			public string ProductFamily { get; set; }

			/// <summary>
			/// 
			/// </summary>
			public string ManufactureYear { get; set; }
		}
	}
}