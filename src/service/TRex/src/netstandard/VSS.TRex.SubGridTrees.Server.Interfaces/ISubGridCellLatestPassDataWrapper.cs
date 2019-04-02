using System;
using System.IO;
using VSS.TRex.Cells;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
  public interface ISubGridCellLatestPassDataWrapper
  {
    SubGridTreeBitmapSubGridBits PassDataExistenceMap { get; }

    SubGridTreeBitmapSubGridBits CCVValuesAreFromLastPass { get; }
    SubGridTreeBitmapSubGridBits RMVValuesAreFromLastPass { get; }
    SubGridTreeBitmapSubGridBits FrequencyValuesAreFromLastPass { get; }
    SubGridTreeBitmapSubGridBits AmplitudeValuesAreFromLastPass { get; }
    SubGridTreeBitmapSubGridBits GPSModeValuesAreFromLatestCellPass { get; }
    SubGridTreeBitmapSubGridBits TemperatureValuesAreFromLastPass { get; }
    SubGridTreeBitmapSubGridBits MDPValuesAreFromLastPass { get; }
    SubGridTreeBitmapSubGridBits CCAValuesAreFromLastPass { get; }

    void Clear();
    void AssignValuesFromLastPassFlags(ISubGridCellLatestPassDataWrapper Source);

    void Assign(ISubGridCellLatestPassDataWrapper Source);

    /// <summary>
    /// An indexer supporting accessing the 2D array of 'last' pass information for the cell in a sub grid or segment
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    CellPass this[uint x, uint y] { get; set; }

    void Read(BinaryReader reader, byte[] buffer);
    void Write(BinaryWriter writer, byte[] buffer);

    short ReadInternalMachineIndex(uint Col, uint Row);

    DateTime ReadTime(uint Col, uint Row);
    float ReadHeight(uint Col, uint Row);
    short ReadCCV(uint Col, uint Row);
    short ReadRMV(uint Col, uint Row);
    ushort ReadFrequency(uint Col, uint Row);
    ushort ReadAmplitude(uint Col, uint Row);
    byte ReadCCA(uint Col, uint Row);
    GPSMode ReadGPSMode(uint Col, uint Row);
    short ReadMDP(uint Col, uint Row);
    ushort ReadTemperature(uint Col, uint Row);

    bool IsImmutable();

    void ClearPasses();

    bool HasCCVData();
    bool HasRMVData();
    bool HasFrequencyData();
    bool HasAmplitudeData();
    bool HasGPSModeData();
    bool HasTemperatureData();
    bool HasMDPData();
    bool HasCCAData();
  }
}
