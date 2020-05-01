namespace VSS.Common.Abstractions.Clients.CWS.Enums
{
  // device relationship to an account
  //   in theory a device can only be ACTIVE with one account
  //   What does pending mean (in the process of moving from one account to another, perhaps?
  public enum RelationStatusEnum
  {
    Unknown = 0, // null
    Active,      // cws passes all upper ACTIVE
    Pending,     // cws passes all upper PENDING
  }
}
