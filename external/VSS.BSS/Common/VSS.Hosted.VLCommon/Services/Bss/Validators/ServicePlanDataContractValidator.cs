using System;
using VSS.Hosted.VLCommon.Bss.Schema.V2;

namespace VSS.Hosted.VLCommon.Bss
{
  public class ServicePlanDataContractValidator : DataContractValidator<ServicePlan>
  {
    public override void Validate(ServicePlan message)
    {
      Require.IsNotNull(message, "ServicePlan");

      base.Validate(message);

      if (message.Action.isStringWithNoSpaces() && !BssMessageAction.IsValidForMessage(message.Action, message))
        AddError(BssFailureCode.ActionInvalid, string.Format(BssConstants.ACTION_INVALID_FOR_MESSAGE, message.Action, "ServicePlan"));

      if (!message.IBKey.isNumeric())
        AddError(BssFailureCode.IbKeyInvalid, BssConstants.IBKEY_NOT_VALID);

      if (message.IBKey.isNumeric() && Convert.ToInt64(message.IBKey).IsNotDefined())
        AddError(BssFailureCode.IbKeyInvalid, BssConstants.IBKEY_NOT_DEFINED);

      if (message.ServicePlanName.IsNotDefined())
        AddError(BssFailureCode.ServicePlanNameNotDefined, BssConstants.ServicePlan.SERVICE_PLAN_NAME_NOT_DEFINED);

      if (message.ServiceTerminationDate.IsDefined() && !message.ServiceTerminationDate.isDateTimeValid())
        AddError(BssFailureCode.ServiceCancelDateInvalid, BssConstants.ServicePlan.SERVICE_TERMINATION_DATE_INVALID);

      if (message.ServicePlanlineID.IsNotDefined())
        AddError(BssFailureCode.ServicePlanLineIdNotDefined, BssConstants.ServicePlan.SERVICE_PLAN_LINE_ID_NOT_DEFINED);

      if (message.ServicePlanlineID.IsDefined() && !message.ServicePlanlineID.isNumeric())
        AddError(BssFailureCode.ServicePlanLineIdInvalid, BssConstants.ServicePlan.SERVICE_PLAN_LINE_ID_INVALID);

      if (message.OwnerVisibilityDate.IsDefined() 
          && (!message.OwnerVisibilityDate.isDateTimeValid()
          || DateTime.Parse(message.OwnerVisibilityDate) < new DateTime(2009, 01, 01)
          || DateTime.Parse(message.OwnerVisibilityDate) > new DateTime(2029, 01, 01)))
        AddError(BssFailureCode.OwnerVisibilityDateInvalid, BssConstants.ServicePlan.OWNER_VISIBILITY_DATE_INVALID);
    }
  }
}