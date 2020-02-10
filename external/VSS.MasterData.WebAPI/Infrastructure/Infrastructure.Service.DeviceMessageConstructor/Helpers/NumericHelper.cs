

namespace Infrastructure.Service.DeviceMessageConstructor.Helpers
{
    public static class NumericHelper
    {
        public static double ConvertKilometersToMiles(double kilometers)
        {
            return kilometers * 0.621371192;
        }

        public static double ConvertMetersToFeet(double meters)
        {
            return meters * 3.28084;
        }
    }
}
