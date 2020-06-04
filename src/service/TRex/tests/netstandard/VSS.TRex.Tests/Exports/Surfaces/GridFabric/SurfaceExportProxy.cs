using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Tests.TestFixtures;

namespace VSS.TRex.Tests.Exports.Surfaces.GridFabric
{
  public class SurfaceExportProxy : DITAGFileAndSubGridRequestsWithIgniteFixture
  {
    public SurfaceExportProxy()
    {
      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.UploadToBucket(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()));

      var mockTransferProxyFactory = new Mock<ITransferProxyFactory>();
      mockTransferProxyFactory.Setup(x => x.NewProxy(It.IsAny<TransferProxyType>())).Returns(mockTransferProxy.Object);

      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton(mockTransferProxyFactory.Object))
        .Complete();
    }
  }
}
