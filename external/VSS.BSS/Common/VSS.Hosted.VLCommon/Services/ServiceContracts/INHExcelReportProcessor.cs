using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.ServiceModel;
using VSS.Hosted.VLCommon.ServiceContracts;

namespace VSS.Hosted.VLCommon
{
  [MessageContract]
  public class NHExcelReportRequest
  {
    [MessageHeader]
    public string reportSessionID;

    [MessageBodyMember]
    public Stream bitmapData;
  }

  [MessageContract]
  public class NHExcelScheduleReportRequest
  {
    [MessageHeader]
    public string reportSessionID;

    [MessageHeader]
    public string folderName;

    [MessageHeader]
    public int startKeyDate;

    [MessageHeader]
    public int endKeyDate;

    [MessageHeader]
    public int fileLocationID;  
  }

  [MessageContract]
  public class NHExcelReportResponse
  {
    [MessageBodyMember]
    public byte[] reportData;
  }

  [ServiceContract]
  public interface INHExcelReportProcessor
  {
    [OperationContract]
    NHExcelReportResponse ProcessReport(NHExcelReportRequest request);
  }

  [ServiceContract]
  public interface INHExcelScheduleReportProcessor
  {
    [OperationContract]
    NHExcelReportResponse ProcessScheduleReport(NHExcelScheduleReportRequest request);
  }
}
