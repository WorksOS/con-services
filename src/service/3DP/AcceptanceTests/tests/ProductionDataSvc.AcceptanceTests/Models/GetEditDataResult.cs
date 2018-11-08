using System.Collections.Generic;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// The represenation of the results of an edit data request.
  /// </summary>
  public class GetEditDataResult : ResponseBase
  {
    public List<ProductionDataEdit> dataEdits { get; set; }

    public GetEditDataResult()
        : base("success")
    { }
  }
}
