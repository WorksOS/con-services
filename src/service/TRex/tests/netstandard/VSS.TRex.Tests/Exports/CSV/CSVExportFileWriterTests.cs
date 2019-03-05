using System;
using System.Collections.Generic;
using System.IO;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Filters;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.ConfigurationStore;
using VSS.TRex.DI;
using Moq;

namespace VSS.TRex.Tests.Exports.CSV
{
  public class CSVExportFileWriterTests : IDisposable
  {

    [Fact]
    public void Persist_Successful()
    {
      var projectUid = Guid.NewGuid();

      var csvExportUserPreference = new CSVExportUserPreferences();
      var requestArgument = new CSVExportRequestArgument
      (
        projectUid, new FilterSet(new CombinedFilter()), "the filename",
        Productivity3D.Models.Enums.CoordType.Northeast, Productivity3D.Models.Enums.OutputTypes.PassCountLastPass,
        csvExportUserPreference, new List<CSVExportMappedMachine>(), false, false
      );
      requestArgument.TRexNodeID = Guid.NewGuid().ToString();

      var dataRows = new List<string>() { "string one", "string two" };

      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.UploadToBucket(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()));
      var mockConfig = new Mock<IConfigurationStore>();
      mockConfig.Setup(x => x.GetValueString("AWS_BUCKET_NAME")).Returns("vss-exports-stg");

      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton(mockConfig.Object))
        .Add(x => x.AddSingleton(mockTransferProxy.Object))
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
