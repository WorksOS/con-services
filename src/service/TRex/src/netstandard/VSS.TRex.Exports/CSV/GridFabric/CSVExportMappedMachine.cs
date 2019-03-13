using System;

namespace VSS.TRex.Exports.CSV.GridFabric
{
  /// <summary>
  /// The user may select a set of machine names.
  /// These will be mapped to TRex machine names (from tag file) where available.
  /// When producing the report data in compute Func,
  ///     the machine name will be used if in this list, else 'Unknown'
  /// </summary>
  public class CSVExportMappedMachine 
  {
      public Guid Uid { get; set; }
      public short InternalSiteModelMachineIndex { get; set; }
      public string Name { get; set; }
  }
}
