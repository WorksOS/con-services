namespace TagFiles.Common
{
  /// <summary>
  /// Constants common to all classes
  /// </summary>
  public static class TagConstants
  {
    public const short CALLBACK_PARSE_PACKET = 0;
    public const short CALLBACK_CONNECTION_MADE = 1;
    public const int TAG_NAME_LENGHT = 3;
    public const double TAG_FILE_MONITOR_SECS = 10; 
    public const byte SOH = 0x01;
    public const byte STX = 0x02;
    public const byte ETX = 0x03;
    public const byte EOT = 0x04;
    public const byte ENQ = 0x05;
    public const byte ACK = 0x06;
    public const byte NAK = 0x15;
    public const byte RS  = 0x1E;

    public const char CHAR_ETX = (char)TagConstants.ETX;
    public const char CHAR_STX = (char)TagConstants.STX;
    public const char CHAR_ACK = (char)TagConstants.ACK;
    public const char CHAR_ENQ = (char)TagConstants.ENQ;
    public const char CHAR_EOT = (char)TagConstants.EOT;
    public const char CHAR_RS  = (char)TagConstants.RS;

    public const string TIME = "TME";
    public const string LEFT_EASTING_BLADE = "LEB";
    public const string LEFT_NORTHING_BLADE = "LNB";
    public const string LEFT_HEIGHT_BLADE = "LHB";
    public const string RIGHT_EASTING_BLADE = "REB";
    public const string RIGHT_NORTHING_BLADE = "RNB";
    public const string RIGHT_HEIGHT_BLADE = "RHB";
    public const string GPS_MODE = "GPM";
    public const string BLADE_ON_GROUND = "BOG";
    public const string DESIGN = "DES";
    public const string LATITUDE = "LAT";
    public const string LONTITUDE = "LON";
    public const string HEIGHT = "HGT";
    public const string MESSAGE_ID = "MID";
    public const string MACHINE_SPEED = "MSD";
    public const string MACHINE_TYPE = "MTP";
    public const string HEADING = "HDG";
    public const string SERIAL = "SER";
    public const string UTM = "UTM";

    public const string TAGFILE_FOLDER = "Tagfiles";
    public const string TAGFILE_FOLDER_TOSEND = "ToSend";
    public const string LOG_FOLDER = "Logs";

  }
}
