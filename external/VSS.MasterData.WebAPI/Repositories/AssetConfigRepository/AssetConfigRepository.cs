using DbModel.AssetSettings;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;

namespace AssetConfigRepository
{
	public class AssetConfigRepository : IAssetConfigRepository
	{
        private readonly ITransactions _transactions;
        private readonly ILoggingService _loggingService;

        public AssetConfigRepository(ITransactions transactions, ILoggingService loggingService)
        {
            this._transactions = transactions;
            this._loggingService = loggingService;
            this._loggingService.CreateLogger(GetType());
        }

        public async Task<IEnumerable<AssetSettingsDto>> FetchAssetConfig(List<string> assetUids, AssetSettingsDto request)
        {
            try
            {
                this._loggingService.Debug("Started executing query", MethodInfo.GetCurrentMethod().Name);
                var result = await this._transactions.GetAsync<AssetSettingsDto>(
                    string.Format(
                        Queries.FetchAssetConfig,
                        string.Join(",", assetUids.Select(x => "UNHEX('" + x + "')")).Replace("-", string.Empty), //Asset UIDs Lists
                        this.BuildFilterCriteria(request),
                        this.BuildTargetTypes(request)
                        ), request);
                this._loggingService.Debug("Ended executing query", MethodInfo.GetCurrentMethod().Name);
                return result;
            }
            catch (Exception ex)
            {
                this._loggingService.Error("Exception occurred while executing query", MethodInfo.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }

        private string BuildTargetTypes(AssetSettingsDto request)
        {
            return string.Join(",", request.TargetValues.Keys.Select(x => string.Format("'{0}'", x.ToString())));
        }

        private string BuildFilterCriteria(AssetSettingsDto request)
        {
            try
            {
                this._loggingService.Debug("Started Building Filter Criteria", MethodInfo.GetCurrentMethod().Name);
                StringBuilder filterCriteria = new StringBuilder(string.Empty);
                if (request.FilterCriteria != null)
                {
                    foreach (var filter in request.FilterCriteria)
                    {
                        var filterValue = filter.Value.Item2;
                        Type filterValueType;
                        if (filterValue != null)
                        {
                            filterValueType = filterValue.GetType();
                            if (filterValueType == typeof(string) || filterValueType == typeof(DateTime))
                            {
                                filterCriteria.AppendFormat("{0} {1} '{2}' AND ", filter.Value.Item1, filter.Key, filterValue);
                            }
                            else if (filterValueType == null)
                            {
                                filterCriteria.AppendFormat("{0} {1} {2} AND ", filter.Value.Item1, filter.Key, filterValue);
                            }
                        }
                        else
                        {
                            if (string.Compare(filter.Key, "is", true) >= 0)
                            {
                                filterCriteria.AppendFormat("{0} is null AND ", filter.Value.Item1);
                            }
                        }
                    }
                }
                this._loggingService.Debug("Built Filter Criteria : " + filterCriteria.ToString(), MethodInfo.GetCurrentMethod().Name);
                this._loggingService.Debug("Ended Building Filter Criteria", MethodInfo.GetCurrentMethod().Name);
                return filterCriteria.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
