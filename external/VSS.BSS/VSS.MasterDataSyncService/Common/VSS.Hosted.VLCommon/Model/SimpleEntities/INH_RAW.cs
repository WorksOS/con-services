//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
// Architectural overview and usage guide: 
// http://blogofrab.blogspot.com/2010/08/maintenance-free-mocking-for-unit.html
//------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;

namespace VSS.Hosted.VLCommon
{
    /// <summary>
    /// The interface for the specialised object context. This contains all of
    /// the <code>ObjectSet</code> properties that are implemented in both the
    /// functional context class and the mock context class.
    /// </summary>
    public interface INH_RAW : System.IDisposable
    {
        IObjectSet<ECMAddressClaim> ECMAddressClaim { get; }
        IObjectSet<ECMAddressClaim> ECMAddressClaimReadOnly { get; }
        IObjectSet<GatewayServiceProvider> GatewayServiceProvider { get; }
        IObjectSet<GatewayServiceProvider> GatewayServiceProviderReadOnly { get; }
        IObjectSet<MTSBIT> MTSBIT { get; }
        IObjectSet<MTSBIT> MTSBITReadOnly { get; }
        IObjectSet<MTSDevice> MTSDevice { get; }
        IObjectSet<MTSDevice> MTSDeviceReadOnly { get; }
        IObjectSet<MTSMessage> MTSMessage { get; }
        IObjectSet<MTSMessage> MTSMessageReadOnly { get; }
        IObjectSet<MTSOut> MTSOut { get; }
        IObjectSet<MTSOut> MTSOutReadOnly { get; }
        IObjectSet<MTSPortBasedMessages> MTSPortBasedMessages { get; }
        IObjectSet<MTSPortBasedMessages> MTSPortBasedMessagesReadOnly { get; }
        IObjectSet<PLDevice> PLDevice { get; }
        IObjectSet<PLDevice> PLDeviceReadOnly { get; }
        IObjectSet<PLMessage> PLMessage { get; }
        IObjectSet<PLMessage> PLMessageReadOnly { get; }
        IObjectSet<PLOut> PLOut { get; }
        IObjectSet<PLOut> PLOutReadOnly { get; }
        IObjectSet<RuntimeCalibration> RuntimeCalibration { get; }
        IObjectSet<RuntimeCalibration> RuntimeCalibrationReadOnly { get; }
        IObjectSet<Sequence> Sequence { get; }
        IObjectSet<Sequence> SequenceReadOnly { get; }
        IObjectSet<SFD_MTSMessage> SFD_MTSMessage { get; }
        IObjectSet<SFD_MTSMessage> SFD_MTSMessageReadOnly { get; }
        IObjectSet<TTDevice> TTDevice { get; }
        IObjectSet<TTDevice> TTDeviceReadOnly { get; }
        IObjectSet<TTMessage> TTMessage { get; }
        IObjectSet<TTMessage> TTMessageReadOnly { get; }
        IObjectSet<TTOut> TTOut { get; }
        IObjectSet<TTOut> TTOutReadOnly { get; }
        IObjectSet<UnitType> UnitType { get; }
        IObjectSet<UnitType> UnitTypeReadOnly { get; }
        IObjectSet<J1939ParameterParser> J1939ParameterParser { get; }
        IObjectSet<J1939ParameterParser> J1939ParameterParserReadOnly { get; }
      System.Data.Common.DbConnection Connection { get; }
    	int SaveChanges();
    }
}
