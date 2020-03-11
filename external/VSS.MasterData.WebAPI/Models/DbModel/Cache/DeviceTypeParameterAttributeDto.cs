namespace DbModel.Cache
{
	public class DeviceTypeParameterAttributeDto
	{
		public ulong AttributeID { get; set; }
		public string AttributeName { get; set; }
		public ulong DeviceParameterID { get; set; }
		public string ParameterName { get; set; }
		public ulong DeviceParamGroupID { get; set; }
		public string GroupName { get; set; }
		public string TypeName { get; set; }
		public string DefaultValueJSON { get; set; }
		public ulong DeviceTypeParameterID { get; set; }
		public ulong DeviceParamAttrID { get; set; }
		public bool IncludeInd { get; set; }
	}
}
