using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LegacyApiUserProvisioning.UserManagement.Interfaces
{
    public interface IUserManager
    {
        IEnumerable<IUser> GetUsersByOrganization(string customerUid);
		HttpStatusCode UpdateCustomerOfApiUsers(IMigrateUsersRequest migrateUsers);
        IEnumerable<IApiFeature> GetApiFeaturesByUserName(string useName);
        IUserCreationResponse CreateUser(IUserCreationRequest request);
        IUserEditResponse EditUser(IUserEditRequest userEditRequest);
        IUserDeleteResponse DeleteUsers(IUserDeleteRequest userDeleteRequest);
    }
}
