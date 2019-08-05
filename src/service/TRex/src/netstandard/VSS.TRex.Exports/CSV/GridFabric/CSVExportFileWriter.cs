using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using FileSystem = System.IO.File;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities;
using VSS.TRex.DI;

namespace VSS.TRex.Exports.CSV.GridFabric
{
  public class CSVExportFileWriter
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CSVExportFileWriter>();

    private const ushort RestrictedOutputRowCount = 65535;
    private const string CSV_extension = ".csv";
    private const string ZIP_extension = ".zip";
    private readonly CSVExportRequestArgument requestArgument;
    private readonly string awsBucketNameKey = "AWS_BUCKET_NAME"; // vss-exports-stg/prod
    private readonly string awsBucketName;
    private readonly bool retainLocalCopyForTesting = false;

    public CSVExportFileWriter(CSVExportRequestArgument requestArgument, bool retainLocalCopyForTesting = false)
    {
      this.requestArgument = requestArgument;
      awsBucketName = DIContext.Obtain<IConfigurationStore>().GetValueString(awsBucketNameKey);
      if (string.IsNullOrEmpty(awsBucketName))
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            $"Environment variable is missing: {awsBucketNameKey}"));
      }

      this.retainLocalCopyForTesting = retainLocalCopyForTesting;
    }

    public string PersistResult(List<string> dataRows)
    {
      bool fileLoadedOk = false;
      string s3FullPath = null;

      try
      {
        // local path and zip fileName include a unique ID to avoid overwriting someone else file
        // write file/s to a local, unique, directory
        var uniqueFileName = requestArgument.FileName + "__" + requestArgument.TRexNodeID;
        var localExportPath = FilePathHelper.GetTempFolderForExport(requestArgument.ProjectID, uniqueFileName);
        var localPath = Path.Combine(localExportPath, uniqueFileName);
        PersistLocally(dataRows, localPath);

        // zip the directory
        var zipFullPath = Path.Combine(localExportPath, uniqueFileName) + ZIP_extension;

        ZipFile.CreateFromDirectory(localPath, zipFullPath, CompressionLevel.Optimal, false, Encoding.UTF8);
        // copy zip to S3
        s3FullPath = $"project/{requestArgument.ProjectID}/TRexExport/{uniqueFileName}{ZIP_extension}";
        fileLoadedOk = S3FileTransfer.WriteFileToBucket(zipFullPath, s3FullPath, awsBucketName);

        // delete the export folder
        localExportPath = FilePathHelper.GetTempFolderForExport(requestArgument.ProjectID, "");
        if (Directory.Exists(localExportPath) && !retainLocalCopyForTesting)
          Directory.Delete(localExportPath, true);
      }
      catch (Exception e)
      {
        Log.LogError(e, "Error persisting export data");
        throw;
      }

      return fileLoadedOk ? s3FullPath : string.Empty;
    }

    private void PersistLocally(List<string> dataRows, string localPath)
    {
      var columnHeaders = CreateHeaders();
      string targetFile;

      if (!requestArgument.RestrictOutputSize || dataRows.Count <= RestrictedOutputRowCount)
      {
        // Write the header and all rows to a file
        targetFile = Path.Combine(localPath, requestArgument.FileName) + CSV_extension;
        using (var fs = new StreamWriter(targetFile) {NewLine = "\r\n" })
        {
          fs.WriteLine(columnHeaders);
          foreach (var row in dataRows)
          {
            fs.WriteLine(row);
          }
        }
        Log.LogInformation($"Saved CSV export file locally: {targetFile}");
      }
      else
      {
        int fileNumber = 1;
        int startRowNumberInBlock = 0;
        for (; startRowNumberInBlock <= dataRows.Count 
            ; fileNumber++, startRowNumberInBlock += RestrictedOutputRowCount)
        {
          // Write the header and n rows to a file
          targetFile = Path.Combine(localPath, requestArgument.FileName + $"({fileNumber})") + CSV_extension;
          using (var fs = new StreamWriter(targetFile) { NewLine = "\r\n" })
          {
            fs.WriteLine(columnHeaders);
            var countToWrite = startRowNumberInBlock + RestrictedOutputRowCount > dataRows.Count 
              ? dataRows.Count - startRowNumberInBlock 
              : RestrictedOutputRowCount;
            foreach (var row in dataRows.GetRange(startRowNumberInBlock, countToWrite))
            {
              fs.WriteLine(row);
            }
          }
          Log.LogInformation($"Saved CSV export file locally: {targetFile}");
        }
      }
    }

    // not raw headers
    private const string columnHeaders1a = "Time,CellN,CellE,Elevation,";
    private const string columnHeaders1b = "Time,Lat,Long,Elevation,";
    private const string columnHeaders2a = "PassCount,";
    private const string columnHeaders2b = "PassNumber,";
    private const string columnHeaders3 = "LastRadioLtncy,DesignName,Machine,Speed,LastGPSMode,GPSAccTol,TargPassCount,";
    private const string columnHeaders4a = "TotalPasses,";
    private const string columnHeaders4b = "ValidPos,";
    private const string columnHeaders5 = "Lift,LastCMV,TargCMV,LastMDP,TargMDP,LastRMV,LastFreq,LastAmp,TargThickness,MachineGear,VibeState,LastTemp";

    // Raw headers in Metric
    private const string columnHeaders1aM = "Time,CellN_m,CellE_m,Elevation_m,";
    private const string columnHeaders1bM = "Time,Lat,Long,Elevation_m,";
    private const string columnHeaders2aM = "PassCount,";
    private const string columnHeaders2bM = "PassNumber,";
    private const string columnHeaders3M = "LastRadioLtncy,DesignName,Machine,Speed_km/h,LastGPSMode,GPSAccTol_m,TargPassCount,";
    private const string columnHeaders4aM = "TotalPasses,";
    private const string columnHeaders4bM = "ValidPos,";
    private const string columnHeaders5M = "Lift,LastCMV,TargCMV,LastMDP,TargMDP,LastRMV,LastFreq_Hz,LastAmp_mm,TargThickness_m,MachineGear,VibeState,LastTemp,";
    private const string columnHeaders6aM = "_c";
    private const string columnHeaders6bM = "_f";

    // Raw headers in Feet
    private const string columnHeaders1aF = "Time,CellN_FT,CellE_FT,Elevation_FT,";
    private const string columnHeaders1bF = "Time,Lat,Long,Elevation_FT,";
    private const string columnHeaders2aF = "PassCount,";
    private const string columnHeaders2bF = "PassNumber,";
    private const string columnHeaders3F = "lastRadioLtncy,DesignName,Machine,Speed_mph,LastGPSMode,GPSAccTol_FT,TargPassCount,";
    private const string columnHeaders4aF = "TotalPasses,";
    private const string columnHeaders4bF = "ValidPos,";
    private const string columnHeaders5F = "Lift,LastCMV,TargCMV,LastMDP,TargMDP,LastRMV,LastFreq_Hz,LastAmp_mm,TargThickness_FT,MachineGear,VibeState,LastTemp,";
    private const string columnHeaders6aF = "_c";
    private const string columnHeaders6bF = "_f";

    private string CreateHeaders()
    {
      var header = new StringBuilder();
      if (requestArgument.OutputType == OutputTypes.VedaAllPasses ||
          requestArgument.OutputType == OutputTypes.VedaFinalPass ||
          requestArgument.RawDataAsDBase
        )
      {
        if (requestArgument.UserPreferences.Units == UnitsTypeEnum.Metric)
        {
          if (requestArgument.CoordType == CoordType.Northeast)
            header.Append(columnHeaders1aM); // time, CellN...
          else
            header.Append(columnHeaders1bM); // time, Lat...

          if (requestArgument.OutputType == OutputTypes.PassCountLastPass ||
              requestArgument.OutputType == OutputTypes.VedaFinalPass)
            header.Append(columnHeaders2aM); // pass count
          else
            header.Append(columnHeaders2bM); // pass number

          header.Append(columnHeaders3M); // LastRadioLatency,...

          if (requestArgument.OutputType == OutputTypes.PassCountLastPass ||
              requestArgument.OutputType == OutputTypes.VedaFinalPass)
            header.Append(columnHeaders4aM); //TotalPasses
          else
            header.Append(columnHeaders4bM); // ValidPos

          header.Append(columnHeaders5M);

          if (requestArgument.UserPreferences.TemperatureUnits == TemperatureUnitEnum.Celsius)
            header.Append(columnHeaders6aM); // _c celsius
          else
            header.Append(columnHeaders6bM); // _f Fahrenheit
        }
        else
        {
          if (requestArgument.CoordType == CoordType.Northeast)
            header.Append(columnHeaders1aF); // time, CellN...
          else
            header.Append(columnHeaders1bF); // time, Lat...

          if (requestArgument.OutputType == OutputTypes.PassCountLastPass ||
              requestArgument.OutputType == OutputTypes.VedaFinalPass)
            header.Append(columnHeaders2aF); // pass count
          else
            header.Append(columnHeaders2bF); // pass number

          header.Append(columnHeaders3F); // LastRadioLatency,...

          if (requestArgument.OutputType == OutputTypes.PassCountLastPass ||
              requestArgument.OutputType == OutputTypes.VedaFinalPass)
            header.Append(columnHeaders4aF); //TotalPasses
          else
            header.Append(columnHeaders4bF); // ValidPos

          header.Append(columnHeaders5F);

          if (requestArgument.UserPreferences.TemperatureUnits == TemperatureUnitEnum.Celsius)
            header.Append(columnHeaders6aF); // _c celsius
          else
            header.Append(columnHeaders6bF); // _f Fahrenheit
        }
      }
      else // not Raw (headers unitless)
      {
        if (requestArgument.CoordType == CoordType.Northeast)
          header.Append(columnHeaders1a); // time, CellN...
        else
          header.Append(columnHeaders1b); // time, Lat...

        if (requestArgument.OutputType == OutputTypes.PassCountLastPass ||
            requestArgument.OutputType == OutputTypes.VedaFinalPass)
          header.Append(columnHeaders2a); // pass count
        else
          header.Append(columnHeaders2b); // pass number

        header.Append(columnHeaders3); // LastRadioLatency,...

        if (requestArgument.OutputType == OutputTypes.PassCountLastPass ||
            requestArgument.OutputType == OutputTypes.VedaFinalPass)
          header.Append(columnHeaders4a); //TotalPasses
        else
          header.Append(columnHeaders4b); // ValidPos

        header.Append(columnHeaders5);
      }

      return header.ToString();
    }
  }
}
