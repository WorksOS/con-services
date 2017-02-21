
using System.Collections.Generic;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using VSS.Raptor.Service.Common.Contracts;

namespace VSS.Raptor.Service.WebApiModels.ProductionData.ResultHandling
{
  /// <summary>
  /// The represenation of the results of an edit data request.
  /// </summary>
  public class EditDataResult : ContractExecutionResult
  {
    /// <summary>
    /// The collection of data edits applied to the production data.
    /// </summary>
    public List<ProductionDataEdit> dataEdits { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private EditDataResult()
    {}

    /// <summary>
    /// Create instance of EditDataResult
    /// </summary>
    public static EditDataResult CreateEditDataResult(List<ProductionDataEdit> dataEdits)
    {
      return new EditDataResult
      {
        dataEdits = dataEdits
      };
    }

    /// <summary>
    /// Create example instance of EditDataResult to display in Help documentation.
    /// </summary>
    public static EditDataResult HelpSample
    {
      get
      {
        return new EditDataResult()
        {
          dataEdits = new List<ProductionDataEdit> { ProductionDataEdit.HelpSample }
        };
      }
    }

  }
}