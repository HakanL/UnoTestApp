using System;
using System.Collections.Generic;
using System.Text;

namespace DMXCore.DMXCore100.Common;

public struct Consts
{
    public const int LongPressMilliseconds = 1_000;

    public const string PrimaryButtonName = "PrimaryButton";

    public const string SecondaryButtonName = "SecondaryButton";

    public const string PluginNameQSys = "QSYS";

    public const string PluginNameSymetrix = "SYMETRIX";

    public const string CloudApiKey = "g6wcG}`ey>|P)J*zZb?pJI)puJ4?wR";

    public const string ExternalStorageCloudIdentifier = "CLOUD";

    public struct FileTypes
    {
        public const string DeviceBackup = "DEVICEBACKUP";
    }

    public struct Tags
    {
        public const string OperatingEnvironment = "env";

        public const string LicenseString = "lic";

        public const string HardwareInfo = "hw";
    }

    public static IList<TimeZoneOption> TimeZones => [
            new TimeZoneOption("Azores Standard Time", "(UTC-01:00) Azores"),
            new TimeZoneOption("Cape Verde Standard Time","(UTC-01:00) Cabo Verde Is."),
            new TimeZoneOption("UTC-02","(UTC-02:00) Coordinated Universal Time-02"),
            new TimeZoneOption("Greenland Standard Time","(UTC-02:00) Greenland"),
            new TimeZoneOption("Mid-Atlantic Standard Time","(UTC-02:00) Mid-Atlantic - Old"),
            new TimeZoneOption("Tocantins Standard Time","(UTC-03:00) Araguaina"),
            new TimeZoneOption("E. South America Standard Time","(UTC-03:00) Brasilia"),
            new TimeZoneOption("SA Eastern Standard Time","(UTC-03:00) Cayenne, Fortaleza"),
            new TimeZoneOption("Argentina Standard Time","(UTC-03:00) City of Buenos Aires"),
            new TimeZoneOption("Montevideo Standard Time","(UTC-03:00) Montevideo"),
            new TimeZoneOption("Magallanes Standard Time","(UTC-03:00) Punta Arenas"),
            new TimeZoneOption("Saint Pierre Standard Time","(UTC-03:00) Saint Pierre and Miquelon"),
            new TimeZoneOption("Bahia Standard Time","(UTC-03:00) Salvador"),
            new TimeZoneOption("Newfoundland Standard Time","(UTC-03:30) Newfoundland"),
            new TimeZoneOption("Paraguay Standard Time","(UTC-04:00) Asuncion"),
            new TimeZoneOption("Atlantic Standard Time","(UTC-04:00) Atlantic Time (Canada)"),
            new TimeZoneOption("Venezuela Standard Time","(UTC-04:00) Caracas"),
            new TimeZoneOption("Central Brazilian Standard Time","(UTC-04:00) Cuiaba"),
            new TimeZoneOption("SA Western Standard Time","(UTC-04:00) Georgetown, La Paz, Manaus, San Juan"),
            new TimeZoneOption("Pacific SA Standard Time","(UTC-04:00) Santiago"),
            new TimeZoneOption("SA Pacific Standard Time","(UTC-05:00) Bogota, Lima, Quito, Rio Branco"),
            new TimeZoneOption("Eastern Standard Time (Mexico)","(UTC-05:00) Chetumal"),
            new TimeZoneOption("Eastern Standard Time","(UTC-05:00) Eastern Time (US & Canada)"),
            new TimeZoneOption("Haiti Standard Time","(UTC-05:00) Haiti"),
            new TimeZoneOption("Cuba Standard Time","(UTC-05:00) Havana"),
            new TimeZoneOption("US Eastern Standard Time","(UTC-05:00) Indiana (East)"),
            new TimeZoneOption("Turks And Caicos Standard Time","(UTC-05:00) Turks and Caicos"),
            new TimeZoneOption("Central America Standard Time","(UTC-06:00) Central America"),
            new TimeZoneOption("Central Standard Time","(UTC-06:00) Central Time (US & Canada)"),
            new TimeZoneOption("Easter Island Standard Time","(UTC-06:00) Easter Island"),
            new TimeZoneOption("Central Standard Time (Mexico)","(UTC-06:00) Guadalajara, Mexico City, Monterrey"),
            new TimeZoneOption("Canada Central Standard Time","(UTC-06:00) Saskatchewan"),
            new TimeZoneOption("US Mountain Standard Time","(UTC-07:00) Arizona"),
            new TimeZoneOption("Mountain Standard Time (Mexico)","(UTC-07:00) La Paz, Mazatlan"),
            new TimeZoneOption("Mountain Standard Time","(UTC-07:00) Mountain Time (US & Canada)"),
            new TimeZoneOption("Yukon Standard Time","(UTC-07:00) Yukon"),
            new TimeZoneOption("Pacific Standard Time (Mexico)","(UTC-08:00) Baja California"),
            new TimeZoneOption("UTC-08","(UTC-08:00) Coordinated Universal Time-08"),
            new TimeZoneOption("Pacific Standard Time","(UTC-08:00) Pacific Time (US & Canada)"),
            new TimeZoneOption("Alaskan Standard Time","(UTC-09:00) Alaska"),
            new TimeZoneOption("UTC-09","(UTC-09:00) Coordinated Universal Time-09"),
            new TimeZoneOption("Marquesas Standard Time","(UTC-09:30) Marquesas Islands"),
            new TimeZoneOption("Aleutian Standard Time","(UTC-10:00) Aleutian Islands"),
            new TimeZoneOption("Hawaiian Standard Time","(UTC-10:00) Hawaii"),
            new TimeZoneOption("UTC-11","(UTC-11:00) Coordinated Universal Time-11"),
            new TimeZoneOption("Dateline Standard Time","(UTC-12:00) International Date Line West"),
            new TimeZoneOption("UTC","(UTC) Coordinated Universal Time"),
            new TimeZoneOption("GMT Standard Time","(UTC+00:00) Dublin, Edinburgh, Lisbon, London"),
            new TimeZoneOption("Greenwich Standard Time","(UTC+00:00) Monrovia, Reykjavik"),
            new TimeZoneOption("Sao Tome Standard Time","(UTC+00:00) Sao Tome"),
            new TimeZoneOption("W. Europe Standard Time","(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna"),
            new TimeZoneOption("Central Europe Standard Time","(UTC+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague"),
            new TimeZoneOption("Romance Standard Time","(UTC+01:00) Brussels, Copenhagen, Madrid, Paris"),
            new TimeZoneOption("Morocco Standard Time","(UTC+01:00) Casablanca"),
            new TimeZoneOption("Central European Standard Time","(UTC+01:00) Sarajevo, Skopje, Warsaw, Zagreb"),
            new TimeZoneOption("W. Central Africa Standard Time","(UTC+01:00) West Central Africa"),
            new TimeZoneOption("GTB Standard Time","(UTC+02:00) Athens, Bucharest"),
            new TimeZoneOption("Middle East Standard Time","(UTC+02:00) Beirut"),
            new TimeZoneOption("Egypt Standard Time","(UTC+02:00) Cairo"),
            new TimeZoneOption("E. Europe Standard Time","(UTC+02:00) Chisinau"),
            new TimeZoneOption("West Bank Standard Time","(UTC+02:00) Gaza, Hebron"),
            new TimeZoneOption("South Africa Standard Time","(UTC+02:00) Harare, Pretoria"),
            new TimeZoneOption("FLE Standard Time","(UTC+02:00) Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius"),
            new TimeZoneOption("Israel Standard Time","(UTC+02:00) Jerusalem"),
            new TimeZoneOption("South Sudan Standard Time","(UTC+02:00) Juba"),
            new TimeZoneOption("Kaliningrad Standard Time","(UTC+02:00) Kaliningrad"),
            new TimeZoneOption("Sudan Standard Time","(UTC+02:00) Khartoum"),
            new TimeZoneOption("Libya Standard Time","(UTC+02:00) Tripoli"),
            new TimeZoneOption("Namibia Standard Time","(UTC+02:00) Windhoek"),
            new TimeZoneOption("Jordan Standard Time","(UTC+03:00) Amman"),
            new TimeZoneOption("Arabic Standard Time","(UTC+03:00) Baghdad"),
            new TimeZoneOption("Syria Standard Time","(UTC+03:00) Damascus"),
            new TimeZoneOption("Turkey Standard Time","(UTC+03:00) Istanbul"),
            new TimeZoneOption("Arab Standard Time","(UTC+03:00) Kuwait, Riyadh"),
            new TimeZoneOption("Belarus Standard Time","(UTC+03:00) Minsk"),
            new TimeZoneOption("Russian Standard Time","(UTC+03:00) Moscow, St. Petersburg"),
            new TimeZoneOption("E. Africa Standard Time","(UTC+03:00) Nairobi"),
            new TimeZoneOption("Volgograd Standard Time","(UTC+03:00) Volgograd"),
            new TimeZoneOption("Iran Standard Time","(UTC+03:30) Tehran"),
            new TimeZoneOption("Arabian Standard Time","(UTC+04:00) Abu Dhabi, Muscat"),
            new TimeZoneOption("Astrakhan Standard Time","(UTC+04:00) Astrakhan, Ulyanovsk"),
            new TimeZoneOption("Azerbaijan Standard Time","(UTC+04:00) Baku"),
            new TimeZoneOption("Russia Time Zone 3","(UTC+04:00) Izhevsk, Samara"),
            new TimeZoneOption("Mauritius Standard Time","(UTC+04:00) Port Louis"),
            new TimeZoneOption("Saratov Standard Time","(UTC+04:00) Saratov"),
            new TimeZoneOption("Georgian Standard Time","(UTC+04:00) Tbilisi"),
            new TimeZoneOption("Caucasus Standard Time","(UTC+04:00) Yerevan"),
            new TimeZoneOption("Afghanistan Standard Time","(UTC+04:30) Kabul"),
            new TimeZoneOption("West Asia Standard Time","(UTC+05:00) Ashgabat, Tashkent"),
            new TimeZoneOption("Qyzylorda Standard Time","(UTC+05:00) Astana"),
            new TimeZoneOption("Ekaterinburg Standard Time","(UTC+05:00) Ekaterinburg"),
            new TimeZoneOption("Pakistan Standard Time","(UTC+05:00) Islamabad, Karachi"),
            new TimeZoneOption("India Standard Time","(UTC+05:30) Chennai, Kolkata, Mumbai, New Delhi"),
            new TimeZoneOption("Sri Lanka Standard Time","(UTC+05:30) Sri Jayawardenepura"),
            new TimeZoneOption("Nepal Standard Time","(UTC+05:45) Kathmandu"),
            new TimeZoneOption("Central Asia Standard Time","(UTC+06:00) Bishkek"),
            new TimeZoneOption("Bangladesh Standard Time","(UTC+06:00) Dhaka"),
            new TimeZoneOption("Omsk Standard Time","(UTC+06:00) Omsk"),
            new TimeZoneOption("Myanmar Standard Time","(UTC+06:30) Yangon (Rangoon)"),
            new TimeZoneOption("SE Asia Standard Time","(UTC+07:00) Bangkok, Hanoi, Jakarta"),
            new TimeZoneOption("Altai Standard Time","(UTC+07:00) Barnaul, Gorno-Altaysk"),
            new TimeZoneOption("W. Mongolia Standard Time","(UTC+07:00) Hovd"),
            new TimeZoneOption("North Asia Standard Time","(UTC+07:00) Krasnoyarsk"),
            new TimeZoneOption("N. Central Asia Standard Time","(UTC+07:00) Novosibirsk"),
            new TimeZoneOption("Tomsk Standard Time","(UTC+07:00) Tomsk"),
            new TimeZoneOption("China Standard Time","(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"),
            new TimeZoneOption("North Asia East Standard Time","(UTC+08:00) Irkutsk"),
            new TimeZoneOption("Singapore Standard Time","(UTC+08:00) Kuala Lumpur, Singapore"),
            new TimeZoneOption("W. Australia Standard Time","(UTC+08:00) Perth"),
            new TimeZoneOption("Taipei Standard Time","(UTC+08:00) Taipei"),
            new TimeZoneOption("Ulaanbaatar Standard Time","(UTC+08:00) Ulaanbaatar"),
            new TimeZoneOption("Aus Central W. Standard Time","(UTC+08:45) Eucla"),
            new TimeZoneOption("Transbaikal Standard Time","(UTC+09:00) Chita"),
            new TimeZoneOption("Tokyo Standard Time","(UTC+09:00) Osaka, Sapporo, Tokyo"),
            new TimeZoneOption("North Korea Standard Time","(UTC+09:00) Pyongyang"),
            new TimeZoneOption("Korea Standard Time","(UTC+09:00) Seoul"),
            new TimeZoneOption("Yakutsk Standard Time","(UTC+09:00) Yakutsk"),
            new TimeZoneOption("Cen. Australia Standard Time","(UTC+09:30) Adelaide"),
            new TimeZoneOption("AUS Central Standard Time","(UTC+09:30) Darwin"),
            new TimeZoneOption("E. Australia Standard Time","(UTC+10:00) Brisbane"),
            new TimeZoneOption("AUS Eastern Standard Time","(UTC+10:00) Canberra, Melbourne, Sydney"),
            new TimeZoneOption("West Pacific Standard Time","(UTC+10:00) Guam, Port Moresby"),
            new TimeZoneOption("Tasmania Standard Time","(UTC+10:00) Hobart"),
            new TimeZoneOption("Vladivostok Standard Time","(UTC+10:00) Vladivostok"),
            new TimeZoneOption("Lord Howe Standard Time","(UTC+10:30) Lord Howe Island"),
            new TimeZoneOption("Bougainville Standard Time","(UTC+11:00) Bougainville Island"),
            new TimeZoneOption("Russia Time Zone 10","(UTC+11:00) Chokurdakh"),
            new TimeZoneOption("Magadan Standard Time","(UTC+11:00) Magadan"),
            new TimeZoneOption("Norfolk Standard Time","(UTC+11:00) Norfolk Island"),
            new TimeZoneOption("Sakhalin Standard Time","(UTC+11:00) Sakhalin"),
            new TimeZoneOption("Central Pacific Standard Time","(UTC+11:00) Solomon Is., New Caledonia"),
            new TimeZoneOption("Russia Time Zone 11","(UTC+12:00) Anadyr, Petropavlovsk-Kamchatsky"),
            new TimeZoneOption("New Zealand Standard Time","(UTC+12:00) Auckland, Wellington"),
            new TimeZoneOption("UTC+12","(UTC+12:00) Coordinated Universal Time+12"),
            new TimeZoneOption("Fiji Standard Time","(UTC+12:00) Fiji"),
            new TimeZoneOption("Kamchatka Standard Time","(UTC+12:00) Petropavlovsk-Kamchatsky - Old"),
            new TimeZoneOption("Chatham Islands Standard Time","(UTC+12:45) Chatham Islands"),
            new TimeZoneOption("UTC+13","(UTC+13:00) Coordinated Universal Time+13"),
            new TimeZoneOption("Tonga Standard Time","(UTC+13:00) Nuku'alofa"),
            new TimeZoneOption("Samoa Standard Time","(UTC+13:00) Samoa"),
            new TimeZoneOption("Line Islands Standard Time","(UTC+14:00) Kiritimati Island")
        ];
}
