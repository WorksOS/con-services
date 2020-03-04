using DbModel.WorkDefinition;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ClientModel.WorkDefinition
{
	public class WorkDefinitionEvent
	{
		/// <summary>
		/// 
		/// </summary>
		[Required, JsonProperty(PropertyName = "AssetUID")]
		public Guid AssetUID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[Required, JsonProperty(PropertyName = "WorkDefinitionType")]
		public string WorkDefinitionType { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[JsonProperty(PropertyName = "SensorNumber")]
		public int? SensorNumber { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[JsonProperty(PropertyName = "StartIsOn")]
		public bool? StartIsOn { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[Required, JsonProperty(PropertyName = "ActionUTC")]
		public DateTime ActionUTC { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[Required, JsonProperty(PropertyName = "ReceivedUTC")]
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
				AssetUID = AssetUID,
				SwitchNumber = SensorNumber,
				SwitchWorkStartState = StartIsOn,
				WorkDefinitionType = WorkDefinitionType
			};
			dto.StartDate = dto.InsertUTC = dto.UpdateUTC = DateTime.UtcNow;
			return dto;
		}
	}
}
