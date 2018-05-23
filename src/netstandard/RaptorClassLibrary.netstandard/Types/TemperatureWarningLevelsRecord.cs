using System;
using System.Collections.Generic;
using System.Text;
using Apache.Ignite.Core.Lifecycle;
using VSS.TRex.Cells;
using VSS.TRex.Common;

namespace VSS.TRex.Types
{
  public class TemperatureWarningLevelsRecord
  {
		/// <summary>
		/// Minimum temperature warning levels value.
		/// </summary>
		public ushort Min { get; set; }
		/// <summary>
		/// Maximum temperature warning levels value.
		/// </summary>
		public ushort Max { get; set; }

	  /// <summary>
	  /// Constractor with arguments.
	  /// </summary>
	  /// <param name="min"></param>
	  /// <param name="max"></param>
	  public TemperatureWarningLevelsRecord(ushort min, ushort max)
	  {
		  Min = min;
		  Max = max;
	  }

		/// <summary>
		/// Initialises the Min and Max properties with null values.
		/// </summary>
	  public void Clear()
	  {
		  Min = CellPass.NullMaterialTemperatureValue;
		  Max = CellPass.NullMaterialTemperatureValue;
		}
  }
}
