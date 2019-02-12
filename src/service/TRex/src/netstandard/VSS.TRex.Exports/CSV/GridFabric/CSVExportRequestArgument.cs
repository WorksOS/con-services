using System;
using System.Collections.Generic;
using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Exports.CSV.GridFabric
{
  /// <summary>
  /// The argument to be supplied to the grid request
  /// </summary>
  public class CSVExportRequestArgument : BaseApplicationServiceRequestArgument
  {

    /// <summary>
    /// Type of Coordinates required in result e.g. NE
    /// </summary>
    public CoordType CoordType { get; private set; }

    /// <summary>
    /// which type of passes
    /// </summary>
    public OutputTypes OutputType { get; private set; }
    
    /// <summary>
    /// Include the names of these machines
    /// </summary>
    public List<CSVExportMappedMachine> MappedMachines { get; set; }

    public CSVExportRequestArgument()
    {
      Clear();
    }

    private void Clear()
    {
      CoordType = CoordType.Northeast;
      OutputType = OutputTypes.PassCountLastPass;
      MappedMachines = new List<CSVExportMappedMachine>();
    }

    public CSVExportRequestArgument(Guid siteModelUid, IFilterSet filters,
      CoordType coordType, OutputTypes outputType, List<CSVExportMappedMachine> mappedMachines)
    {
      ProjectID = siteModelUid;
      Filters = filters;
      CoordType = coordType; 
      OutputType  = outputType;
      MappedMachines = mappedMachines;
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);
      writer.WriteInt((int)CoordType);
      writer.WriteInt((int)OutputType);
      var count = MappedMachines.Count;
      writer.WriteInt(count);
      foreach (var machine in MappedMachines)
      {
        writer.WriteGuid(machine.Uid);
        writer.WriteShort(machine.InternalSiteModelMachineIndex);
        writer.WriteString(machine.Name);
      }
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);
      CoordType = (CoordType)reader.ReadInt();
      OutputType = (OutputTypes)reader.ReadInt();
      var count = reader.ReadInt();
      MappedMachines = new List<CSVExportMappedMachine>(count);
      for(int i = 0; i < count; i++)
      {
        MappedMachines.Add(new CSVExportMappedMachine()
        {
          Uid = reader.ReadGuid() ?? Guid.Empty,
          InternalSiteModelMachineIndex = reader.ReadShort(),
          Name = reader.ReadString()
        });
      }
    }
  }
}
