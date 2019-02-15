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
    public CoordType CoordType { get; set; }

    public OutputTypes OutputType { get; private set; }


    public CSVExportUserPreferences UserPreferences { get; set; }

    // the following machineNames is for veta export only
    public List<CSVExportMappedMachine> MappedMachines { get; set; }

    // the following 2 flags are for passcount export only
    public bool RestrictOutputSize { get; private set; }

    public bool RawDataAsDBase { get; private set; }


    public CSVExportRequestArgument()
    {
      Clear();
    }

    private void Clear()
    {
      CoordType = CoordType.Northeast;
      OutputType = OutputTypes.PassCountLastPass;
      MappedMachines = new List<CSVExportMappedMachine>();
      UserPreferences = new CSVExportUserPreferences();
    }

    public CSVExportRequestArgument(Guid siteModelUid, IFilterSet filters,
      CoordType coordType, OutputTypes outputType, CSVExportUserPreferences userPreferences,
      List<CSVExportMappedMachine> mappedMachines, bool restrictOutputSize, bool rawDataAsDBase)
    {
      ProjectID = siteModelUid;
      Filters = filters;
      CoordType = coordType; 
      OutputType  = outputType;
      UserPreferences = userPreferences;
      MappedMachines = mappedMachines;
      RestrictOutputSize = restrictOutputSize;
      RawDataAsDBase = rawDataAsDBase;
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
      UserPreferences.ToBinary(writer);
      var count = MappedMachines.Count;
      writer.WriteInt(count);
      foreach (var machine in MappedMachines)
      {
        writer.WriteGuid(machine.Uid);
        writer.WriteShort(machine.InternalSiteModelMachineIndex);
        writer.WriteString(machine.Name);
      }
      writer.WriteBoolean(RestrictOutputSize);
      writer.WriteBoolean(RawDataAsDBase);
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
      UserPreferences = new CSVExportUserPreferences();
      UserPreferences.FromBinary(reader);
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
      RestrictOutputSize = reader.ReadBoolean();
      RawDataAsDBase = reader.ReadBoolean();
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ CoordType.GetHashCode();
        hashCode = (hashCode * 397) ^ OutputType.GetHashCode();
        hashCode = (hashCode * 397) ^ UserPreferences.GetHashCode();
        hashCode = (hashCode * 397) ^ MappedMachines.GetHashCode();
        hashCode = (hashCode * 397) ^ RestrictOutputSize.GetHashCode();
        hashCode = (hashCode * 397) ^ RawDataAsDBase.GetHashCode();
        return hashCode;
      }
    }
  }
}
