using System;
using System.Collections.Generic;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class EditDataResult : ResponseBase
  {
    public EditDataResult()
        : base("success")
    { }
  }

  /// <summary>
  /// List of ProductionDataEdit used as Injected Scenario Context for the tests.
  /// </summary>
  public class DataEditContext
  {
    public List<ProductionDataEdit> DataEdits { get; set; }
    public DataEditContext()
    {
      DataEdits = new List<ProductionDataEdit>();
    }
  }

  /// <summary>
  /// A representation of an edit applied to production data.
  /// </summary>
  public class ProductionDataEdit
  {
    /// <summary>
    /// The id of the machine whose data is overridden. Required.
    /// </summary>
    public long assetId { get; set; }

    /// <summary>
    /// Start of the period with overridden data. Required.
    /// </summary>
    public DateTime startUTC { get; set; }

    /// <summary>
    /// End of the period with overridden data. Required.
    /// </summary>
    public DateTime endUTC { get; set; }

    /// <summary>
    /// The design name used for the specified override period. May be null.
    /// </summary>
    public string onMachineDesignName { get; set; }

    /// <summary>
    /// The lift number used for the specified override period. May be null.
    /// </summary>
    public int? liftNumber { get; set; }
  }
}
