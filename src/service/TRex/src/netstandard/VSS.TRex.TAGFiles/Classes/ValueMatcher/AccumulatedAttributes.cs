using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher
{
    /// <summary>
    /// AccumulatedAttributes stores a list of AccumulatedAttribute instances
    /// the record a series of attribute value observations
    /// </summary>
    public class AccumulatedAttributes
    {
        /// <summary>
        /// List of items being tracked. This list is managed with the aid of the NumAttrs field to
        /// remove the need to resize the attributes list frequently when all but the latest attributes are
        /// discarded.
        /// </summary>
        private List<AccumulatedAttribute> list = new List<AccumulatedAttribute>();

        public AccumulatedAttributes()
        {
        }

        /// <summary>
        /// Provides the count of the internal list of attributes being maintained
        /// </summary>
        /// <returns></returns>
        public int NumAttrs { get; set; }

        // DiscardAllButLatest discards all but the most recently added
        // value from the list;
        public void DiscardAllButLatest()
        {
            if (NumAttrs > 1)
            {
                list[0] = list[NumAttrs - 1];
                NumAttrs = 1;
            }
        }

        /// <summary>
        /// Add adds a value recorded at DateTime to the list. It is assumed
        /// that the date time of this item is after the date time
        /// of any previously added item in the list
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="value"></param>
        public void Add(DateTime dateTime, object value)
        {
          // If there are available entries to reuse, then reuse them...
          if (NumAttrs < list.Count)
            list[NumAttrs - 1] = new AccumulatedAttribute(dateTime, value);
          else
            list.Add(new AccumulatedAttribute(dateTime, value));

          NumAttrs++;
        }

        // GetLatest simply returns the last value in the list
        public object GetLatest()
        {
            Debug.Assert(NumAttrs > 0, "NumAttrs is zero in GetLatest");

            return list[NumAttrs - 1].value;
        }

        // GetValueAtDateTime locates the value appropriate for the given datetime
        // Requesting a value for a datetime prior to the first value in the list
        // will return the first value in the list. If there are no values in the list
        // then the function returns false.
        public bool GetValueAtDateTime(DateTime dateTime, out object value)
        {
            value = null;

            switch (NumAttrs)
            {
                case 0:
                    return false;
                case 1:
                    value = list[0].value;
                    return true;
            }

            if (dateTime < list[0].dateTime)
            {
                value = list[0].value;
                return true;
            }

            if (dateTime >= list[NumAttrs - 1].dateTime)
            {
                value = list[NumAttrs - 1].value;
                return true;
            }

            for (int I = 0; I < NumAttrs - 1; I++)
            {
                if (dateTime >= list[I].dateTime && dateTime < list[I + 1].dateTime)
                {
                    value = list[I].value;
                    return true;
                }
            }
            
            // It should not be possible to get here...
            return false;                     
        }

        /// <summary>
        /// Gets the appropriate GPSMode given a UTC datetime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public GPSMode GetGPSModeAtDateTime(DateTime dateTime)
        {
            return GetValueAtDateTime(dateTime, out object value) ? (GPSMode)value : CellPassConsts.NullGPSMode;
        }

        /// <summary>
        /// Gets the appropriate CCV value given a UTC datetime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public short GetCCVValueAtDateTime(DateTime dateTime)
        {
            return GetValueAtDateTime(dateTime, out object value) ? (short)value : CellPassConsts.NullCCV;
        }

        /// <summary>
        /// Gets the appropriate RMV value given a UTC datetime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>        
        public short GetRMVValueAtDateTime(DateTime dateTime)
        {
            return GetValueAtDateTime(dateTime, out object value) ? (short)value : CellPassConsts.NullRMV;
        }

        /// <summary>
        /// Gets the appropriate vibration frequency value given a UTC datetime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>   
        public ushort GetFrequencyValueAtDateTime(DateTime dateTime)
        {
            return GetValueAtDateTime(dateTime, out object value) ? (ushort)value : CellPassConsts.NullFrequency;
        }

        /// <summary>
        /// Gets the appropriate vibration amplitude value given a UTC datetime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>   
        public ushort GetAmplitudeValueAtDateTime(DateTime dateTime)
        {
            return GetValueAtDateTime(dateTime, out object value) ? (ushort)value : CellPassConsts.NullAmplitude;
        }

        /// <summary>
        /// Gets the appropriate correction age given a UTC datetime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public byte GetAgeOfCorrectionValueAtDateTime(DateTime dateTime)
        {
            return GetValueAtDateTime(dateTime, out object value) ? (byte)value : (byte)0;
        }

        /// <summary>
        /// Gets the appropriate on ground state given a UTC datetime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public OnGroundState GetOnGroundAtDateTime(DateTime dateTime)
        {
            return GetValueAtDateTime(dateTime, out object value) ? (OnGroundState)value : OnGroundState.No;
        }

        /// <summary>
        /// Gets the appropriate material temperature value state given a UTC datetime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public ushort GetMaterialTemperatureValueAtDateTime(DateTime dateTime)
        {
            return GetValueAtDateTime(dateTime, out object value) ? (ushort)value : CellPassConsts.NullMaterialTemperatureValue;
        }

        /// <summary>
        /// Gets the appropriate MDP value state given a UTC datetime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public short GetMDPValueAtDateTime(DateTime dateTime)
        {
            return GetValueAtDateTime(dateTime, out object value) ? (short)value : CellPassConsts.NullMDP;
        }

        /// <summary>
        /// Gets the appropriate machine speed given a UTC datetime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public double GetMachineSpeedValueAtDateTime(DateTime dateTime)
        {
            return GetValueAtDateTime(dateTime, out object value) ? (double)value : Consts.NullDouble;
        }

        /// <summary>
        /// Gets the appropriate CCA value given a UTC datetime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public byte GetCCAValueAtDateTime(DateTime dateTime)
        {
            return GetValueAtDateTime(dateTime, out object value) ? (byte)value : CellPassConsts.NullCCA;
        }
    }
}
