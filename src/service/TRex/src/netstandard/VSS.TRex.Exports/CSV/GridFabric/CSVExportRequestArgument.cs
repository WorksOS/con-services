using System;
using System.Collections.Generic;
using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Exports.CSV.GridFabric
{
  /// <summary>
  /// The argument to be supplied to the grid request
  /// </summary>
  public class CSVExportRequestArgument : BaseApplicationServiceRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    public string FileName { get; set; }

    public CoordType CoordType { get; set; }

    public OutputTypes OutputType { get; set; }


    public CSVExportUserPreferences UserPreferences { get; set; }

    // MappedMachines is for veta export only
    public List<CSVExportMappedMachine> MappedMachines { get; set; }

    // RestrictOutputSize and RawDataAsDBase are for pass count export only
    public bool RestrictOutputSize { get; set; }
    public bool RawDataAsDBase { get; set; }


    public CSVExportRequestArgument()
    {
      Clear();
    }

    private void Clear()
    {
      FileName = string.Empty;
      Filters = new FilterSet(new CombinedFilter());
      CoordType = CoordType.Northeast;
      OutputType = OutputTypes.PassCountLastPass;
      UserPreferences = new CSVExportUserPreferences();
      MappedMachines = new List<CSVExportMappedMachine>();
      RestrictOutputSize = false;
      RawDataAsDBase = false;
    }

    public CSVExportRequestArgument(Guid siteModelUid, IFilterSet filters,
      string fileName, CoordType coordType, OutputTypes outputType, CSVExportUserPreferences userPreferences,
      List<CSVExportMappedMachine> mappedMachines, bool restrictOutputSize, bool rawDataAsDBase)
    {
      ProjectID = siteModelUid;
      Filters = filters;
      FileName = fileName;
      CoordType = coordType;
      OutputType = outputType;
      UserPreferences = userPreferences;
      MappedMachines = mappedMachines;
      RestrictOutputSize = restrictOutputSize;
      RawDataAsDBase = rawDataAsDBase;
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteString(FileName);
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
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        FileName = reader.ReadString();
        CoordType = (CoordType) reader.ReadInt();
        OutputType = (OutputTypes) reader.ReadInt();
        UserPreferences = new CSVExportUserPreferences();
        UserPreferences.FromBinary(reader);
        var count = reader.ReadInt();
        MappedMachines = new List<CSVExportMappedMachine>(count);
        for (var i = 0; i < count; i++)
        {
          MappedMachines.Add(new CSVExportMappedMachine
          {
            Uid = reader.ReadGuid() ?? Guid.Empty,
            InternalSiteModelMachineIndex = reader.ReadShort(),
            Name = reader.ReadString()
          });
        }

        RestrictOutputSize = reader.ReadBoolean();
        RawDataAsDBase = reader.ReadBoolean();
      }
    }
  }
}
