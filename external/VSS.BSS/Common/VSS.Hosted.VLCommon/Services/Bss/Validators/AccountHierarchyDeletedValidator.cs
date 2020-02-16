using VSS.Hosted.VLCommon.Bss.Schema.V2;

namespace VSS.Hosted.VLCommon.Bss
{
  public class AccountHierarchyDeletedValidator : Validator<AccountHierarchy>
  {
    public override void Validate(AccountHierarchy message)
    {
      Require.IsNotNull(message, "AccountHierarchy");

      //Action Delete - ParentBSSID not specified in the message - Invalid scenario
      if (string.IsNullOrWhiteSpace(message.ParentBSSID) == true)
        AddError(BssFailureCode.ParentBssIdNotDefined, BssConstants.Hierarchy.PARENT_BSSID_NOT_DEFINED);

      //Action Delete - RelationshipID not specified in the message - Invalid scenario
      if (string.IsNullOrWhiteSpace(message.RelationshipID) == true)
        AddError(BssFailureCode.RelationshipIdNotDefined, BssConstants.Hierarchy.RELATIONSHIPID_NOT_DEFINED);
    }
  }
}
