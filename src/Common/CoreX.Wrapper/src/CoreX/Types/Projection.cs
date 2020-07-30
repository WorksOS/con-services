namespace CoreX.Wrapper.Types
{
  public class Projection
  {
    public static string GetProjectionName(string typeId)
    {
      // As defined in file:///J:/Survey/DC_Formats/Current/FIELD.HTM#PROJECTION
      return typeId switch
      {
        "2" => "Plane",
        "3" => "Transverse Mercator",
        "4" => "Mercator",
        "5" => "Lambert Conformal Conic 1 Parallel",
        "6" => "Lambert Conformal Conic 2 Parallel",
        "7" => "New Zealand Map Grid",
        "8" => "Rectified Skew Orthomorphic (RSO)",
        "9" => "Cassini-Soldner",
        ":" => "Oblique Stereographic",
        "," => "RD Stereographic",
        "<" => "UPS North",
        "=" => "UPS South",
        ">" => "Scale only",
        "?" => "Oblique Mercator Angle",
        "@" => "Oblique Conformal Cylindrical",
        "A" => "Polar Stereographic",
        "B" => "Albers Equal Area Conic",
        "C" => "Krovak",
        "D" => "United Kingdom National Grid",
        "E" => "Denmark",
        "F" => "Hungarian EOV",
        "G" => "Stereographic Double",
        "H" => "Projection Grid",
        _ => "No Projection"
      };
    }
  }
}
