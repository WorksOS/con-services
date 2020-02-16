using System;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Hosted.VLCommon;
using System.Text.RegularExpressions;

namespace VSS.Hosted.VLCommon.Bss
{
  public class AccountHierarchyValidator : Validator<AccountHierarchy>
  {
    private const string EMAIL_PATTERN = @"^([\w+_\-'”.]{1,})@(([\w_\-'”]+\.)+[\w_\-'”]{2,})$";    

    public override void Validate(AccountHierarchy message)
    {
      Require.IsNotNull(message, "AccountHierarchy");

      #region General Validations

      // Error - Sequence Number is not defined
      if (message.SequenceNumber.IsNotDefined())
        AddError(BssFailureCode.SequenceNumberNotDefined, BssConstants.SEQUENCE_NUMBER_NOT_DEFINED);

      // Error - Control Number is not defined
      if (message.ControlNumber.IsNotDefined())
        AddError(BssFailureCode.ControlNumberNotDefined, BssConstants.CONTROL_NUMBER_NOT_DEFINED);

      #endregion

      #region Account Hierarchy Validations

      // Error - Action is invalid
      if (!BssMessageAction.IsValidForMessage(message.Action, message))
        AddError(BssFailureCode.ActionInvalid, string.Format(BssConstants.ACTION_INVALID_FOR_MESSAGE, message.Action, typeof(AccountHierarchy).Name));

      // Error - BSSID is not defined
      if (message.BSSID.IsNotDefined())
        AddError(BssFailureCode.BssIdNotDefined, BssConstants.BSSID_NOT_DEFINED);

      // Error - Customer Name is not defined
      if (message.CustomerName.IsNotDefined())
        AddError(BssFailureCode.CustomerNameNotDefined, BssConstants.Hierarchy.CUSTOMER_NAME_NOT_DEFINED);

      // Error - ParentBSSID is not defined and RelationshipID is defined
      if (message.ParentBSSID.IsNotDefined() && message.RelationshipID.IsDefined())
        AddError(BssFailureCode.ParentBssIdNotDefined, BssConstants.Hierarchy.PARENT_BSSID_NOT_DEFINED);

      // Error - RelationshipID is defined and ParentBSSID is not defined
      if (message.ParentBSSID.IsDefined() && message.RelationshipID.IsNotDefined())
        AddError(BssFailureCode.RelationshipIdNotDefined, BssConstants.Hierarchy.RELATIONSHIPID_NOT_DEFINED);

      if (message.ParentBSSID.IsDefined() && message.BSSID.IsStringEqual(message.ParentBSSID))
        AddError(BssFailureCode.RelationshipInvalid, BssConstants.Hierarchy.RELATIONSHIP_TO_SELF_INVALID);

      /*
		   * PrimaryContact is a special case
             * It can only appear on a Created or Updated Action
       * Instead of putting checks for PrimaryContact
       * in all other Validators, we put a single one here.
		   */

      // Error - Action not Created and Primary Contact is defined
      if (IsContactDefined(message.contact) && !(string.Equals(message.Action, ActionEnum.Created.ToString(), StringComparison.InvariantCultureIgnoreCase) ||
        string.Equals(message.Action, ActionEnum.Updated.ToString(), StringComparison.InvariantCultureIgnoreCase)))
      {
        AddError(BssFailureCode.PrimaryContactInvalid, BssConstants.Hierarchy.PRIMARY_CONTACT_DEFINED);
      }
      else
      {
        if (!string.Equals(AccountHierarchy.BSSCustomerTypeEnum.ACCOUNT.ToString(), message.CustomerType, StringComparison.InvariantCultureIgnoreCase))
        {
          if (message.contact.IsNotDefined() && message.Action == ActionEnum.Created.ToString())
            AddError(BssFailureCode.PrimaryContactInvalid, BssConstants.Hierarchy.PRIMARY_CONTACT_NOT_DEFINED);

          // For Contact - all fields must be defined.
          if (!message.contact.IsNotDefined())
          {
            // Error - Primary Contact First Name not defined
            if (message.contact.FirstName.IsNotDefined())
              AddError(BssFailureCode.PrimaryContactInvalid, BssConstants.Hierarchy.PRIMARY_CONTACT_FIRST_NAME_NOT_DEFINED);

            // Eror - Primary Contact Last Name not defined
            if (message.contact.LastName.IsNotDefined())
              AddError(BssFailureCode.PrimaryContactInvalid, BssConstants.Hierarchy.PRIMARY_CONTACT_LAST_NAME_NOT_DEFINED);

            // Error - Primary Contact Email not defined
            if (message.contact.Email.IsNotDefined())
              AddError(BssFailureCode.PrimaryContactInvalid, BssConstants.Hierarchy.PRIMARY_CONTACT_EMAIL_NOT_DEFINED);
            // Error - Primary Contact Email Format is invalid
            else if (!Regex.IsMatch(message.contact.Email, EMAIL_PATTERN))
              AddError(BssFailureCode.PrimaryContactInvalid, string.Format(BssConstants.Hierarchy.PRIMARY_CONTACT_EMAIL_INVALID, message.contact.Email));
          }
        }
      }

      #endregion

      #region CustomerType Validations

      switch (message.CustomerType.ToEnum<AccountHierarchy.BSSCustomerTypeEnum>())
      {

        #region Dealer CustomerType

        case AccountHierarchy.BSSCustomerTypeEnum.DEALER:

          // Error - Customer Type is Dealer - Hierarchy Type is TCS Customer
          if (message.HierarchyType != "TCS Dealer")
            AddError(BssFailureCode.HierarchyTypeInvalid, string.Format(BssConstants.Hierarchy.HIERARCHY_TYPE_INVALID, message.HierarchyType));

          // Warning - Customer Type is Dealer - Dealer Network is not defined
          if (message.DealerNetwork.IsNotDefined())
            AddWarning(BssConstants.Hierarchy.DEALER_NETWORK_NOT_DEFINED);

          // Warning - Customer Type is Dealer - Network Customer Code is defined
          if (message.NetworkCustomerCode.IsDefined())
            AddWarning(BssConstants.Hierarchy.NETWORK_CUSTOMER_CODE_DEFINED);

          // Warning - Customer Type is Dealer - Dealer Account Code is defined
          if (message.DealerAccountCode.IsDefined())
            AddWarning(BssConstants.Hierarchy.DEALER_ACCOUNT_CODE_DEFINED);

          // Warning - Customer Type is Dealer - Network Dealer Code is not defined
          if (message.NetworkDealerCode.IsNotDefined())
            AddWarning(BssConstants.Hierarchy.NETWORK_DEALER_CODE_NOT_DEFINED);

          break;

        #endregion

        #region Customer CustomerType

        case AccountHierarchy.BSSCustomerTypeEnum.CUSTOMER:

          // Error - Customer Type is Customer - Hierarchy Type is not TCS Customer
          if (message.HierarchyType != "TCS Customer")
            AddError(BssFailureCode.HierarchyTypeInvalid, string.Format(BssConstants.Hierarchy.HIERARCHY_TYPE_INVALID, message.HierarchyType));

          // Warning - Customer Type is Customer - Dealer Network is defined
          if (message.DealerNetwork.IsDefined())
            AddWarning(BssConstants.Hierarchy.DEALER_NETWORK_DEFINED);

          // Warning - Customer Type is Customer - Network Dealer Code is defined
          if (message.NetworkDealerCode.IsDefined())
            AddWarning(BssConstants.Hierarchy.NETWORK_DEALER_CODE_DEFINED);

          // Warning - Customer Type is Customer - Network Customer Code is defined
          if (message.NetworkCustomerCode.IsDefined())
            AddWarning(BssConstants.Hierarchy.NETWORK_CUSTOMER_CODE_DEFINED);

          // Warning - Customer Type is Customer - Dealer Account Code is defined
          if (message.DealerAccountCode.IsDefined())
            AddWarning(BssConstants.Hierarchy.DEALER_ACCOUNT_CODE_DEFINED);

          break;

        #endregion

        #region Account CustomerType

        case AccountHierarchy.BSSCustomerTypeEnum.ACCOUNT:

          // Failure when Hierarchy Type is invalid
          if (message.HierarchyType != "TCS Dealer" &&
              message.HierarchyType != "TCS Customer")
            AddError(BssFailureCode.HierarchyTypeInvalid, string.Format(BssConstants.Hierarchy.HIERARCHY_TYPE_INVALID, message.HierarchyType));

          // Warning - Customer Type is Account - Dealer Network is defined
          if (message.DealerNetwork.IsDefined())
            AddWarning(BssConstants.Hierarchy.DEALER_NETWORK_DEFINED);

          // Error - Customer Type is Account - Network Dealer Code is defined
          if (message.NetworkDealerCode.IsDefined())
            AddWarning(BssConstants.Hierarchy.NETWORK_DEALER_CODE_DEFINED);

          // Warning - Customer Type is Account - Dealer Account Code is not defined
          if (message.DealerAccountCode.IsNotDefined())
            AddWarning(BssConstants.Hierarchy.DEALER_ACCOUNT_CODE_NOT_DEFINED);

          // Warning - Customer Type is Account - Network Customer Code not is defined
          if (message.NetworkCustomerCode.IsNotDefined())
            AddWarning(BssConstants.Hierarchy.NETWORK_CUSTOMER_CODE_NOT_DEFINED);

          break;

        #endregion

        default:
          AddError(BssFailureCode.CustomerTypeInvalid, string.Format(BssConstants.Hierarchy.CUSTOMER_TYPE_INVALID, message.CustomerType));
          break;
      }

      #endregion
    }

    private bool IsContactDefined(PrimaryContact contact)
    {
      return
        contact != null &&
        (!string.IsNullOrWhiteSpace(contact.FirstName) ||
        !string.IsNullOrWhiteSpace(contact.LastName) ||
        !string.IsNullOrWhiteSpace(contact.Email));
    }
  }
}