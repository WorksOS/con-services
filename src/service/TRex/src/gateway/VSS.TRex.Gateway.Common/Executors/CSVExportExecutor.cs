using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.Helpers;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Gateway.Common.ResultHandling;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class CSVExportExecutor : BaseExecutor
  {
    public CSVExportExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CSVExportExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as CompactionCSVExportRequest;

      if (request == null)
      {
        ThrowRequestTypeCastException<CompactionCSVExportRequest>();
        return null; // to keep compiler happy
      }

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.Filter, siteModel);
      // setting this date is possibly redundant - depending on how TRex handles this
      var startEndDate = CSVExportHelper.GetDateRange(siteModel, request.Filter);
      filter.AttributeFilter.StartTime = startEndDate.Item1;
      filter.AttributeFilter.EndTime = startEndDate.Item2;

      var tRexRequest = new Exports.CSV.GridFabric.CSVExportRequest();
      var csvExportRequestArgument = AutoMapperUtility.Automapper.Map<CSVExportRequestArgument>(request);
      csvExportRequestArgument.MappedMachines = CSVExportHelper.MapRequestedMachines(siteModel, request.MachineNames);

      csvExportRequestArgument.Filters = new FilterSet(filter);

      var response = tRexRequest.Execute(csvExportRequestArgument);
      var columnHeaders = CreateHeaders(request);

      byte[] toReturn = Write(columnHeaders, response.dataRows);
      // todoJeannie veta vs passcount? setup Headers, sort and string together dataRows
      //    then zip and encrypt (or v.v?)
      //   see:
      // /src/Common/Productivity3DModels/src/Models/DxfFileRequest.cs
      // public byte[] GetFileAsByteArray(IFormFile file) 
      return new CSVExportResult(toReturn);
    }

    // todoJeannie temp put somewhere else? along with headers
    // todo should I just do a FileStream
    private byte[] Write(string columnHeaders, List<string> dataRows)
    {
      using (var ms = new MemoryStream())
      {
        using (var bw = new BinaryWriter(ms, Encoding.UTF8, true))
        {
          bw.Write(columnHeaders + "\r\n");
          foreach (var row in dataRows)
          {
            bw.Write(row + "\r\n");
          }
        }

        return ms.ToArray();
      }
    }

    // not raw headers
    private const string columnHeaders1a = "Time,CellN,CellE,Elevation,";
    private const string columnHeaders1b = "Time,Lat,Long,Elevation,";
    private const string columnHeaders2a = "PassCount,";
    private const string columnHeaders2b = "PassNumber,";
    private const string columnHeaders3  = "LastRadioLtncy,DesignName,Machine,Speed,LastGPSMode,GPSAccTol,TargPassCount,";
    private const string columnHeaders4a = "TotalPasses,";
    private const string columnHeaders4b = "ValidPos,";
    private const string columnHeaders5  = "Lift,LastCMV,TargCMV,LastMDP,TargMDP,LastRMV,LastFreq,LastAmp,TargThickness,MachineGear,VibeState,LastTemp";

    // Raw headers in Metric
    private const string columnHeaders1aM = "Time,CellN_m,CellE_m,Elevation_m,";
    private const string columnHeaders1bM = "Time,Lat,Long,Elevation_m,";
    private const string columnHeaders2aM = "PassCount,";
    private const string columnHeaders2bM = "PassNumber,";
    private const string columnHeaders3M  = "LastRadioLtncy,DesignName,Machine,Speed_km/h,LastGPSMode,GPSAccTol_m,TargPassCount,";
    private const string columnHeaders4aM = "TotalPasses,";
    private const string columnHeaders4bM = "ValidPos,";
    private const string columnHeaders5M  = "Lift,LastCMV,TargCMV,LastMDP,TargMDP,LastRMV,LastFreq_Hz,LastAmp_mm,TargThickness_m,MachineGear,VibeState,LastTemp,";
    private const string columnHeaders6aM = "_c";
    private const string columnHeaders6bM = "_f";

    // Raw headers in Feet
    private const string columnHeaders1aF = "Time,CellN_FT,CellE_FT,Elevation_FT,";
    private const string columnHeaders1bF = "Time,Lat,Long,Elevation_FT,";
    private const string columnHeaders2aF = "PassCount,";
    private const string columnHeaders2bF = "PassNumber,";
    private const string columnHeaders3F  = "lastRadioLtncy,DesignName,Machine,Speed_mph,LastGPSMode,GPSAccTol_FT,TargPassCount,";
    private const string columnHeaders4aF = "TotalPasses,";
    private const string columnHeaders4bF = "ValidPos,";
    private const string columnHeaders5F  = "Lift,LastCMV,TargCMV,LastMDP,TargMDP,LastRMV,LastFreq_Hz,LastAmp_mm,TargThickness_FT,MachineGear,VibeState,LastTemp,";
    private const string columnHeaders6aF = "_c";
    private const string columnHeaders6bF = "_f";

    private string CreateHeaders(CompactionCSVExportRequest request)
    {
      // no translations in raptor, so none here

      var header = string.Empty;
      if (request.OutputType == OutputTypes.VedaAllPasses ||
          request.OutputType == OutputTypes.VedaFinalPass /* ||
          request.isRawdb3Required  todoJeannie */
        )
      {
        if (request.UserPreferences.Units == (int) UnitsTypeEnum.Metric)
        {
          if (request.CoordType == CoordType.Northeast)
            header += columnHeaders1aM; // time, CellN...
          else
            header += columnHeaders1bM; // time, Lat...

          if (request.OutputType == OutputTypes.PassCountLastPass ||
              request.OutputType == OutputTypes.VedaFinalPass)
            header += columnHeaders2aM; // passcount
          else
            header += columnHeaders2bM; // passnumber

          header += columnHeaders3M; // LastRadioLtncy,...

          if (request.OutputType == OutputTypes.PassCountLastPass ||
              request.OutputType == OutputTypes.VedaFinalPass)
            header += columnHeaders4aM; //TotalPasses
          else
            header += columnHeaders4bM; // ValidPos

          header += columnHeaders5M;

          if (request.UserPreferences.TemperatureUnits == (int)TemperatureUnitEnum.Celsius)
            header += columnHeaders6aM; // _c celcuis
          else
            header += columnHeaders6bM; // _f Fahrenheit
        }
        else
        {
          if (request.CoordType == CoordType.Northeast)
            header += columnHeaders1aF; // time, CellN...
          else
            header += columnHeaders1bF; // time, Lat...

          if (request.OutputType == OutputTypes.PassCountLastPass ||
              request.OutputType == OutputTypes.VedaFinalPass)
            header += columnHeaders2aF; // passcount
          else
            header += columnHeaders2bF; // passnumber

          header += columnHeaders3F; // LastRadioLtncy,...

          if (request.OutputType == OutputTypes.PassCountLastPass ||
              request.OutputType == OutputTypes.VedaFinalPass)
            header += columnHeaders4aF; //TotalPasses
          else
            header += columnHeaders4bF; // ValidPos

          header += columnHeaders5F;

          if (request.UserPreferences.TemperatureUnits == (int)TemperatureUnitEnum.Celsius)
            header += columnHeaders6aF; // _c celcuis
          else
            header += columnHeaders6bF; // _f Fahrenheit
        }
      }
      else // not Raw (headers unitless)
      {
        if (request.CoordType == CoordType.Northeast)
          header += columnHeaders1a; // time, CellN...
        else
          header += columnHeaders1b; // time, Lat...

        if (request.OutputType == OutputTypes.PassCountLastPass ||
            request.OutputType == OutputTypes.VedaFinalPass)
          header += columnHeaders2a; // passcount
        else
          header += columnHeaders2b; // passnumber

        header += columnHeaders3; // LastRadioLtncy,...

        if (request.OutputType == OutputTypes.PassCountLastPass ||
            request.OutputType == OutputTypes.VedaFinalPass)
          header += columnHeaders4a; //TotalPasses
        else
          header += columnHeaders4b; // ValidPos

        header += columnHeaders5;
      }

      return header;
    }

    /// <summary>
    /// Processes the request asynchronously.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
