using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    class MyDefine
    {
        // ====================     날씨     ====================
        public const String RSS_URL = "http://www.weather.go.kr/weather/forecast/mid-term-rss3.jsp";
        public const string RSS_URL_KWANGMYONG = "http://www.kma.go.kr/wid/queryDFSRSS.jsp?zone=4121063300";

        public static readonly string[] RSS_URL_PARAM = new string[(int)LocationType.Max] 
        { "srnLd=109", "srnLd=105", "srnLd=131", "srnLd=133", "srnLd=146", "srnLd=156", "srnLd=143", "srnLd=159", "srnLd=184" };
        public enum LocationType
        {
            SeoulAndGyeonggi = 0,
            Gangwon,
            ChungBuk,
            ChungNam,
            JeonBuk,
            JeonNam,
            GyeongBuk,
            GyeongNam,
            Jeju,
            Max
        }

        public const int INTERVAL_SECOND = 1000 * 60 * 60;

        public enum WeatherType
        {
            Unknown = 0,
            Clear,              // 맑음
            MostlyCloudy,       // 구름 많음
            Cloudy,             // 흐림
            Rain,               // 비
            RainAndSnow,        // 비 / 눈
            Snow,               // 눈
            Shower              // 소나기
        }

        public enum RainType
        {
            None = 0,
            Raining,
            RainingAndSnowing,
            Snowing,
            Showering
        }

        internal static WeatherType ConvertWeatherType(string value)
        {
            if (value.Equals("Clear") || value.Equals("맑음"))         return WeatherType.Clear;
            else if (value.Equals("Mostly Cloudy"))     return WeatherType.MostlyCloudy;
            else if (value.Equals("Cloudy"))            return WeatherType.Cloudy;
            else if (value.Equals("Rain"))              return WeatherType.Rain;
            else if (value.Equals("Rain/Snow"))         return WeatherType.RainAndSnow;
            else if (value.Equals("Snow"))              return WeatherType.Snow;
            else if (value.Equals("Shower"))            return WeatherType.Shower;
            else                                        return WeatherType.Unknown;
        }
        internal static RainType ConvertRainType(string value)
        {
            switch (Convert.ToInt32(value))
            {
                case 0:         return RainType.None;
                case 1:         return RainType.Raining;
                case 2:         return RainType.RainingAndSnowing;
                case 3:         return RainType.Snowing;
                case 4:         return RainType.Showering;
                default:        return RainType.None;
            }
        }
        // ====================     날씨     ====================


        // ====================     캘린더     ====================
        internal static string[] SCOPES = { Google.Apis.Calendar.v3.CalendarService.Scope.CalendarReadonly };
        internal static string APP_NAME = "Google Calendar API .NET Quickstart";
        // ====================     캘린더     ====================





        // ====================     서버     ====================
        internal static int PORT = 27501;
        public enum JSON_KEY
        {
            USER_NAME = 0,
            MESSAGE = 1,
            CONNECT_STATUS = 2,
        }
        // ====================     서버     ====================
    }
}
