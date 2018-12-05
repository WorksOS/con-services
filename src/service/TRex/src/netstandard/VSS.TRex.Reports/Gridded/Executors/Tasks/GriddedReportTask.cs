using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Pipelines.Tasks;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Reports.Gridded.Executors.Tasks
{
  /// <summary>
  /// The task responsible for receiving subgrids to be aggregated into a grid response
  /// </summary>
  public class GriddedReportTask : PipelinedSubGridTask
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    ///// <summary>
    ///// The collection of subgrids being collected for a patch response
    ///// </summary>
    //public List<IClientLeafSubGrid> PatchSubgrids = new List<IClientLeafSubGrid>();

    public GriddedReportTask()
    { }

    /// <summary>
    /// Constructs the grid task
    /// </summary>
    /// <param name="requestDescriptor"></param>
    /// <param name="tRexNodeId"></param>
    /// <param name="gridDataType"></param>
    public GriddedReportTask(Guid requestDescriptor, string tRexNodeId, GridDataType gridDataType) : base(requestDescriptor, tRexNodeId, gridDataType)
    {
    }

    /// <summary>
    /// Accept a subgrid response from the processing engine and incorporate into the result for the request.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public override bool TransferResponse(object response)
    {
      // Log.InfoFormat("Received a SubGrid to be processed: {0}", (response as IClientLeafSubGrid).Moniker());

      if (!base.TransferResponse(response))
      {
        Log.LogWarning("Base TransferResponse returned false");
        return false;
      }

      if (!(response is IClientLeafSubGrid[] subGridResponses) || subGridResponses.Length == 0)
      {
        Log.LogWarning("No subgrid responses returned");
        return false;
      }

      // todoJeannie packager to loop through and store as below. like Raptor packager
      //foreach (var subGrid in subGridResponses)
      //{
      //  if (subGrid == null)
      //    continue;

      //  PatchSubgrids.Add(subGrid);
      //}

      return true;
    }
  }
}


/* todoJeannie from Raptor to go in GridTask to
   with SubgridResult.Subgrids[0].Subgrid do
    begin
      FIndexOriginOffset := SubgridResult.Subgrids[0].Subgrid.IndexOriginOffset;
      CalculateWorldOrigin(SubgridWorldOriginX, SubgridWorldOriginY);

      HaveDesignElevationDataForThisSubgrid := FCutFill and
                                               Assigned(FDesignSubgridExistanceMap) and
                                               FDesignSubgridExistanceMap.Cells[OriginX SHR kSubGridIndexBitsPerLevel,
                                                                                OriginY SHR kSubGridIndexBitsPerLevel];
    end;

  for I := 0 to kSubGridTreeDimension - 1 do
    for J := 0 to kSubGridTreeDimension - 1 do
      begin
        PassCountValue := CellProfileSubGrid.Cells[I, J].PassCount;

        if PassCountValue <> kICNullPassCountValue then // if pass data
          with CellProfileSubGrid.Cells[I,J] do
            begin
              myDataRow := TGridRow.Create;

              with SubgridResult.Subgrids[0].Subgrid do
                begin
                  myDataRow.Easting := SubgridWorldOriginX + CellXOffset;
                  myDataRow.Northing := SubgridWorldOriginY + CellYOffset;
                end;

              myDataRow.Elevation := Height;
              myDataRow.CMV       := LastPassValidCCV;
              myDataRow.MDP       := LastPassValidMDP;
              myDataRow.PassCount := PassCount;
              myDataRow.Temperature := LastPassValidTemperature;
              myDataRow.MDP       := LastPassValidMDP;
              myDataRow.CutFill   := kICNullHeight;

              // todo move cutfill lookup to psnode
              if HaveDesignElevationDataForThisSubgrid and (Height <> kICNullHeight) then
                with DesignProfilerLayerLoadBalancer.LoadBalancedDesignProfilerService do
                  begin
                    DesignResult := RequestDesignElevationSpot(Construct_CalculateDesignElevationSpot_Args(FProjectID,
                                                               myDataRow.Easting,
                                                               myDataRow.Northing,
                                                               FCellSize,
                                                               FDesignDescriptor),
                                                               DesignElevation);

                    if (DesignResult = dppiOK) and Designelevation.Success then
                      if DesignElevation.Height <> kICNullHeight then
                        myDataRow.CutFill := Height - DesignElevation.Height;
                  end;

              FReportPackager.GridReport.Rows.Add(myDataRow);
   
*/

