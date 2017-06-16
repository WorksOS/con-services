using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Cells;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server
{
    /// <summary>
    /// A wrapper for a subgrid containing all information related to the 'latest kown' information related to each cell.
    /// This includes 'existence' information which indicates if the cell in question has any cell passes recorded for it.
    /// </summary>
    public class SubGridCellLatestPassDataWrapperBase 
    {
        /// <summary>
        /// The existence map detailed which cells have pass data recorded for them
        /// </summary>
        public SubGridTreeBitmapSubGridBits PassDataExistanceMap { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits.SubGridBitsCreationOptions.Unfilled);

        public SubGridTreeBitmapSubGridBits CCVValuesAreFromLastPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits.SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits RMVValuesAreFromLastPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits.SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits FrequencyValuesAreFromLastPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits.SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits AmplitudeValuesAreFromLastPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits.SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits GPSModeValuesAreFromLatestCellPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits.SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits TemperatureValuesAreFromLastPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits.SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits MDPValuesAreFromLastPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits.SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits CCAValuesAreFromLastPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits.SubGridBitsCreationOptions.Unfilled);

        public bool HasCCVData => true;
        public bool HasRMVData => true;
        public bool HasFrequencyData => true;
        public bool HasAmplitudeData => true;
        public bool HasGPSModeData => true;
        public bool HasTemperatureData => true;
        public bool HasMDPData => true;
        public bool HasCCAData => true;

        public SubGridCellLatestPassDataWrapperBase()
        {
            ClearPasses();
        }

        /// <summary>
        /// Clear all latest information for the subgrid
        /// </summary>
        public void Clear()
        {
            PassDataExistanceMap.Clear();

            CCVValuesAreFromLastPass.Clear();
            RMVValuesAreFromLastPass.Clear();
            FrequencyValuesAreFromLastPass.Clear();
            AmplitudeValuesAreFromLastPass.Clear();
            GPSModeValuesAreFromLatestCellPass.Clear();
            TemperatureValuesAreFromLastPass.Clear();
            MDPValuesAreFromLastPass.Clear();
            CCAValuesAreFromLastPass.Clear();

            ClearPasses();
        }

        /// <summary>
        /// Clear all latest information for the subgrid
        /// </summary>
        public virtual void ClearPasses()
        {
        }

        /// <summary>
        /// Copies the flags information from the source latest cell pass wrapper to this one
        /// </summary>
        /// <param name="Source"></param>
        public void AssignValuesFromLastPassFlags(SubGridCellLatestPassDataWrapperBase Source)
        {
            CCVValuesAreFromLastPass.Assign(Source.CCVValuesAreFromLastPass);
            RMVValuesAreFromLastPass.Assign(Source.RMVValuesAreFromLastPass);
            FrequencyValuesAreFromLastPass.Assign(Source.FrequencyValuesAreFromLastPass);
            AmplitudeValuesAreFromLastPass.Assign(Source.AmplitudeValuesAreFromLastPass);
            GPSModeValuesAreFromLatestCellPass.Assign(Source.GPSModeValuesAreFromLatestCellPass);
            TemperatureValuesAreFromLastPass.Assign(Source.TemperatureValuesAreFromLastPass);
            MDPValuesAreFromLastPass.Assign(Source.MDPValuesAreFromLastPass);
            CCAValuesAreFromLastPass.Assign(Source.CCAValuesAreFromLastPass);
        }
    }

    public class SubGridCellLatestPassDataWrapper_NonStatic : SubGridCellLatestPassDataWrapperBase
    {
        /// <summary>
        /// The array of 32x32 cells containing a cell pass representing the latest known values for a variety of cell attributes
        /// </summary>
        public CellPass[,] PassData = new CellPass[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

        /// <summary>
        /// Provides the 'NonStatic' behaviour for clearing the passes in the latest pass information
        /// </summary>
        public override void ClearPasses()
        {
            base.ClearPasses();

            for (int I = 0; I < SubGridTree.SubGridTreeDimension; I++)
            {
                for (int J = 0; J < SubGridTree.SubGridTreeDimension; J++)
                {
                    PassData[I, J].Clear();
                }
            }
        }
    }

}
