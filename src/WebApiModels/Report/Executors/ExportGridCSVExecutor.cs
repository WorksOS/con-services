using System;
using System.Configuration;
using System.IO;
using System.Net;
using ASNode.ExportProductionDataCSV.RPC;
using ASNode.UserPreferences;
using ASNodeDecls;
using SVOICFilterSettings;
using VLPDDecls;
using ASNodeRaptorReports;
using System.Globalization;
using System.Text;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.WebApiModels.Report.Models;
using VSS.Raptor.Service.WebApiModels.Report.ResultHandling;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Nighthawk.ReportSvc.WebApi.Models;
using System.IO.Compression;

namespace VSS.Raptor.Service.WebApiModels.Report.Executors
{
    /// <summary>
    /// The executor which passes the summary pass counts request to Raptor
    /// </summary>
    public class ExportGridCSVExecutor : RequestExecutorContainer
    {
        private const double NO_HEIGHT = 1E9;
        private const int NO_CCV = SVOICDecls.__Global.kICNullCCVValue;
        private const int NO_MDP = SVOICDecls.__Global.kICNullMDPValue;
        private const int NO_TEMPERATURE = SVOICDecls.__Global.kICNullMaterialTempValue;
        private const int NO_PASSCOUNT = SVOICDecls.__Global.kICNullPassCountValue;
        private const float NULL_SINGLE = DTXModelDecls.__Global.NullSingle;

        /// <summary>
        /// 
        /// </summary>
        public IASNodeClient client { get; private set; }

        /// <summary>
        /// This constructor allows us to mock client
        /// </summary>
        /// <param name="client"></param>
        public ExportGridCSVExecutor(IASNodeClient client)
        {
            this.client = client;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ExportGridCSVExecutor()
        {
            this.client = new ASNodeClient();
        }

        /// <summary>
        /// Processes the summary pass counts request by passing the request to Raptor and returning the result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns>a PassCountSummaryResult if successful</returns>      
        protected override ContractExecutionResult ProcessEx<T>(T item)
        {
            ContractExecutionResult result = null;
            try
            {
                ExportGridCSV request = item as ExportGridCSV;

                TICFilterSettings raptorFilter = RaptorConverters.ConvertFilter(request.filterID, request.filter, request.projectId);
                MemoryStream outputStream = null;
//                DeflateStream compressorStream = null;
                Stream writerStream = null;

                MemoryStream ResponseData = null;
                ZipArchive archive = null;

                int Result = client.GetGriddedOrAlignmentCSVExport
                   (request.projectId ?? -1,
                    (int)request.reportType,
                    ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.callId ?? Guid.NewGuid()), 0, TASNodeCancellationDescriptorType.cdtProdDataExport),
                    request.designFile,
                    request.interval,
                    request.reportElevation,
                    request.reportCutFill,
                    request.reportCMV,
                    request.reportMDP,
                    request.reportPassCount,
                    request.reportTemperature,
                    (int)request.reportOption,
                    request.startNorthing,
                    request.startEasting,
                    request.endNorthing,
                    request.endEasting,
                    request.direction,
                    raptorFilter,
                    RaptorConverters.ConvertLift(request.liftBuildSettings, raptorFilter.LayerMethod),
                    new SVOICOptionsDecls.TSVOICOptions(), // ICOptions, need to resolve what this should be
                    out ResponseData);

                bool success = Result == 1; // icsrrNoError

                if (success)
                {
                    outputStream = new MemoryStream();

                    if (request.compress)
                    {
                        archive = new ZipArchive(outputStream, ZipArchiveMode.Create, true);
                        writerStream = archive.CreateEntry("asbuilt.csv", CompressionLevel.Optimal).Open();
                        //                    var demoFile = archive.CreateEntry("data");
                        //                    using (var entryStream = demoFile.Open())

//                        compressorStream = new DeflateStream(outputStream, CompressionMode.Compress);
//                        writerStream = compressorStream;
                    }
                    else
                    {
                        writerStream = outputStream;
                    }

                    // Unpack the data for the report and construct a stream containing the result
                    TRaptorReportsPackager ReportPackager = new TRaptorReportsPackager(TRaptorReportType.rrtGridReport);

                    ReportPackager.ReturnCode = TRaptorReportReturnCode.rrrcUnknownError;

                    if (request.reportType == GriddedCSVReportType.Gridded)
                    {
                        ReportPackager.GridReport.ElevationReport = request.reportElevation;
                        ReportPackager.GridReport.CutFillReport = request.reportCutFill;
                        ReportPackager.GridReport.CMVReport = request.reportCMV;
                        ReportPackager.GridReport.MDPReport = request.reportMDP;
                        ReportPackager.GridReport.PassCountReport = request.reportPassCount;
                        ReportPackager.GridReport.TemperatureReport = request.reportTemperature;
                    }
                    else
                    {
                        if (request.reportType == GriddedCSVReportType.Alignment)
                        {
                            ReportPackager.StationOffsetReport.ElevationReport = request.reportElevation;
                            ReportPackager.StationOffsetReport.CutFillReport = request.reportCutFill;
                            ReportPackager.StationOffsetReport.CMVReport = request.reportCMV;
                            ReportPackager.StationOffsetReport.MDPReport = request.reportMDP;
                            ReportPackager.StationOffsetReport.PassCountReport = request.reportPassCount;
                            ReportPackager.StationOffsetReport.TemperatureReport = request.reportTemperature;
                        }
                        else
                        {
                            throw new ArgumentException("Unknown gridded CSV report type");
                        }
                    }

                    ReportPackager.ReadFromStream(ResponseData);

                    StringBuilder sb = new StringBuilder();

                    if (request.reportType == GriddedCSVReportType.Gridded)
                    {
                        sb.Append("Northing, Easting");
                        if (ReportPackager.GridReport.ElevationReport) sb.Append(", Elevation");
                        if (ReportPackager.GridReport.CutFillReport) sb.Append(", Cut/Fill");
                        if (ReportPackager.GridReport.CMVReport) sb.Append(", CMV");
                        if (ReportPackager.GridReport.MDPReport) sb.Append(", MDP");
                        if (ReportPackager.GridReport.PassCountReport) sb.Append(", PassCount");
                        if (ReportPackager.GridReport.TemperatureReport) sb.Append(", Temperature");
                        sb.Append("\n");

                        // Write a header
                        byte[] bytes = System.Text.Encoding.ASCII.GetBytes(sb.ToString());
                        writerStream.Write(bytes, 0, bytes.Length);

                        // Write a series of CSV records from the data
                        foreach (TGridRow row in ReportPackager.GridReport.Rows)
                        {
                            sb.Clear();

                            sb.Append(String.Format(CultureInfo.InvariantCulture, "{0:F3}, {1:F3}", row.Northing, row.Easting));
                            if (ReportPackager.GridReport.ElevationReport) sb.Append(String.Format(CultureInfo.InvariantCulture, ", {0:F3}", row.Elevation));
                            if (ReportPackager.GridReport.CutFillReport) sb.Append(row.CutFill == NULL_SINGLE ? ", " : String.Format(CultureInfo.InvariantCulture, ", {0:F3}", row.CutFill));
                            if (ReportPackager.GridReport.CMVReport) sb.Append(row.CMV == NO_CCV ? ", " : String.Format(CultureInfo.InvariantCulture, ", {0}", row.CMV));
                            if (ReportPackager.GridReport.MDPReport) sb.Append(row.MDP == NO_MDP ? ", " : String.Format(CultureInfo.InvariantCulture, ", {0}", row.MDP));
                            if (ReportPackager.GridReport.PassCountReport) sb.Append(row.PassCount == NO_PASSCOUNT ? ", " : String.Format(CultureInfo.InvariantCulture, ", {0}", row.PassCount));
                            if (ReportPackager.GridReport.TemperatureReport) sb.Append(row.Temperature == NO_TEMPERATURE ? ", " : String.Format(CultureInfo.InvariantCulture, ", {0}", row.Temperature));
                            sb.Append("\n");

                            bytes = System.Text.Encoding.ASCII.GetBytes(sb.ToString());
                            writerStream.Write(bytes, 0, bytes.Length);
                        }
                    }
                    else if (request.reportType == GriddedCSVReportType.Alignment)
                    {
                        throw new NotImplementedException("Conversion of export data to CSV for alignment export is not yet implemented");
                    }
                }

                /*
                switch (Result)
                {
                    case TASNodeErrorStatus.asneOK:
                        ReturnCode = 0;
                        break;
                    case TASNodeErrorStatus.asneUnknown:
                        ReturnCode = 1;
                        break;
                    case TASNodeErrorStatus.asneExportNoData:
                        ReturnCode = 2;
                        break;
                    case TASNodeErrorStatus.asneExportTimeOut:
                        ReturnCode = 3;
                        break;
                    case TASNodeErrorStatus.asneAbortedDueToPipelineTimeout:
                        ReturnCode = 3;
                        break;
                    case TASNodeErrorStatus.asneExportCancelled:
                        ReturnCode = 4;
                        break;
                    case TASNodeErrorStatus.asneExportLimitReached:
                        ReturnCode = 5;
                        break;
                    case TASNodeErrorStatus.asneExportInvalidDateRange:
                        ReturnCode = 6;
                        break;
                    case TASNodeErrorStatus.asneExportDateRangesNoOverlap:
                        ReturnCode = 7;
                        break;
                    default:
                        ReturnCode = 1;
                        break;
                }
                */

                try
                {
                    /* if (compressorStream != null)
                                        {
                                            // Close the compression stream to allow the compression activities to be finalised
                                            compressorStream.Close();
                                        }
                    */

                    writerStream.Close();
                    if (request.compress)
                    {
                        archive.Dispose(); // Force ZIPArchive to emit all data to the stream
                    }

                    result = ExportResult.CreateExportDataResult(outputStream.ToArray(), (short)Result);
                }
                catch
                {
                    throw new ServiceException(HttpStatusCode.BadRequest,
                        new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                            "Failed to get requested export data"));
                }
            }
            finally
            {
                //ContractExecutionStates.ClearDynamic();
            }
            return result;
        }
    }
}