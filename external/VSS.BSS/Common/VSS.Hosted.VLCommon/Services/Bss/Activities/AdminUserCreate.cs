using System;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class AdminUserCreate : Activity
  {
    public const string FIRSTNAME_NOT_DEFINED = @"FirstName is not defined.";
    public const string LASTNAME_NOT_DEFINED = @"LastName is not defined.";
    public const string EMAIL_NOT_DEFINED = @"Email is not defined.";
    
    public const string SUCCESS_MESSAGE = @"Created AdminUser Name: {0} {1} Email: {2} for {3} Name: {4} BSSID: {5}.";
    public const string USER_NULL_MESSAGE = @"Creation of user came back null for unknown reason.";
    public const string FAILURE_MESSAGE =  @"Failed to create Admin User for {0} Name: {1} BSSID: {2}. See InnerException for details.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<CustomerContext>();

      if(string.IsNullOrWhiteSpace(context.AdminUser.FirstName))
      {
        context.AdminUser.FirstName = string.Empty;
        AddSummary(FIRSTNAME_NOT_DEFINED);
      }
        
      if(string.IsNullOrWhiteSpace(context.AdminUser.LastName))
      {
        context.AdminUser.LastName = string.Empty;
        AddSummary(LASTNAME_NOT_DEFINED);
      }
        
      if(string.IsNullOrWhiteSpace(context.AdminUser.Email))
      {
        context.AdminUser.Email = "NoEmailProvided@empty.com"+ DateTime.UtcNow.Ticks.ToString();
        AddSummary(EMAIL_NOT_DEFINED);
      }

      User newUser;

      try
      {
         newUser = Services.Customers().CreateAdminUser(context.Id, context.AdminUser.FirstName, context.AdminUser.LastName, context.AdminUser.Email);
        
        if(newUser == null)
          return Error(USER_NULL_MESSAGE);
        
        context.AdminUserExists = true;
      }
      catch (Exception ex)
      {
        return Exception(ex, FAILURE_MESSAGE, 
          context.New.Type, 
          context.New.Name, 
          context.New.BssId);
      }

      return Success(SUCCESS_MESSAGE, 
        newUser.FirstName, 
        newUser.LastName, 
        newUser.EmailContact, 
        context.New.Type,
        context.New.Name, 
        context.New.BssId);
    }
  }
}