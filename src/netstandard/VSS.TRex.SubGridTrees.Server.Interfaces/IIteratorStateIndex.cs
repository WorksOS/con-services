using System;
using System.Collections;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
  public interface IIteratorStateIndex
  {
    DateTime StartSegmentTime { get; set; }
    DateTime EndSegmentTime { get; set; }  
    IterationDirection IterationDirection { get; set; }
    IServerLeafSubGrid SubGrid { get; set; }
    ISubGridDirectory Directory { get; set; }
    int Idx { get; set; }
    BitArray MachineIDSet { get; set; }
    double MinIterationElevation { get; set; }
    double MaxIterationElevation { get; set; }
    void Initialise();
    bool NextSegment();
    bool AtLastSegment();
    void SetTimeRange(DateTime startSegmentTime, DateTime endSegmentTime);
    void SetIteratorElevationRange(double minIterationElevation, double maxIterationElevation);
    void SetMachineRestriction(BitArray machineIDSet);
    void SegmentListExtended();
  }

}
