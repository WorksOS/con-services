//using Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Utilities.Logging;
//using VSS.MasterData.Asset.Common.Enums;
//using VSS.MasterData.Asset.Common.Helpers;
//using VSS.MasterData.Asset.DataAccess.MySql.Models;

namespace AssetWeeklyConfigRepository
{
//namespace AssetSettingsRepository
//{
//	//public class ProductivityTargetsService : IProductivityTargetsDataService
// //   {
//	//	private readonly IConnection _connection;
//	//	private readonly ILoggingService _loggingService;

//	//	public ProductivityTargetsService(IConnection connection, ILoggingService loggingService)
//	//	{
//	//		this._connection = connection;
//	//		this._loggingService = loggingService;
//	//		this._loggingService.CreateLogger(this.GetType());
//	//	}

//	//	public async Task<IEnumerable<ProductivityTargetsDBResponse>> GetAssetUtilizationTargetRunTimeByStartDateAndAssetUID(string AssetUIDs, DateTime startDate, DateTime endDate)
// //       {
// //           try
// //           {
// //               var sql = string.Format(@"Select HEX(AWC.fk_AssetUID) As AssetID, HEX(AWC.AssetWeeklyConfigUID) As AssetWeeklyConfigUID, AWC.SundayConfigValue As Sunday, AWC.MondayConfigValue As Monday, AWC.TuesdayConfigValue As Tuesday, 
// //                                           AWC.WednesdayConfigValue AS Wednesday, AWC.ThursdayConfigValue AS Thursday, AWC.FridayConfigValue As Friday, 
// //                                           AWC.SaturdayConfigValue AS Saturday, ACT.ConfigTypeName AS 'ConfigType' FRom AssetWeeklyConfig AWC 
// //                                           Inner Join AssetConfigType ACT On ACT.AssetConfigTypeID = AWC.fk_AssetConfigTypeID And ACT.ConfigTypeName IN ('" +
// //                                           AssetTargetType.PayloadinTonnes.ToString() + "', '" + AssetTargetType.CycleCount + "', '" + AssetTargetType.VolumeinCuMeter + @"')
// //                                           Where AWC.fk_AssetUID In ({0}) AND ('{1}' Between StartDate And EndDate OR '{2}' Between StartDate And EndDate) Order By AWC.StartDate)A 
// //                                           UNION
// //                                           Select * From (Select HEX(AWC.fk_AssetUID) As AssetID, HEX(AWC.AssetWeeklyConfigUID) As AssetWeeklyConfigUID, AWC.SundayConfigValue As Sunday, AWC.MondayConfigValue As Monday, AWC.TuesdayConfigValue As Tuesday, 
// //                                           AWC.WednesdayConfigValue AS Wednesday, AWC.ThursdayConfigValue AS Thursday, AWC.FridayConfigValue As Friday, 
// //                                           AWC.SaturdayConfigValue AS Saturday, AWC.StartDate as StartDate, AWC.EndDate as EndDate, ACT.ConfigTypeName AS 'ConfigType' FRom AssetWeeklyConfig AWC 
// //                                           Inner Join AssetConfigType ACT On ACT.AssetConfigTypeID = AWC.fk_AssetConfigTypeID And ACT.ConfigTypeName IN ('" +
// //                                           AssetTargetType.PayloadinTonnes.ToString() + "', '" + AssetTargetType.CycleCount + "', '" + AssetTargetType.VolumeinCuMeter + @"')
// //                                           Where AWC.fk_AssetUID In ({0}) AND (StartDate Between '{1}' AND '{2}' OR EndDate Between '{1}' AND '{2}') Order By AWC.StartDate ASC) B",
// //                           AssetUIDs, startDate.ToDateTimeStringWithYearMonthDayFormat(), endDate.ToDateTimeStringWithYearMonthDayFormat());

// //               var result = await _connection.FetchAsync<ProductivityTargetsDBResponse>(sql);
//	//			return result;
// //           }
// //           catch (Exception e)
// //           {
// //               throw;
// //           }     
// //       }

// //       public async Task<IEnumerable<ProductivityTargetsDBResponse>> GetProductivityTargetsDetailsByAssetId(string AssetUID)
// //       {
// //           try
// //           {
// //               var sql = string.Format(@"Select HEX(AWC.fk_AssetUID) As AssetUID, HEX(AWC.AssetWeeklyConfigUID) As AssetWeeklyConfigUID, AWC.SundayConfigValue As Sunday, AWC.MondayConfigValue As Monday, AWC.TuesdayConfigValue As Tuesday, 
// //                                           AWC.WednesdayConfigValue AS Wednesday, AWC.ThursdayConfigValue AS Thursday, AWC.FridayConfigValue As Friday, 
// //                                           AWC.SaturdayConfigValue AS Saturday, AWC.StartDate as StartDate, AWC.EndDate as EndDate, ACT.ConfigTypeName AS 'ConfigType' FRom AssetWeeklyConfig AWC 
// //                                           Inner Join AssetConfigType ACT On ACT.AssetConfigTypeID = AWC.fk_AssetConfigTypeID And ACT.ConfigTypeName IN ('" +
// //                                           AssetTargetType.PayloadinTonnes.ToString() + "', '" + AssetTargetType.CycleCount + "', '" + AssetTargetType.VolumeinCuMeter + @"')
// //                                           Where AWC.fk_AssetUID In ({0}) Order By AWC.StartDate Desc",
// //                           AssetUID);

//	//			var result = await _connection.FetchAsync<ProductivityTargetsDBResponse>(sql);
//	//			return result;
//	//		}
// //           catch (Exception e)
// //           {
// //               throw;
// //           }
// //       }

// //       public async Task<int> UpdateProductivityTargets(List<ProductivityTargetsDBResponse> targetDBResponse)
// //       {
// //           try
// //           {
// //               var sql = @"Update AssetWeeklyConfig SET StartDate=@StartDate,EndDate=@EndDate,SundayConfigValue=@Sunday,MondayConfigValue=@Monday,
// //                       TuesdayConfigValue = @Tuesday,WednesdayConfigValue = @Wednesday,ThursdayConfigValue = @Thursday,FridayConfigValue = @Friday,SaturdayConfigValue = @Saturday,UpdateUTC = '{0}'
// //                       Where AssetWeeklyConfigUID = UNHEX(@AssetWeeklyConfigUID) and fk_AssetConfigTypeID = @ConfigValue";

//	//			var result = await _connection.ExecuteAsync(string.Format(sql, DateTime.UtcNow.ToDateTimeStringWithYearMonthDayFormat()), targetDBResponse);
//	//			return result;
// //           }
// //           catch (Exception e)
// //           {
// //               throw;
// //           }
// //       }

// //       public async Task<int> InsertProductivityTargets(List<ProductivityTargetsDBResponse> targetDBResponse)
// //       {
// //               try
// //               {
// //                   var InsertUTC = DateTime.UtcNow;
// //                   const string insert =
// //                          @"INSERT INTO AssetWeeklyConfig (AssetWeeklyConfigUID,fk_AssetUID,fk_AssetConfigTypeID,StartDate,EndDate,SundayConfigValue,MondayConfigValue,
// //                           TuesdayConfigValue,WednesdayConfigValue,ThursdayConfigValue,FridayConfigValue,SaturdayConfigValue, InsertUTC) 
// //                           Values (UNHEX(@AssetWeeklyConfigUID), UNHEX(@AssetID), @ConfigValue, @StartDate, @EndDate, @Sunday, @Monday, @Tuesday, @Wednesday, @Thursday, @Friday, @Saturday, '{0}');";

//	//				var result = await _connection.ExecuteAsync(string.Format(insert, InsertUTC.ToDateTimeStringWithYearMonthDayFormat()), targetDBResponse);
//	//				return result;
// //               }
// //               catch (MySqlException ex)
// //               {
// //                   throw ex;
// //               }

// //       }

// //       public async Task<int> DeleteAssetTargets(string assetWeeklyIdentifiers)
// //       {
// //           try
// //           {
// //               var sql = @"Delete From AssetWeeklyConfig Where AssetWeeklyConfigUID In ({0})";
// //               return await _connection.ExecuteAsync(string.Format(sql, assetWeeklyIdentifiers));
// //           }
// //           catch (Exception e)
// //           {
// //               throw;
// //           }
// //       }
// //   }
//}
}