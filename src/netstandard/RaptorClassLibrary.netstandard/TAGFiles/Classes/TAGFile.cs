using System;
using System.IO;
using VSS.TRex.TAGFiles.Classes.Sinks;
using VSS.TRex.TAGFiles.Types;

/*
{ This unit defines the schema used in the new EBML based Tag file format
  to represent snail trail and IC information stored by the machine

  Some excerpts from the file format definition document:

  The file format is based on 4-bit words (nybbles) which are packed two to
  a byte in the file. Conceptually, then, the file is a header of nybbles,
  and padding to byte boundaries is never performed. In any given byte in the
  file, the high-order nybble is considered the first nybble, and the
  low-nybble the second.

  Var Int
  A concept has been borrowed from EBML, which was in turn modified from UTF-8.
  A variable sized integer type, that may span 1 to 4 nybbles. This is used to
  encode an unsigned integer value from zero to 4679. The datatype will be
  referred to as a Var Int in this document, and its format is described below.

  First Nybble	2nd	3rd	4th	Offset	ID Range
  1xxx				0	0-7
  01xx	xxxx			8	8-71
  001x	xxxx	xxxx		72	72-583
  0001	xxxx	xxxx	xxxx	584	584-4679

  Note that the offset must be subtracted from the value to be stored in the
  Var Int before the value is encoded.

  Field Data Type Signature

  4-bit signature	Data Width (nybbles)	Data Range	Data Description.
  0	1	-8 , +7	Integer
  1	1	0 , 15	Unsigned integer
  2	2	0 , +255	Unsigned integer
  3	2	-128 , +127	Integer
  4	3	0, +4095	Unsigned integer
  5	3	-211 , +211 - 1	Integer
  6	4	0, +65535	Unsigned integer
  7	4	-32768, +32767	Integer
  8	8	-231 , +231 - 1	Unsigned integer
  9	8	0, +232	Integer
  10	8		IEEE Floating point
  11	16		IEEE double-precision Floating point
  12	N*2 + 2 (for NUL) nybbles		NUL-terminated ANSI string
  13	N*4 + 4 (for NUL) nybbles		NUL-terminated UNICODE string
  14	0		Empty Type
  15			Variable-Length binary type. Length is coded as an unsigned value in the first four nybbles of the data

  All the integer types described above are big-endian. That is, the first
  nybble encountered corresponds to the most significant nybble of the integer
  type.

  File Format

  The file starts with a header that identifies the file version and the data
  dictionary (schema) ID and version that was used to create the file. The
  file version refers to the version of this document, and the data dictionary
  ID and version refers to the ID and version of a document describing the
  actual meaning of the record and field names found in the file.

  File Header
  Width (Nybbles)	Data Type	Meaning
  1	Unsigned Int	File Major version
  1	Unsigned Int	File Minor version
  4	Unsigned Int	Data Dictionary ID number
  1	Unsigned Int	Data Dictionary Major version
  1	Unsigned Int	Data Dictionary Minor version
  8	Unsigned Int	Byte offset to field and type table

  If the byte offset in the header is zero, then the field and type table is
  found immedatley following the header, and the data begins immediatley
  afterwards. This form of the file may be used when the file does not support
  seeking (for example, receiving the file over a socket).

  If the byte offset is non-zero, the data immediatley follows the header,
  and the file and type table is found at the end of the file at the byte
  offset specified.

  In this case the data continues until the byte offset specified in the
  header is reached, at which point the field and type table begins. Note that
  a zero-value padding nybble is written at the end of the data if the data
  ends in the middle of a byte.

  Data [Repeated]
  Width (Nybbles)	Type / Value	Meaning
  1-4	Var Int	Field and Type ID	Repeated
      Field data


  The field and type which contains one entry per type of field found in the
  file. Note that single field names may occur multiple times in the field
  name table with different data types. This means that fields of the same
  name may contain different data types – an oddity that is included so that
  a field may use numeric types of varying width / precision.


  Field And Type Table
  Width (Nybbles)	Type / Value	Meaning
  N*2	ASCII string	Field name (Max 64. characters)	Repeated
  2	0x00	Field name NUL terminator
  1	Int	Field type
  1-4	Var Int	ID in this file
  2	0x00	Record name table terminator
}
*/

namespace VSS.TRex.TAGFiles.Classes
{
    public class TAGFile
    {
        private TAGHeader Header = new TAGHeader();

        private TAGDictionary Dictionary { get; set; } = new TAGDictionary();

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public TAGFile()
        {
        }

        /// <summary>
        /// Reads the context of a TAG file usign the provided reader and sink
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="sink"></param>
        /// <returns></returns>
        public TAGReadResult Read(TAGReader reader, TAGValueSinkBase sink)
        {
            try
            {
                if (reader.GetSize() == 0)
                {
                    return TAGReadResult.ZeroLengthFile;
                }

                try
                {
                    Header.Read(reader);
                }
                catch // (Exception E)
                {
                    // TODO add when logging available
                    // SIGLogMessage.Publish(Self, Format('Invalid tagfile. Exception in TTagFile.ReadStream - FHeader.LoadFromStream: %s', [E.Message]), slmcWarning);
                    return TAGReadResult.InvalidDictionary;
                }

                // If the offset to the dictionary is zero, then it follows immediately after the header
                long DataEndPos;
                try
                {
                    if (Header.FieldAndTypeTableOffset != 0)
                    {
                        long StreamPos = reader.NybblePosition;
                        reader.NybblePosition = Header.FieldAndTypeTableOffset * 2; // FieldAndTypeTableOffset is in bytes

                        if (!Dictionary.Read(reader))
                            return TAGReadResult.InvalidDictionary;

                        reader.NybblePosition = StreamPos;

                        DataEndPos = Header.FieldAndTypeTableOffset * 2; // FieldAndTypeTableOffset is in bytes
                    }
                    else
                    {
                        if (!Dictionary.Read(reader))
                            return TAGReadResult.InvalidDictionary;

                        DataEndPos = reader.GetSize();
                    }
                }
                catch // (Exception E)
                {
                    // TODO when logging is available
                    //SIGLogMessage.Publish(Self, Format('Exception in TagFile.ReadFile: %s', [E.Message]), slmcDebug);
                    return TAGReadResult.InvalidDictionary;
                }

                // Now read in the data from the file
                if (!sink.Starting())
                {
                    return TAGReadResult.SinkStartingFailure;
                }

                while (!sink.Aborting() && !sink.ProcessingTerminated && (reader.NybblePosition < DataEndPos))
                {
                    if (!reader.ReadVarInt(out short ValueTypeID))
                    {
                        if (reader.NybblePosition >= DataEndPos)
                        {
                            break; // We have finished
                        }
                        else
                        {
                            return TAGReadResult.InvalidValueTypeID; // This is an invalid tag file
                        }
                    }

                    if (Dictionary.Entries.Keys.Count == 0)
                    {
                        return TAGReadResult.InvalidDictionary;
                    }

                    if (!Dictionary.Entries.TryGetValue(ValueTypeID, out TAGDictionaryItem DictionaryEntry))
                    {
                        return TAGReadResult.InvalidValueTypeID;
                    }

                    try
                    {
                        switch (DictionaryEntry.Type)
                        {
                            case TAGDataType.t4bitInt:
                            case TAGDataType.t8bitInt:
                            case TAGDataType.t12bitInt:
                            case TAGDataType.t16bitInt:
                            case TAGDataType.t32bitInt:
                                sink.ReadIntegerValue(DictionaryEntry, reader.ReadSignedIntegerValue(IntegerNybbleSizes.Nybbles(DictionaryEntry.Type)));
                                break;

                            case TAGDataType.t4bitUInt:
                            case TAGDataType.t8bitUInt:
                            case TAGDataType.t12bitUInt:
                            case TAGDataType.t16bitUInt:
                            case TAGDataType.t32bitUInt:
                                sink.ReadUnsignedIntegerValue(DictionaryEntry, reader.ReadUnSignedIntegerValue(IntegerNybbleSizes.Nybbles(DictionaryEntry.Type)));
                                break;

                            case TAGDataType.tIEEESingle:
                                sink.ReadIEEESingleValue(DictionaryEntry, reader.ReadSinglePrecisionIEEEValue());
                                break;

                            case TAGDataType.tIEEEDouble:
                                sink.ReadIEEEDoubleValue(DictionaryEntry, reader.ReadDoublePrecisionIEEEValue());
                                break;

                            case TAGDataType.tANSIString:
                                sink.ReadANSIStringValue(DictionaryEntry, reader.ReadANSIString());
                                break;

                            case TAGDataType.tUnicodeString:
                                sink.ReadUnicodeStringValue(DictionaryEntry, reader.ReadUnicodeString());
                                break;

                            case TAGDataType.tEmptyType:
                                sink.ReadEmptyValue(DictionaryEntry);
                                break;
                        }
                    }
                    catch // (Exception E)
                    {
                        // TODO add when logging is available
                        //SIGLogMessage.PublishNoODS(Self, Format('Exception in TagFile.ReadFile while reading field value: %s', [E.Message]), E, slmcException);
                        return TAGReadResult.InvalidValue;
                    }
                }

                if (!sink.ProcessingTerminated)
                {
                    if (!sink.Finishing())
                    {
                        return TAGReadResult.SinkFinishingFailure;
                    }

                    if (sink.ProcessingTerminated)
                    {
                        return TAGReadResult.ProcessingTerminated;
                    }
                }
            }
            catch (Exception E)
            {
                if (E is IOException)
                {
                    // TODO add then loggin available
                    // SIGLogMessage.Publish(Self, Format('Exception in TagFile.ReadFile: %s', [E.Message]), slmcDebug);
                    return TAGReadResult.CouldNotOpenFile;
                }

                throw;
            }

            return TAGReadResult.NoError;
        }
            
        /// <summary>
        /// Read the content of a TAG file and routes it to the provided sink
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sink"></param>
        /// <returns></returns>
        public TAGReadResult Read(string fileName, TAGValueSinkBase sink)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                TAGReader reader = new TAGReader(fs);
                return Read(reader, sink);
            }
        }
    }
}
