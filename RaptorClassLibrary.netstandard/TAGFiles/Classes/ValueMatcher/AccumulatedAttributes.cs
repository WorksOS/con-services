using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes
{
    /// <summary>
    /// AccumulatedAttribute records the state of an attribute supplied to the
    /// snail trail processor, in conjunction with the date/time it was recorded
    /// </summary>
    public struct AccumulatedAttribute
    {
        public DateTime dateTime;
        public object value;

        public AccumulatedAttribute(DateTime dateTime, object value)
        {
            this.dateTime = dateTime;
            this.value = value;
        }
    }

    /// <summary>
    /// AccumulatedAttributes stores a list of TAccumulatedAttribute instances
    /// the record a series of attribute value obsevations
    /// </summary>
    public class AccumulatedAttributes
    {
        // List of items being tracked
        private List<AccumulatedAttribute> list = new List<AccumulatedAttribute>();

        public AccumulatedAttributes()
        {
        }

        /// <summary>
        /// Provides the count of the internal list of attributes being maintained
        /// </summary>
        /// <returns></returns>
        public int NumAttrs => list.Count;

        // DiscardAllButLatest discards all but the most recently added
        // value from the list;
        public void DiscardAllButLatest()
        {
            if (list.Count > 1)
            {
                list = list.GetRange(list.Count - 1, 1);
            }
        }

        /// <summary>
        /// Add adds a value recorded at DateTime to the list. It is assumed
        /// that the date time of this item is after the date time
        /// of any previously added item in the list
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="value"></param>
        public void Add(DateTime dateTime, object value) => list.Add(new AccumulatedAttribute(dateTime, value));


        // GetLatest simply returns the last value in the list
        public object GetLatest()
        {
            Debug.Assert(list.Count > 0, "List length is zero in GetLatest");

            return list.Last().value;
        }

        // GetValueAtDateTime locates the value appropriate for the given datetime
        // Requesting a value for a datetime prior to the first value in the list
        // will return the first value in the list. If there are no values in the list
        // then the function returns false.
        public bool GetValueAtDateTime(DateTime dateTime, out object value)
        {
            value = null;

            switch (list.Count)
            {
                case 0:
                    return false;
                case 1:
                    value = list[0].value;
                    return true;
            //    default:
            //        break;
            }

            if (dateTime < list[0].dateTime)
            {
                value = list[0].value;
                return true;
            }
            else
            {
                if (dateTime >= list.Last().dateTime)
                {
                    value = list.Last().value;
                    return true;
                }
                else
                {
                    for (int I = 0; I < list.Count - 1; I++)
                    {
                        if ((dateTime >= list[I].dateTime) && (dateTime < list[I + 1].dateTime))
                        {
                            value = list[I].value;
                            return true;
                        }
                    }

                    // It should not be possible to get here...
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the appropriate GPSMode given a UTC datetime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public GPSMode GetGPSModeAtDateTime(DateTime dateTime)
        {
            return GetValueAtDateTime(dateTime, out object value) ? (GPSMode)value : CellPass.NullGPSMode;
        }

        /// <summary>
        /// Gets the appropriate CCV value given a UTC datetime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public short GetCCVValueAtDateTime(DateTime dateTime)
        {
            return GetValueAtDateTime(dateTime, out object value) ? (short)value : CellPass.NullCCV;
        }

        /// <summary>
        /// Gets the appropriate RMV value given a UTC datetime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>        
        public short GetRMVValueAtDateTime(DateTime dateTime)
        {
            return GetValueAtDateTime(dateTime, out object value) ? (short)value : CellPass.NullRMV;
        }

        /// <summary>
        /// Gets the appropriate vibration frequency value given a UTC datetime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>   
        public ushort GetFrequencyValueAtDateTime(DateTime dateTime)
        {
            return GetValueAtDateTime(dateTime, out object value) ? (ushort)value : CellPass.NullFrequency;
        }

        /// <summary>
        /// Gets the appropriate vibration amplitude value given a UTC datetime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>   
        public ushort GetAmplitudeValueAtDateTime(DateTime dateTime)
        {
            return GetValueAtDateTime(dateTime, out object value) ? (ushort)value : CellPass.NullAmplitude;
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
            return GetValueAtDateTime(dateTime, out object value) ? (ushort)value : CellPass.NullMaterialTemp;
        }

        /// <summary>
        /// Gets the appropriate MDP value state given a UTC datetime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public short GetMDPValueAtDateTime(DateTime dateTime)
        {
            return GetValueAtDateTime(dateTime, out object value) ? (short)value : CellPass.NullMDP;
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
            return GetValueAtDateTime(dateTime, out object value) ? (byte)value : CellPass.NullCCA;
        }
    }
}
