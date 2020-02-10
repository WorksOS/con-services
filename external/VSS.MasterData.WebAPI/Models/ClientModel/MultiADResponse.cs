using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class MultiADResponse
	{
		/// <summary>
		/// 
		/// </summary>
		[XmlRoot(ElementName = "Assets", Namespace = "")]
		public class Assets
		{
			/// <summary>
			/// 
			/// </summary>
			[XmlArray("Nav")]
			public List<Link> NavigationLinks { get; set; }

			/// <summary>
			/// 
			/// </summary>
			[XmlElement]
			public bool IsLastPage { get; set; }

			/// <summary>
			/// 
			/// </summary>
			[XmlElement(ElementName = "Asset")]
			public List<Asset> Asset { get; set; }
		}

		/// <summary>
		/// 
		/// </summary>
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
			public string Model { get; set; }

			/// <summary>
			/// 
			/// </summary>
			public string ProductFamily { get; set; }

			/// <summary>
			/// 
			/// </summary>
			public string ManufactureYear { get; set; }

			/// <summary>
			/// 
			/// </summary>
			public string DeviceType { get; set; }

			/// <summary>
			/// 
			/// </summary>
			public string DeviceSerialNumber { get; set; }
		}

		/// <summary>
		/// 
		/// </summary>
		[XmlRoot("Link")]
		public class Link
		{
			/// <summary>
			/// 
			/// </summary>
			[XmlAttribute("rel")]
			public string Relation { get; set; }

			/// <summary>
			/// 
			/// </summary>
			[XmlAttribute("methods")]
			public string Methods { get; set; }

			/// <summary>
			/// 
			/// </summary>
			[XmlAttribute("href")]
			public string Href { get; set; }

		}
	}
}