using ClientModel.Response;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ClientModel.AssetSettings.Response
{
	[ExcludeFromCodeCoverage]
    public abstract class SearchBaseResponse<TLists, TErrorInfo> : BaseResponse<TLists, TErrorInfo> where TLists : class
    {
        [JsonProperty("pageInfo", NullValueHandling = NullValueHandling.Ignore)]
        public RecordInfo RecordInfo { get; set; }

        public SearchBaseResponse() { }

        public SearchBaseResponse(TErrorInfo error) : base(error) { }

        public SearchBaseResponse(List<TErrorInfo> errors) : base(errors) { }

        public SearchBaseResponse(List<TLists> lists) : base(lists) { }

        public SearchBaseResponse(List<TLists> lists, int currentPageSize, int currentPageNumber, long totalRows) : base(lists)
        {
            this.RecordInfo = RecordInfo.GetRecordInfo(currentPageSize, currentPageNumber, totalRows, lists.Count());
        }
    }
}