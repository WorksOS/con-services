using System;
using System.Collections.Generic;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Filters;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.ConfigurationStore;
using VSS.TRex.DI;

namespace VSS.TRex.Tests.Exports.CSV
{
  public class CSVExportFileWriterTests : IDisposable
  {
    [Fact]
    public void Persist_Successful()
    {
      var projectUid = Guid.NewGuid();

      var csvExportUserPreference = new CSVExportUserPreferences();
      var requestArgument = CSVExportRequestArgument.Create
      (
        projectUid, new FilterSet(new CombinedFilter()), "the filename",
        Productivity3D.Models.Enums.CoordType.Northeast, Productivity3D.Models.Enums.OutputTypes.PassCountLastPass,
        csvExportUserPreference, new List<CSVExportMappedMachine>(), false, false
      );
      requestArgument.TRexNodeID = Guid.NewGuid().ToString();

      var dataRows = new List<string>() { "string one", "string two" };
      
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
        .Add(x => x.AddSingleton<ITransferProxy>(sp => new TransferProxy(sp.GetRequiredService<IConfigurationStore>(), "AWS_DESIGNIMPORT_BUCKET_NAME")))
        .Complete();


      var csvExportFileWriter = new CSVExportFileWriter(requestArgument);
      var s3FullPath = csvExportFileWriter.PersistResult(dataRows);

      s3FullPath.Should().NotBeNull();
      s3FullPath.Should().Be($"project/{requestArgument.ProjectID}/TRexExport/{requestArgument.FileName}__{requestArgument.TRexNodeID}.zip");
    }

    public void Dispose()
    {
      DIBuilder.Eject();
    }
  }
}
