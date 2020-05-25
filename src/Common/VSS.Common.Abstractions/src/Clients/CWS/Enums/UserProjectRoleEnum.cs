using System.Runtime.Serialization;

namespace VSS.Common.Abstractions.Clients.CWS.Enums
{
  // A user may be invited to view a project
  // Until she has been invited, and accepted, in WM only the summary of the project will be available
  // Currently, WM only supports ADMIN role. If the user is not part of that project in that account then the value for role will be null.
  // What are rules in WorksOS?
  public enum UserProjectRoleEnum
  {
    [EnumMember(Value = null)]
    Unknown = 0,  // null

    [EnumMember(Value = "ADMIN")]
    Admin,        // user has accepted invitation and can now edit project btw strings in cws responses are all UPPERCASE
  }
}
