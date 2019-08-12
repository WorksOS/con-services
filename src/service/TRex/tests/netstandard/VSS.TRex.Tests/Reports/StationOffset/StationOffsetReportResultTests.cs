using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using VSS.Common.Abstractions.Http;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Reports.StationOffset.GridFabric.Responses;
using Xunit;

namespace VSS.TRex.Tests.Reports.StationOffset
{
  public class StationOffsetReportResultTests
  {
    [Fact]
    public void LoadOffsetsSingleOffset_Successful()
    {
      var stationOffsetRow1 = new StationOffsetRow(
        1.0, -1, 1.0, 2.0, 3.0,
        4.0, 5, 6, 7, 8
      );

      var stationOffsetRows = new List<StationOffsetRow>() {stationOffsetRow1};

      var stationOffsetDataRowManual = new StationOffsetReportDataRow_ApplicationService
        (stationOffsetRow1.Station, stationOffsetRows);

      StationOffsetReportDataRow_ApplicationService stationOffsetDataRowAuto = null;
      var queryStations = from stationOffsetRow in stationOffsetRows
        group stationOffsetRow by stationOffsetRow.Station
        into newGroup
        orderby newGroup.Key
        select newGroup;

      Assert.Single(queryStations);

      foreach (var stationGroup in queryStations)
      {
        Assert.Equal(stationOffsetRow1.Station, stationGroup.Key);
        Assert.Single(stationGroup);
        foreach (var offset in stationGroup)
        {
          Assert.Equal(stationOffsetRow1.Station, offset.Station);
          Assert.Equal(stationOffsetRow1.Offset, offset.Offset);
          Assert.Equal(stationOffsetRow1.Cmv, offset.Cmv);
        }

        // making a new copy, ensures the compare doesn't pass solely on ReferenceEquals case
        var stationOffsetRowCopy = stationGroup.First();
        var stationOffsetRowCopyList = new List<StationOffsetRow>()
        {
          new StationOffsetRow(stationOffsetRowCopy.Station, stationOffsetRowCopy.Offset,
            stationOffsetRowCopy.Northing, stationOffsetRowCopy.Easting, stationOffsetRowCopy.Elevation, stationOffsetRowCopy.CutFill,
            stationOffsetRowCopy.Cmv, stationOffsetRowCopy.Mdp, stationOffsetRowCopy.PassCount, stationOffsetRowCopy.Temperature)
        };

        stationOffsetDataRowAuto = new StationOffsetReportDataRow_ApplicationService
          (stationGroup.Key, stationOffsetRowCopyList);
      }

      Assert.Equal(stationOffsetDataRowManual.Station, stationOffsetDataRowAuto.Station);
      Assert.Equal(stationOffsetDataRowManual.Offsets.Count, stationOffsetDataRowAuto.Offsets.Count);
      Assert.Equal(stationOffsetDataRowManual.Offsets[0].Offset, stationOffsetDataRowAuto.Offsets[0].Offset);
      Assert.Equal(stationOffsetDataRowManual.Offsets[0].Northing, stationOffsetDataRowAuto.Offsets[0].Northing);
      Assert.Equal(stationOffsetDataRowManual.Minimum.CutFill, stationOffsetDataRowAuto.Minimum.CutFill);
      Assert.Equal(stationOffsetDataRowManual.Maximum.Elevation, stationOffsetDataRowAuto.Maximum.Elevation);
    }

    [Fact]
    public void LoadOffsetsMultiOffset_Successful()
    {
      double station1 = 1;
      var stationOffsetRow1 = new StationOffsetRow(
        station1, -1, 1.0, 2.0, 3.0,
        4.0, 5, 6, 7, 8
      );
      var stationOffsetRow2 = new StationOffsetRow(
        station1, 5, 1.2, 2.2, 3.2,
        4.2, 1, 2, 3, 4
      );

      var stationOffsetRows = new List<StationOffsetRow>() { stationOffsetRow1, stationOffsetRow2 };

      var stationOffsetDataRowManual = new StationOffsetReportDataRow_ApplicationService
        (station1, stationOffsetRows);

      StationOffsetReportDataRow_ApplicationService stationOffsetDataRowAuto = null;
      var queryStations = from stationOffsetRow in stationOffsetRows
                          group stationOffsetRow by stationOffsetRow.Station
        into newGroup
                          orderby newGroup.Key
                          select newGroup;

      Assert.Single(queryStations);

      foreach (var stationGroup in queryStations)
      {
        Assert.Equal(stationOffsetRow1.Station, stationGroup.Key);
        Assert.Equal(2, stationGroup.Count());
        stationOffsetDataRowAuto = new StationOffsetReportDataRow_ApplicationService
          (stationGroup.Key, stationGroup.ToList());
      }

      Assert.Equal(stationOffsetDataRowManual.Station, stationOffsetDataRowAuto.Station);
      Assert.Equal(stationOffsetDataRowManual.Offsets.Count, stationOffsetDataRowAuto.Offsets.Count);
      Assert.Equal(stationOffsetDataRowManual.Offsets[0].Offset, stationOffsetDataRowAuto.Offsets[0].Offset);
      Assert.Equal(stationOffsetDataRowManual.Offsets[0].Northing, stationOffsetDataRowAuto.Offsets[0].Northing);
      Assert.Equal(stationOffsetDataRowManual.Minimum.CutFill, stationOffsetDataRowAuto.Minimum.CutFill);
      Assert.Equal(stationOffsetDataRowManual.Maximum.Elevation, stationOffsetDataRowAuto.Maximum.Elevation);
    }

    [Fact]
    public void LoadOffsetsMultiStations_Successful()
    {
      double station1 = 10;
      var stationOffsetRowStation1_1 = new StationOffsetRow(
        station1, -1, 1.0, 2.0, 3.0,
        4.0, 5, 6, 7, 8
      );
      var stationOffsetRowStation1_2 = new StationOffsetRow(
        station1, 5, 1.2, 2.2, 3.2,
        4.2, 1, 2, 3, 4
      );

      double station2 = 20;
      var stationOffsetRowStation2_1 = new StationOffsetRow(
        station2, -1, 1.0, 2.0, 3.0,
        4.0, 5, 6, 7, 8
      );
      var stationOffsetRowStation2_2 = new StationOffsetRow(
        station2, 5, 1.2, 2.2, 3.2,
        4.2, 1, 2, 3, 4
      );

      var stationOffsetRows = new List<StationOffsetRow>()
      {
        stationOffsetRowStation1_1, stationOffsetRowStation1_2,
        stationOffsetRowStation2_1, stationOffsetRowStation2_2
      };
      Assert.Equal(4, stationOffsetRows.Count);

      var stationOffsetResponse = new StationOffsetReportRequestResponse_ApplicationService() { };
      stationOffsetResponse.LoadStationOffsets(stationOffsetRows);

      Assert.Equal(2, stationOffsetResponse.StationOffsetReportDataRowList.Count);

      var retrievedStation1 = stationOffsetResponse.StationOffsetReportDataRowList.Where(x => x.Station == station1).First();
      Assert.Equal(station1, stationOffsetRowStation1_1.Station);
      Assert.Equal(2, retrievedStation1.Offsets.Count);
      Assert.Equal(station1, retrievedStation1.Offsets[0].Station);
      Assert.Equal(stationOffsetRowStation1_1.Offset, retrievedStation1.Offsets[0].Offset);
      Assert.Equal(stationOffsetRowStation1_1.Cmv, retrievedStation1.Offsets[0].Cmv);

      var retrievedStation2 = stationOffsetResponse.StationOffsetReportDataRowList.Where(x => x.Station == station2).First();
      Assert.Equal(station2, stationOffsetRowStation2_2.Station);
      Assert.Equal(2, retrievedStation2.Offsets.Count);
      Assert.Equal(station2, retrievedStation2.Offsets[1].Station);
      Assert.Equal(stationOffsetRowStation2_2.Offset, retrievedStation2.Offsets[1].Offset);
      Assert.Equal(stationOffsetRowStation2_2.Cmv, retrievedStation2.Offsets[1].Cmv);
    }

    [Fact]
    public void LoadOffsetsMultiStationsStatistics_Successful()
    {
      double station1 = 10;
      var stationOffsetRowStation1_1 = new StationOffsetRow(
        station1, -1, 1.0, 2.0, 3.0,
        4.0, 5, 6, 7, 8
      );
      var stationOffsetRowStation1_2 = new StationOffsetRow(
        station1, 5, 1.2, 2.2, 3.2,
        3.6, 1, 2, 3, 4
      );

      double station2 = 20;
      var stationOffsetRowStation2_1 = new StationOffsetRow(
        station2, -1, 1.0, 2.0, 3.0,
        4.0, 5, 6, 7, 8
      );
      var stationOffsetRowStation2_2 = new StationOffsetRow(
        station2, 5, 1.2, 2.2, 3.2,
        4.2, 1, 2, 3, 4
      );

      var stationOffsetRows = new List<StationOffsetRow>()
      {
        stationOffsetRowStation1_1, stationOffsetRowStation1_2,
        stationOffsetRowStation2_1, stationOffsetRowStation2_2
      };
      Assert.Equal(4, stationOffsetRows.Count);

      var stationOffsetResponse = new StationOffsetReportRequestResponse_ApplicationService() { };
      stationOffsetResponse.LoadStationOffsets(stationOffsetRows);

      Assert.Equal(2, stationOffsetResponse.StationOffsetReportDataRowList.Count);

      var retrievedStation1 = stationOffsetResponse.StationOffsetReportDataRowList.Where(x => x.Station == station1).First();
      Assert.Equal(0, retrievedStation1.Minimum.Northing);
      Assert.Equal(0, retrievedStation1.Minimum.Easting);
      Assert.Equal(stationOffsetRowStation1_1.Elevation, retrievedStation1.Minimum.Elevation);
      Assert.Equal(stationOffsetRowStation1_2.CutFill, retrievedStation1.Minimum.CutFill);
      Assert.Equal(stationOffsetRowStation1_2.Cmv, retrievedStation1.Minimum.Cmv);
      Assert.Equal(stationOffsetRowStation1_2.Mdp, retrievedStation1.Minimum.Mdp);
      Assert.Equal(stationOffsetRowStation1_2.PassCount, retrievedStation1.Minimum.PassCount);
      Assert.Equal(stationOffsetRowStation1_2.Temperature, retrievedStation1.Minimum.Temperature);

      Assert.Equal(0, retrievedStation1.Maximum.Northing);
      Assert.Equal(0, retrievedStation1.Maximum.Easting);
      Assert.Equal(stationOffsetRowStation1_2.Elevation, retrievedStation1.Maximum.Elevation);
      Assert.Equal(stationOffsetRowStation1_1.CutFill, retrievedStation1.Maximum.CutFill);
      Assert.Equal(stationOffsetRowStation1_1.Cmv, retrievedStation1.Maximum.Cmv);
      Assert.Equal(stationOffsetRowStation1_1.Mdp, retrievedStation1.Maximum.Mdp);
      Assert.Equal(stationOffsetRowStation1_1.PassCount, retrievedStation1.Maximum.PassCount);
      Assert.Equal(stationOffsetRowStation1_1.Temperature, retrievedStation1.Maximum.Temperature);

      Assert.Equal(0, retrievedStation1.Average.Northing);
      Assert.Equal(0, retrievedStation1.Average.Easting);
      Assert.Equal(3.1, retrievedStation1.Average.Elevation);
      Assert.Equal(3.8, retrievedStation1.Average.CutFill);
      Assert.Equal(3, retrievedStation1.Average.Cmv);
      Assert.Equal(4, retrievedStation1.Average.Mdp);
      Assert.Equal(5, retrievedStation1.Average.PassCount);
      Assert.Equal(6, retrievedStation1.Average.Temperature);
    }

    [Fact]
    public void LoadOffsetsSingleOffset_Streaming_Successful()
    {
      // When Response returned via MasterData Proxies,
      //  it is converted to byte[], then stream then file stream, then back

      double station1 = 10;
      var stationOffsetRowStation1_1 = new StationOffsetRow(
        station1, -1, 1.0, 2.0, 3.0,
        4.0, 5, 6, 7, 8
      );
     
      var stationOffsetRows = new List<StationOffsetRow>(){stationOffsetRowStation1_1};
      var stationOffsetResponse = new StationOffsetReportRequestResponse_ApplicationService() { };
      stationOffsetResponse.LoadStationOffsets(stationOffsetRows);

      var retrievedStation1 = stationOffsetResponse.StationOffsetReportDataRowList.Where(x => x.Station == station1).First();
      Assert.NotNull(retrievedStation1);

      var original = new StationOffsetReportResult(ReportType.StationOffset);
      original.GriddedData.Rows.Add(retrievedStation1);
      original.GriddedData.ReportElevation = true;
      original.GriddedData.ReportCutFill = true;
      original.GriddedData.ReportCmv = false;
      original.GriddedData.ReportMdp = true;
      original.GriddedData.ReportPassCount = false;
      original.GriddedData.ReportTemperature = true;

      var byteArrayOfOriginal = original.Write();
      var copyOfOrig = new StationOffsetReportResult();
      copyOfOrig.Read(byteArrayOfOriginal);

      // Graceful WebReq
      var fileStream = new FileStreamResult(new MemoryStream(byteArrayOfOriginal), ContentTypeConstants.ApplicationOctetStream);
      var memoryStream = (MemoryStream)fileStream.FileStream;
      var resultFromStream = new StationOffsetReportResult();

      resultFromStream.Read(memoryStream.ToArray());

      Assert.True(ReportType.StationOffset == resultFromStream.ReportType, "Invalid report type");
      Assert.True(original.GriddedData.Rows.Count == resultFromStream.GriddedData.Rows.Count, "Invalid number of rows");
      Assert.Equal(original.GriddedData.Rows[0].Station, resultFromStream.GriddedData.Rows[0].Station);
      Assert.Equal(original.GriddedData.Rows[0].Offsets.Count, resultFromStream.GriddedData.Rows[0].Offsets.Count);
      Assert.Equal(original.GriddedData.Rows[0].Offsets[0].Offset, resultFromStream.GriddedData.Rows[0].Offsets[0].Offset);
      Assert.Equal(original.GriddedData.Rows[0].Offsets[0].Northing, resultFromStream.GriddedData.Rows[0].Offsets[0].Northing);
      Assert.Equal(original.GriddedData.Rows[0].Minimum.CutFill, resultFromStream.GriddedData.Rows[0].Minimum.CutFill);
      Assert.Equal(original.GriddedData.Rows[0].Maximum.Elevation, resultFromStream.GriddedData.Rows[0].Maximum.Elevation);
    }

    [Fact]
    public void LoadOffsetsNoOffsets_Failed()
    {
      var stationOffsetDataRowManual = new StationOffsetReportDataRow_ApplicationService();
      Assert.Equal(double.MinValue, stationOffsetDataRowManual.Station);
      Assert.NotNull(stationOffsetDataRowManual.Offsets);
      Assert.Equal(0, stationOffsetDataRowManual?.Offsets.Count);
      Assert.NotNull(stationOffsetDataRowManual.Minimum);
      Assert.NotNull(stationOffsetDataRowManual.Maximum);
      Assert.NotNull(stationOffsetDataRowManual.Average);
    }

  }
}
