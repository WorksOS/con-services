namespace VSS.Productivity3D.Filter.Abstractions.Models
{
  public enum FilterCombinationRole
  {
    /// <summary>
    /// Filter has no specific role to play in terms of special semantics governing its combination
    /// with other filters. The filter combiner may reject filters with this role
    /// </summary>
    Undefined,

    /// <summary>
    /// The initial master filter to which filters with other combination roles may be added
    /// </summary>
    MasterFilter,

    /// <summary>
    /// A filter defining VL 3DPM widget specific filtering to be combined with a master filter
    /// </summary>
    WidgetFilter,

    /// <summary>
    /// A filter defining specific volumes widget only filtering conditions (such as time range) to be applied as a
    /// final combined filter decoration
    /// </summary>
    VolumesFilter
  }
}
