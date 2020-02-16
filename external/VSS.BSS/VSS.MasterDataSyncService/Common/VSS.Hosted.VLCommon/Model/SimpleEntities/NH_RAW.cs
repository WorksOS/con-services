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
using System.Transactions;

namespace VSS.Hosted.VLCommon
{
    /// <summary>
    /// The functional concrete object context. This is just like the normal
    /// context that would be generated using the POCO artefact generator, 
    /// apart from the fact that this one implements an interface containing 
    /// the entity set properties and exposes <code>IObjectSet</code>
    /// instances for entity set properties.
    ///
    /// The context can be constructed as "read-only", in which case SNAPSHOT IsolationLevel is used for queries
    /// and calls to SaveChanges are not supported.
    /// </summary>
    public partial class NH_RAW : ObjectContext, INH_RAW 
    {
        public const string ConnectionString = "name=NH_RAW";
        public const string ContainerName = "NH_RAW";
    
        private bool _readOnly = false;
    
        #region Constructors
    
        public NH_RAW(bool readOnly = false):
            base(ConnectionString, ContainerName)
        {
            this.ContextOptions.LazyLoadingEnabled = true;
        	  _readOnly = readOnly;
    
        }
    
        public NH_RAW(string connectionString, bool readOnly = false):
            base(connectionString, ContainerName)
        {
            this.ContextOptions.LazyLoadingEnabled = true;
        	  _readOnly = readOnly;
    
        }
    
        public NH_RAW(EntityConnection connection,bool readOnly = false):
            base(connection, ContainerName)
        {
            this.ContextOptions.LazyLoadingEnabled = true;
        	  _readOnly = readOnly;
    
        }
    
    	~NH_RAW()
        {
          Dispose();
        }
    
        #endregion
    
    	#region IDisposable
    	new public void Dispose()
        {
          base.Dispose();
        }
    
        protected override void Dispose(bool disposing)
        {
          base.Dispose(disposing);
        }
    
        #endregion
    
        #region SaveChanges
    
    	public override int SaveChanges(SaveOptions options)
    	{
    	  if ( _readOnly )
    	    throw new System.NotSupportedException("This ObjectContext cannot be used to change data on the database");
    
        return base.SaveChanges(options);
    	}
    
        #endregion
    
        #region ObjectSet Properties
    
        public IObjectSet<ECMAddressClaim> ECMAddressClaim
        {
            get { return _eCMAddressClaim ?? (_eCMAddressClaim = CreateObjectSet<ECMAddressClaim>("ECMAddressClaim")); }
        }
        private ObjectSet<ECMAddressClaim> _eCMAddressClaim;
        public IObjectSet<ECMAddressClaim> ECMAddressClaimReadOnly
        {
            get 
            { 
              if (null == _eCMAddressClaimRO)
              {
                _eCMAddressClaimRO = CreateObjectSet<ECMAddressClaim>("ECMAddressClaim"); 
                _eCMAddressClaimRO.MergeOption=MergeOption.NoTracking;
              }
              return _eCMAddressClaimRO;
            }
        }
        private ObjectSet<ECMAddressClaim> _eCMAddressClaimRO;
    
        public IObjectSet<GatewayServiceProvider> GatewayServiceProvider
        {
            get { return _gatewayServiceProvider ?? (_gatewayServiceProvider = CreateObjectSet<GatewayServiceProvider>("GatewayServiceProvider")); }
        }
        private ObjectSet<GatewayServiceProvider> _gatewayServiceProvider;
        public IObjectSet<GatewayServiceProvider> GatewayServiceProviderReadOnly
        {
            get 
            { 
              if (null == _gatewayServiceProviderRO)
              {
                _gatewayServiceProviderRO = CreateObjectSet<GatewayServiceProvider>("GatewayServiceProvider"); 
                _gatewayServiceProviderRO.MergeOption=MergeOption.NoTracking;
              }
              return _gatewayServiceProviderRO;
            }
        }
        private ObjectSet<GatewayServiceProvider> _gatewayServiceProviderRO;
    
        public IObjectSet<MTSBIT> MTSBIT
        {
            get { return _mTSBIT ?? (_mTSBIT = CreateObjectSet<MTSBIT>("MTSBIT")); }
        }
        private ObjectSet<MTSBIT> _mTSBIT;
        public IObjectSet<MTSBIT> MTSBITReadOnly
        {
            get 
            { 
              if (null == _mTSBITRO)
              {
                _mTSBITRO = CreateObjectSet<MTSBIT>("MTSBIT"); 
                _mTSBITRO.MergeOption=MergeOption.NoTracking;
              }
              return _mTSBITRO;
            }
        }
        private ObjectSet<MTSBIT> _mTSBITRO;
    
        public IObjectSet<MTSDevice> MTSDevice
        {
            get { return _mTSDevice ?? (_mTSDevice = CreateObjectSet<MTSDevice>("MTSDevice")); }
        }
        private ObjectSet<MTSDevice> _mTSDevice;
        public IObjectSet<MTSDevice> MTSDeviceReadOnly
        {
            get 
            { 
              if (null == _mTSDeviceRO)
              {
                _mTSDeviceRO = CreateObjectSet<MTSDevice>("MTSDevice"); 
                _mTSDeviceRO.MergeOption=MergeOption.NoTracking;
              }
              return _mTSDeviceRO;
            }
        }
        private ObjectSet<MTSDevice> _mTSDeviceRO;
    
        public IObjectSet<MTSMessage> MTSMessage
        {
            get { return _mTSMessage ?? (_mTSMessage = CreateObjectSet<MTSMessage>("MTSMessage")); }
        }
        private ObjectSet<MTSMessage> _mTSMessage;
        public IObjectSet<MTSMessage> MTSMessageReadOnly
        {
            get 
            { 
              if (null == _mTSMessageRO)
              {
                _mTSMessageRO = CreateObjectSet<MTSMessage>("MTSMessage"); 
                _mTSMessageRO.MergeOption=MergeOption.NoTracking;
              }
              return _mTSMessageRO;
            }
        }
        private ObjectSet<MTSMessage> _mTSMessageRO;
    
        public IObjectSet<MTSOut> MTSOut
        {
            get { return _mTSOut ?? (_mTSOut = CreateObjectSet<MTSOut>("MTSOut")); }
        }
        private ObjectSet<MTSOut> _mTSOut;
        public IObjectSet<MTSOut> MTSOutReadOnly
        {
            get 
            { 
              if (null == _mTSOutRO)
              {
                _mTSOutRO = CreateObjectSet<MTSOut>("MTSOut"); 
                _mTSOutRO.MergeOption=MergeOption.NoTracking;
              }
              return _mTSOutRO;
            }
        }
        private ObjectSet<MTSOut> _mTSOutRO;
    
        public IObjectSet<MTSPortBasedMessages> MTSPortBasedMessages
        {
            get { return _mTSPortBasedMessages ?? (_mTSPortBasedMessages = CreateObjectSet<MTSPortBasedMessages>("MTSPortBasedMessages")); }
        }
        private ObjectSet<MTSPortBasedMessages> _mTSPortBasedMessages;
        public IObjectSet<MTSPortBasedMessages> MTSPortBasedMessagesReadOnly
        {
            get 
            { 
              if (null == _mTSPortBasedMessagesRO)
              {
                _mTSPortBasedMessagesRO = CreateObjectSet<MTSPortBasedMessages>("MTSPortBasedMessages"); 
                _mTSPortBasedMessagesRO.MergeOption=MergeOption.NoTracking;
              }
              return _mTSPortBasedMessagesRO;
            }
        }
        private ObjectSet<MTSPortBasedMessages> _mTSPortBasedMessagesRO;
    
        public IObjectSet<PLDevice> PLDevice
        {
            get { return _pLDevice ?? (_pLDevice = CreateObjectSet<PLDevice>("PLDevice")); }
        }
        private ObjectSet<PLDevice> _pLDevice;
        public IObjectSet<PLDevice> PLDeviceReadOnly
        {
            get 
            { 
              if (null == _pLDeviceRO)
              {
                _pLDeviceRO = CreateObjectSet<PLDevice>("PLDevice"); 
                _pLDeviceRO.MergeOption=MergeOption.NoTracking;
              }
              return _pLDeviceRO;
            }
        }
        private ObjectSet<PLDevice> _pLDeviceRO;
    
        public IObjectSet<PLMessage> PLMessage
        {
            get { return _pLMessage ?? (_pLMessage = CreateObjectSet<PLMessage>("PLMessage")); }
        }
        private ObjectSet<PLMessage> _pLMessage;
        public IObjectSet<PLMessage> PLMessageReadOnly
        {
            get 
            { 
              if (null == _pLMessageRO)
              {
                _pLMessageRO = CreateObjectSet<PLMessage>("PLMessage"); 
                _pLMessageRO.MergeOption=MergeOption.NoTracking;
              }
              return _pLMessageRO;
            }
        }
        private ObjectSet<PLMessage> _pLMessageRO;
    
        public IObjectSet<PLOut> PLOut
        {
            get { return _pLOut ?? (_pLOut = CreateObjectSet<PLOut>("PLOut")); }
        }
        private ObjectSet<PLOut> _pLOut;
        public IObjectSet<PLOut> PLOutReadOnly
        {
            get 
            { 
              if (null == _pLOutRO)
              {
                _pLOutRO = CreateObjectSet<PLOut>("PLOut"); 
                _pLOutRO.MergeOption=MergeOption.NoTracking;
              }
              return _pLOutRO;
            }
        }
        private ObjectSet<PLOut> _pLOutRO;
    
        public IObjectSet<RuntimeCalibration> RuntimeCalibration
        {
            get { return _runtimeCalibration ?? (_runtimeCalibration = CreateObjectSet<RuntimeCalibration>("RuntimeCalibration")); }
        }
        private ObjectSet<RuntimeCalibration> _runtimeCalibration;
        public IObjectSet<RuntimeCalibration> RuntimeCalibrationReadOnly
        {
            get 
            { 
              if (null == _runtimeCalibrationRO)
              {
                _runtimeCalibrationRO = CreateObjectSet<RuntimeCalibration>("RuntimeCalibration"); 
                _runtimeCalibrationRO.MergeOption=MergeOption.NoTracking;
              }
              return _runtimeCalibrationRO;
            }
        }
        private ObjectSet<RuntimeCalibration> _runtimeCalibrationRO;
    
        public IObjectSet<Sequence> Sequence
        {
            get { return _sequence ?? (_sequence = CreateObjectSet<Sequence>("Sequence")); }
        }
        private ObjectSet<Sequence> _sequence;
        public IObjectSet<Sequence> SequenceReadOnly
        {
            get 
            { 
              if (null == _sequenceRO)
              {
                _sequenceRO = CreateObjectSet<Sequence>("Sequence"); 
                _sequenceRO.MergeOption=MergeOption.NoTracking;
              }
              return _sequenceRO;
            }
        }
        private ObjectSet<Sequence> _sequenceRO;
    
        public IObjectSet<SFD_MTSMessage> SFD_MTSMessage
        {
            get { return _sFD_MTSMessage ?? (_sFD_MTSMessage = CreateObjectSet<SFD_MTSMessage>("SFD_MTSMessage")); }
        }
        private ObjectSet<SFD_MTSMessage> _sFD_MTSMessage;
        public IObjectSet<SFD_MTSMessage> SFD_MTSMessageReadOnly
        {
            get 
            { 
              if (null == _sFD_MTSMessageRO)
              {
                _sFD_MTSMessageRO = CreateObjectSet<SFD_MTSMessage>("SFD_MTSMessage"); 
                _sFD_MTSMessageRO.MergeOption=MergeOption.NoTracking;
              }
              return _sFD_MTSMessageRO;
            }
        }
        private ObjectSet<SFD_MTSMessage> _sFD_MTSMessageRO;
    
        public IObjectSet<TTDevice> TTDevice
        {
            get { return _tTDevice ?? (_tTDevice = CreateObjectSet<TTDevice>("TTDevice")); }
        }
        private ObjectSet<TTDevice> _tTDevice;
        public IObjectSet<TTDevice> TTDeviceReadOnly
        {
            get 
            { 
              if (null == _tTDeviceRO)
              {
                _tTDeviceRO = CreateObjectSet<TTDevice>("TTDevice"); 
                _tTDeviceRO.MergeOption=MergeOption.NoTracking;
              }
              return _tTDeviceRO;
            }
        }
        private ObjectSet<TTDevice> _tTDeviceRO;
    
        public IObjectSet<TTMessage> TTMessage
        {
            get { return _tTMessage ?? (_tTMessage = CreateObjectSet<TTMessage>("TTMessage")); }
        }
        private ObjectSet<TTMessage> _tTMessage;
        public IObjectSet<TTMessage> TTMessageReadOnly
        {
            get 
            { 
              if (null == _tTMessageRO)
              {
                _tTMessageRO = CreateObjectSet<TTMessage>("TTMessage"); 
                _tTMessageRO.MergeOption=MergeOption.NoTracking;
              }
              return _tTMessageRO;
            }
        }
        private ObjectSet<TTMessage> _tTMessageRO;
    
        public IObjectSet<TTOut> TTOut
        {
            get { return _tTOut ?? (_tTOut = CreateObjectSet<TTOut>("TTOut")); }
        }
        private ObjectSet<TTOut> _tTOut;
        public IObjectSet<TTOut> TTOutReadOnly
        {
            get 
            { 
              if (null == _tTOutRO)
              {
                _tTOutRO = CreateObjectSet<TTOut>("TTOut"); 
                _tTOutRO.MergeOption=MergeOption.NoTracking;
              }
              return _tTOutRO;
            }
        }
        private ObjectSet<TTOut> _tTOutRO;
    
        public IObjectSet<UnitType> UnitType
        {
            get { return _unitType ?? (_unitType = CreateObjectSet<UnitType>("UnitType")); }
        }
        private ObjectSet<UnitType> _unitType;
        public IObjectSet<UnitType> UnitTypeReadOnly
        {
            get 
            { 
              if (null == _unitTypeRO)
              {
                _unitTypeRO = CreateObjectSet<UnitType>("UnitType"); 
                _unitTypeRO.MergeOption=MergeOption.NoTracking;
              }
              return _unitTypeRO;
            }
        }
        private ObjectSet<UnitType> _unitTypeRO;
    
        public IObjectSet<J1939ParameterParser> J1939ParameterParser
        {
            get { return _j1939ParameterParser ?? (_j1939ParameterParser = CreateObjectSet<J1939ParameterParser>("J1939ParameterParser")); }
        }
        private ObjectSet<J1939ParameterParser> _j1939ParameterParser;
        public IObjectSet<J1939ParameterParser> J1939ParameterParserReadOnly
        {
            get 
            { 
              if (null == _j1939ParameterParserRO)
              {
                _j1939ParameterParserRO = CreateObjectSet<J1939ParameterParser>("J1939ParameterParser"); 
                _j1939ParameterParserRO.MergeOption=MergeOption.NoTracking;
              }
              return _j1939ParameterParserRO;
            }
        }
        private ObjectSet<J1939ParameterParser> _j1939ParameterParserRO;

        #endregion

    }
}
