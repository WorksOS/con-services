using System;

namespace VSS.Hosted.VLCommon
{
  public class RobbinsFormulae
  {
    public RobbinsFormulae()
    {
    }

    public static void EllipsoidRobbinsForward( double startLatitude,
      double startLongitude,
      double bearing,
      double distanceInMeters,
      out double endLatitude,
      out double endLongitude )
    {
      startLatitude = ConvertToRadians( startLatitude );
      startLongitude = ConvertToRadians( startLongitude );
      bearing = ConvertToRadians( bearing );

      if ( ApproxEQ( distanceInMeters, 0.0, nearZeroTolerance ) )
      {
        endLongitude = ConvertToDegrees( startLongitude );
        endLatitude = ConvertToDegrees( startLatitude );
        return;
      }

      double longDistance = 5000000.0;

      if ( distanceInMeters > longDistance )
      {
        double midLatitude;
        double midLongitude;

        EllipsoidRobbinsForward( startLatitude,
          startLongitude,
          bearing,
          longDistance,
          out midLatitude,
          out midLongitude );

        EllipsoidRobbinsForward( midLatitude,
          midLongitude,
          bearing,
          distanceInMeters - longDistance,
          out endLatitude,
          out endLongitude );

        return;
      }

      startLatitude = NormaliseLatitude( startLatitude );
      startLongitude = NormaliseLongitude( startLongitude );

      double angleStartToEnd = bearing;
      double sinAngleStartToEnd = Math.Sin( angleStartToEnd );
      double cosAngleStartToEnd = Math.Cos( angleStartToEnd );

      double eccentricity = Math.Sqrt( 1 - ( semiMinorAxis * semiMinorAxis ) / ( semiMajorAxis * semiMajorAxis ) );
      double eccentricitySquared = eccentricity * eccentricity;
      double eccentricityPrimeSquared = eccentricitySquared / ( 1 - eccentricitySquared );
      double eccentricityPrime = Math.Sqrt( eccentricityPrimeSquared );

      double sinStartLatitude = Math.Sin( startLatitude );
      double cosStartLatitude = Math.Cos( startLatitude );
      double nu = semiMajorAxis /
        Math.Sqrt( 1 - eccentricitySquared * sinStartLatitude * sinStartLatitude );

      double hPrime = eccentricityPrime * cosStartLatitude * cosAngleStartToEnd;
      double hPrimeSquared = hPrime * hPrime;

      double g = eccentricityPrime * sinStartLatitude;

      double eta = distanceInMeters / nu;
      double etaSquared = eta * eta;
      double etaToTheFour = etaSquared * etaSquared;

      double term1 = 1 + etaSquared * hPrimeSquared * ( 1 - hPrimeSquared ) / 6;
      double term2 = -eta * etaSquared * g * hPrime * ( 1 - hPrimeSquared ) / 8;
      double term3 = -( etaToTheFour / 120 ) * ( hPrimeSquared * ( 4 - 7 * hPrimeSquared )
        - 3 * g * g * ( 1 - 7 * hPrimeSquared ) );
      double term4 = etaToTheFour * eta * g * hPrime / 48;

      double sum = term1 + term2 + term3 + term4;
      double sigmaPrime = eta * sum;
      double sinSigmaPrime = Math.Sin( sigmaPrime );

      double sinZeta = sinStartLatitude * Math.Cos( sigmaPrime ) + cosStartLatitude * cosAngleStartToEnd
        * sinSigmaPrime;
      double zeta = Math.Asin( sinZeta );
      double cosZeta = Math.Cos( zeta );

      double deltaLongitude = Math.Asin( sinSigmaPrime * sinAngleStartToEnd / cosZeta );

      endLongitude = startLongitude + deltaLongitude;

      double mu = 1 + eccentricityPrimeSquared /
        2 * ( sinZeta - sinStartLatitude ) * ( sinZeta - sinStartLatitude );
      if ( Math.Abs( sinZeta ) < nearZeroTolerance )
      {
        //For this to occur the starting latitude must be zero and the azimuth
        //(bearing) must be 90 or 270 degrees. That is, we are moving due
        //east or west of zero latitude, so end latitude is the same.
        endLatitude = 0.0;
      }
      else
      {
        endLatitude = Math.Atan( Math.Tan( zeta ) * ( 1 + eccentricityPrimeSquared ) *
          ( 1 - eccentricitySquared * mu * sinStartLatitude / sinZeta ) );
      }

      endLatitude = ConvertToDegrees( NormaliseLatitude( endLatitude ) );
      endLongitude = ConvertToDegrees( NormaliseLongitude( endLongitude ) );
    }

    public static void EllipsoidRobbinsReverse( double startLatitude,
      double startLongitude,
      double endLatitude,
      double endLongitude,
      out double forwardBearing,
      out double backBearing,
      out double distance )
    {
      //method
      // Takes the lat and long of each of two points and from them 
      // calculates the distance between them, as well
      // as the azimuth of the connecting geodesic at the start and at the end.

      //Caution
      // This can return null values (kNullDoubles) for forwardBearing
      // and ReverseBearing if the Start and End points are too close. Calling
      // functions must test for this.

      startLatitude = ConvertToRadians( startLatitude );
      startLongitude = ConvertToRadians( startLongitude );
      endLatitude = ConvertToRadians( endLatitude );
      endLongitude = ConvertToRadians( endLongitude );

      startLatitude = NormaliseLatitude( startLatitude );
      startLongitude = NormaliseLongitude( startLongitude );
      endLatitude = NormaliseLatitude( endLatitude );
      endLongitude = NormaliseLongitude( endLongitude );


      double AngleAbout_1mm = 1e-10;

      bool TooClose = ApproxEQ( startLatitude, endLatitude, AngleAbout_1mm ) &&
        ApproxEQ( startLongitude, endLongitude, AngleAbout_1mm );

      if ( TooClose )
      {
        forwardBearing = 0.0;
        backBearing = 0.0;
        distance = 0.0;
        return;
      }

      double _DeltaLatitude = CalculateDeltaLatitude( startLatitude, endLatitude );
      double _DeltaLongitude = CalculateDeltaLongitude( startLongitude, endLongitude );

      bool TooFar = Math.Abs( _DeltaLatitude ) > Math.PI / 4 ||
        Math.Abs( _DeltaLongitude ) > Math.PI / 2;

      if ( TooFar )
      {
        // We Divide the job into two separations, recurse, and add them up afterwards.

        double MidLongitude = startLongitude + ( _DeltaLongitude * 0.5 );
        double MidLatitude = startLatitude + ( _DeltaLatitude * 0.5 );

        double FirstForwardBearing;
        double FirstBackBearing;
        double FirstDistance;

        EllipsoidRobbinsReverse( startLatitude, startLongitude,
          MidLatitude, MidLongitude,
          out FirstForwardBearing, out FirstBackBearing,
          out FirstDistance );

        double SecondForwardBearing;
        double SecondBackBearing;
        double SecondDistance;

        EllipsoidRobbinsReverse( MidLatitude, MidLongitude,
          endLatitude, endLongitude,
          out SecondForwardBearing, out SecondBackBearing,
          out SecondDistance );

        // Should not get any nulls back in these cases, as the points are not close        

        //    _ASSERT(!IsNull(FirstForwardBearing) &&
        //          !IsNull(FirstBackBearing) &&
        //          !IsNull(FirstDistance) &&
        //          !IsNull(SecondForwardBearing) &&
        //          !IsNull(SecondBackBearing) &&
        //          !IsNull(SecondDistance), "");

        forwardBearing = ConvertToDegrees( AverageBearings( FirstForwardBearing, SecondForwardBearing ) );
        backBearing = ConvertToDegrees( AverageBearings( FirstBackBearing, SecondBackBearing ) );
        distance = FirstDistance + SecondDistance;

        return;
      }

      // Ok... Normal situation ...

      double SinStartLatitude = Math.Sin( startLatitude );
      double CosStartLatitude = Math.Cos( startLatitude );
      double SinEndLatitude = Math.Sin( endLatitude );
      double CosEndLatitude = Math.Cos( endLatitude );
      double Eccentricity = Math.Sqrt( 1 - ( semiMinorAxis * semiMinorAxis ) / ( semiMajorAxis * semiMajorAxis ) );
      double EccentricitySquared = Eccentricity * Eccentricity;
      double EccentricityPrimeSquared = EccentricitySquared / ( 1 - EccentricitySquared );
      double EccentricityPrime = Math.Sqrt( EccentricityPrimeSquared );

      double Nu1 = semiMajorAxis /
        Math.Sqrt( 1 - EccentricitySquared * SinStartLatitude * SinStartLatitude );
      double Nu2 = semiMajorAxis /
        Math.Sqrt( 1 - EccentricitySquared * SinEndLatitude * SinEndLatitude );
      double TanZeta2 = ( 1 - EccentricitySquared ) * Math.Tan( endLatitude ) +
        EccentricitySquared * Nu1 * SinStartLatitude / ( Nu2 * CosEndLatitude );
      double DeltaLongitude = endLongitude - startLongitude;
      double SinDeltaLongitude = Math.Sin( DeltaLongitude );
      double Tau1 = CosStartLatitude * TanZeta2 - SinStartLatitude * Math.Cos( DeltaLongitude );

      double AngleAtStart = Math.Atan2( SinDeltaLongitude, Tau1 );

      double SinAngleAtStart = Math.Sin( AngleAtStart );
      double CosAngleAtStart = Math.Cos( AngleAtStart );

      double TanZeta1 = ( 1 - EccentricitySquared ) * Math.Tan( startLatitude ) +
        EccentricitySquared * Nu2 * SinEndLatitude
        / ( Nu1 * CosStartLatitude );

      double Tau2 = CosEndLatitude * TanZeta1 - SinEndLatitude * Math.Cos( -DeltaLongitude );
      double AngleAtEnd = Math.Atan2( Math.Sin( -DeltaLongitude ), Tau2 );

      double Chi;

      if ( Math.Abs( SinAngleAtStart ) < nearZeroTolerance )
      {
        Chi = Tau1 / CosAngleAtStart;
      }
      else
      {
        Chi = SinDeltaLongitude / SinAngleAtStart;
      }

      double SigmaPrime = Math.Asin( Chi * Math.Cos( Math.Atan( TanZeta2 ) ) );
      double SigmaPrimeSquared = SigmaPrime * SigmaPrime;
      double SigmaPrimeToTheFour = SigmaPrimeSquared * SigmaPrimeSquared;

      double g = EccentricityPrime * SinStartLatitude;
      double hPrime = EccentricityPrime * CosStartLatitude * CosAngleAtStart;
      double hPrimeSquared = hPrime * hPrime;

      double Term1 = 1 - SigmaPrimeSquared * hPrimeSquared * ( 1 - hPrimeSquared ) / 6;
      double Term2 = SigmaPrimeSquared * SigmaPrime * g * hPrime * ( 1 - 2 * hPrimeSquared ) / 8;
      double Term3 = SigmaPrimeToTheFour * ( hPrimeSquared * ( 4 - 7 * hPrimeSquared )
        - 3 * g * g * ( 1 - 7 * hPrimeSquared ) ) / 120;
      double Term4 = -SigmaPrimeToTheFour * SigmaPrime * g * hPrime / 48;

      double Sum = Term1 + Term2 + Term3 + Term4;
      distance = Nu1 * SigmaPrime * Sum;

      NormalizeAzimuth( AngleAtEnd );
      NormalizeAzimuth( AngleAtStart );

      forwardBearing = ConvertToDegrees( AngleAtStart );
      backBearing = ConvertToDegrees( AngleAtEnd );
    }

    public static bool ApproxEQ( double operand1, double operand2, double precision )
    {
      if ( operand1 >= operand2 )
      {
        return ( operand1 - operand2 ) < precision;
      }
      else
      {
        return ( operand2 - operand1 ) < precision;
      }
    }

    public static double AverageBearings( double b1, double b3 )
    {
      double big = b1;
      double small = b3;

      if ( big < small )
      {
        double Swap = big;
        big = small;
        small = Swap;
      }

      if ( ( big - small ) > Math.PI )
      {
        big = big - ( 2.0 * Math.PI );
      }

      return ( big + small ) / 2.0;
    }

    public static double CalculateDeltaLatitude( double start, double end )
    {
      return end - start;
    }

    public static double CalculateDeltaLongitude( double start, double end )
    {
      double k180Degrees = Math.PI;
      double k360Degrees = Math.PI * 2.0;

      start += k180Degrees;
      end += k180Degrees;

      double negative = 1.0;

      if ( start > end )
      {
        double Swap = start;
        start = end;
        end = Swap;
        negative = -1.0;
      }

      double delta = end - start;

      if ( delta > Math.PI )
      {
        delta = ( start + k360Degrees ) - end;
        negative = negative * -1.0;
      }

      delta *= negative;

      return delta;
    }

    public static double NormaliseLatitude( double latitude )
    {

      bool negative = latitude < 0.0;

      if ( negative )
      {
        latitude = -latitude;
      }

      // We fmod the input down to between 0 and 180 degrees.   Anything over 90 degrees is 
      // wrong so we 'wrap' these numbers back to -90 degrees. Thus
      //
      //  91 -> -89
      //  92 -> -88 ...
      // 180 -> 0 degrees.

      latitude = latitude % Math.PI;

      if ( latitude > ( Math.PI / 2 ) )
      {
        latitude = latitude - Math.PI;
      }

      if ( negative )
      {
        latitude *= -1.0;
      }

      return latitude;
    }

    public static double NormaliseLongitude( double longitude )
    {
      if ( longitude == Math.PI )
      {
        return Math.PI;
      }
      else
      {
        return NormalizeAngle( longitude, -Math.PI );
      }
    }

    public static double NormalizeAngle( double angle, double lowerLimit )
    {

      angle -= lowerLimit;
      angle = angle % ( Math.PI * 2 );

      if ( angle < 0.0 )
      {
        angle += ( Math.PI * 2 );
      }

      angle += lowerLimit;

      return angle;
    }

    public static double NormalizeAzimuth( double azimuth )
    {
      azimuth = azimuth % ( Math.PI * 2 );

      if ( azimuth < 0.0 )
      {
        azimuth += ( Math.PI * 2 );
      }
      return azimuth;
    }

    public static double ConvertToRadians( double degrees )
    {
      return degrees / 57.2957795786;
    }

    public static double ConvertToDegrees( double radians )
    {
      return radians * 57.2957795786;
    }

    static readonly double semiMajorAxis = 6378137;
    static readonly double semiMinorAxis = 6356752.3142;
    static readonly double nearZeroTolerance = 0.0000000001;
  }
}
