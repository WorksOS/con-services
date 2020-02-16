using System;
using System.IO;
using System.Text;

namespace VSS.Hosted.VLCommon
{
   [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
   public sealed class HexDump 
   {
      public static int HexCharToValue(char ch) 
      {
         const string hexDigits = "0123456789abcdef";

         return hexDigits.IndexOf(Char.ToLower(ch));
      }

      public static byte[] HexStringToBytes(string hexString) 
      {
         int    i           = 0;
         int    resultIndex = 0;
         bool   msn         = true; // Most significant nibble
         byte[] result      = new byte[hexString.Length/2+1];  // This may be too large, but we don't care.

         while (i < hexString.Length) 
         {
            int index = HexCharToValue(hexString[i++]);

            if (index == -1) 
            {
               continue;
            }

            // This is a valid digit.

            if (msn) 
            {
               result[resultIndex] = (byte)(index << 4);
            } 
            else 
            {
               result[resultIndex++] |= (byte) index;
            }

            msn = !msn;
         }

         if (!msn) 
         {
            // There weren't enough nibbles to fill the last byte so fill it ourselves.

            resultIndex++;
         }

         if (result.Length != resultIndex) 
         {
            // Crop the array.

            byte[] newResult = new byte[resultIndex];
            //Array.Copy(result, newResult, resultIndex);  v-- is faster
            Buffer.BlockCopy(result, 0, newResult, 0, newResult.Length);
            result = newResult;
         }

         return result;
      }

      public static string BytesToHexString(byte[] bytes) 
      {
        string result = BitConverter.ToString(bytes);
        result = result.Replace("-", "");
        return result.ToString();
      }

      public static void HexDumpByteArray(byte[] raw, TextWriter sw)
      {
         if (raw != null) 
         {
            HexDumpByteArray(raw, 0, raw.Length, sw);
         }
      }

      public static void HexDumpByteArray(byte[] raw, int start, int length, TextWriter sw)
      {
         HexDumpByteArray(raw, start, length, 0, sw);
      }

      public static void HexDumpByteArray(byte[] raw, int start, int length, int startOffset, TextWriter sw)
      {
         // Now hexdump the message

         int i = start;
         byte[] lastBytesForAscii = new byte[16];
         int lastStartForAscii = 0;

         while (i < raw.Length  &&  length-- > 0) 
         {
            if ((i % 16) == 0) 
            {
               if (i > 0) 
               {
                  // Write out the ASCII of the previous characters.

                  string text = System.Text.Encoding.ASCII.GetString(lastBytesForAscii, 0, lastStartForAscii);

                  lastStartForAscii = 0;

                  if (text.Length > 8) 
                  {
                     text = text.Substring(0, 8)+' '+text.Substring(8);
                  }

                  sw.Write("  ");
                  sw.Write(text);

                  sw.WriteLine();
               }

               sw.Write((startOffset+i).ToString("x6"));
               sw.Write(':');
            }

            if ((i % 8) == 0) 
            {
               sw.Write(' ');
            }

            sw.Write(' ');
            sw.Write(raw[i].ToString("x2"));

            lastBytesForAscii[lastStartForAscii++] = (raw[i] < 32 || raw[i] > 127) ? (byte) '.' : raw[i];

            i++;
         }

         // Write the ASCII for the last (partial?) line

         if (lastStartForAscii > 0) 
         {
            int bytesMissingForFullLine = 16-lastStartForAscii;

            sw.Write(new string(' ', bytesMissingForFullLine*3+(bytesMissingForFullLine >= 8 ? 1 : 0)+2));

            string text = System.Text.Encoding.ASCII.GetString(lastBytesForAscii, 0, lastStartForAscii);

            if (text.Length > 8) 
            {
               text = text.Substring(0, 8)+' '+text.Substring(8);
            }

            sw.Write(text);
         }

         sw.WriteLine();
      }
   }
}
