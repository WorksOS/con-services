using Interfaces;
using DbModel.AssetSettings;
using CommonModel.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;
using VSS.MasterData.WebAPI.Utilities.Extensions;

namespace AssetWeeklyConfigRepository
{

	public class WeeklyAssetSettingsRepository : IWeeklyAssetSettingsRepository
	{
		private readonly string _connectionString;
		private readonly ILoggingService _loggingService;
		private readonly ITransactions _transaction;

		public WeeklyAssetSettingsRepository(ITransactions transaction, ILoggingService loggingService)
		{
			_transaction = transaction;
			_loggingService = loggingService;
			_loggingService.CreateLogger(typeof(WeeklyAssetSettingsRepository));
		}

		public List<AssetSettingsGetDBResponse> GetAssetUtilizationTargetRunTimeByStartDateAndAssetUID(string AssetUIDs, DateTime startDate, DateTime endDate)
		{

			try
			{
				var sql = string.Format(@"Select * From (Select HEX(AWC.fk_AssetUID) As AssetID, HEX(AWC.AssetWeeklyConfigUID) As AssetWeeklyConfigUID, AWC.SundayConfigValue As Sunday, AWC.MondayConfigValue As Monday, AWC.TuesdayConfigValue As Tuesday, 
                                                AWC.WednesdayConfigValue AS Wednesday, AWC.ThursdayConfigValue AS Thursday, AWC.FridayConfigValue As Friday, 
                                                AWC.SaturdayConfigValue AS Saturday, AWC.StartDate as StartDate, AWC.EndDate as EndDate, ACT.ConfigTypeName AS 'ConfigType' FRom md_asset_AssetWeeklyConfig AWC 
                                                Inner Join md_asset_AssetConfigType ACT On ACT.AssetConfigTypeID = AWC.fk_AssetConfigTypeID And ACT.ConfigTypeName IN ('" +
											AssetTargetType.RuntimeHours.ToString() + "', '" + AssetTargetType.IdletimeHours.ToString() + @"')
                                                Where AWC.fk_AssetUID In ({0}) AND ('{1}' Between StartDate And EndDate OR '{2}' Between StartDate And EndDate) Order By AWC.StartDate)A 
                                                UNION
                                                Select * From (Select HEX(AWC.fk_AssetUID) As AssetID, HEX(AWC.AssetWeeklyConfigUID) As AssetWeeklyConfigUID, AWC.SundayConfigValue As Sunday, AWC.MondayConfigValue As Monday, AWC.TuesdayConfigValue As Tuesday, 
                                                AWC.WednesdayConfigValue AS Wednesday, AWC.ThursdayConfigValue AS Thursday, AWC.FridayConfigValue As Friday, 
                                                AWC.SaturdayConfigValue AS Saturday, AWC.StartDate as StartDate, AWC.EndDate as EndDate, ACT.ConfigTypeName AS 'ConfigType' FRom md_asset_AssetWeeklyConfig AWC 
                                                Inner Join md_asset_AssetConfigType ACT On ACT.AssetConfigTypeID = AWC.fk_AssetConfigTypeID And ACT.ConfigTypeName IN ('" +
											AssetTargetType.RuntimeHours.ToString() + "', '" + AssetTargetType.IdletimeHours.ToString() + @"')
                                                Where AWC.fk_AssetUID In ({0}) AND (StartDate Between '{1}' AND '{2}' OR EndDate Between '{1}' AND '{2}') Order By AWC.StartDate ASC) B",
							AssetUIDs, startDate.ToDateTimeStringWithYearMonthDayFormat(), endDate.ToDateTimeStringWithYearMonthDayFormat());

				_loggingService.Debug(string.Format("Data Access requested for the query {0}", sql), "WeeklyAssetSettingsRepository.GetAssetUtilizationTargetRunTimeByStartDateAndAssetUID");

				return _transaction.Get<AssetSettingsGetDBResponse>(sql).ToList();
			}
			catch (Exception e)
			{
				throw;
			}
		}



		public List<AssetSettingsGetDBResponse> GetAssetUtilizationTargetRunTimeByAssetUID(string AssetUIDs)
		{

			try
			{
				var sql = string.Format(@"Select HEX(AWC.fk_AssetUID) As AssetID, HEX(AWC.AssetWeeklyConfigUID) As AssetWeeklyConfigUID, AWC.SundayConfigValue As Sunday, AWC.MondayConfigValue As Monday, AWC.TuesdayConfigValue As Tuesday, 
                                                AWC.WednesdayConfigValue AS Wednesday, AWC.ThursdayConfigValue AS Thursday, AWC.FridayConfigValue As Friday, 
                                                AWC.SaturdayConfigValue AS Saturday, AWC.StartDate as StartDate, AWC.EndDate as EndDate, ACT.ConfigTypeName AS 'ConfigType' FRom md_asset_AssetWeeklyConfig AWC 
                                                Inner Join md_asset_AssetConfigType ACT On ACT.AssetConfigTypeID = AWC.fk_AssetConfigTypeID And ACT.ConfigTypeName IN ('" +
											AssetTargetType.RuntimeHours.ToString() + "', '" + AssetTargetType.IdletimeHours.ToString() + @"')
                                                Where AWC.fk_AssetUID In ({0}) Order By AWC.StartDate Desc",
							AssetUIDs);
				_loggingService.Debug(string.Format("Data Access requested for the query {0}", sql), "WeeklyAssetSettingsRepository.GetAssetUtilizationTargetRunTimeByAssetUID");
				return this._transaction.Get<AssetSettingsGetDBResponse>(sql).ToList();
			}
			catch (Exception e)
			{
				throw;
			}


		}



		public List<AssetSettingsGetDBResponse> GetProductivityTargetsByStartDateAndAssetUID(string AssetUIDs, DateTime startDate, DateTime endDate)
		{

			try
			{
				var sql = string.Format(@"Select * From (Select HEX(AWC.fk_AssetUID) As AssetID, HEX(AWC.AssetWeeklyConfigUID) As AssetWeeklyConfigUID, AWC.SundayConfigValue As Sunday, AWC.MondayConfigValue As Monday, AWC.TuesdayConfigValue As Tuesday, 
                                                AWC.WednesdayConfigValue AS Wednesday, AWC.ThursdayConfigValue AS Thursday, AWC.FridayConfigValue As Friday, 
                                                AWC.SaturdayConfigValue AS Saturday,AWC.StartDate AS StartDate, AWC.EndDate AS EndDate, ACT.ConfigTypeName AS 'ConfigType' FRom md_asset_AssetWeeklyConfig AWC 
                                                Inner Join md_asset_AssetConfigType ACT On ACT.AssetConfigTypeID = AWC.fk_AssetConfigTypeID And ACT.ConfigTypeName IN ('" +
											AssetTargetType.PayloadinTonnes.ToString() + "', '" + AssetTargetType.CycleCount + "', '" + AssetTargetType.VolumeinCuMeter + @"')
                                                Where AWC.fk_AssetUID In ({0}) AND ('{1}' Between StartDate And EndDate OR '{2}' Between StartDate And EndDate) Order By AWC.StartDate)A 
                                                UNION
                                                Select * From (Select HEX(AWC.fk_AssetUID) As AssetID, HEX(AWC.AssetWeeklyConfigUID) As AssetWeeklyConfigUID, AWC.SundayConfigValue As Sunday, AWC.MondayConfigValue As Monday, AWC.TuesdayConfigValue As Tuesday, 
                                                AWC.WednesdayConfigValue AS Wednesday, AWC.ThursdayConfigValue AS Thursday, AWC.FridayConfigValue As Friday, 
                                                AWC.SaturdayConfigValue AS Saturday, AWC.StartDate as StartDate, AWC.EndDate as EndDate, ACT.ConfigTypeName AS 'ConfigType' FRom md_asset_AssetWeeklyConfig AWC 
                                                Inner Join md_asset_AssetConfigType ACT On ACT.AssetConfigTypeID = AWC.fk_AssetConfigTypeID And ACT.ConfigTypeName IN ('" +
											AssetTargetType.PayloadinTonnes.ToString() + "', '" + AssetTargetType.CycleCount + "', '" + AssetTargetType.VolumeinCuMeter + @"')
                                                Where AWC.fk_AssetUID In ({0}) AND (StartDate Between '{1}' AND '{2}' OR EndDate Between '{1}' AND '{2}') Order By AWC.StartDate ASC) B",
							AssetUIDs, startDate.ToDateTimeStringWithYearMonthDayFormat(), endDate.AddDays(1).ToDateTimeStringWithYearMonthDayFormat());

				_loggingService.Debug(string.Format("Data Access requested for the query {0}", sql), "WeeklyAssetSettingsRepository.GetProductivityTargetsByStartDateAndAssetUID");

				return this._transaction.Get<AssetSettingsGetDBResponse>(sql).ToList();
			}
			catch (Exception e)
			{
				throw;
			}

		}


		public List<AssetSettingsGetDBResponse> GetProductivityTargetsDetailsByAssetId(string AssetUID)
		{

			try
			{
				var sql = string.Format(@"Select HEX(AWC.fk_AssetUID) As AssetID, HEX(AWC.AssetWeeklyConfigUID) As AssetWeeklyConfigUID, AWC.SundayConfigValue As Sunday, AWC.MondayConfigValue As Monday, AWC.TuesdayConfigValue As Tuesday, 
                                                AWC.WednesdayConfigValue AS Wednesday, AWC.ThursdayConfigValue AS Thursday, AWC.FridayConfigValue As Friday, 
                                                AWC.SaturdayConfigValue AS Saturday, AWC.StartDate as StartDate, AWC.EndDate as EndDate, ACT.ConfigTypeName AS 'ConfigType' FRom md_asset_AssetWeeklyConfig AWC 
                                                Inner Join md_asset_AssetConfigType ACT On ACT.AssetConfigTypeID = AWC.fk_AssetConfigTypeID And ACT.ConfigTypeName IN ('" +
											AssetTargetType.PayloadinTonnes.ToString() + "', '" + AssetTargetType.CycleCount + "', '" + AssetTargetType.VolumeinCuMeter + @"')
                                                Where AWC.fk_AssetUID In ({0}) Order By AWC.StartDate Desc",
							AssetUID);
				_loggingService.Debug(string.Format("Data Access requested for the query {0}", sql), "WeeklyAssetSettingsRepository.GetProductivityTargetsDetailsByAssetId");
				return this._transaction.Get<AssetSettingsGetDBResponse>(sql).ToList();
			}
			catch (Exception e)
			{
				throw;
			}
		}
	}
}
