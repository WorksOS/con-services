using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTypeParameterAttributeRepository
{
	public class Queries
	{
		public const string FetchDeviceTypesGroupsParametersAttributes = @"SELECT DISTINCT
                                                                            A.AttributeID,
                                                                            A.AttributeName,
                                                                            DP.DeviceParameterID,
                                                                            DP.ParameterName,
                                                                            DPG.DeviceParamGroupID,
                                                                            DPG.GroupName,
                                                                            DT.TypeName,
                                                                            DTP.DeviceTypeParameterID,
                                                                            DPA.DeviceParamAttrID,
																			DTP.IncludeInd,
																			DTP.DefaultValueJson
                                                                        FROM
                                                                            md_device_DeviceType AS DT
                                                                                INNER JOIN
                                                                            md_device_DeviceTypeParameter AS DTP ON DTP.fk_DeviceTypeID = DT.DeviceTypeID
                                                                                INNER JOIN
                                                                            md_device_DeviceParameter DP ON DTP.fk_DeviceParameterID = DP.DeviceParameterID
                                                                                INNER JOIN
                                                                            md_device_DeviceParamAttr AS DPA ON DPA.fk_DeviceParameterID = DP.DeviceParameterID
                                                                                INNER JOIN
                                                                            md_device_Attribute AS A ON A.AttributeID = DPA.fk_AttributeID
                                                                                INNER JOIN
                                                                            md_device_DeviceParamGroupParameter AS DPGP ON DPGP.fk_DeviceParameterID = DP.DeviceParameterID
                                                                                INNER JOIN
                                                                            md_device_DeviceParamGroup AS DPG ON DPG.DeviceParamGroupID = DPGP.fk_DeviceParamGroupID LIMIT 2000;";
	}
}
