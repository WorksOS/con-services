using System;
using System.Collections.Generic;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class EditDataRequest : RequestBase
  {
    /// <summary>
    /// Project ID. Required.
    /// </summary>
    public long projectId { get; set; }

    /// <summary>
    /// Flag which determines if the edit is applied or undone. Required.
    /// </summary>
    public bool undo { get; set; }

    /// <summary>
    /// Details of the edit to apply or undo. Required for applying an edit and for a single undo.
    /// If null and undo is true then all edits to the production data for the project will be undone.
    /// </summary>
    public ProductionDataEdit dataEdit { get; set; }
  }

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
