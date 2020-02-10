using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonModel.DeviceSettings;

namespace Infrastructure.Service.DeviceConfig.Interfaces
{
    public interface IDeviceConfigUserMasterDataService
    {
        Task<LoginUserModel> FetchByUserUID(LoginUserModel loginUserModel);
        Task<LoginUserModel> Insert(LoginUserModel loginUserModel);
        Task<LoginUserModel> Update(LoginUserModel loginUserModel);
    }
}
