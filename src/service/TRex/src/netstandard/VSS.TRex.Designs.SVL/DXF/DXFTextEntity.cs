using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VSS.TRex.Common;

namespace VSS.TRex.Designs.SVL.DXF
{
  public class DXFTextEntity : DXFEntity
  {
//    private
    //     FTextStyle : TDXFStyle;
    public double X, Y, Z;
    public string Text;
    public double Orientation;
    public double Size;

    public string FontName;

    //   fStyles : TFontStyles;
    //   fFormat : Word; // DQM text alignment format

    public double AlignX;
    public double AlignY;

 //   extMinX, extMinY, extMinZ,
 //   extMaxX, extMaxY, extMaxZ : Double; // Text extents...

    public DXFTextEntity(string layer, int colour, double x, double y, double z, string text, 
      double orientation, double size, string fontName, double alignX, double alignY) : base (layer, colour)
    {
      //TextStyle= Nil;

      X= x;
      Y= y;
      Z= z;
      Text= text;
      Orientation= orientation;
      Size= size;
      FontName= fontName;
      //Styles= styles;
      //Format= format;
      AlignX= alignX;
      AlignY= alignY;

/*      extMinX= Nullreal;
      extMinY= Nullreal;
      extMinZ= Nullreal;
      extMaxX= Nullreal;
      extMaxY= Nullreal;
      extMaxZ= Nullreal;*/
    }

    public override void SaveToFile(StreamWriter writer, DistanceUnitsType OutputUnits)
    {
//  text_format : text_format_rec;
//      XJust, YJust: Word;
//    DXFText: string;
      //   FontCodes: string;

      /*
      string ReplaceDXFANSIChars(string S)
      {
        Result = S;
     
        AnsiReplaceText(Result, '%', '%%%');
        AnsiReplaceText(Result, AnsiDegreeSymbol, '%%d');
        AnsiReplaceText(Result, AnsiPlusMinusSymbol, '%%p');
        AnsiReplaceText(Result, AnsiCircleAndStrokeSymbol, '%%c');
      }
      */

      var DXFText = Text; //ReplaceChars(fText);

      if (DXFText.Length > 255)
      {
        /*
     WriteDXFRecord(F, 0, 'MTEXT');
     FontCodes= '';
     if fsBold in fStyles then FontCodes= FontCodes + '|b1';
     if fsItalic in fStyles then FontCodes= FontCodes + '|i1';
     if fsUnderline in fStyles then DXFText= '\L' + DXFText;
     if Length(FontCodes) <> 0 then
      DXFText = '\f' + fFontName + FontCodes + ';' + DXFText;
      */
      }
      else
      {
        DXFUtils.WriteDXFRecord(writer, 0, "TEXT");
      }

      base.SaveToFile(writer, OutputUnits);

      DXFUtils.WriteXYZToDXF(writer, 0, X, Y, Z, OutputUnits);
      DXFUtils.WriteDXFRecord(writer, DXFConsts.TextSizeId, DXFUtils.NoLocaleFloatToStrF(DXFUtils.DXFDistance(Size, OutputUnits), 3));

      // Note: Underline text attributes must be supported by use of the %%u descriptor
      //       embedded in the text, rather than in a style (can't be supported by styles)

      //  if fsUnderline in fStyles then
      //DXFText= '%%u' + DXFText;

      DXFUtils.WriteDXFRecord(writer, 1, DXFText);

      /*
        DXFUtils.WriteDXFRecord(writer, 1, copy(ReplaceDXFANSIChars(DXFText), 1, 255));
        Delete(DXFText, 1, 255);
        while Length(DXFText) <> 0 do
            begin
        DXFUtils.WriteDXFRecord(writer, 3, copy(ReplaceDXFANSIChars(DXFText), 1, 255));
        Delete(DXFText, 1, 255);
        end;
        */

      /*
      if Assigned(FTextStyle) then
      DXFUtils.WriteDXFRecord(writer, DXFConsts.FontNameId, FTextStyle.Name);
      else
      DXFUtils.WriteDXFRecord(writer, DXFConsts.FontNameId, FontName);
      */

      DXFUtils.WriteDXFRecord(writer, DXFConsts.TextOrientationId, DXFUtils.NoLocaleFloatToStrF(((Math.PI / 2) - Orientation) * (180 / Math.PI), 3)); // fOrientation is in radians, but degress in file

      /*
        unpack_text_format(fFormat, text_format);
      
      int XJust = 0;
      int YJust = 0;

      Case Text_format.posn of
        top_left, top_right, top_centre: YJust = 3;
      centre_left, centre_right, centre_centre:
      YJust = 2;
      bottom_left, bottom_right, bottom_centre:
      YJust = 0; // 0 for baseline, not bottom of descender (=1)!;
      descender_left, descender_right, descender_centre:
      YJust = 1;
      end;

      Case Text_format.posn of
        bottom_left, top_left, centre_left, descender_left: XJust = 0;
      bottom_centre, top_centre, centre_centre, descender_centre:
      XJust = 1;
      bottom_right, top_right, centre_right, descender_right:
      XJust = 2;
      end;

      if (XJust != 0)
        DXFUtils.WriteDXFRecord(writer, DXFConsts.TextJustificationX, XJust.ToString());
      if (YJust != 0)
        DXFUtils.WriteDXFRecord(writer, DXFConsts.TextJustificationY, YJust.ToString());

      if (XJust != 0 || YJust != 0) // write out secondary position, same as initial position
        DXFUtils.WriteXYZToDXF(writer, 1, AlignX, AlignY, 0, OutputUnits);
        */
    }

    //    procedure CalculateExtents(var EMinX, EMinY, EMinZ, EMaxX, EMaxY, EMaxZ : Double); Override;
    //    procedure SetExtents(AextMinX, AextMinY, AextMaxX, AextMaxY : Double);
    //    Function ReplaceChars(const Text  : String) : String;

    public override DXFEntityTypes EntityType() => DXFEntityTypes.Text;

//    Procedure ConvertTo2D; Override;
    public override bool Is3D() => Z != Consts.NullDouble;

    public override double GetInitialHeight() => Z;
  }
}
