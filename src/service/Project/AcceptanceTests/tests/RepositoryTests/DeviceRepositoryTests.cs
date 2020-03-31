using System;
using RepositoryTests.Internal;
using VSS.Productivity3D.Project.Repository;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;
using Xunit;

namespace RepositoryTests
{
  public class DeviceRepositoryTests : TestControllerBase
  {
    DeviceRepository deviceRepository;

    public DeviceRepositoryTests()
    {
      SetupLogging();
      deviceRepository = new DeviceRepository(configStore, loggerFactory);
    }

    /// <summary>
    /// Happy path i.e. snm940 device doesn't exist already.
    /// </summary>
    [Fact]
    public void CreateDevice_HappyPath()
    {
      deviceRepository.InRollbackTransactionAsync<object>(async o =>
      {
        var firstCreatedUTC = new DateTime(2015, 1, 1, 2, 30, 3);
        var deviceEvent = new CreateDeviceEvent()
        {
          DeviceUID = Guid.NewGuid().ToString(),
          ActionUTC = firstCreatedUTC
        };

        var g = await deviceRepository.GetDevice(deviceEvent.DeviceUID);
        Assert.Null(g);

        var s = await deviceRepository.StoreEvent(deviceEvent);
        Assert.Equal(1, s);

        g = await deviceRepository.GetDevice(deviceEvent.DeviceUID);
        Assert.NotNull(g);
        Assert.Equal(deviceEvent.DeviceUID, g.DeviceUID);
        Assert.True(g.ShortRaptorAssetID > 0);
        Assert.Equal(deviceEvent.ActionUTC, g.LastActionedUTC);
        return null;
      }).Wait();
    }

    /// <summary>
    /// Happy path create 2 and check that shortRaptorId increments
    /// </summary>
    [Fact]
    public void Create2Devices_HappyPath()
    {
      deviceRepository.InRollbackTransactionAsync<object>(async o =>
      {
        var firstCreatedUTC = new DateTime(2015, 1, 1, 2, 30, 3);
        var deviceEvent = new CreateDeviceEvent()
        {
          DeviceUID = Guid.NewGuid().ToString(),
          ActionUTC = firstCreatedUTC
        };

        var deviceEvent2 = new CreateDeviceEvent()
        {
          DeviceUID = Guid.NewGuid().ToString(),
          ActionUTC = firstCreatedUTC.AddDays(1)
        };

        await deviceRepository.StoreEvent(deviceEvent);
        await deviceRepository.StoreEvent(deviceEvent2);

        var firstDevice = await deviceRepository.GetDevice(deviceEvent.DeviceUID);
        Assert.Equal(deviceEvent.DeviceUID, firstDevice.DeviceUID);
        Assert.True(firstDevice.ShortRaptorAssetID > 0);

        var secondDevice = await deviceRepository.GetDevice(deviceEvent2.DeviceUID);
        Assert.Equal(deviceEvent2.DeviceUID, secondDevice.DeviceUID);
        Assert.True(secondDevice.ShortRaptorAssetID > firstDevice.ShortRaptorAssetID);

        return null;
      }).Wait();
    }
  }

}

