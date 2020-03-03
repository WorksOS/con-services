//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Utilities.Logging;
//using Interfaces;
//using DbModel;
//using DbModel.DeviceConfig;
//using VSS.MasterData.WebAPI.Transactions;

//namespace DeviceConfigRepository.MySql
//{
//    public class LoginUserRepository : ILoginUserRepository
//    {
//        private readonly ITransactions _transactions;
//        private readonly ILoggingService _loggingService;

//        public LoginUserRepository(ITransactions transactions, ILoggingService loggingService)
//        {
//            this._transactions = transactions;
//            this._loggingService = loggingService;
//            this._loggingService.CreateLogger(this.GetType());
//        }

//        public async Task<LoginUserDto> Insert(LoginUserDto loginUserDto)
//        {
//            try
//            {
//                this._loggingService.Debug("Started executing query", "LoginUserDto.Insert");
//                var response = await this._transactions.Upsert(loginUserDto);
//                this._loggingService.Debug("Ended executing query", "LoginUserDto.Insert");
//                return response > 0 ? loginUserDto : null;
//            }
//            catch (Exception ex)
//            {
//                this._loggingService.Error("Exception occurred while executing query", "LoginUserDto.Insert", ex);
//                throw ex;
//            }
//        }

//        public async Task<LoginUserDto> Update(LoginUserDto loginUserDto)
//        {
//            try
//            {
//                this._loggingService.Debug("Started executing query", "LoginUserDto.Update");
//                var response = await this._transactions.Upsert(loginUserDto);
//                this._loggingService.Debug("Ended executing query", "LoginUserDto.Update");
//                return response > 0 ? loginUserDto : null;
//            }
//            catch (Exception ex)
//            {
//                this._loggingService.Error("Exception occurred while executing query", "LoginUserDto.Update", ex);
//                throw ex;
//            }
//        }

//        public async Task<LoginUserDto> Fetch(LoginUserDto loginUserDto)
//        {
//            try
//            {
//                this._loggingService.Debug("Started executing query", "LoginUserDto.Fetch");
//                var response = await this._connection.FetchAsync<LoginUserDto>(Queries.SELECT_LOGINUSER_BY_USERUID, loginUserDto);
//                this._loggingService.Debug("Ended executing query", "LoginUserDto.Fetch");
//                return response.FirstOrDefault();
//            }
//            catch (Exception ex)
//            {
//                this._loggingService.Error("Exception occurred while executing query", "LoginUserDto.Fetch", ex);
//                throw ex;
//            }
//        }
//    }
//}
