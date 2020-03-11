using ClientModel.AssetSettings.Request;
using DbModel.AssetSettings;
using DbModel.Cache;
using CommonModel.Enum;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;
using Constants = AssetSettingsRepository.Helpers.Constants;

namespace AssetSettingsRepository
{
	public class AssetSettingsListRepository : IAssetSettingsListRepository
    {
        private readonly ITransactions _transactions;
        private readonly ILoggingService _loggingService;
        private const string LogMethodFormat = "AssetSettingsListRepository.{0}";

        public AssetSettingsListRepository(ITransactions transactions, ILoggingService loggingService)
        {
            this._transactions = transactions;
            this._loggingService = loggingService;
            this._loggingService.CreateLogger(typeof(AssetSettingsListRepository));
        }

        public async Task<IEnumerable<string>> FetchValidAssetUIds(List<string> assetUids, AssetSettingsListRequestDto request)
        {
            try
            {
                this._loggingService.Debug("Started executing query", string.Format(LogMethodFormat, "FetchValidAssetUIds"));
                var result = await this._transactions.GetAsync<string>(
                    string.Format(Queries.FetchAssetUIdsWithUserCustomerAndAssets,
                    string.Join(",", assetUids.Select(x => "UNHEX('" + x + "')")).Replace("-", string.Empty)),
                    request);
                this._loggingService.Debug("Ended executing query", string.Format(LogMethodFormat, "FetchValidAssetUIds"));
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Tuple<int, IList<AssetSettingsListDto>>> FetchEssentialAssets(AssetSettingsListRequestDto request)
        {
            try
            {
                int foundRows = 0;
                this._loggingService.Debug("Building query", string.Format(LogMethodFormat, "FetchEssentialAssets"));
                var query = string.Format(string.Concat(Queries.FetchAssetsForCustomerAndUserUId, Queries.SelectFoundRows),
                    (request.PageNumber > 0) ? string.Format(Queries.LimitClause, (request.PageNumber - 1) * request.PageSize, request.PageSize) : string.Empty, // Limit offset and limit page size
                    !string.IsNullOrEmpty(request.SortColumn) ? string.Format(Queries.OrderByClause, this.BuildSortColumn(request), request.SortDirection) : string.Empty, // order by clause                
                    this.BuildTargetsFilterCriteria(request), // FilterCriteria
                    request.DeviceType);

                this._loggingService.Debug("Started executing query", string.Format(LogMethodFormat, "FetchEssentialAssets"));
                var resultSet = await this._transactions.GetMultipleResultSetAsync<AssetSettingsListDto, long>(query.ToString(), request);
                this._loggingService.Debug("Ended executing query", string.Format(LogMethodFormat, "FetchEssentialAssets"));
                var assetsLists = resultSet.Item1 as IList<AssetSettingsListDto>;
                var assetFoundRows = (resultSet.Item2 as IList<long>);
                if (assetFoundRows != null && assetFoundRows.Any())
                {
                    foundRows = Convert.ToInt32(assetFoundRows.FirstOrDefault());
                }
                return new Tuple<int, IList<AssetSettingsListDto>>(foundRows, assetsLists);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Tuple<int, IEnumerable<DeviceTypeDto>>> FetchDeviceTypesByAssetUID(AssetDeviceTypeRequest request)
        {
            try
            {
                string subAccountCustomerCondition = string.Empty;
                int foundRows = 0;
                this._loggingService.Debug("Started executing query", string.Format(LogMethodFormat, "FetchDeviceTypes"));

                if (!string.IsNullOrEmpty(request.SubAccountCustomerUid))
                {
                    subAccountCustomerCondition = subAccountCustomerCondition = " AND " + string.Format(" ca.fk_AssetUID IN ({0}) ", Queries.FetchAssetUIDsForSubAccountCustomerUID);
                }

                var resultSet = request.IsSwitchRequest ? request.AssetUIDs != null && request.AssetUIDs.Count() > 0 && !request.AllAssets
                                    ? await this._transactions.GetMultipleResultSetAsync<DeviceTypeDto, long>(string.Format(Queries.FetchDeviceTypesForCustomerAndUserAndSwitchesGroup, subAccountCustomerCondition, string.Format(Queries.OrderByClause, "TypeName", "ASC"), Queries.SelectFoundRows, string.Format("And {0}", string.Format(Constants.AssetSettingsFilterConfig[AssetSettingsFilters.AssetUID], string.Join(",", request.AssetUIDs.Select(assetUID => string.Format("UNHEX('{0}')", assetUID.ToString("N"))))))), request)
                                    : await this._transactions.GetMultipleResultSetAsync<DeviceTypeDto, long>(string.Format(Queries.FetchDeviceTypesForCustomerAndUserAndSwitchesGroup, subAccountCustomerCondition, string.Format(Queries.OrderByClause, "TypeName", "ASC"), Queries.SelectFoundRows, string.Empty), request) :
                                    request.AssetUIDs != null && request.AssetUIDs.Count() > 0 && !request.AllAssets
                                    ? await this._transactions.GetMultipleResultSetAsync<DeviceTypeDto, long>(string.Format(Queries.FetchDeviceTypesForCustomerAndUser, subAccountCustomerCondition, string.Format(Queries.OrderByClause, "TypeName", "ASC"), Queries.SelectFoundRows, string.Format("And {0}", string.Format(Constants.AssetSettingsFilterConfig[AssetSettingsFilters.AssetUID], string.Join(",", request.AssetUIDs.Select(assetUID => string.Format("UNHEX('{0}')", assetUID.ToString("N"))))))), request)
                                    : await this._transactions.GetMultipleResultSetAsync<DeviceTypeDto, long>(string.Format(Queries.FetchDeviceTypesForCustomerAndUser, subAccountCustomerCondition, string.Format(Queries.OrderByClause, "TypeName", "ASC"), Queries.SelectFoundRows, string.Empty), request);

                this._loggingService.Debug("Ended executing query", string.Format(LogMethodFormat, "FetchEssentialAssets"));

                var deviceLists = resultSet.Item1 as IList<DeviceTypeDto>;
                var deviceFoundRows = resultSet.Item2 as IList<long>;
                if (deviceFoundRows != null && deviceFoundRows.Any())
                {
                    foundRows = Convert.ToInt32((deviceFoundRows).FirstOrDefault());
                }
                this._loggingService.Debug("Ended executing query", string.Format(LogMethodFormat, "FetchDeviceTypes"));
                return new Tuple<int, IEnumerable<DeviceTypeDto>>(foundRows, deviceLists);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private string BuildTargetsFilterCriteria(AssetSettingsListRequestDto request)
        {
            try
            {
                this._loggingService.Debug("Started Building filter criteria ", string.Format(LogMethodFormat, "BuildTargetsFilterCriteria"));
                StringBuilder criteria = new StringBuilder(string.Empty);

                var filterValue = this.GetFiltervalue(request.FilterValue);

                if (!string.IsNullOrEmpty(filterValue))
                {
                    AssetSettingsFilters filterType = (AssetSettingsFilters)Enum.Parse(typeof(AssetSettingsFilters), request.FilterName, true);
                    criteria.Append("AND " + string.Format(Constants.AssetSettingsFilterConfig[filterType], filterValue));
                }

                var deviceType = this.GetFiltervalue(request.DeviceType);

                if (!string.IsNullOrEmpty(deviceType))
                {
                    criteria.Append(" AND " + string.Format(Constants.AssetSettingsFilterConfig[AssetSettingsFilters.DeviceType], deviceType));
                }
                if (!string.IsNullOrEmpty(request.SubAccountCustomerUid))
                {
                    criteria.Append(" AND " + string.Format(Constants.AssetSettingsFilterConfig[AssetSettingsFilters.SubAccountCustomerUid], Queries.FetchAssetUIDsForSubAccountCustomerUID));
                }

                this._loggingService.Debug(" Filter Criteria : " + criteria.ToString(), MethodInfo.GetCurrentMethod().Name);
                this._loggingService.Debug("Ended Building Filter Criteria", MethodInfo.GetCurrentMethod().Name);
                return criteria.ToString();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private string GetFiltervalue(string filterValue)
        {
            string result = null;
            if (!string.IsNullOrEmpty(filterValue))
            {
                result = filterValue.Trim();
                if (!string.IsNullOrEmpty(result))
                {
                    result = result.Replace("%", "\\%").Replace("_", "\\_").Replace("'", "\\'");
                }
            }
            return result;
        }

        private string BuildSortColumn(AssetSettingsListRequestDto request)
        {
            try
            {
                this._loggingService.Debug("Started Building sort column ", string.Format(LogMethodFormat, "BuildSortColumn"));
                StringBuilder criteria = new StringBuilder(string.Empty);

                if (!string.IsNullOrEmpty(request.SortColumn))
                {
                    AssetSettingsSortColumns sortColumn = (AssetSettingsSortColumns)Enum.Parse(typeof(AssetSettingsSortColumns), request.SortColumn, true);
                    criteria.Append(Constants.AssetSettingsSortConfig[sortColumn]);
                }

                this._loggingService.Debug("sort column : " + criteria.ToString(), string.Format(LogMethodFormat, "BuildSortColumn"));
                this._loggingService.Debug("Ended Building sort column", string.Format(LogMethodFormat, "BuildSortColumn"));
                return criteria.ToString();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
