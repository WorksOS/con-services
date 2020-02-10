using System;

namespace AssetConfigTypeRepository
{
	public class Queries
	{
		public const string SELECT_ASSET_CONFIG_TYPE = @"SELECT 
                                                            AssetConfigTypeID,
                                                            ConfigTypeName,
                                                            ConfigTypeDescr,
                                                            InsertUTC,
                                                            UpdateUTC
                                                        FROM
                                                            md_asset_AssetConfigType
                                                        WHERE
                                                            ConfigTypeName IN @configTypeNames";
	}
}
