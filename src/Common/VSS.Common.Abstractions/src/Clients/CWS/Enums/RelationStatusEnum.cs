namespace VSS.Common.Abstractions.Clients.CWS.Enums
{
  // device relationship to an account
  //   in theory a device can only be ACTIVE with one account
  //   it will be "ACTIVE" only when the device is claimed(i.e Device login with this account)
  //   What does pending mean (in the process of moving from one account to another, perhaps?
  //   sometimes referred within cws as accountRegistrationStatus 

  // current rule in TFA is:
  //    "Only tag files from devices which have been claimed (i.e Device has logged with appropriate account) will be considered for manual or auto tag file ingress"
  public enum RelationStatusEnum
  {
    Unknown = 0, // null
    Active,      // cws passes all upper ACTIVE
    Pending,     // cws passes all upper PENDING
  }
}
