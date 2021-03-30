using System;

namespace WpfApp1
{
    class WeatherData
    {
        public int Day { set; get; }
        public int Hour { set; get; }
        public MyDefine.WeatherType WeatherType { set; get; }
        public double Temp { set; get; }
        public MyDefine.RainType Rain { set; get; }
        public double RainPercent { set; get; }
        public int Humidity { set; get; }

    }
}