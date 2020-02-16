using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon
{
  public enum PLQueryCommandEnum
  {
    PositionReportQuery = 1,
    SMUReportQuery,
    StatusQuery,
    EventDiagnosticQuery,
    FuelReportQuery,
    ProductWatchQuery,
    HardwareSoftwarePartNumber,
    RequestBDTAvailableFeatures,
    FuelLevelQuery,
    DeviceIDQuery,
    J1939EventDiagnosticQuery,
    Deregistration,
    ClearEvents,
    ProductWatchActivateDeactivate,
    RegistrationRequest,
    R2RegistrationRequest,
    ForcedDeregistration,
    BillingEnable,
    BillingDisable,
    InitialUpgradeRequest,
    UpgradeRequest,
    InitialDowngradeRequest,
    DowngradeRequest,
  }
}
