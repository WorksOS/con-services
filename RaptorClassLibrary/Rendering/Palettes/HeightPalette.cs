using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.Rendering.Palettes.Interfaces;
using VSS.VisionLink.Raptor.Utilities;

namespace VSS.VisionLink.Raptor.Rendering.Palettes
{
    public class HeightPalette : IPlanViewPalette
    {
        private double MinElevation = Consts.NullDouble;
        private double MaxElevation = Consts.NullDouble;
        private double ElevationPerBand = Consts.NullDouble;

        private static Color[] ElevationPalette = new Color[]
        {
        Color.Aqua,
        Color.Yellow,
        Color.Fuchsia,
        Color.Lime,
        Color.FromArgb(0x80, 0x80, 0xFF),
        Color.LightGray,  
        Color.FromArgb(0xEB, 0xFD, 0xAC),
        Color.FromArgb(0xFF, 0x80, 0x00),
        Color.FromArgb(0xFF, 0xC0, 0xFF),
        Color.FromArgb(0x96, 0xCB, 0xFF),
        Color.FromArgb(0xB5, 0x8E, 0x6C),
        Color.FromArgb(0xFF, 0xFF, 0x80),
        Color.FromArgb(0xFF, 0x80, 0x80),
        Color.FromArgb(0x80, 0xFF, 0x00),
        Color.FromArgb(0x00, 0x80, 0xFF),
        Color.FromArgb(0xFF, 0x00, 0x80),
        Color.Teal,     
        Color.FromArgb(0xFF, 0xC0, 0xC0),
        Color.FromArgb(0xFF, 0x80, 0xFF),
        Color.FromArgb(0x00, 0xFF, 0x80)
        };
    
        public HeightPalette(double minElevation, double maxElevation)
        {
            MinElevation = minElevation;
            MaxElevation = maxElevation;
            ElevationPerBand = (MaxElevation - MinElevation) / (ElevationPalette.Count() - 1);
        }

        public Color ChooseColour(double value)
        {
            if (Range.InRange(value, MinElevation, MaxElevation))
            {
                int index = (int)Math.Floor((value - MinElevation) / ElevationPerBand);
                return ElevationPalette[index];
            }
            else
            {
                return Color.Empty;
            }
        }
    }
}
