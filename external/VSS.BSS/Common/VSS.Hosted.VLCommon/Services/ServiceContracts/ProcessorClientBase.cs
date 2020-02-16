using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using log4net;
using VSS.Hosted.VLCommon;
using System.Net.Security;
using System.Configuration;

namespace VSS.Hosted.VLCommon.ServiceContracts
{
  public class ProcessorClientBase
  {
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);
    protected static string remoteNHDataIPs = ConfigurationManager.AppSettings["RemoteNhDataIPs"];

    protected T GetProxy<T>(TransportType transport, bool useLargeTCPBinding = false)
    {
      ChannelFactory<T> channel = null;
      switch (transport.bindingType)
      {
        case BindingType.NamedPipe:
          log.IfDebugFormat("Using the Pipe Channel...");
          channel = CreateChannel<T>(transport.channel as ChannelFactory<T>, GetPipeBinding(), transport.Address);
          break;
        case BindingType.MSMQ:
          log.IfDebugFormat("Using the MSMQ Channel...");
          channel = CreateChannel<T>(transport.channel as ChannelFactory<T>, GetMSMQBinding(), transport.Address);
          break;
        case BindingType.TCP:
          log.IfDebugFormat("Using the TCP Channel...");
          NetTcpBinding binding = useLargeTCPBinding ? GetLargeTCPBinding() : GetTCPBinding();
          channel = CreateChannel<T>(transport.channel as ChannelFactory<T>, binding, transport.Address);
          break;
      }
      return channel.CreateChannel();
    }

    protected ChannelFactory<T> CreateChannel<T>(ChannelFactory<T> channel, Binding binding, string address)
    {
      if (channel == null)
      {
        log.IfDebug("Creating new Channel...");
        channel = new ChannelFactory<T>(binding, address);
        foreach (OperationDescription operation in channel.Endpoint.Contract.Operations)
          operation.Behaviors.Find<DataContractSerializerOperationBehavior>().MaxItemsInObjectGraph = 4194304;
      }

      return channel;
    }

    #region Binding Creation

    protected NetNamedPipeBinding GetPipeBinding()
    {
      NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
      binding.CloseTimeout = TimeSpan.FromMinutes(1);
      binding.OpenTimeout = TimeSpan.FromSeconds(15);
      binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
      binding.SendTimeout = TimeSpan.FromMinutes(1);
      binding.TransactionFlow = false;
      binding.TransactionProtocol = TransactionProtocol.OleTransactions;
      binding.TransferMode = TransferMode.Buffered;
      binding.Security.Transport.ProtectionLevel = ProtectionLevel.None;
      binding.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
      binding.MaxBufferPoolSize = 524288;
      binding.MaxBufferSize = 4194304;
      binding.MaxConnections = 500;
      binding.MaxReceivedMessageSize = 65536;

      return binding;
    }

    protected NetMsmqBinding GetMSMQBinding()
    {
      NetMsmqBinding binding = new NetMsmqBinding(NetMsmqSecurityMode.None);
      binding.CloseTimeout = TimeSpan.FromMinutes(1);
      binding.OpenTimeout = TimeSpan.FromMinutes(1);
      binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
      binding.SendTimeout = TimeSpan.FromMinutes(1);
      binding.MaxBufferPoolSize = 524288;
      binding.MaxReceivedMessageSize = 65536;

      return binding;
    }

    protected NetTcpBinding GetTCPBinding()
    {
      NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
      binding.PortSharingEnabled = true;
      return binding;
    }

    protected NetTcpBinding GetLargeTCPBinding()
    {
      NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
      binding.PortSharingEnabled = true;
      binding.MaxReceivedMessageSize = 2147483647;
      binding.MaxBufferSize = 2147483647;
      binding.MaxBufferPoolSize = 2147483647;
      binding.ReaderQuotas.MaxArrayLength = 2147483647;
      binding.ReaderQuotas.MaxBytesPerRead = 2147483647;
      binding.ReaderQuotas.MaxStringContentLength = 2147483647;
      return binding;
    }

    #endregion

    #region Helper Classes
    
    protected enum BindingType
    {
      NamedPipe = 0,
      MSMQ = 1,
      TCP = 2,
    }

    protected class TransportType
    {
      private static readonly int msmqMaxMessages = 300;
      private static readonly int pipeMaxMessages = 500;
      public BindingType bindingType;
      public string Address;
      public ChannelFactory channel { get; set; }

      public int MaxMessages
      {
        get { return bindingType == BindingType.MSMQ ? msmqMaxMessages : pipeMaxMessages; }
      }
    }

    #endregion
  }
}
