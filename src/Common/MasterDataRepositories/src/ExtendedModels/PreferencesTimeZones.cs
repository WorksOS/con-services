using System;
using System.Collections.Generic;
using System.Linq;

namespace VSS.MasterData.Repositories.ExtendedModels
{
  /// <summary>
  /// The collection of time zones used in the UI.  The services use this to validate project time zone names and to convert from a Windows time zone to an IANA time zone.
  /// </summary>
  public class PreferencesTimeZones
  {
    public static IEnumerable<string> WindowsTimeZoneNames() => preferencesTimeZones.timezones.Select(t => t.APIValue);
    
    public static string WindowsToIana(string windowsTimeZoneStandardName)
    {
      if (string.IsNullOrEmpty(windowsTimeZoneStandardName))
        return string.Empty;

      var tz = preferencesTimeZones.timezones
        .SingleOrDefault(t => t.APIValue.Equals(windowsTimeZoneStandardName, StringComparison.OrdinalIgnoreCase)); 
      return tz?.momentTimezone;
    }

    public static string IanaToWindows(string ianaZoneId)
    {
      if (string.IsNullOrEmpty(ianaZoneId))
        return string.Empty;

      var tz = preferencesTimeZones.timezones
        .SingleOrDefault(t => t.momentTimezone.Equals(ianaZoneId, StringComparison.OrdinalIgnoreCase)); 
      return tz?.APIValue;
    }

    private List<PreferencesTimeZone> timezones;

    private static readonly PreferencesTimeZones preferencesTimeZones = new PreferencesTimeZones { 
      timezones = new List<PreferencesTimeZone>{
        new PreferencesTimeZone {
          displayName = "(UTC) Casablanca",
          APIValue = "Morocco Standard Time",
          localeValue = "(GMT) Casablanca",
          token = "MOROCCO_STANDARD_TIME",
          momentTimezone = "Africa/Casablanca"
        },
        new PreferencesTimeZone {
          displayName = "(UTC) Coordinated Universal Time",
          APIValue = "Coordinated Universal Time",
          localeValue = "(GMT) Coordinated Universal Time",
          token = "COORDINATED_UNIVERSAL_TIME",
          momentTimezone = "Etc/GMT"
        },
        new PreferencesTimeZone {
          displayName = "(UTC) Dublin, Edinburgh, Lisbon, London",
          APIValue = "GMT Standard Time",
          localeValue = "(GMT) Greenwich Mean Time : Dublin, Edinburgh, Lisbon, London",
          token = "GMTSTANDARD_TIME",
          momentTimezone = "Europe/London"
        },
        new PreferencesTimeZone {
          displayName = "(UTC) Monrovia, Reykjavik",
          APIValue = "Greenwich Standard Time",
          localeValue = "(GMT) Monrovia, Reykjavik",
          token = "GREENWICH_STANDARD_TIME",
          momentTimezone = "Atlantic/Reykjavik"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna",
          APIValue = "W. Europe Standard Time",
          localeValue = "(GMT+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna",
          token = "WEUROPE_STANDARD_TIME",
          momentTimezone = "Europe/Berlin"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague",
          APIValue = "Central Europe Standard Time",
          localeValue = "(GMT+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague",
          token = "CENTRAL_EUROPE_STANDARD_TIME",
          momentTimezone = "Europe/Budapest"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+01:00) Brussels, Copenhagen, Madrid, Paris",
          APIValue = "Romance Standard Time",
          localeValue = "(GMT+01:00) Brussels, Copenhagen, Madrid, Paris",
          token = "ROMANCE_STANDARD_TIME",
          momentTimezone = "Europe/Paris"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+01:00) Sarajevo, Skopje, Warsaw, Zagreb",
          APIValue = "Central European Standard Time",
          localeValue = "(GMT+01:00) Sarajevo, Skopje, Warsaw, Zagreb",
          token = "CENTRAL_EUROPEAN_STANDARD_TIME",
          momentTimezone = "Europe/Warsaw"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+01:00) West Central Africa",
          APIValue = "W. Central Africa Standard Time",
          localeValue = "(GMT+01:00) West Central Africa",
          token = "WCENTRAL_AFRICA_STANDARD_TIME",
          momentTimezone = "Africa/Lagos"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+01:00) Windhoek",
          APIValue = "Namibia Standard Time",
          localeValue = "(GMT+01:00) Windhoek",
          token = "NAMIBIA_STANDARD_TIME",
          momentTimezone = "Africa/Windhoek"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+02:00) Amman",
          APIValue = "Jordan Standard Time",
          localeValue = "(GMT+02:00) Amman",
          token = "JORDAN_STANDARD_TIME",
          momentTimezone = "Asia/Amman"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+02:00) Athens, Bucharest",
          APIValue = "GTB Standard Time",
          localeValue = "(GMT+02:00) Athens, Bucharest, Istanbul",
          token = "GTBSTANDARD_TIME",
          momentTimezone = "Europe/Bucharest"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+02:00) Beirut",
          APIValue = "Middle East Standard Time",
          localeValue = "(GMT+02:00) Beirut",
          token = "MIDDLE_EAST_STANDARD_TIME",
          momentTimezone = "Asia/Beirut"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+02:00) Cairo",
          APIValue = "Egypt Standard Time",
          localeValue = "(GMT+02:00) Cairo",
          token = "EGYPT_STANDARD_TIME",
          momentTimezone = "Africa/Cairo"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+02:00) Damascus",
          APIValue = "Syria Standard Time",
          localeValue = "(GMT+02:00) Damascus",
          token = "SYRIA_STANDARD_TIME",
          momentTimezone = "Asia/Damascus"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+02:00) E. Europe",
          APIValue = "E. Europe Standard Time",
          localeValue = "(GMT+02:00) Minsk",
          token = "EEUROPE_STANDARD_TIME",
          momentTimezone = "Europe/Chisinau"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+02:00) Harare, Pretoria",
          APIValue = "South Africa Standard Time",
          localeValue = "(GMT+02:00) Harare, Pretoria",
          token = "SOUTH_AFRICA_STANDARD_TIME",
          momentTimezone = "Africa/Johannesburg"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+02:00) Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius",
          APIValue = "FLE Standard Time",
          localeValue = "(GMT+02:00) Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius",
          token = "FLESTANDARD_TIME",
          momentTimezone = "Europe/Kiev"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+02:00) Istanbul",
          APIValue = "Turkey Standard Time",
          localeValue = "(GMT+02:00) Istanbul",
          token = "TURKEY_STANDARD_TIME",
          momentTimezone = "Europe/Istanbul"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+02:00) Jerusalem",
          APIValue = "Jerusalem Standard Time",
          localeValue = "(GMT+02:00) Jerusalem",
          token = "JERUSALEM_STANDARD_TIME",
          momentTimezone = "Asia/Jerusalem"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+02:00) Kaliningrad (RTZ 1)",
          APIValue = "Russia TZ 1 Standard Time",
          localeValue = "(UTC+02:00) Kaliningrad (RTZ 1)",
          token = "RUSSIA_TZ1STANDARD_TIME",
          momentTimezone = "Europe/Kaliningrad"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+02:00) Tripoli",
          APIValue = "Libya Standard Time",
          localeValue = "(UTC+02:00) Tripoli",
          token = "LIBYA_STANDARD_TIME",
          momentTimezone = "Africa/Tripoli"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+03:00) Baghdad",
          APIValue = "Arabic Standard Time",
          localeValue = "(GMT+03:00) Baghdad",
          token = "ARABIC_STANDARD_TIME",
          momentTimezone = "Asia/Baghdad"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+03:00) Kuwait, Riyadh",
          APIValue = "Arab Standard Time",
          localeValue = "(GMT+03:00) Kuwait, Riyadh",
          token = "ARAB_STANDARD_TIME",
          momentTimezone = "Asia/Riyadh"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+03:00) Minsk",
          APIValue = "Belarus Standard Time",
          localeValue = "(UTC+03:00) Minsk",
          token = "BELARUS_STANDARD_TIME",
          momentTimezone = "Europe/Minsk"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+03:00) Moscow, St. Petersburg, Volgograd (RTZ 2)",
          APIValue = "Russia TZ 2 Standard Time",
          localeValue = "(UTC+03:00) Moscow, St. Petersburg, Volgograd (RTZ 2)",
          token = "RUSSIA_TZ2STANDARD_TIME",
          momentTimezone = "Europe/Moscow"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+03:00) Nairobi",
          APIValue = "E. Africa Standard Time",
          localeValue = "(GMT+03:00) Nairobi",
          token = "EAFRICA_STANDARD_TIME",
          momentTimezone = "Africa/Nairobi"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+03:30) Tehran",
          APIValue = "Iran Standard Time",
          localeValue = "(GMT+03:30) Tehran",
          token = "IRAN_STANDARD_TIME",
          momentTimezone = "Asia/Tehran"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+04:00) Abu Dhabi, Muscat",
          APIValue = "Arabian Standard Time",
          localeValue = "(GMT+04:00) Abu Dhabi, Muscat",
          token = "ARABIAN_STANDARD_TIME",
          momentTimezone = "Asia/Dubai"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+04:00) Baku",
          APIValue = "Azerbaijan Standard Time",
          localeValue = "(GMT+04:00) Baku",
          token = "AZERBAIJAN_STANDARD_TIME",
          momentTimezone = "Asia/Baku"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+04:00) Izhevsk, Samara (RTZ 3)",
          APIValue = "Russia TZ 3 Standard Time",
          localeValue = "(UTC+04:00) Izhevsk, Samara (RTZ 3)",
          token = "RUSSIA_TZ3STANDARD_TIME",
          momentTimezone = "Europe/Samara"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+04:00) Port Louis",
          APIValue = "Mauritius Standard Time",
          localeValue = "(GMT+04:00) Port Louis",
          token = "MAURITIUS_STANDARD_TIME",
          momentTimezone = "Indian/Mauritius"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+04:00) Tbilisi",
          APIValue = "Georgian Standard Time",
          localeValue = "(GMT+04:00) Tbilisi",
          token = "GEORGIAN_STANDARD_TIME",
          momentTimezone = "Asia/Tbilisi"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+04:00) Yerevan",
          APIValue = "Caucasus Standard Time",
          localeValue = "(GMT+04:00) Yerevan",
          token = "CAUCASUS_STANDARD_TIME",
          momentTimezone = "Asia/Yerevan"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+04:30) Kabul",
          APIValue = "Afghanistan Standard Time",
          localeValue = "(GMT+04:30) Kabul",
          token = "AFGHANISTAN_STANDARD_TIME",
          momentTimezone = "Asia/Kabul"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+05:00) Ashgabat, Tashkent",
          APIValue = "West Asia Standard Time",
          localeValue = "(GMT+05:00) Tashkent",
          token = "WEST_ASIA_STANDARD_TIME",
          momentTimezone = "Asia/Tashkent"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+05:00) Ekaterinburg (RTZ 4)",
          APIValue = "Russia TZ 4 Standard Time",
          localeValue = "(UTC+05:00) Ekaterinburg (RTZ 4)",
          token = "RUSSIA_TZ4STANDARD_TIME",
          momentTimezone = "Asia/Yekaterinburg"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+05:00) Islamabad, Karachi",
          APIValue = "Pakistan Standard Time",
          localeValue = "(GMT+05:00) Islamabad, Karachi",
          token = "PAKISTAN_STANDARD_TIME",
          momentTimezone = "Asia/Karachi"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+05:30) Chennai, Kolkata, Mumbai, New Delhi",
          APIValue = "India Standard Time",
          localeValue = "(GMT+05:30) Chennai, Kolkata, Mumbai, New Delhi",
          token = "INDIA_STANDARD_TIME",
          momentTimezone = "Asia/Kolkata"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+05:30) Sri Jayawardenepura",
          APIValue = "Sri Lanka Standard Time",
          localeValue = "(GMT+05:30) Sri Jayawardenepura",
          token = "SRI_LANKA_STANDARD_TIME",
          momentTimezone = "Asia/Colombo"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+05:45) Kathmandu",
          APIValue = "Nepal Standard Time",
          localeValue = "(GMT+05:45) Kathmandu",
          token = "NEPAL_STANDARD_TIME",
          momentTimezone = "Asia/Kathmandu"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+06:00) Astana",
          APIValue = "Central Asia Standard Time",
          localeValue = "(GMT+06:00) Astana, Dhaka",
          token = "CENTRAL_ASIA_STANDARD_TIME",
          momentTimezone = "Asia/Almaty"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+06:00) Dhaka",
          APIValue = "Bangladesh Standard Time",
          localeValue = "(GMT+06:00) Dhaka",
          token = "BANGLADESH_STANDARD_TIME",
          momentTimezone = "Asia/Dhaka"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+06:00) Novosibirsk (RTZ 5)",
          APIValue = "Russia TZ 5 Standard Time",
          localeValue = "(UTC+06:00) Novosibirsk (RTZ 5)",
          token = "RUSSIA_TZ5STANDARD_TIME",
          momentTimezone = "Asia/Novosibirsk"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+06:30) Yangon (Rangoon)",
          APIValue = "Myanmar Standard Time",
          localeValue = "(GMT+06:30) Yangon (Rangoon)",
          token = "MYANMAR_STANDARD_TIME",
          momentTimezone = "Asia/Rangoon"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+07:00) Bangkok, Hanoi, Jakarta",
          APIValue = "SE Asia Standard Time",
          localeValue = "(GMT+07:00) Bangkok, Hanoi, Jakarta",
          token = "SEASIA_STANDARD_TIME",
          momentTimezone = "Asia/Bangkok"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+07:00) Krasnoyarsk (RTZ 6)",
          APIValue = "Russia TZ 6 Standard Time",
          localeValue = "(UTC+07:00) Krasnoyarsk (RTZ 6)",
          token = "RUSSIA_TZ6STANDARD_TIME",
          momentTimezone = "Asia/Krasnoyarsk"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi",
          APIValue = "China Standard Time",
          localeValue = "(GMT+08:00) Beijing, Chongqing, Hong Kong, Urumqi",
          token = "CHINA_STANDARD_TIME",
          momentTimezone = "Asia/Shanghai"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+08:00) Irkutsk (RTZ 7)",
          APIValue = "Russia TZ 7 Standard Time",
          localeValue = "(UTC+08:00) Irkutsk (RTZ 7)",
          token = "RUSSIA_TZ7STANDARD_TIME",
          momentTimezone = "Asia/Irkutsk"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+08:00) Kuala Lumpur, Singapore",
          APIValue = "Malay Peninsula Standard Time",
          localeValue = "(GMT+08:00) Kuala Lumpur, Singapore",
          token = "MALAY_PENINSULA_STANDARD_TIME",
          momentTimezone = "Asia/Singapore"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+08:00) Perth",
          APIValue = "W. Australia Standard Time",
          localeValue = "(GMT+08:00) Perth",
          token = "WAUSTRALIA_STANDARD_TIME",
          momentTimezone = "Australia/Perth"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+08:00) Taipei",
          APIValue = "Taipei Standard Time",
          localeValue = "(GMT+08:00) Taipei",
          token = "TAIPEI_STANDARD_TIME",
          momentTimezone = "Asia/Taipei"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+08:00) Ulaanbaatar",
          APIValue = "Ulaanbaatar Standard Time",
          localeValue = "(GMT+08:00) Ulaanbaatar",
          token = "ULAANBAATAR_STANDARD_TIME",
          momentTimezone = "Asia/Ulaanbaatar"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+09:00) Osaka, Sapporo, Tokyo",
          APIValue = "Tokyo Standard Time",
          localeValue = "(GMT+09:00) Osaka, Sapporo, Tokyo",
          token = "TOKYO_STANDARD_TIME",
          momentTimezone = "Asia/Tokyo"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+09:00) Seoul",
          APIValue = "Korea Standard Time",
          localeValue = "(GMT+09:00) Seoul",
          token = "KOREA_STANDARD_TIME",
          momentTimezone = "Asia/Seoul"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+09:00) Yakutsk (RTZ 8)",
          APIValue = "Russia TZ 8 Standard Time",
          localeValue = "(UTC+09:00) Yakutsk (RTZ 8)",
          token = "RUSSIA_TZ8STANDARD_TIME",
          momentTimezone = "Asia/Yakutsk"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+09:30) Adelaide",
          APIValue = "Cen. Australia Standard Time",
          localeValue = "(GMT+09:30) Adelaide",
          token = "CEN_AUSTRALIA_STANDARD_TIME",
          momentTimezone = "Australia/Adelaide"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+09:30) Darwin",
          APIValue = "AUS Central Standard Time",
          localeValue = "(GMT+09:30) Darwin",
          token = "AUSCENTRAL_STANDARD_TIME",
          momentTimezone = "Australia/Darwin"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+10:00) Brisbane",
          APIValue = "E. Australia Standard Time",
          localeValue = "(GMT+10:00) Brisbane",
          token = "EAUSTRALIA_STANDARD_TIME",
          momentTimezone = "Australia/Brisbane"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+10:00) Canberra, Melbourne, Sydney",
          APIValue = "AUS Eastern Standard Time",
          localeValue = "(GMT+10:00) Canberra, Melbourne, Sydney",
          token = "AUSEASTERN_STANDARD_TIME",
          momentTimezone = "Australia/Sydney"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+10:00) Guam, Port Moresby",
          APIValue = "West Pacific Standard Time",
          localeValue = "(GMT+10:00) Guam, Port Moresby",
          token = "WEST_PACIFIC_STANDARD_TIME",
          momentTimezone = "Pacific/Port_Moresby"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+10:00) Hobart",
          APIValue = "Tasmania Standard Time",
          localeValue = "(GMT+10:00) Hobart",
          token = "TASMANIA_STANDARD_TIME",
          momentTimezone = "Australia/Hobart"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+10:00) Magadan",
          APIValue = "Magadan Standard Time",
          localeValue = "(GMT+12:00) Magadan",
          token = "MAGADAN_STANDARD_TIME",
          momentTimezone = "Asia/Magadan"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+10:00) Vladivostok, Magadan (RTZ 9)",
          APIValue = "Russia TZ 9 Standard Time",
          localeValue = "(UTC+10:00) Vladivostok, Magadan (RTZ 9)",
          token = "RUSSIA_TZ9STANDARD_TIME",
          momentTimezone = "Asia/Vladivostok"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+11:00) Chokurdakh (RTZ 10)",
          APIValue = "Russia TZ 10 Standard Time",
          localeValue = "(UTC+11:00) Chokurdakh (RTZ 10)",
          token = "RUSSIA_TZ10STANDARD_TIME",
          momentTimezone = "Asia/Srednekolymsk"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+11:00) Solomon Is., New Caledonia",
          APIValue = "Central Pacific Standard Time",
          localeValue = "(GMT+11:00) Solomon Is., New Caledonia",
          token = "CENTRAL_PACIFIC_STANDARD_TIME",
          momentTimezone = "Pacific/Guadalcanal"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+12:00) Anadyr, Petropavlovsk-Kamchatsky (RTZ 11)",
          APIValue = "Russia TZ 11 Standard Time",
          localeValue = "(UTC+12:00) Anadyr, Petropavlovsk-Kamchatsky (RTZ 11)",
          token = "RUSSIA_TZ11STANDARD_TIME",
          momentTimezone = "Asia/Anadyr"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+12:00) Auckland, Wellington",
          APIValue = "New Zealand Standard Time",
          localeValue = "(GMT+12:00) Auckland, Wellington",
          token = "NEW_ZEALAND_STANDARD_TIME",
          momentTimezone = "Pacific/Auckland"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+12:00) Coordinated Universal Time+12",
          APIValue = "UTC+12",
          localeValue = "(GMT+12:00) Coordinated Universal Time+12",
          token = "UTC12",
          momentTimezone = "Etc/GMT-12"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+12:00) Fiji",
          APIValue = "Fiji Standard Time",
          localeValue = "(GMT+12:00) Fiji",
          token = "FIJI_STANDARD_TIME",
          momentTimezone = "Pacific/Fiji"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+12:00) Petropavlovsk-Kamchatsky - Old",
          APIValue = "Kamchatka Standard Time",
          localeValue = "(GMT+12:00) Petropavlovsk-Kamchatsky",
          token = "KAMCHATKA_STANDARD_TIME",
          momentTimezone = "Asia/Kamchatka"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+13:00) Nuku'alofa",
          APIValue = "Tonga Standard Time",
          localeValue = "(GMT+13:00) Nuku'alofa",
          token = "TONGA_STANDARD_TIME",
          momentTimezone = "Pacific/Tongatapu"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+13:00) Samoa",
          APIValue = "Samoa Standard Time",
          localeValue = "(GMT+13:00) Samoa",
          token = "SAMOA_STANDARD_TIME",
          momentTimezone = "Pacific/Apia"
        },
        new PreferencesTimeZone {
          displayName = "(UTC+14:00) Kiritimati Island",
          APIValue = "Line Islands Standard Time",
          localeValue = "(UTC+14:00) Kiritimati Island",
          token = "LINE_ISLANDS_STANDARD_TIME",
          momentTimezone = "Pacific/Kiritimati"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-01:00) Azores",
          APIValue = "Azores Standard Time",
          localeValue = "(GMT-01:00) Azores",
          token = "AZORES_STANDARD_TIME",
          momentTimezone = "Atlantic/Azores"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-01:00) Cabo Verde Is.",
          APIValue = "Cabo Verde Standard Time",
          localeValue = "(UTC-01:00) Cabo Verde Is.",
          token = "CABO_VERDE_STANDARD_TIME",
          momentTimezone = "Atlantic/Cape_Verde"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-02:00) Coordinated Universal Time-02",
          APIValue = "UTC-02",
          localeValue = "(GMT-02:00) Coordinated Universal Time-02",
          token = "UTCHYPHEN02",
          momentTimezone = "Etc/GMT+2"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-03:00) Brasilia",
          APIValue = "E. South America Standard Time",
          localeValue = "(GMT-03:00) Brasilia",
          token = "ESOUTH_AMERICA_STANDARD_TIME",
          momentTimezone = "America/Sao_Paulo"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-03:00) Buenos Aires",
          APIValue = "Argentina Standard Time",
          localeValue = "(GMT-03:00) Buenos Aires",
          token = "ARGENTINA_STANDARD_TIME",
          momentTimezone = "America/Buenos_Aires"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-03:00) Cayenne, Fortaleza",
          APIValue = "SA Eastern Standard Time",
          localeValue = "(GMT-03:00) Cayenne, Fortaleza",
          token = "SAEASTERN_STANDARD_TIME",
          momentTimezone = "America/Cayenne"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-03:00) Greenland",
          APIValue = "Greenland Standard Time",
          localeValue = "(GMT-03:00) Greenland",
          token = "GREENLAND_STANDARD_TIME",
          momentTimezone = "America/Godthab"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-03:00) Montevideo",
          APIValue = "Montevideo Standard Time",
          localeValue = "(GMT-03:00) Montevideo",
          token = "MONTEVIDEO_STANDARD_TIME",
          momentTimezone = "America/Montevideo"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-03:00) Salvador",
          APIValue = "Bahia Standard Time",
          localeValue = "(GMT-03:00) Salvador",
          token = "BAHIA_STANDARD_TIME",
          momentTimezone = "America/Bahia"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-03:00) Santiago",
          APIValue = "Pacific SA Standard Time",
          localeValue = "(GMT-04:00) Santiago",
          token = "PACIFIC_SASTANDARD_TIME",
          momentTimezone = "America/Santiago"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-03:30) Newfoundland",
          APIValue = "Newfoundland Standard Time",
          localeValue = "(GMT-03:30) Newfoundland",
          token = "NEWFOUNDLAND_STANDARD_TIME",
          momentTimezone = "America/St_Johns"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-04:00) Asuncion",
          APIValue = "Paraguay Standard Time",
          localeValue = "(GMT-04:00) Asuncion",
          token = "PARAGUAY_STANDARD_TIME",
          momentTimezone = "America/Asuncion"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-04:00) Atlantic Time (Canada)",
          APIValue = "Atlantic Standard Time",
          localeValue = "(GMT-04:00) Atlantic Time (Canada)",
          token = "ATLANTIC_STANDARD_TIME",
          momentTimezone = "America/Halifax"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-04:00) Cuiaba",
          APIValue = "Central Brazilian Standard Time",
          localeValue = "(GMT-04:00) Cuiaba",
          token = "CENTRAL_BRAZILIAN_STANDARD_TIME",
          momentTimezone = "America/Cuiaba"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-04:00) Georgetown, La Paz, Manaus, San Juan",
          APIValue = "SA Western Standard Time",
          localeValue = "(GMT-04:00) Georgetown, La Paz, Manaus, San Juan",
          token = "SAWESTERN_STANDARD_TIME",
          momentTimezone = "America/La_Paz"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-04:30) Caracas",
          APIValue = "Venezuela Standard Time",
          localeValue = "(GMT-04:30) Caracas",
          token = "VENEZUELA_STANDARD_TIME",
          momentTimezone = "America/Caracas"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-05:00) Bogota, Lima, Quito, Rio Branco",
          APIValue = "SA Pacific Standard Time",
          localeValue = "(GMT-05:00) Bogota, Lima, Quito",
          token = "SAPACIFIC_STANDARD_TIME",
          momentTimezone = "America/Bogota"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-05:00) Chetumal",
          APIValue = "Eastern Standard Time (Mexico)",
          localeValue = "(UTC-05:00) Chetumal",
          token = "EASTERN_STANDARD_TIME(MEXICO)",
          momentTimezone = "America/Cancun"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-05:00) Eastern Time (US & Canada)",
          APIValue = "Eastern Standard Time",
          localeValue = "(GMT-05:00) Eastern Time (US & Canada)",
          token = "EASTERN_STANDARD_TIME",
          momentTimezone = "America/New_York"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-05:00) Indiana (East)",
          APIValue = "US Eastern Standard Time",
          localeValue = "(GMT-05:00) Indiana (East)",
          token = "USEASTERN_STANDARD_TIME",
          momentTimezone = "America/Indianapolis"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-06:00) Central America",
          APIValue = "Central America Standard Time",
          localeValue = "(GMT-06:00) Central America",
          token = "CENTRAL_AMERICA_STANDARD_TIME",
          momentTimezone = "America/Costa_Rica"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-06:00) Central Time (US & Canada)",
          APIValue = "Central Standard Time",
          localeValue = "(GMT-06:00) Central Time (US & Canada)",
          token = "CENTRAL_STANDARD_TIME",
          momentTimezone = "America/Chicago"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-06:00) Guadalajara, Mexico City, Monterrey",
          APIValue = "Central Standard Time (Mexico)",
          localeValue = "(GMT-06:00) Guadalajara, Mexico City, Monterrey",
          token = "CENTRAL_STANDARD_TIME(MEXICO)",
          momentTimezone = "America/Mexico_City"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-06:00) Saskatchewan",
          APIValue = "Canada Central Standard Time",
          localeValue = "(GMT-06:00) Saskatchewan",
          token = "CANADA_CENTRAL_STANDARD_TIME",
          momentTimezone = "America/Regina"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-07:00) Arizona",
          APIValue = "US Mountain Standard Time",
          localeValue = "(GMT-07:00) Arizona",
          token = "USMOUNTAIN_STANDARD_TIME",
          momentTimezone = "America/Phoenix"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-07:00) Chihuahua, La Paz, Mazatlan",
          APIValue = "Mountain Standard Time (Mexico)",
          localeValue = "(GMT-07:00) Chihuahua, La Paz, Mazatlan",
          token = "MOUNTAIN_STANDARD_TIME(MEXICO)",
          momentTimezone = "America/Chihuahua"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-07:00) Mountain Time (US & Canada)",
          APIValue = "Mountain Standard Time",
          localeValue = "(GMT-07:00) Mountain Time (US & Canada)",
          token = "MOUNTAIN_STANDARD_TIME",
          momentTimezone = "America/Denver"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-08:00) Baja California",
          APIValue = "Pacific Standard Time (Mexico)",
          localeValue = "(GMT-08:00) Baja California",
          token = "PACIFIC_STANDARD_TIME(MEXICO)",
          momentTimezone = "America/Tijuana"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-08:00) Pacific Time (US & Canada)",
          APIValue = "Pacific Standard Time",
          localeValue = "(GMT-08:00) Pacific Time (US & Canada)",
          token = "PACIFIC_STANDARD_TIME",
          momentTimezone = "America/Los_Angeles"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-09:00) Alaska",
          APIValue = "Alaskan Standard Time",
          localeValue = "(GMT-09:00) Alaska",
          token = "ALASKAN_STANDARD_TIME",
          momentTimezone = "America/Anchorage"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-10:00) Hawaii",
          APIValue = "Hawaiian Standard Time",
          localeValue = "(GMT-10:00) Hawaii",
          token = "HAWAIIAN_STANDARD_TIME",
          momentTimezone = "Pacific/Honolulu"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-11:00) Coordinated Universal Time-11",
          APIValue = "UTC-11",
          localeValue = "(GMT-11:00) Coordinated Universal Time-11",
          token = "UTCHYPHEN11",
          momentTimezone = "Pacific/Niue"
        },
        new PreferencesTimeZone {
          displayName = "(UTC-12:00) International Date Line West",
          APIValue = "Dateline Standard Time",
          localeValue = "(GMT-12:00) International Date Line West",
          token = "DATELINE_STANDARD_TIME",
          momentTimezone = "Etc/GMT+12"
        }
      }
    };

    /// <summary>
    /// Time zone model used by UI. APIValue is the Windows standard time zone name. momentTimezone is the IANA time zone.
    /// </summary>
    private class PreferencesTimeZone
    {
      public string displayName { get; set; }
      public string APIValue { get; set; }
      public string localeValue { get; set; }
      public string token { get; set; }
      public string momentTimezone { get; set; }
    }
  }

 
}
