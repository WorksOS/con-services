using System;
using System.Collections.Generic;
using System.Text;

namespace AssetConfigRepository
{
	public class Queries
	{
		public const string FetchAssetConfig = @"SELECT 
                                                AC.AssetConfigID as AssetConfigID,
                                                AC.AssetConfigUID as AssetConfigUID,
                                                AC.fk_AssetUID as AssetUID,
                                                ACT.ConfigTypeName as TargetType,
                                                AC.StartDate as StartDate,
                                                AC.EndDate as EndDate,
                                                AC.ConfigValue as TargetValue,
                                                AC.InsertUTC as InsertUTC,
                                                AC.UpdateUTC as UpdateUTC,
                                                AC.StatusInd as StatusInd
                                            FROM
                                                md_asset_AssetConfig as AC INNER JOIN
                                                md_asset_AssetConfigType as ACT 
                                                    ON AC.fk_AssetConfigTypeID = ACT.AssetConfigTypeID
                                            WHERE
                                                1 = 1 AND
                                                {1}
                                                ACT.ConfigTypeName IN ({2}) AND
                                                AC.fk_AssetUID IN ({0})";
	}
}
