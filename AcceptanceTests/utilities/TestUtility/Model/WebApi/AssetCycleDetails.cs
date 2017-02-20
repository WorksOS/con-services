using System.Collections.Generic;

namespace TestUtility.Model.WebApi
{
    public class AssetCycleDetails
    {
        public AssetCycleData asset;

        public List<Cycle> cycles;
        public ContractExecutionStatesEnum Code { get; set; }
        public string Message { get; set; }
    }
}
