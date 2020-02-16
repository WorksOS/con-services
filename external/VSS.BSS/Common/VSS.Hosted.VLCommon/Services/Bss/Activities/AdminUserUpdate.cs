using System;

namespace VSS.Hosted.VLCommon.Bss
{
  public class AdminUserUpdate : Activity
  {
    const string SUCCESS_MESSAGE = @"Updated Admin User with First Name: {0} Last Name: {1} Email: {2} UserName: {3} ";
    const string SKIP_UPDATE_MESSAGE = @"Skipping Admin User Update";
    const string FAILURE_MESSAGE = @"Failed to Update Admin User for Customer: {0}. See InnerException for details.";
    const string ADMIN_USER_VERIFIED = @"Admin User is already verified with Email: {0}.";
    const string ADMIN_USER_DOESNT_EXIST = @"Admin User doesn't exist for Customer: {0}.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<CustomerContext>();
      User firstAdminUser;

      try
      {

        firstAdminUser = Services.Customers().GetFirstAdminUser(context.Id);

        if (firstAdminUser != null && firstAdminUser.Active)
        {
          if (!firstAdminUser.IsEmailValidated && firstAdminUser.EmailVerificationUTC != null && firstAdminUser.EmailVerificationGUID != null)
          {
            Services.Customers().UpdateAdminUser(firstAdminUser.ID, context.AdminUser.FirstName, context.AdminUser.LastName, context.AdminUser.Email);
            return Success(SUCCESS_MESSAGE, firstAdminUser.FirstName, firstAdminUser.LastName, firstAdminUser.EmailContact, firstAdminUser.Name);
          }
          else
            AddSummary(ADMIN_USER_VERIFIED,firstAdminUser.EmailContact);
        }
        else
        {
          AddSummary(ADMIN_USER_DOESNT_EXIST,context.Name);
        }

      }
      catch (Exception ex)
      {
        return Exception(ex, FAILURE_MESSAGE,
          context.Name);
      }
      return Warning(SKIP_UPDATE_MESSAGE);
    }
  }
}

