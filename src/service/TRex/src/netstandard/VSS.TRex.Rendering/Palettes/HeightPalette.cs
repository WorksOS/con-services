using System;
using Draw = System.Drawing;
using VSS.TRex.Common;
using VSS.TRex.Rendering.Palettes.Interfaces;
using VSS.TRex.Common.Utilities;

namespace VSS.TRex.Rendering.Palettes
{
    public class HeightPalette : IPlanViewPalette
    {
        private double MinElevation;// = Consts.NullDouble;
        private double MaxElevation;// = Consts.NullDouble;
        private double ElevationPerBand;// = Consts.NullDouble;

        private static readonly Draw.Color[] ElevationPalette = 
        {
        Draw.Color.Aqua,
        Draw.Color.Yellow,
        Draw.Color.Fuchsia,
        Draw.Color.Lime,
        Draw.Color.FromArgb(0x80, 0x80, 0xFF),
        Draw.Color.LightGray,  
        Draw.Color.FromArgb(0xEB, 0xFD, 0xAC),
        Draw.Color.FromArgb(0xFF, 0x80, 0x00),
        Draw.Color.FromArgb(0xFF, 0xC0, 0xFF),
        Draw.Color.FromArgb(0x96, 0xCB, 0xFF),
        Draw.Color.FromArgb(0xB5, 0x8E, 0x6C),
        Draw.Color.FromArgb(0xFF, 0xFF, 0x80),
        Draw.Color.FromArgb(0xFF, 0x80, 0x80),
        Draw.Color.FromArgb(0x80, 0xFF, 0x00),
        Draw.Color.FromArgb(0x00, 0x80, 0xFF),
        Draw.Color.FromArgb(0xFF, 0x00, 0x80),
        Draw.Color.Teal,     
        Draw.Color.FromArgb(0xFF, 0xC0, 0xC0),
        Draw.Color.FromArgb(0xFF, 0x80, 0xFF),
        Draw.Color.FromArgb(0x00, 0xFF, 0x80)
        };
    
        public HeightPalette(double minElevation, double maxElevation)
        {
            MinElevation = minElevation;
            MaxElevation = maxElevation;
            ElevationPerBand = (MaxElevation - MinElevation) / ElevationPalette.Length;
        }

        public Draw.Color ChooseColour(double value)
        {
            var color = Draw.Color.Black;

            if (value != Consts.NullDouble)
            {
              int index = (int) Math.Floor((value - MinElevation) / ElevationPerBand);
              color = Range.InRange(index, 0, ElevationPalette.Length - 1) ? ElevationPalette[index] : Draw.Color.Black; // Color.Empty;
            }

            return color;
        }
    }
}
