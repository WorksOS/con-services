
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.MasterData.Models.ResultHandling
{
  /// <summary>
  ///  Describes preference data returned by the preference master data service.
  /// </summary>
  public class UserPreferenceResult :  IMasterDataModel
  {
    /// <summary>
    /// THe preference key e.g. "global"
    /// </summary>
    public string PreferenceKeyName { get; set; }
    /// <summary>
    /// THe preference values as JSON
    /// </summary>
    public string PreferenceJson { get; set; }
    /// <summary>
    /// UID for the preference key
    /// </summary>
    public string PreferenceKeyUID { get; set; }
    /// <summary>
    /// Schema version
    /// </summary>
    public string SchemaVersion { get; set; }

    public List<string> GetIdentifiers() => new List<string>()
    {
      PreferenceKeyUID
    };
  }
}
