
using log4net;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Reflection;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.MTSMessages;
using VSS.Nighthawk.NHDataSvc.Interfaces.Events;


namespace VSS.Nighthawk.NHDataSvc.Consumers
{
  public class RuntimeAdjustmentCreatedEventConsumer : Consumes<IRuntimeAdjustmentCreatedEvent>.Context
  {
    protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly INHDataProcessor _nhDataProcessor;

    public RuntimeAdjustmentCreatedEventConsumer(INHDataProcessor nhData)
    {
      _nhDataProcessor = nhData;
    }
    public void Consume(IConsumeContext<IRuntimeAdjustmentCreatedEvent> message)
    {
      if (message == null)
      {
        Log.IfError("Received empty context");
      }
      else
      {
        try
        {
          var adjustment = new DataServiceMeterAdjustment
          {
            RuntimeBeforeHours = message.Message.RuntimeBeforeHours,
            RuntimeAfterHours = message.Message.RuntimeAfterHours,
            GPSDeviceID = message.Message.GpsDeviceID,
            DeviceType = message.Message.DeviceType,
            AssetID = message.Message.AssetID ?? 0,
            EventUTC = message.Message.EventUTC,
            SourceMsgID = message.Message.SourceMsgID,
            DebugRefID = message.Message.DebugRefID,
            fk_DimSourceID = (int)message.Message.Source
          };
          AdjustRuntime(message.Message.GpsDeviceID, message.Message.DeviceType, message.Message.Id, message.Message.RuntimeAfterHours);
          _nhDataProcessor.Process(new List<NHDataWrapper> { new NHDataWrapper { Data = adjustment } });
        }
        catch (Exception e)
        {
          Log.IfError("Unexpected Error Processing message from RabbitMQ", e);
          message.RetryLater();
        }
      }

    }

    private void AdjustRuntime(string serialNumber, DeviceTypeEnum deviceType,
                                long? runtimeID, double? smhAfterCalibration)
    {
      if (runtimeID.HasValue && smhAfterCalibration.HasValue)
      {
        MTSUpdateDeviceConfig.UpdateRuntimeCalibration(serialNumber, deviceType,
                                                       smhAfterCalibration.Value,
                                                       runtimeID.Value);
      }
    }
  }
}
