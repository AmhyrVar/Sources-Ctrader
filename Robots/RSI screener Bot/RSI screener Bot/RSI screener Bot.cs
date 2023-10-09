using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class RSIscreenerBot : Robot
    {

        private RelativeStrengthIndex _rsiFast;
        private RelativeStrengthIndex _rsiSlow;


        private RelativeStrengthIndex _rsi5F;
        private RelativeStrengthIndex _rsi15F;
        private RelativeStrengthIndex _rsi30F;
        private RelativeStrengthIndex _rsiH1F;
        private RelativeStrengthIndex _rsiH4F;
        private RelativeStrengthIndex _rsiD1F;

        private RelativeStrengthIndex _rsi5S;
        private RelativeStrengthIndex _rsi15S;
        private RelativeStrengthIndex _rsi30S;
        private RelativeStrengthIndex _rsiH1S;
        private RelativeStrengthIndex _rsiH4S;
        private RelativeStrengthIndex _rsiD1S;





        [Parameter()]
        public DataSeries Source { get; set; }





        [Parameter(DefaultValue = 14)]
        public int RSI_Fast_Period { get; set; }

        [Parameter(DefaultValue = 20)]
        public int RSI_Slow_Period { get; set; }


        [Parameter(DefaultValue = 25)]
        public int OverSold { get; set; }
        [Parameter(DefaultValue = 75)]
        public int OverBought { get; set; }
        [Parameter(DefaultValue = 35)]
        public int BeforeOversold { get; set; }
        [Parameter(DefaultValue = 65)]
        public int BeforeOverbought { get; set; }




        //TFs RSI




        private MarketSeries series5;
        private MarketSeries series15;


        private MarketSeries series30;
        private MarketSeries seriesH1;
        private MarketSeries seriesH4;
        private MarketSeries seriesD1;



        protected override void OnStart()
        {
            series5 = MarketData.GetSeries(TimeFrame.Minute5);
            series15 = MarketData.GetSeries(TimeFrame.Minute15);
            series30 = MarketData.GetSeries(TimeFrame.Minute30);
            seriesH1 = MarketData.GetSeries(TimeFrame.Hour);
            seriesH4 = MarketData.GetSeries(TimeFrame.Hour4);
            seriesD1 = MarketData.GetSeries(TimeFrame.Daily);


            _rsiFast = Indicators.RelativeStrengthIndex(Source, RSI_Fast_Period);
            _rsiSlow = Indicators.RelativeStrengthIndex(Source, RSI_Slow_Period);


            _rsi15F = Indicators.RelativeStrengthIndex(series15.Close, RSI_Fast_Period);
            _rsi5F = Indicators.RelativeStrengthIndex(series5.Close, RSI_Fast_Period);
            _rsi30F = Indicators.RelativeStrengthIndex(series30.Close, RSI_Fast_Period);
            _rsiH1F = Indicators.RelativeStrengthIndex(seriesH1.Close, RSI_Fast_Period);
            _rsiH4F = Indicators.RelativeStrengthIndex(seriesH4.Close, RSI_Fast_Period);
            _rsiD1F = Indicators.RelativeStrengthIndex(seriesD1.Close, RSI_Fast_Period);

            _rsi15S = Indicators.RelativeStrengthIndex(series15.Close, RSI_Slow_Period);
            _rsi5S = Indicators.RelativeStrengthIndex(series5.Close, RSI_Slow_Period);
            _rsi30S = Indicators.RelativeStrengthIndex(series30.Close, RSI_Slow_Period);
            _rsiH1S = Indicators.RelativeStrengthIndex(seriesH1.Close, RSI_Slow_Period);
            _rsiH4S = Indicators.RelativeStrengthIndex(seriesH4.Close, RSI_Slow_Period);
            _rsiD1S = Indicators.RelativeStrengthIndex(seriesD1.Close, RSI_Slow_Period);


        }

        private string GetColor(DataSeries Series)
        {
            if (Series.LastValue > OverBought)
            {
                return "Orange";
            }

            if (Series.LastValue < OverSold)
            {
                return "Lime";
            }
            if (Series.LastValue >= OverSold && Series.LastValue <= BeforeOversold)
            {
                return "Green";
            }

            if (Series.LastValue >= BeforeOverbought && Series.LastValue <= OverBought)
            {
                return "Red";
            }

            else
            {
                return "Black";
            }
        }

        private string GetDirection(DataSeries Series)
        {
            if (Series.IsRising())
            {
                return "R";
            }

            else
            {
                return "F";
            }
        }




        protected override void OnTick()
        {
            Chart.DrawStaticText("Line 1", "RSI M5:", VerticalAlignment.Top, HorizontalAlignment.Left, "Black");
            Chart.DrawStaticText("Line 2", "\nRSI M15:", VerticalAlignment.Top, HorizontalAlignment.Left, "Black");
            Chart.DrawStaticText("Line 3", "\n\nRSI M30:", VerticalAlignment.Top, HorizontalAlignment.Left, "Black");
            Chart.DrawStaticText("Line 4", "\n\n\nRSI H1:", VerticalAlignment.Top, HorizontalAlignment.Left, "Black");
            Chart.DrawStaticText("Line 5", "\n\n\n\nRSI H4:", VerticalAlignment.Top, HorizontalAlignment.Left, "Black");
            Chart.DrawStaticText("Line 6", "\n\n\n\n\nRSI D1:", VerticalAlignment.Top, HorizontalAlignment.Left, "Black");






            Chart.DrawStaticText("RSI 5F", "               " + ((int)_rsi5F.Result.LastValue).ToString() + GetDirection(_rsi5F.Result), VerticalAlignment.Top, HorizontalAlignment.Left, GetColor(_rsi5F.Result));
            Chart.DrawStaticText("RSI 5S", "                       " + ((int)_rsi5S.Result.LastValue).ToString() + GetDirection(_rsi5S.Result), VerticalAlignment.Top, HorizontalAlignment.Left, GetColor(_rsi5S.Result));




            Chart.DrawStaticText("RSI 15F", "\n               " + ((int)_rsi15F.Result.LastValue).ToString() + GetDirection(_rsi15F.Result), VerticalAlignment.Top, HorizontalAlignment.Left, GetColor(_rsi15F.Result));
            Chart.DrawStaticText("RSI 15S", "\n                       " + ((int)_rsi15S.Result.LastValue).ToString() + GetDirection(_rsi15S.Result), VerticalAlignment.Top, HorizontalAlignment.Left, GetColor(_rsi15S.Result));





            Chart.DrawStaticText("RSI 30F", "\n\n               " + ((int)_rsi30F.Result.LastValue).ToString() + GetDirection(_rsi30F.Result), VerticalAlignment.Top, HorizontalAlignment.Left, GetColor(_rsi30F.Result));
            Chart.DrawStaticText("RSI 30S", "\n\n                       " + ((int)_rsi30S.Result.LastValue).ToString() + GetDirection(_rsi30S.Result), VerticalAlignment.Top, HorizontalAlignment.Left, GetColor(_rsi30S.Result));




            Chart.DrawStaticText("RSI H1F", "\n\n\n               " + ((int)_rsiH1F.Result.LastValue).ToString() + GetDirection(_rsiH1F.Result), VerticalAlignment.Top, HorizontalAlignment.Left, GetColor(_rsiH1F.Result));
            Chart.DrawStaticText("RSI H1S", "\n\n\n                       " + ((int)_rsiH1S.Result.LastValue).ToString() + GetDirection(_rsiH1S.Result), VerticalAlignment.Top, HorizontalAlignment.Left, GetColor(_rsiH1S.Result));





            Chart.DrawStaticText("RSI H4F", "\n\n\n\n               " + ((int)_rsiH4F.Result.LastValue).ToString() + GetDirection(_rsiH4F.Result), VerticalAlignment.Top, HorizontalAlignment.Left, GetColor(_rsiH4F.Result));
            Chart.DrawStaticText("RSI H4S", "\n\n\n\n                       " + ((int)_rsiH4S.Result.LastValue).ToString() + GetDirection(_rsiH4S.Result), VerticalAlignment.Top, HorizontalAlignment.Left, GetColor(_rsiH4S.Result));




            Chart.DrawStaticText("RSI D1F", "\n\n\n\n\n               " + ((int)_rsiD1F.Result.LastValue).ToString() + GetDirection(_rsiD1F.Result), VerticalAlignment.Top, HorizontalAlignment.Left, GetColor(_rsiD1F.Result));
            Chart.DrawStaticText("RSI D1S", "\n\n\n\n\n                      " + ((int)_rsiD1S.Result.LastValue).ToString() + GetDirection(_rsiD1S.Result), VerticalAlignment.Top, HorizontalAlignment.Left, GetColor(_rsiD1S.Result));


        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
