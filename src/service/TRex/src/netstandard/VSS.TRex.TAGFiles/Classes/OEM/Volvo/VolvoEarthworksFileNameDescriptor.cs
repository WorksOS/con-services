using System;
using System.IO;

namespace VSS.TRex.TAGFiles.Classes.OEM.Volvo
{
  public class VolvoEarthworksFileNameDescriptor
  {
    public string Lift { get; set; }
    public string DesignName { get; set; }
    public int Counter { get; set; }
    public string CSName { get; set; }
    public DateTime Date { get; set; }
    public string MachineID { get; set; } // ??? Is this machine hardware ID instead

    private void DecodeFromFileName(string fileName)
    {
      var parts = Path.GetFileNameWithoutExtension(fileName).Split('_');
      Lift = parts[0];
      DesignName = parts[1];
      Counter = int.Parse(parts[2]);
      CSName = parts[3];

      var dateParts = parts[4].Split(' ');
      var dayParts = dateParts[0].Split('-');
      var timeParts = dateParts[1].Split('-');
      Date = new DateTime(int.Parse(dayParts[0]), int.Parse(dayParts[1]), int.Parse(dayParts[2]), int.Parse(timeParts[0]), int.Parse(timeParts[1]), int.Parse(timeParts[2]));

      MachineID = parts[5];
    }

    public VolvoEarthworksFileNameDescriptor(string fileName)
    {
      DecodeFromFileName(fileName);
    }
  }
}
