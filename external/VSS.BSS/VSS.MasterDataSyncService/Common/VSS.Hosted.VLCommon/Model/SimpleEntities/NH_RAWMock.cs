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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using VSS.Hosted.VLCommon.NH_RAWMockObjectSet;

namespace VSS.Hosted.VLCommon
{
    /// <summary>
    /// The concrete mock context object that implements the context's interface.
    /// Provide an instance of this mock context class to client logic when testing, 
    /// instead of providing a functional context object.
    /// </summary>
    public partial class NH_RAWMock : INH_RAW
    {
        private bool _readOnlyCurrent = false;
        private Stack<bool> _readOnlyStack = new Stack<bool>();
    		public System.Data.Common.DbConnection Connection { get {return null;} }
        public int SaveChanges() 
        { 
          if (_readOnlyCurrent == true)
            throw new System.NotSupportedException("This ObjectContext cannot be used to change data on the database");
          return 1; 
        }
    
        public void SetReadOnlyness(bool readOnly)
        {
          _readOnlyStack.Push(_readOnlyCurrent);
          _readOnlyCurrent = readOnly;
        }
    
        public NH_RAWMock()
        {
        }
    
    	  ~NH_RAWMock()
        {
          Dispose();
        }
    
    
        #region IDisposable
    	  public void Dispose()
        {
            if (_readOnlyStack.Count > 0)
            {
              _readOnlyCurrent = _readOnlyStack.Pop();
            }
        }
        #endregion
    
        public IObjectSet<ECMAddressClaim> ECMAddressClaim
        {
            get { return _eCMAddressClaim  ?? (_eCMAddressClaim = new MockObjectSet<ECMAddressClaim>()); }
        }
        private IObjectSet<ECMAddressClaim> _eCMAddressClaim;
    
        public IObjectSet<ECMAddressClaim> ECMAddressClaimReadOnly{get { return ECMAddressClaim; }}
    
        public IObjectSet<GatewayServiceProvider> GatewayServiceProvider
        {
            get { return _gatewayServiceProvider  ?? (_gatewayServiceProvider = new MockObjectSet<GatewayServiceProvider>()); }
        }
        private IObjectSet<GatewayServiceProvider> _gatewayServiceProvider;
    
        public IObjectSet<GatewayServiceProvider> GatewayServiceProviderReadOnly{get { return GatewayServiceProvider; }}
    
        public IObjectSet<MTSBIT> MTSBIT
        {
            get { return _mTSBIT  ?? (_mTSBIT = new MockObjectSet<MTSBIT>()); }
        }
        private IObjectSet<MTSBIT> _mTSBIT;
    
        public IObjectSet<MTSBIT> MTSBITReadOnly{get { return MTSBIT; }}
    
        public IObjectSet<MTSDevice> MTSDevice
        {
            get { return _mTSDevice  ?? (_mTSDevice = new MockObjectSet<MTSDevice>()); }
        }
        private IObjectSet<MTSDevice> _mTSDevice;
    
        public IObjectSet<MTSDevice> MTSDeviceReadOnly{get { return MTSDevice; }}
    
        public IObjectSet<MTSMessage> MTSMessage
        {
            get { return _mTSMessage  ?? (_mTSMessage = new MockObjectSet<MTSMessage>()); }
        }
        private IObjectSet<MTSMessage> _mTSMessage;
    
        public IObjectSet<MTSMessage> MTSMessageReadOnly{get { return MTSMessage; }}
    
        public IObjectSet<MTSOut> MTSOut
        {
            get { return _mTSOut  ?? (_mTSOut = new MockObjectSet<MTSOut>()); }
        }
        private IObjectSet<MTSOut> _mTSOut;
    
        public IObjectSet<MTSOut> MTSOutReadOnly{get { return MTSOut; }}
    
        public IObjectSet<MTSPortBasedMessages> MTSPortBasedMessages
        {
            get { return _mTSPortBasedMessages  ?? (_mTSPortBasedMessages = new MockObjectSet<MTSPortBasedMessages>()); }
        }
        private IObjectSet<MTSPortBasedMessages> _mTSPortBasedMessages;
    
        public IObjectSet<MTSPortBasedMessages> MTSPortBasedMessagesReadOnly{get { return MTSPortBasedMessages; }}
    
        public IObjectSet<PLDevice> PLDevice
        {
            get { return _pLDevice  ?? (_pLDevice = new MockObjectSet<PLDevice>()); }
        }
        private IObjectSet<PLDevice> _pLDevice;
    
        public IObjectSet<PLDevice> PLDeviceReadOnly{get { return PLDevice; }}
    
        public IObjectSet<PLMessage> PLMessage
        {
            get { return _pLMessage  ?? (_pLMessage = new MockObjectSet<PLMessage>()); }
        }
        private IObjectSet<PLMessage> _pLMessage;
    
        public IObjectSet<PLMessage> PLMessageReadOnly{get { return PLMessage; }}
    
        public IObjectSet<PLOut> PLOut
        {
            get { return _pLOut  ?? (_pLOut = new MockObjectSet<PLOut>()); }
        }
        private IObjectSet<PLOut> _pLOut;
    
        public IObjectSet<PLOut> PLOutReadOnly{get { return PLOut; }}
    
        public IObjectSet<RuntimeCalibration> RuntimeCalibration
        {
            get { return _runtimeCalibration  ?? (_runtimeCalibration = new MockObjectSet<RuntimeCalibration>()); }
        }
        private IObjectSet<RuntimeCalibration> _runtimeCalibration;
    
        public IObjectSet<RuntimeCalibration> RuntimeCalibrationReadOnly{get { return RuntimeCalibration; }}
    
        public IObjectSet<Sequence> Sequence
        {
            get { return _sequence  ?? (_sequence = new MockObjectSet<Sequence>()); }
        }
        private IObjectSet<Sequence> _sequence;
    
        public IObjectSet<Sequence> SequenceReadOnly{get { return Sequence; }}
    
        public IObjectSet<SFD_MTSMessage> SFD_MTSMessage
        {
            get { return _sFD_MTSMessage  ?? (_sFD_MTSMessage = new MockObjectSet<SFD_MTSMessage>()); }
        }
        private IObjectSet<SFD_MTSMessage> _sFD_MTSMessage;
    
        public IObjectSet<SFD_MTSMessage> SFD_MTSMessageReadOnly{get { return SFD_MTSMessage; }}
    
        public IObjectSet<TTDevice> TTDevice
        {
            get { return _tTDevice  ?? (_tTDevice = new MockObjectSet<TTDevice>()); }
        }
        private IObjectSet<TTDevice> _tTDevice;
    
        public IObjectSet<TTDevice> TTDeviceReadOnly{get { return TTDevice; }}
    
        public IObjectSet<TTMessage> TTMessage
        {
            get { return _tTMessage  ?? (_tTMessage = new MockObjectSet<TTMessage>()); }
        }
        private IObjectSet<TTMessage> _tTMessage;
    
        public IObjectSet<TTMessage> TTMessageReadOnly{get { return TTMessage; }}
    
        public IObjectSet<TTOut> TTOut
        {
            get { return _tTOut  ?? (_tTOut = new MockObjectSet<TTOut>()); }
        }
        private IObjectSet<TTOut> _tTOut;
    
        public IObjectSet<TTOut> TTOutReadOnly{get { return TTOut; }}
    
        public IObjectSet<UnitType> UnitType
        {
            get { return _unitType  ?? (_unitType = new MockObjectSet<UnitType>()); }
        }
        private IObjectSet<UnitType> _unitType;
    
        public IObjectSet<UnitType> UnitTypeReadOnly{get { return UnitType; }}
    
        public IObjectSet<J1939ParameterParser> J1939ParameterParser
        {
            get { return _j1939ParameterParser  ?? (_j1939ParameterParser = new MockObjectSet<J1939ParameterParser>()); }
        }
        private IObjectSet<J1939ParameterParser> _j1939ParameterParser;
    
        public IObjectSet<J1939ParameterParser> J1939ParameterParserReadOnly{get { return J1939ParameterParser; }}
    
    }
}
