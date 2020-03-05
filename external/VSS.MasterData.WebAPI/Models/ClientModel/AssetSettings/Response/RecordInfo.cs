using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientModel.AssetSettings.Response
{
    public class RecordInfo
    {
        [JsonProperty("totalRecords", NullValueHandling = NullValueHandling.Include)]
        public long TotalRecords { get; set; }

        [JsonProperty("totalPages", NullValueHandling = NullValueHandling.Include)]
        public long TotalPages { get; set; }

        [JsonProperty("currentPageNumber", NullValueHandling = NullValueHandling.Include)]
        public long CurrentPageNumber { get; set; }

        [JsonProperty("currentPageSize", NullValueHandling = NullValueHandling.Include)]
        public long CurrentPageSize { get; set; }

		public static RecordInfo GetRecordInfo(int currentPageSize, int currentPageNumber, long totalRows, int recordsCount)
		{
			var totalPages = (int)Math.Ceiling((double)totalRows / currentPageSize);
			return new RecordInfo
			{
				TotalRecords = totalRows,
				TotalPages = totalPages,
				CurrentPageSize = currentPageSize,
				CurrentPageNumber = currentPageNumber
			};
		}
	}


}
