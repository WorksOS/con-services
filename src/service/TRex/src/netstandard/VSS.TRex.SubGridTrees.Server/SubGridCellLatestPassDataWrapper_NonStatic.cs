using System;
using System.IO;
using VSS.TRex.Cells;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.IO;
using VSS.TRex.IO.Helpers;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Server
{
    public class SubGridCellLatestPassDataWrapper_NonStatic : SubGridCellLatestPassDataWrapperBase, ISubGridCellLatestPassDataWrapper
    {
        /// <summary>
        /// The array of 32x32 cells containing a cell pass representing the latest known values for a variety of cell attributes
        /// This is represented internally by a slab allocated array of cell passes. 
        /// </summary>
        public TRexSpan<CellPass> PassData;

        /// <summary>
        /// Implement the last pass indexer from the interface.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public CellPass this[int x, int y]
        {
            get => PassData.Elements[PassData.Offset + x + y * SubGridTreeConsts.SubGridTreeDimension];
            set => PassData.Elements[PassData.Offset + x + y * SubGridTreeConsts.SubGridTreeDimension] = value;
        }

        public SubGridCellLatestPassDataWrapper_NonStatic()
        {
          PassData = SlabAllocatedArrayPoolHelper<CellPass>.Caches.Rent(SubGridTreeConsts.CellsPerSubGrid);

          if (PassData.Capacity != SubGridTreeConsts.CellsPerSubGrid)
          {
            throw new TRexException($"Size of rented CellPass array from slab allocate is not {SubGridTreeConsts.CellsPerSubGrid} as expected");
          }

          PassData.Count = SubGridTreeConsts.CellsPerSubGrid;
        }

        /// <summary>
        /// Provides the 'NonStatic' behaviour for clearing the passes in the latest pass information
        /// </summary>
        public override void ClearPasses()
        {
            base.ClearPasses();

            for (int i = PassData.Offset, limit = PassData.Offset + SubGridTreeConsts.CellsPerSubGrid; i < limit; i++)
            {
                PassData.Elements[i] = CellPass.CLEARED_CELL_PASS;
            }
        }

        public bool HasCCVData() => true;
     
        public bool HasRMVData() => true;
     
        public bool HasFrequencyData() => true;
     
        public bool HasAmplitudeData() => true;
     
        public bool HasGPSModeData() => true;
     
        public bool HasTemperatureData() => true;
     
        public bool HasMDPData() => true;
     
        public bool HasCCAData() => true;
     
        public override void Read(BinaryReader reader)
        {
          base.Read(reader);
     
          // Read in the latest call passes themselves
          for (int i = PassData.Offset, limit = PassData.Offset + SubGridTreeConsts.CellsPerSubGrid; i < limit; i++)
          {
            PassData.Elements[i].Read(reader);
          }
        }

        /// <summary>
        /// ReadInternalMachineIndex will read the internal machine ID from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public short ReadInternalMachineIndex(int x, int y) => PassData.Elements[PassData.Offset + x + y * SubGridTreeConsts.SubGridTreeDimension].InternalSiteModelMachineIndex;

      
        /// <summary>
        /// ReadTime will read the Time from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public DateTime ReadTime(int x, int y) => PassData.Elements[PassData.Offset + x + y * SubGridTreeConsts.SubGridTreeDimension].Time;

        /// <summary>
        /// ReadHeight will read the Height from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public float ReadHeight(int x, int y) => PassData.Elements[PassData.Offset + x + y * SubGridTreeConsts.SubGridTreeDimension].Height;

        /// <summary>
        /// ReadCCV will read the CCV from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public short ReadCCV(int x, int y) => PassData.Elements[PassData.Offset + x + y * SubGridTreeConsts.SubGridTreeDimension].CCV;

        /// <summary>
        /// ReadRMV will read the RMV from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public short ReadRMV(int x, int y) => PassData.Elements[PassData.Offset + x + y * SubGridTreeConsts.SubGridTreeDimension].RMV;

        /// <summary>
        /// ReadFrequency will read the Frequency from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public ushort ReadFrequency(int x, int y) => PassData.Elements[PassData.Offset + x + y * SubGridTreeConsts.SubGridTreeDimension].Frequency;

        // ReadAmplitude will read the Amplitude from the latest cell identified by the Row and Col
        public ushort ReadAmplitude(int x, int y) => PassData.Elements[PassData.Offset + x + y * SubGridTreeConsts.SubGridTreeDimension].Amplitude;

        /// <summary>
        /// ReadCCA will read the CCA from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public byte ReadCCA(int x, int y) => PassData.Elements[PassData.Offset + x + y * SubGridTreeConsts.SubGridTreeDimension].CCA;

        /// <summary>
        /// ReadGPSMode will read the GPSMode from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public GPSMode ReadGPSMode(int x, int y) => PassData.Elements[PassData.Offset + x + y * SubGridTreeConsts.SubGridTreeDimension].gpsMode;

        /// <summary>
        /// ReadMDP will read the MDP from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public short ReadMDP(int x, int y) => PassData.Elements[PassData.Offset + x + y * SubGridTreeConsts.SubGridTreeDimension].MDP;

        /// <summary>
        /// ReadTemperature will read the Temperature from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public ushort ReadTemperature(int x, int y) => PassData.Elements[PassData.Offset + x + y * SubGridTreeConsts.SubGridTreeDimension].MaterialTemperature;

        /// <summary>
        /// Writes the contents of the NonStatic latest passes using a supplied BinaryWriter
        /// </summary>
        /// <param name="writer"></param>
        public override void Write(BinaryWriter writer)
        {
          base.Write(writer);

          // Write out the latest call passes themselves
          for (int i = PassData.Offset, limit = PassData.Offset + SubGridTreeConsts.CellsPerSubGrid; i < limit; i++)
          {
            PassData.Elements[i].Write(writer);
          }
        }

    #region IDisposable Support
    private bool disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
            SlabAllocatedArrayPoolHelper<CellPass>.Caches.Return(ref PassData);
        }

        disposedValue = true;
      }
    }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
    }
    #endregion
  }
}
