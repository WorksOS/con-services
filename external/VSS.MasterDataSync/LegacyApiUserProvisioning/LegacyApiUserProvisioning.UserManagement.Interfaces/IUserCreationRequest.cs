using System.Collections.Generic;

namespace LegacyApiUserProvisioning.UserManagement.Interfaces
{
    public interface IUserCreationRequest
    {
        string Email { get; set; }
        long CustomerId { get; set; }
        string UserName { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string Password { get; set; }
        IEnumerable<int> Features { get; set; }        
        string CreatedBy { get; set; }
        string CustomerUid { get; set; }

    }
}