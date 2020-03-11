using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class WorkDefinitionEvent
	{
		/// <summary>
		/// 
		/// </summary>
		[Required]
		public Guid AssetUID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[Required]
		public string WorkDefinitionType { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int? SensorNumber { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public bool? StartIsOn { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[Required]
		public DateTime ActionUTC { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DateTime ReceivedUTC { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string DeviceType { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

		public string ErrorMessage { get; set; }
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public WorkDefinitionDto ToWorkDefinitionDto()
		{
			WorkDefinitionDto dto = new WorkDefinitionDto
			{
				AssetUID = AssetUID.ToString(),
				SwitchNumber = SensorNumber,
				SwitchWorkStartState = StartIsOn,
				WorkDefinitionType = WorkDefinitionType
			};
			dto.StartDate = dto.InsertUTC = dto.UpdateUTC = DateTime.UtcNow;
			return dto;
		}
	}

}

