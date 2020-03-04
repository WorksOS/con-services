using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbModel.Cache
{
	public class ServiceTypeParameterDto
	{
		public int DeviceParameterID { get; set; }
		public int ServiceTypeID { get; set; }
		public bool IncludeInd { get; set; }
		public DateTime InsertUTC { get; set; }
		public DateTime UpdateUTC { get; set; }
		public string DeviceParamGroupName { get; set; }
		public string DeviceParameterName { get; set; }
		public string ServiceTypeName { get; set; }
		public string ServiceTypeFamilyName { get; set; }
	}
}
