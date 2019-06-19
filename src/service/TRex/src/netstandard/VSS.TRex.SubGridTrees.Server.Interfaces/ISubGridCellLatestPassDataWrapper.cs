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
    CellPass this[int x, int y] { get; set; }

    void Read(BinaryReader reader);
    void Write(BinaryWriter writer);

    short ReadInternalMachineIndex(int Col, int Row);

    DateTime ReadTime(int Col, int Row);
    float ReadHeight(int Col, int Row);
    short ReadCCV(int Col, int Row);
    short ReadRMV(int Col, int Row);
    ushort ReadFrequency(int Col, int Row);
    ushort ReadAmplitude(int Col, int Row);
    byte ReadCCA(int Col, int Row);
    GPSMode ReadGPSMode(int Col, int Row);
    short ReadMDP(int Col, int Row);
    ushort ReadTemperature(int Col, int Row);

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
