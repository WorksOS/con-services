using CommonModel.Error;
using System.Collections.Generic;

namespace ClientModel.AssetSettings.Response
{
	public class AssetSettingsServiceResponse<T>
    { 
        public IList<T> AssetSettingsLists { get; set; }
        public IList<IErrorInfo> Errors { get; set; }
    }
}
