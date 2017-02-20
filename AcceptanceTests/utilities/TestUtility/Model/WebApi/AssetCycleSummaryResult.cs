using System.Collections.Generic;

namespace TestUtility.Model.WebApi
{
    public class AssetCycleSummaryResult
    {
        public List<AssetCycleData> assetCycles;
        public ContractExecutionStatesEnum Code { get; set; }
        public string Message { get; set; }
    }
}
