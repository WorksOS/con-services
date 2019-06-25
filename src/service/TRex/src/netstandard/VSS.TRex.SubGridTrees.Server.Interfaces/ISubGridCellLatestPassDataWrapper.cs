using System;
using System.IO;
using VSS.TRex.Cells;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
  public interface ISubGridCellLatestPassDataWrapper : IDisposable
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

    short ReadInternalMachineIndex(int x, int y);

    DateTime ReadTime(int x, int y);
    float ReadHeight(int x, int y);
    short ReadCCV(int x, int y);
    short ReadRMV(int x, int y);
    ushort ReadFrequency(int x, int y);
    ushort ReadAmplitude(int x, int y);
    byte ReadCCA(int x, int y);
    GPSMode ReadGPSMode(int x, int y);
    short ReadMDP(int x, int y);
    ushort ReadTemperature(int x, int y);

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
