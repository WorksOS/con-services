namespace VSS.VisionLink.Interfaces.Events.Commands.Models
{
	public enum PrimaryDataSourceEnum
	{
		RTERM_AC_50HZ = 0, // Alternate Sources  with R-term AC input  (using 50Hz as frequency threshold)
		//  Note: (if it is selected, the SMH & Engine start/stop will be from be from R-term AC with 50Hz threshold as default for R-term, 
		//  the source of “Odometer/Distance” travel will be from GPS).

		J1939 = 1, // default setting

		RTERM_DC_4S = 2, // Alternate source with R-term DC input (using 4 seconds denounce for the DC input)
		// Note: if it is selected, the SMH & Engine start/stop will be from be from R-term DC, 
		// the source of “Odometer/Distance” travel will be coming from GPS).

		RTERM_DC_6S = 3, // Alternate source with R-term DC input (using 6s denounce)
		RTERM_AC_30HZ = 4, // Alternate source with R-term AC input (30Hz as frequency threshold)
		RTERM_AC_70HZ = 5  // Alternate source with R-term AC input (70Hz as frequency threshold)
	}
}