using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModel;
using DbModel.DeviceConfig;

namespace Interfaces
{
    public interface ILoginUserRepository
    {
        Task<LoginUserDto> Insert(LoginUserDto loginUserDto);
        Task<LoginUserDto> Update(LoginUserDto loginUserDto);
        Task<LoginUserDto> Fetch(LoginUserDto loginUserDto);
    }
}
