using System;

namespace VSS.Common.Abstractions.Clients.CWS
{
  public class TRNHelper
  {
    const int TRN_UID = 5;
    public const string TRN_ACCOUNT = "account";
    public const string TRN_USER = "user";
    public const string TRN_PROJECT = "project";
    public const string TRN_DEVICE = "device";

    /// <summary>
    /// Extracts the Guid at the end of the TRN for internal use as a unique Guid
    ///    e.g. "trn::profilex:us-west-2:project:eaf7260e-946a-4019-a92d-fab11683149e"
    ///         "trn::profilex:us-west-2:device:08d6f403-521c-997d-d30d-af0001000938"
    ///  public string ProjectTRN { get; set; } = null;
    //   public Guid? ProjectUID {get { return TRNHelper.ExtractGuid(ProjectTRN); }}
    /// </summary>
    public static Guid? ExtractGuid(string TRN)
    {
      if (TRN != null)
      {
        var allFields = TRN.Split(':');
        if (allFields.Length == TRN_UID + 1
            && Guid.TryParse(allFields[TRN_UID], out var guid))
          return guid;
      }
      return null;
    }
    public static string ExtractGuidAsString(string TRN)
    {
      var uid = ExtractGuid(TRN);
      return uid == null ? null : uid.ToString();
    }

    public static string MakeTRN(Guid guid, string type = TRN_PROJECT, string region = "us-west-2", string source = "trn::profilex")
    {
      if (guid != null && guid != Guid.Empty)
      {
        return $"{source}:{region}:{type}:{guid}";
      }
      return null;
    }

    public static string MakeTRN(string guid, string type = TRN_PROJECT, string region = "us-west-2", string source = "trn::profilex")
    {
      if (!string.IsNullOrEmpty(guid))
      {
        return $"{source}:{region}:{type}:{guid}";
      }
      return null;
    }
  }
}
