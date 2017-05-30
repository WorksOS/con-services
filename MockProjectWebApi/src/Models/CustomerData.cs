namespace MockProjectWebApi.Models
{
  /// <summary>
  ///   Describes VL customer
  /// </summary>
  public class CustomerData 
  {
    /// <summary>
    /// Gets or sets the customer uid.
    /// </summary>
    /// <value>
    /// The customer uid.
    /// </value>
    public string Uid { get; set; }

    /// <summary>
    /// Gets or sets the customer name.
    /// </summary>
    /// <value>
    /// The customer name.
    /// </value>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the type of customer
    /// </summary>
    /// <value>
    /// The Customer Type
    /// </value>
    public string Type { get; set; }


    /// <summary>
    /// no caching of customer master data
    /// </summary>
    public string CacheKey
    {
      get { return ""; }
    }
  }
}
