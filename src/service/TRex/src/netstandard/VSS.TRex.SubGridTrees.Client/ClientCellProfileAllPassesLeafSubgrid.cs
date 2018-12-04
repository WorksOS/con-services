using System;
using VSS.TRex.Filters.Models;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.SubGridTrees.Client
{
  public class ClientCellProfileAllPassesLeafSubgrid : GenericClientLeafSubGrid<ClientCellProfileAllPAssesLeafSubgridRecord>
  {
    /*
  TICClientSubGridTreeLeaf_CellProfileAllPasses = class(TICClientSubGridTreeLeaf_Base<TICSubGridCellPassData_CellProfileAllPasses_Entry>)
    protected
      procedure WriteCellValuesToStream(const Stream : TStream;
                                        const Values : array of TICSubGridCellPassData_CellProfileAllPasses_Entry); Override;
      procedure ReadCellValuesFromStream(const Stream : TStream;
                                         var Values: array of TICSubGridCellPassData_CellProfileAllPasses_Entry); Override;

    Public
      Function CellHasValue(CellX, CellY : Integer) : Boolean; Override;
      Procedure Clear; Override;
      Procedure AssignFilteredValue(const CellX, CellY : Integer;
                                    const Context : TICSubGridFilteredValueAssignmentContext); Override;
      Function AssignableFilteredValueIsNull(const FilteredValue : TICFilteredPassData) : Boolean; Override;
      function WantsLiftProcessingResults     :Boolean; Override;
      constructor Create(AOwner: TSubGridTree;
                         AParent : TSubGridTreeSubGridFunctionalBase;
                         ALevel : Byte;
                         ACellSize : Double;
                         AIndexOriginOffset : Integer); Override;

      function WantsTargetPassCountValues     :Boolean; Override;
      function WantsTargetCCVValues           :Boolean; Override;
      function WantsTargetMDPValues           :Boolean; Override;
      function WantsEventGPSModeValues        :Boolean; Override;
      function WantsGPSAccuracyValues         :Boolean; Override;
      function WantsTargetThicknessValues     :Boolean; Override;
      function WantsEventVibrationStateValues :Boolean; Override;
      function WantsEventMachineGearValues    :Boolean; Override;
      function WantsEventDesignNameValues     :Boolean; Override;
  end;     */

    public override bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue)
    {
      throw new NotImplementedException();
    }

    public override void FillWithTestPattern()
    {
      throw new NotImplementedException();
    }

    public override bool LeafContentEquals(IClientLeafSubGrid other)
    {
      throw new NotImplementedException();
    }

    public override ClientCellProfileAllPAssesLeafSubgridRecord NullCell()
    {
      throw new NotImplementedException();
    }
  }
}
