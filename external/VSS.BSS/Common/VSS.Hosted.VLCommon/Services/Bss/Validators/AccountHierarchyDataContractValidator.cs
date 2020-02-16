using System;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class AccountHierarchyDataContractValidator : DataContractValidator<AccountHierarchy>
  {
    public override void Validate(AccountHierarchy message)
    {
      if (message == null)
        throw new ArgumentNullException("message");

      base.Validate(message);

      if (!message.BSSID.isNumeric())
        AddError(BssFailureCode.BssIdInvalid, BssConstants.Hierarchy.BSSID_NOT_VALID);

      if (message.ParentBSSID.IsDefined() && !message.ParentBSSID.isNumeric())
        AddError(BssFailureCode.ParentBssIdInvalid, BssConstants.Hierarchy.PARENT_BSSID_NOT_VALID);

      if (message.RelationshipID.IsDefined() && !message.RelationshipID.isNumeric())
        AddError(BssFailureCode.RelationshipIdInvalid, BssConstants.Hierarchy.RELATIONSHIPID_NOT_VALID);

      if (!message.CustomerType.isStringWithNoSpaces())
        AddError(BssFailureCode.CustomerTypeInvalid, BssConstants.Hierarchy.CUSTOMER_TYPE_NOT_VALID);
    }
  }
}
