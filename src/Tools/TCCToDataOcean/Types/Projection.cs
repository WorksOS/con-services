namespace TCCToDataOcean.Types
{
  public class Projection
  {
    public static string GetProjectionName(string typeId)
    {
      // As defined in file:///J:/Survey/DC_Formats/Current/FIELD.HTM#PROJECTION
      switch (typeId)
      {
        case "2": return "Plane";
        case "3": return "Transverse Mercator";
        case "4": return "Mercator";
        case "5": return "Lambert Conformal Conic 1 Parallel";
        case "6": return "Lambert Conformal Conic 2 Parallel";
        case "7": return "New Zealand Map Grid";
        case "8": return "Rectified Skew Orthomorphic (RSO)";
        case "9": return "Cassini-Soldner";
        case ":": return "Oblique Stereographic";
        case ";": return "RD Stereographic";
        case "<": return "UPS North";
        case "=": return "UPS South";
        case ">": return "Scale only";
        case "?": return "Oblique Mercator Angle";
        case "@": return "Oblique Conformal Cylindrical";
        case "A": return "Polar Stereographic";
        case "B": return "Albers Equal Area Conic";
        case "C": return "Krovak";
        case "D": return "United Kingdom National Grid";
        case "E": return "Denmark";
        case "F": return "Hungarian EOV";
        case "G": return "Stereographic Double";
        case "H": return "Projection Grid";
        case "1":
        default:
          return "No Projection";
      }
    }
  }
}
