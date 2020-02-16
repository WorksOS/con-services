using System.IdentityModel.Selectors;
using System.Linq;
using System.ServiceModel;

using VSS.Hosted.VLCommon;

namespace VSS.Nighthawk.NHBssSvc
{
  /// <summary>
  /// Used as the membership provider for Basic Authentication on the BssSvc endpoints.
  /// </summary>
  public class BssUserNameValidator : UserNamePasswordValidator
  {
    /// <summary>
    /// Validates a user against NH_OP.User.
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    public override void Validate(string userName, string password)
    {
      const int operations = (int)CustomerTypeEnum.Operations;
      const int admin = (int)FeatureAppEnum.NHAdmin;
      const int fullAccess = (int)FeatureAccessEnum.Full;

      using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var user = (from u in ctx.UserReadOnly
                    let hasAdmin = (from ufs in ctx.UserFeatureReadOnly where ufs.fk_User == u.ID && ufs.fk_Feature == admin && ufs.fk_FeatureAccess == fullAccess select ufs).Any()
                    join c in ctx.CustomerReadOnly on u.fk_CustomerID equals c.ID
                    where u.Name == userName &&
                          c.fk_CustomerTypeID == operations
                          && hasAdmin && u.Active
                    select new { u.PasswordHash, u.Salt }).FirstOrDefault();

        bool valid = false;

        if (user != null)
        {
          string pwdHash = HashUtils.ComputeHash(password, "SHA1", user.Salt);
          if (pwdHash == user.PasswordHash)
            valid = true;
        }
        if (!valid)
        {
          // This throws an informative fault to the client.
          throw new FaultException("Username or Password incorrect.");
        }
      }
    }
  }
}
