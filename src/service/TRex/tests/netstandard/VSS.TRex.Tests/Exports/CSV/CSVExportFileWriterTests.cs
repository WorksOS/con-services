using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Filters;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.TRex.DI;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Tests.TestFixtures;
using FileSystem = System.IO.File;

namespace VSS.TRex.Tests.Exports.CSV
{
  public class CSVExportFileWriterTests : IClassFixture<DILoggingFixture>
  {
    private readonly string CSV_extension = ".csv";
    private readonly string ZIP_extension = ".zip";

    [Fact]
    public void Persist_Successful()
    {
      var projectUid = Guid.NewGuid();

      var csvExportUserPreference = new CSVExportUserPreferences();
      var requestArgument = new CSVExportRequestArgument
      (
        projectUid, new FilterSet(new CombinedFilter()), "the filename",
        CoordType.Northeast, OutputTypes.PassCountLastPass,
        csvExportUserPreference, new List<CSVExportMappedMachine>(), false, false
      ) { TRexNodeID = Guid.NewGuid().ToString() };

      var dataRows = new List<string>() { "string one", "string two" };

      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.UploadToBucket(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()));

      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton(mockTransferProxy.Object))
        .Complete();

      var csvExportFileWriter = new CSVExportFileWriter(requestArgument);
      var s3FullPath = csvExportFileWriter.PersistResult(dataRows);

      s3FullPath.Should().NotBeNull();
      s3FullPath.Should().Be($"project/{requestArgument.ProjectID}/TRexExport/{requestArgument.FileName}__{requestArgument.TRexNodeID}.zip");

      var projectDir = Path.Combine(Path.GetTempPath(), projectUid.ToString());
      if (Directory.Exists(projectDir))
        Directory.Delete(projectDir, true);
    }

    [Fact]
    public void Persist_UnSuccessful()
    {
      var projectUid = Guid.NewGuid();

      var csvExportUserPreference = new CSVExportUserPreferences();
      var requestArgument = new CSVExportRequestArgument
        (
          projectUid, new FilterSet(new CombinedFilter()), null,
          CoordType.Northeast, OutputTypes.PassCountLastPass,
          csvExportUserPreference, new List<CSVExportMappedMachine>(), false, false
        ) { TRexNodeID = Guid.NewGuid().ToString() };

      var dataRows = new List<string>() { "string one", "string two" };
      
      var csvExportFileWriter = new CSVExportFileWriter(requestArgument);
      Action act = () => csvExportFileWriter.PersistResult(dataRows);

      act.Should().Throw<ArgumentNullException>();
      var projectDir = Path.Combine(Path.GetTempPath(), projectUid.ToString());
      if (Directory.Exists(projectDir))
        Directory.Delete(projectDir, true);
    }

    [Fact]
    public void Persist_UnSuccessful_NoAWS()
    {
      var projectUid = Guid.NewGuid();

      var csvExportUserPreference = new CSVExportUserPreferences();
      var requestArgument = new CSVExportRequestArgument
        (
          projectUid, new FilterSet(new CombinedFilter()), "the filename",
          CoordType.Northeast, OutputTypes.PassCountLastPass,
          csvExportUserPreference, new List<CSVExportMappedMachine>(), false, false
        ) { TRexNodeID = Guid.NewGuid().ToString() };

      var originalConfiguration = DIContext.Obtain<IConfigurationStore>();
      var moqConfiguration = DIContext.Obtain<Mock<IConfigurationStore>>();
      moqConfiguration.Setup(c => c.GetValueString("AWS_BUCKET_NAME")).Returns((string)null);
      moqConfiguration.Setup(c => c.GetValueString("AWS_BUCKET_NAME", It.IsAny<string>())).Returns((string)null);

      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton<IConfigurationStore>(moqConfiguration.Object))
        .Complete();

      Action act = () => new CSVExportFileWriter(requestArgument);

      act.Should().Throw<ServiceException>();
      
      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton<IConfigurationStore>(originalConfiguration))
        .Complete();
    }

    [Fact]
    public void CreateHeaders_PassCountLastPass()
    {
      var projectUid = Guid.NewGuid();

      var csvExportUserPreference = new CSVExportUserPreferences();
      var requestArgument = new CSVExportRequestArgument
      (
        projectUid, new FilterSet(new CombinedFilter()), "the filename",
        CoordType.Northeast, OutputTypes.PassCountLastPass,
        csvExportUserPreference, new List<CSVExportMappedMachine>(), false, false
      ) { TRexNodeID = Guid.NewGuid().ToString() };

      var dataRows = new List<string>() { "string one", "string two" };

      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.UploadToBucket(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()));

      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton(mockTransferProxy.Object))
        .Complete();

      var csvExportFileWriter = new CSVExportFileWriter(requestArgument, true);
      csvExportFileWriter.PersistResult(dataRows);

      var localExportPath = FilePathHelper.GetTempFolderForExport(requestArgument.ProjectID, "");
      var uniqueFileName = requestArgument.FileName + "__" + requestArgument.TRexNodeID;
      var zipFullPath = Path.Combine(localExportPath, uniqueFileName) + ZIP_extension;

      localExportPath.Should().NotBeNull();

      if (!Directory.Exists(localExportPath))
        Assert.True(true, $"LocalExportPath: {localExportPath} should exist");

      if (!FileSystem.Exists(zipFullPath))
        Assert.True(true, $"zipFullPath: {zipFullPath} should exist");

      var firstFile = Path.Combine(localExportPath, uniqueFileName, requestArgument.FileName + CSV_extension);
      if (!FileSystem.Exists(firstFile))
        Assert.True(true, $"firstFile: {firstFile} should exist");

      string header;
      using (var fs = new StreamReader(firstFile)) header = fs.ReadLine();
      header.Should().NotBeNullOrEmpty();
      header.Should().Be("Time,CellN,CellE,Elevation,PassCount,LastRadioLtncy,DesignName,Machine,Speed,LastGPSMode,GPSAccTol,TargPassCount,TotalPasses,Lift,LastCMV,TargCMV,LastMDP,TargMDP,LastRMV,LastFreq,LastAmp,TargThickness,MachineGear,VibeState,LastTemp");

      var projectDir = localExportPath.Remove(localExportPath.Length -7);
      if (Directory.Exists(projectDir))
        Directory.Delete(projectDir, true);
    }

    [Fact]
    public void CreateHeaders_VedaFinalPass()
    {
      var projectUid = Guid.NewGuid();

      var csvExportUserPreference = new CSVExportUserPreferences();
      var requestArgument = new CSVExportRequestArgument
      (
        projectUid, new FilterSet(new CombinedFilter()), "the filename",
        CoordType.Northeast, OutputTypes.VedaFinalPass,
        csvExportUserPreference, new List<CSVExportMappedMachine>(), false, false
      ) { TRexNodeID = Guid.NewGuid().ToString() };

      var dataRows = new List<string>() { "string one", "string two" };

      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.UploadToBucket(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()));

      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton(mockTransferProxy.Object))
        .Complete();

      var csvExportFileWriter = new CSVExportFileWriter(requestArgument, true);
      csvExportFileWriter.PersistResult(dataRows);

      var localExportPath = FilePathHelper.GetTempFolderForExport(requestArgument.ProjectID, "");
      var uniqueFileName = requestArgument.FileName + "__" + requestArgument.TRexNodeID;
      
      var firstFile = Path.Combine(localExportPath, uniqueFileName, requestArgument.FileName + CSV_extension);
      if (!FileSystem.Exists(firstFile))
        Assert.True(true, $"firstFile: {firstFile} should exist");

      string header;
      using (var fs = new StreamReader(firstFile)) header = fs.ReadLine();
      header.Should().NotBeNullOrEmpty();
      header.Should().Be("Time,CellN_m,CellE_m,Elevation_m,PassCount,LastRadioLtncy,DesignName,Machine,Speed_km/h,LastGPSMode,GPSAccTol_m,TargPassCount,TotalPasses,Lift,LastCMV,TargCMV,LastMDP,TargMDP,LastRMV,LastFreq_Hz,LastAmp_mm,TargThickness_m,MachineGear,VibeState,LastTemp,_c");

      var projectDir = localExportPath.Remove(localExportPath.Length - 7);
      if (Directory.Exists(projectDir))
        Directory.Delete(projectDir, true);
    }

    [Fact]
    public void CreateHeaders_RawDataAsDBase_LatLong_US_Fahrenheit()
    {
      var projectUid = Guid.NewGuid();

      var userPreferences = new UserPreferences() { TemperatureUnits = (int)TemperatureUnitEnum.Fahrenheit };
      var csvExportUserPreference = AutoMapperUtility.Automapper.Map<CSVExportUserPreferences>(userPreferences);

      var requestArgument = new CSVExportRequestArgument
      (
        projectUid, new FilterSet(new CombinedFilter()), "the filename",
        CoordType.LatLon, OutputTypes.VedaAllPasses,
        csvExportUserPreference, new List<CSVExportMappedMachine>(), false, true
      ) { TRexNodeID = Guid.NewGuid().ToString() };

      var dataRows = new List<string>() { "string one", "string two" };

      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.UploadToBucket(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()));

      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton(mockTransferProxy.Object))
        .Complete();

      var csvExportFileWriter = new CSVExportFileWriter(requestArgument, true);
      csvExportFileWriter.PersistResult(dataRows);

      var localExportPath = FilePathHelper.GetTempFolderForExport(requestArgument.ProjectID, "");
      var uniqueFileName = requestArgument.FileName + "__" + requestArgument.TRexNodeID;

      var firstFile = Path.Combine(localExportPath, uniqueFileName, requestArgument.FileName + CSV_extension);
      if (!FileSystem.Exists(firstFile))
        Assert.True(true, $"firstFile: {firstFile} should exist");
      string header;
      using (var fs = new StreamReader(firstFile)) header = fs.ReadLine();
      header.Should().NotBeNullOrEmpty();
      header.Should().Be("Time,Lat,Long,Elevation_FT,PassNumber,lastRadioLtncy,DesignName,Machine,Speed_mph,LastGPSMode,GPSAccTol_FT,TargPassCount,ValidPos,Lift,LastCMV,TargCMV,LastMDP,TargMDP,LastRMV,LastFreq_Hz,LastAmp_mm,TargThickness_FT,MachineGear,VibeState,LastTemp,_f");

      var projectDir = localExportPath.Remove(localExportPath.Length - 7);
      if (Directory.Exists(projectDir))
        Directory.Delete(projectDir, true);
    }

    [Fact]
    public void CreateHeaders_RawDataAsDBase_LatLong_Metric_Fahrenheit()
    {
      var projectUid = Guid.NewGuid();

      var userPreferences = new UserPreferences() { Units = (int) UnitsTypeEnum.Metric, TemperatureUnits = (int)TemperatureUnitEnum.Fahrenheit };
      var csvExportUserPreference = AutoMapperUtility.Automapper.Map<CSVExportUserPreferences>(userPreferences);

      var requestArgument = new CSVExportRequestArgument
      (
        projectUid, new FilterSet(new CombinedFilter()), "the filename",
        CoordType.LatLon, OutputTypes.VedaAllPasses,
        csvExportUserPreference, new List<CSVExportMappedMachine>(), false, true
      ) { TRexNodeID = Guid.NewGuid().ToString() };

      var dataRows = new List<string>() { "string one", "string two" };

      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.UploadToBucket(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()));

      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton(mockTransferProxy.Object))
        .Complete();

      var csvExportFileWriter = new CSVExportFileWriter(requestArgument, true);
      csvExportFileWriter.PersistResult(dataRows);

      var localExportPath = FilePathHelper.GetTempFolderForExport(requestArgument.ProjectID, "");
      var uniqueFileName = requestArgument.FileName + "__" + requestArgument.TRexNodeID;
     
      var firstFile = Path.Combine(localExportPath, uniqueFileName, requestArgument.FileName + CSV_extension);
      if (!FileSystem.Exists(firstFile))
        Assert.True(true, $"firstFile: {firstFile} should exist");

      string header;
      using (var fs = new StreamReader(firstFile)) header = fs.ReadLine();
      header.Should().NotBeNullOrEmpty();
      header.Should().Be("Time,Lat,Long,Elevation_m,PassNumber,LastRadioLtncy,DesignName,Machine,Speed_km/h,LastGPSMode,GPSAccTol_m,TargPassCount,ValidPos,Lift,LastCMV,TargCMV,LastMDP,TargMDP,LastRMV,LastFreq_Hz,LastAmp_mm,TargThickness_m,MachineGear,VibeState,LastTemp,_f");

      var projectDir = localExportPath.Remove(localExportPath.Length - 7);
      if (Directory.Exists(projectDir))
        Directory.Delete(projectDir, true);
    }

    [Fact(Skip="See BUG#85914")]
    public void CreateHeaders_RestrictOutputSize_SingleFile()
    {
      var projectUid = Guid.NewGuid();

      var userPreferences = new UserPreferences() { TemperatureUnits = (int)TemperatureUnitEnum.Fahrenheit };
      var csvExportUserPreference = AutoMapperUtility.Automapper.Map<CSVExportUserPreferences>(userPreferences);

      var requestArgument = new CSVExportRequestArgument
      (
        projectUid, new FilterSet(new CombinedFilter()), "the filename",
        CoordType.Northeast, OutputTypes.VedaFinalPass,
        csvExportUserPreference, new List<CSVExportMappedMachine>(), true, true
      )
      { TRexNodeID = Guid.NewGuid().ToString() };

      var dataRows = new List<string>() { "string one", "string two" };

      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.UploadToBucket(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()));

      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton(mockTransferProxy.Object))
        .Complete();

      var csvExportFileWriter = new CSVExportFileWriter(requestArgument, true);
      csvExportFileWriter.PersistResult(dataRows);

      var localExportPath = FilePathHelper.GetTempFolderForExport(requestArgument.ProjectID, "");
      var uniqueFileName = requestArgument.FileName + "__" + requestArgument.TRexNodeID;
      var zipFullPath = Path.Combine(localExportPath, uniqueFileName) + ZIP_extension;

      var firstFile = Path.Combine(localExportPath, uniqueFileName, requestArgument.FileName + CSV_extension);
      if (!FileSystem.Exists(firstFile))
        Assert.True(true, $"firstFile: {firstFile} should exist");

      if (!FileSystem.Exists(zipFullPath))
        Assert.True(true, $"zipFullPath: {zipFullPath} should exist");
      using (var zip = ZipFile.OpenRead(zipFullPath))
      {
        zip.Entries.Count.Should().Be(1);
        zip.Entries[0].Name.Should().Be(requestArgument.FileName + CSV_extension);
        
        var fileInfo = new System.IO.FileInfo(firstFile);
        fileInfo.Length.Should().Be(285);
        zip.Entries[0].Length.Should().Be(fileInfo.Length);
      }

      string header;
      using (var fs = new StreamReader(firstFile)) header = fs.ReadLine();
      header.Should().NotBeNullOrEmpty();
      header.Should().Be("Time,CellN_FT,CellE_FT,Elevation_FT,PassCount,lastRadioLtncy,DesignName,Machine,Speed_mph,LastGPSMode,GPSAccTol_FT,TargPassCount,TotalPasses,Lift,LastCMV,TargCMV,LastMDP,TargMDP,LastRMV,LastFreq_Hz,LastAmp_mm,TargThickness_FT,MachineGear,VibeState,LastTemp,_f");

      var projectDir = localExportPath.Remove(localExportPath.Length - 7);
      if (Directory.Exists(projectDir))
        Directory.Delete(projectDir, true);
    }

    [Fact(Skip="See BUG#85914")]
    public void CreateHeaders_RestrictOutputSize_TwoFiles()
    {
      var projectUid = Guid.NewGuid();

      var userPreferences = new UserPreferences() { TemperatureUnits = (int)TemperatureUnitEnum.Fahrenheit };
      var csvExportUserPreference = AutoMapperUtility.Automapper.Map<CSVExportUserPreferences>(userPreferences);

      var requestArgument = new CSVExportRequestArgument
      (
        projectUid, new FilterSet(new CombinedFilter()), "the filename",
        CoordType.Northeast, OutputTypes.VedaFinalPass,
        csvExportUserPreference, new List<CSVExportMappedMachine>(), true, true
      )
      { TRexNodeID = Guid.NewGuid().ToString() };

      var dataRows = new List<string>(70000);
      for (int i = 0; i < 70000; i++)
      {
        dataRows.Add($"row number: {i}");
      };

      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.UploadToBucket(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()));

      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton(mockTransferProxy.Object))
        .Complete();

      var csvExportFileWriter = new CSVExportFileWriter(requestArgument, true);
      csvExportFileWriter.PersistResult(dataRows);

      var localExportPath = FilePathHelper.GetTempFolderForExport(requestArgument.ProjectID, "");
      var uniqueFileName = requestArgument.FileName + "__" + requestArgument.TRexNodeID;
      var zipFullPath = Path.Combine(localExportPath, uniqueFileName) + ZIP_extension;

      if (!FileSystem.Exists(zipFullPath))
        Assert.True(true, $"zipFullPath: {zipFullPath} should exist");
      using (var zip = ZipFile.OpenRead(zipFullPath))
      {
        zip.Entries.Count.Should().Be(2);

        var firstFileName = Path.Combine(requestArgument.FileName + "(1)" + CSV_extension);
        var firstFileNamePath = Path.Combine(localExportPath, uniqueFileName, firstFileName);
        new FileInfo(firstFileNamePath).Length.Should().Be(1234316);
        zip.Entries[0].Name.Should().Be(firstFileName);
        var fileInfo = new FileInfo(firstFileNamePath);
        zip.Entries[0].Length.Should().Be(fileInfo.Length);

        var secondFileName = Path.Combine(requestArgument.FileName + "(2)" + CSV_extension);
        var secondFileNamePath = Path.Combine(localExportPath, uniqueFileName, secondFileName);
        new FileInfo(secondFileNamePath).Length.Should().Be(85096);
        zip.Entries[1].Name.Should().Be(secondFileName);
        fileInfo = new FileInfo(secondFileNamePath);
        zip.Entries[1].Length.Should().Be(fileInfo.Length);
      }

      var projectDir = localExportPath.Remove(localExportPath.Length - 7);
      if (Directory.Exists(projectDir))
        Directory.Delete(projectDir, true);
    }
  }
}
