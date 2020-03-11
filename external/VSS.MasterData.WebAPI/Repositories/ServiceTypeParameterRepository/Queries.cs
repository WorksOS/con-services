using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceTypeParameterRepository
{
	public class Queries
	{
		public const string FETCH_ALL_SERVICETYPE_PARAMETER = @"SELECT 
																	DP.DeviceParameterID,
																	STP.fk_ServiceTypeID AS ServiceTypeID,
																	STP.IncludeInd,
																	STP.InsertUTC,
																	STP.UpdateUTC,
																	DP.ParameterName AS DeviceParameterName,
																	DPG.GroupName AS DeviceParamGroupName,
																	STE.Name AS ServiceTypeName,
																	STFE.FamilyName AS ServiceTypeFamilyName
																FROM
																	md_ServiceTypeParameter AS STP
																		INNER JOIN
																	md_device_DeviceParameter AS DP ON DP.DeviceParameterID = STP.fk_DeviceParameterID
																		INNER JOIN
																	md_device_DeviceParamGroupParameter AS DPGP ON DPGP.fk_DeviceParameterID = DP.DeviceParameterID
																		INNER JOIN
																	md_device_DeviceParamGroup AS DPG ON DPG.DeviceParamGroupID = DPGP.fk_DeviceParamGroupId
																		INNER JOIN
																	md_subscription_ServiceType AS STE ON STE.ServiceTypeID = STP.fk_ServiceTypeID
																		INNER JOIN
																	md_subscription_ServiceTypeFamily AS STFE ON STFE.ServiceTypeFamilyID = STE.fk_ServiceTypeFamilyID";

	}
}
