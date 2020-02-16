using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using log4net;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.DeviceCapabilityService.Helpers;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.DTOs;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Controllers
{
  public class DeviceConfigController : ApiController
  {
    private readonly IDeviceConfigProcessor _processor;
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public DeviceConfigController(IDeviceConfigProcessor processor)
    {
      _processor = processor;
    }

    [ActionName(ActionConstants.GetDigitalSwitchConfigurationEvent)]
    public HttpResponseMessage GetDigitalSwitchConfigurationEvent(HttpRequestMessage request, [ModelBinder(typeof(DeviceQueryModelBinder))]IDeviceQuery device)
    {
      if (ModelState.ContainsKey(ModelBinderConstants.DeviceQueryModelBinderError))
      {
        return request.CreateResponse(HttpStatusCode.NotFound, ModelState[ModelBinderConstants.DeviceQueryModelBinderError].Errors[0].ErrorMessage);
      }

      Log.IfDebugFormat("{0} {1} processing {2} request", GetType().Name, "GetDigitalSwitchConfigurationEvent", ActionConstants.GetDigitalSwitchConfigurationEvent);
      IFactoryOutboundEventTypeDescriptor typeDescriptor = null;

      try
      {
        typeDescriptor = _processor.GetDigitalSwitchConfigurationEvent(device);
      }
      catch (UnknownDeviceException e)
      {
        Log.IfDebugFormat("{0} {1} UnknownDeviceException - {2}", GetType().Name, "GetDigitalSwitchConfigurationEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (NotImplementedException e)
      {
        Log.IfDebugFormat("{0} {1} NotImplementedException - {2}", GetType().Name, "GetDigitalSwitchConfigurationEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (Exception e)
      {
        Log.IfErrorFormat(e, "{0} {1} Exception", GetType().Name, "GetDigitalSwitchConfigurationEvent");
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
      }

      return request.CreateResponse(HttpStatusCode.OK, typeDescriptor);
    }

    [ActionName(ActionConstants.GetDisableMaintenanceModeEvent)]
    public HttpResponseMessage GetDisableMaintenanceModeEvent(HttpRequestMessage request, [ModelBinder(typeof(DeviceQueryModelBinder))]IDeviceQuery device)
    {
      if (ModelState.ContainsKey(ModelBinderConstants.DeviceQueryModelBinderError))
      {
        return request.CreateResponse(HttpStatusCode.NotFound, ModelState[ModelBinderConstants.DeviceQueryModelBinderError].Errors[0].ErrorMessage);
      }

      Log.IfDebugFormat("{0} {1} processing {2} request", GetType().Name, "GetDisableMaintenanceModeEvent", ActionConstants.GetDisableMaintenanceModeEvent);
      IFactoryOutboundEventTypeDescriptor typeDescriptor = null;

      try
      {
        typeDescriptor = _processor.GetDisableMaintenanceModeEvent(device);
      }
      catch (UnknownDeviceException e)
      {
        Log.IfDebugFormat("{0} {1} UnknownDeviceException - {2}", GetType().Name, "GetDisableMaintenanceModeEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (NotImplementedException e)
      {
        Log.IfDebugFormat("{0} {1} NotImplementedException - {2}", GetType().Name, "GetDisableMaintenanceModeEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (Exception e)
      {
        Log.IfErrorFormat(e, "{0} {1} Exception", GetType().Name, "GetDisableMaintenanceModeEvent");
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
      }

      return request.CreateResponse(HttpStatusCode.OK, typeDescriptor);
    }

    [ActionName(ActionConstants.GetDiscreteInputConfigurationEvent)]
    public HttpResponseMessage GetDiscreteInputConfigurationEvent(HttpRequestMessage request, [ModelBinder(typeof(DeviceQueryModelBinder))]IDeviceQuery device)
    {
      if (ModelState.ContainsKey(ModelBinderConstants.DeviceQueryModelBinderError))
      {
        return request.CreateResponse(HttpStatusCode.NotFound, ModelState[ModelBinderConstants.DeviceQueryModelBinderError].Errors[0].ErrorMessage);
      }

      Log.IfDebugFormat("{0} {1} processing {2} request", GetType().Name, "GetDiscreteInputConfigurationEvent", ActionConstants.GetDiscreteInputConfigurationEvent);
      IFactoryOutboundEventTypeDescriptor typeDescriptor = null;

      try
      {
        typeDescriptor = _processor.GetDiscreteInputConfigurationEvent(device);
      }
      catch (UnknownDeviceException e)
      {
        Log.IfDebugFormat("{0} {1} UnknownDeviceException - {2}", GetType().Name, "GetDiscreteInputConfigurationEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (NotImplementedException e)
      {
        Log.IfDebugFormat("{0} {1} NotImplementedException - {2}", GetType().Name, "GetDiscreteInputConfigurationEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (Exception e)
      {
        Log.IfErrorFormat(e, "{0} {1} Exception", GetType().Name, "GetDiscreteInputConfigurationEvent");
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
      }

      return request.CreateResponse(HttpStatusCode.OK, typeDescriptor);
    }

    [ActionName(ActionConstants.GetEnableMaintenanceModeEvent)]
    public HttpResponseMessage GetEnableMaintenanceModeEvent(HttpRequestMessage request, [ModelBinder(typeof(DeviceQueryModelBinder))]IDeviceQuery device)
    {
      if (ModelState.ContainsKey(ModelBinderConstants.DeviceQueryModelBinderError))
      {
        return request.CreateResponse(HttpStatusCode.NotFound, ModelState[ModelBinderConstants.DeviceQueryModelBinderError].Errors[0].ErrorMessage);
      }

      Log.IfDebugFormat("{0} {1} processing {2} request", GetType().Name, "GetEnableMaintenanceModeEvent", ActionConstants.GetEnableMaintenanceModeEvent);
      IFactoryOutboundEventTypeDescriptor typeDescriptor = null;

      try
      {
        typeDescriptor = _processor.GetEnableMaintenanceModeEvent(device);
      }
      catch (UnknownDeviceException e)
      {
        Log.IfDebugFormat("{0} {1} UnknownDeviceException - {2}", GetType().Name, "GetEnableMaintenanceModeEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (NotImplementedException e)
      {
        Log.IfDebugFormat("{0} {1} NotImplementedException - {2}", GetType().Name, "GetEnableMaintenanceModeEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (Exception e)
      {
        Log.IfErrorFormat(e, "{0} {1} Exception", GetType().Name, "GetEnableMaintenanceModeEvent");
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
      }

      return request.CreateResponse(HttpStatusCode.OK, typeDescriptor);
    }

    [ActionName(ActionConstants.GetFirstDailyReportStartTimeUtcChangedEvent)]
    public HttpResponseMessage GetFirstDailyReportStartTimeUtcChangedEvent(HttpRequestMessage request, [ModelBinder(typeof(DeviceQueryModelBinder))]IDeviceQuery device)
    {
      if (ModelState.ContainsKey(ModelBinderConstants.DeviceQueryModelBinderError))
      {
        return request.CreateResponse(HttpStatusCode.NotFound, ModelState[ModelBinderConstants.DeviceQueryModelBinderError].Errors[0].ErrorMessage);
      }

      Log.IfDebugFormat("{0} {1} processing {2} request", GetType().Name, "GetFirstDailyReportStartTimeUtcChangedEvent", ActionConstants.GetFirstDailyReportStartTimeUtcChangedEvent);
      IFactoryOutboundEventTypeDescriptor typeDescriptor = null;

      try
      {
        typeDescriptor = _processor.GetFirstDailyReportStartTimeUtcChangedEvent(device);
      }
      catch (UnknownDeviceException e)
      {
        Log.IfDebugFormat("{0} {1} UnknownDeviceException - {2}", GetType().Name, "GetFirstDailyReportStartTimeUtcChangedEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (NotImplementedException e)
      {
        Log.IfDebugFormat("{0} {1} NotImplementedException - {2}", GetType().Name, "GetFirstDailyReportStartTimeUtcChangedEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (Exception e)
      {
        Log.IfErrorFormat(e, "{0} {1} Exception", GetType().Name, "GetFirstDailyReportStartTimeUtcChangedEvent");
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
      }

      return request.CreateResponse(HttpStatusCode.OK, typeDescriptor);
    }

    [ActionName(ActionConstants.GetHourMeterModifiedEvent)]
    public HttpResponseMessage GetHourMeterModifiedEvent(HttpRequestMessage request, [ModelBinder(typeof(DeviceQueryModelBinder))]IDeviceQuery device)
    {
      if (ModelState.ContainsKey(ModelBinderConstants.DeviceQueryModelBinderError))
      {
        return request.CreateResponse(HttpStatusCode.NotFound, ModelState[ModelBinderConstants.DeviceQueryModelBinderError].Errors[0].ErrorMessage);
      }

      Log.IfDebugFormat("{0} {1} processing {2} request", GetType().Name, "GetHourMeterModifiedEvent", ActionConstants.GetHourMeterModifiedEvent);
      IFactoryOutboundEventTypeDescriptor typeDescriptor = null;

      try
      {
        typeDescriptor = _processor.GetHourMeterModifiedEvent(device);
      }
      catch (UnknownDeviceException e)
      {
        Log.IfDebugFormat("{0} {1} UnknownDeviceException - {2}", GetType().Name, "GetHourMeterModifiedEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (NotImplementedException e)
      {
        Log.IfDebugFormat("{0} {1} NotImplementedException - {2}", GetType().Name, "GetHourMeterModifiedEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (Exception e)
      {
        Log.IfErrorFormat(e, "{0} {1} Exception", GetType().Name, "GetHourMeterModifiedEvent");
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
      }

      return request.CreateResponse(HttpStatusCode.OK, typeDescriptor);
    }

    [ActionName(ActionConstants.GetMovingCriteriaConfigurationChangedEvent)]
    public HttpResponseMessage GetMovingCriteriaConfigurationChangedEvent(HttpRequestMessage request, [ModelBinder(typeof(DeviceQueryModelBinder))]IDeviceQuery device)
    {
      if (ModelState.ContainsKey(ModelBinderConstants.DeviceQueryModelBinderError))
      {
        return request.CreateResponse(HttpStatusCode.NotFound, ModelState[ModelBinderConstants.DeviceQueryModelBinderError].Errors[0].ErrorMessage);
      }

      Log.IfDebugFormat("{0} {1} processing {2} request", GetType().Name, "GetMovingCriteriaConfigurationChangedEvent", ActionConstants.GetMovingCriteriaConfigurationChangedEvent);
      IFactoryOutboundEventTypeDescriptor typeDescriptor = null;

      try
      {
        typeDescriptor = _processor.GetMovingCriteriaConfigurationChangedEvent(device);
      }
      catch (UnknownDeviceException e)
      {
        Log.IfDebugFormat("{0} {1} UnknownDeviceException - {2}", GetType().Name, "GetMovingCriteriaConfigurationChangedEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (NotImplementedException e)
      {
        Log.IfDebugFormat("{0} {1} NotImplementedException - {2}", GetType().Name, "GetMovingCriteriaConfigurationChangedEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (Exception e)
      {
        Log.IfErrorFormat(e, "{0} {1} Exception", GetType().Name, "GetMovingCriteriaConfigurationChangedEvent");
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
      }

      return request.CreateResponse(HttpStatusCode.OK, typeDescriptor);
    }

    [ActionName(ActionConstants.GetOdometerModifiedEvent)]
    public HttpResponseMessage GetOdometerModifiedEvent(HttpRequestMessage request, [ModelBinder(typeof(DeviceQueryModelBinder))]IDeviceQuery device)
    {
      if (ModelState.ContainsKey(ModelBinderConstants.DeviceQueryModelBinderError))
      {
        return request.CreateResponse(HttpStatusCode.NotFound, ModelState[ModelBinderConstants.DeviceQueryModelBinderError].Errors[0].ErrorMessage);
      }

      Log.IfDebugFormat("{0} {1} processing {2} request", GetType().Name, "GetOdometerModifiedEvent", ActionConstants.GetHourMeterModifiedEvent);
      IFactoryOutboundEventTypeDescriptor typeDescriptor = null;

      try
      {
        typeDescriptor = _processor.GetOdometerModifiedEvent(device);
      }
      catch (UnknownDeviceException e)
      {
        Log.IfDebugFormat("{0} {1} UnknownDeviceException - {2}", GetType().Name, "GetOdometerModifiedEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (NotImplementedException e)
      {
        Log.IfDebugFormat("{0} {1} NotImplementedException - {2}", GetType().Name, "GetOdometerModifiedEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (Exception e)
      {
        Log.IfErrorFormat(e, "{0} {1} Exception", GetType().Name, "GetOdometerModifiedEvent");
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
      }

      return request.CreateResponse(HttpStatusCode.OK, typeDescriptor);
    }

    [ActionName(ActionConstants.SetStartModeEvent)]
    public HttpResponseMessage GetSetStartModeEvent(HttpRequestMessage request, [ModelBinder(typeof(DeviceQueryModelBinder))]IDeviceQuery device)
    {
      if (ModelState.ContainsKey(ModelBinderConstants.DeviceQueryModelBinderError))
      {
        return request.CreateResponse(HttpStatusCode.NotFound, ModelState[ModelBinderConstants.DeviceQueryModelBinderError].Errors[0].ErrorMessage);
      }

      Log.IfDebugFormat("{0} {1} processing {2} request", GetType().Name, "SetStartModeEvent", ActionConstants.SetStartModeEvent);
      IFactoryOutboundEventTypeDescriptor typeDescriptor = null;

      try
      {
        typeDescriptor = _processor.SetStartModeEvent(device);
      }
      catch (UnknownDeviceException e)
      {
        Log.IfDebugFormat("{0} {1} UnknownDeviceException - {2}", GetType().Name, "SetStartModeEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (NotImplementedException e)
      {
        Log.IfDebugFormat("{0} {1} NotImplementedException - {2}", GetType().Name, "SetStartModeEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (Exception e)
      {
        Log.IfErrorFormat(e, "{0} {1} Exception", GetType().Name, "SetStartModeEvent");
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
      }

      return request.CreateResponse(HttpStatusCode.OK, typeDescriptor);
    }

    [ActionName(ActionConstants.GetStartModeEvent)]
    public HttpResponseMessage GetStartModeEvent(HttpRequestMessage request, [ModelBinder(typeof(DeviceQueryModelBinder))]IDeviceQuery device)
    {
      if (ModelState.ContainsKey(ModelBinderConstants.DeviceQueryModelBinderError))
      {
        return request.CreateResponse(HttpStatusCode.NotFound, ModelState[ModelBinderConstants.DeviceQueryModelBinderError].Errors[0].ErrorMessage);
      }

      Log.IfDebugFormat("{0} {1} processing {2} request", GetType().Name, "GetStartModeEvent", ActionConstants.GetStartModeEvent);
      IFactoryOutboundEventTypeDescriptor typeDescriptor = null;

      try
      {
        typeDescriptor = _processor.GetStartModeEvent(device);
      }
      catch (UnknownDeviceException e)
      {
        Log.IfDebugFormat("{0} {1} UnknownDeviceException - {2}", GetType().Name, "GetStartModeEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (NotImplementedException e)
      {
        Log.IfDebugFormat("{0} {1} NotImplementedException - {2}", GetType().Name, "GetStartModeEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (Exception e)
      {
        Log.IfErrorFormat(e, "{0} {1} Exception", GetType().Name, "GetStartModeEvent");
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
      }

      return request.CreateResponse(HttpStatusCode.OK, typeDescriptor);
    }

    [ActionName(ActionConstants.SetTamperLevelEvent)]
    public HttpResponseMessage GetSetTamperLevelEvent(HttpRequestMessage request, [ModelBinder(typeof(DeviceQueryModelBinder))]IDeviceQuery device)
    {
      if (ModelState.ContainsKey(ModelBinderConstants.DeviceQueryModelBinderError))
      {
        return request.CreateResponse(HttpStatusCode.NotFound, ModelState[ModelBinderConstants.DeviceQueryModelBinderError].Errors[0].ErrorMessage);
      }

      Log.IfDebugFormat("{0} {1} processing {2} request", GetType().Name, "SetTamperLevelEvent", ActionConstants.SetTamperLevelEvent);
      IFactoryOutboundEventTypeDescriptor typeDescriptor = null;

      try
      {
        typeDescriptor = _processor.SetTamperLevelEvent(device);
      }
      catch (UnknownDeviceException e)
      {
        Log.IfDebugFormat("{0} {1} UnknownDeviceException - {2}", GetType().Name, "SetTamperLevelEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (NotImplementedException e)
      {
        Log.IfDebugFormat("{0} {1} NotImplementedException - {2}", GetType().Name, "SetTamperLevelEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (Exception e)
      {
        Log.IfErrorFormat(e, "{0} {1} Exception", GetType().Name, "SetTamperLevelEvent");
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
      }

      return request.CreateResponse(HttpStatusCode.OK, typeDescriptor);
    }

    [ActionName(ActionConstants.GetTamperLevelEvent)]
    public HttpResponseMessage GetTamperLevelEvent(HttpRequestMessage request, [ModelBinder(typeof(DeviceQueryModelBinder))]IDeviceQuery device)
    {
      if (ModelState.ContainsKey(ModelBinderConstants.DeviceQueryModelBinderError))
      {
        return request.CreateResponse(HttpStatusCode.NotFound, ModelState[ModelBinderConstants.DeviceQueryModelBinderError].Errors[0].ErrorMessage);
      }

      Log.IfDebugFormat("{0} {1} processing {2} request", GetType().Name, "GetTamperLevelEvent", ActionConstants.GetTamperLevelEvent);
      IFactoryOutboundEventTypeDescriptor typeDescriptor = null;

      try
      {
        typeDescriptor = _processor.GetTamperLevelEvent(device);
      }
      catch (UnknownDeviceException e)
      {
        Log.IfDebugFormat("{0} {1} UnknownDeviceException - {2}", GetType().Name, "GetTamperLevelEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (NotImplementedException e)
      {
        Log.IfDebugFormat("{0} {1} NotImplementedException - {2}", GetType().Name, "GetTamperLevelEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (Exception e)
      {
        Log.IfErrorFormat(e, "{0} {1} Exception", GetType().Name, "GetTamperLevelEvent");
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
      }

      return request.CreateResponse(HttpStatusCode.OK, typeDescriptor);
    }

    [ActionName(ActionConstants.SetDailyReportFrequencyEvent)]
    public HttpResponseMessage GetSetDailyReportFrequencyEvent(HttpRequestMessage request, [ModelBinder(typeof(DeviceQueryModelBinder))]IDeviceQuery device)
    {
      if (ModelState.ContainsKey(ModelBinderConstants.DeviceQueryModelBinderError))
      {
        return request.CreateResponse(HttpStatusCode.NotFound, ModelState[ModelBinderConstants.DeviceQueryModelBinderError].Errors[0].ErrorMessage);
      }

      Log.IfDebugFormat("{0} {1} processing {2} request", GetType().Name, "GetSetDailyReportFrequencyEvent", ActionConstants.SetDailyReportFrequencyEvent);
      IFactoryOutboundEventTypeDescriptor typeDescriptor = null;

      try
      {
        typeDescriptor = _processor.SetDailyReportFrequencyEvent(device);
      }
      catch (UnknownDeviceException e)
      {
        Log.IfDebugFormat("{0} {1} UnknownDeviceException - {2}", GetType().Name, "GetSetDailyReportFrequencyEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (NotImplementedException e)
      {
        Log.IfDebugFormat("{0} {1} NotImplementedException - {2}", GetType().Name, "GetSetDailyReportFrequencyEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (Exception e)
      {
        Log.IfErrorFormat(e, "{0} {1} Exception", GetType().Name, "GetSetDailyReportFrequencyEvent");
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
      }

      return request.CreateResponse(HttpStatusCode.OK, typeDescriptor);
    }

    [ActionName(ActionConstants.DisableRapidReportingEvent)]
    public HttpResponseMessage GetDisableRapidReportingEvent(HttpRequestMessage request, [ModelBinder(typeof(DeviceQueryModelBinder))]IDeviceQuery device)
    {
      if (ModelState.ContainsKey(ModelBinderConstants.DeviceQueryModelBinderError))
      {
        return request.CreateResponse(HttpStatusCode.NotFound, ModelState[ModelBinderConstants.DeviceQueryModelBinderError].Errors[0].ErrorMessage);
      }

      Log.IfDebugFormat("{0} {1} processing {2} request", GetType().Name, "DisableRapidReportingEvent", ActionConstants.DisableRapidReportingEvent);
      IFactoryOutboundEventTypeDescriptor typeDescriptor = null;

      try
      {
        typeDescriptor = _processor.DisableRapidReportingEvent(device);
      }
      catch (UnknownDeviceException e)
      {
        Log.IfDebugFormat("{0} {1} UnknownDeviceException - {2}", GetType().Name, "DisableRapidReportingEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (NotImplementedException e)
      {
        Log.IfDebugFormat("{0} {1} NotImplementedException - {2}", GetType().Name, "DisableRapidReportingEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (Exception e)
      {
        Log.IfErrorFormat(e, "{0} {1} Exception", GetType().Name, "DisableRapidReportingEvent");
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
      }

      return request.CreateResponse(HttpStatusCode.OK, typeDescriptor);
    }

    [ActionName(ActionConstants.EnableRapidReportingEvent)]
    public HttpResponseMessage GetEnableRapidReportingEvent(HttpRequestMessage request, [ModelBinder(typeof(DeviceQueryModelBinder))]IDeviceQuery device)
    {
      if (ModelState.ContainsKey(ModelBinderConstants.DeviceQueryModelBinderError))
      {
        return request.CreateResponse(HttpStatusCode.NotFound, ModelState[ModelBinderConstants.DeviceQueryModelBinderError].Errors[0].ErrorMessage);
      }

      Log.IfDebugFormat("{0} {1} processing {2} request", GetType().Name, "EnableRapidReportingEvent", ActionConstants.EnableRapidReportingEvent);
      IFactoryOutboundEventTypeDescriptor typeDescriptor = null;

      try
      {
        typeDescriptor = _processor.EnableRapidReportingEvent(device);
      }
      catch (UnknownDeviceException e)
      {
        Log.IfDebugFormat("{0} {1} UnknownDeviceException - {2}", GetType().Name, "EnableRapidReportingEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (NotImplementedException e)
      {
        Log.IfDebugFormat("{0} {1} NotImplementedException - {2}", GetType().Name, "EnableRapidReportingEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (Exception e)
      {
        Log.IfErrorFormat(e, "{0} {1} Exception", GetType().Name, "EnableRapidReportingEvent");
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
      }

      return request.CreateResponse(HttpStatusCode.OK, typeDescriptor);
    }


    [ActionName(ActionConstants.ReportingFrequencyChangedEvent)]
    public HttpResponseMessage GetReportingFrequencyChangedEvent(HttpRequestMessage request, [ModelBinder(typeof(DeviceQueryModelBinder))]IDeviceQuery device)
    {
      if (ModelState.ContainsKey(ModelBinderConstants.DeviceQueryModelBinderError))
      {
        return request.CreateResponse(HttpStatusCode.NotFound, ModelState[ModelBinderConstants.DeviceQueryModelBinderError].Errors[0].ErrorMessage);
      }

      Log.IfDebugFormat("{0} {1} processing {2} request", GetType().Name, "GetReportingFrequencyChangedEvent", ActionConstants.ReportingFrequencyChangedEvent);
      IFactoryOutboundEventTypeDescriptor typeDescriptor = null;

      try
      {
        typeDescriptor = _processor.GetReportingFrequencyChangedEvent(device);
      }
      catch (UnknownDeviceException e)
      {
        Log.IfDebugFormat("{0} {1} UnknownDeviceException - {2}", GetType().Name, "GetReportingFrequencyChangedEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (NotImplementedException e)
      {
        Log.IfDebugFormat("{0} {1} NotImplementedException - {2}", GetType().Name, "GetReportingFrequencyChangedEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (Exception e)
      {
        Log.IfErrorFormat(e, "{0} {1} Exception", GetType().Name, "GetReportingFrequencyChangedEvent");
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
      }

      return request.CreateResponse(HttpStatusCode.OK, typeDescriptor);
    }
    
  }
}