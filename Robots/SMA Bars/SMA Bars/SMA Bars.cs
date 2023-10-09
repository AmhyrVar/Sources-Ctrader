using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class SMABars : Robot
    {
        [Parameter("Entry 1", DefaultValue = true)]
        public bool Entry_1 { get; set; }
        [Parameter("Entry 2", DefaultValue = true)]
        public bool Entry_2 { get; set; }


        [Parameter("SMA Period", DefaultValue = 200)]
        public int Period { get; set; }
        [Parameter("SMA Source")]
        public DataSeries Source { get; set; }

        [Parameter("LotSize", DefaultValue = 0.1)]
        public double Volume { get; set; }

        [Parameter("TP", DefaultValue = 100)]
        public double TP { get; set; }
        [Parameter("SL", DefaultValue = 50)]
        public double SL { get; set; }




        private SimpleMovingAverage _sma;

        protected override void OnStart()
        {
            _sma = Indicators.SimpleMovingAverage(Source, Period);
        }

        private bool Bullish(int x)
        {
            if (Bars.OpenPrices.Last(x) < Bars.ClosePrices.Last(x))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool UnderSMA(int x)
        {
            if (Bars.ClosePrices.Last(x) < _sma.Result.Last(x) && Bars.OpenPrices.Last(x) < _sma.Result.Last(x))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool WickLogic(int x)
        {
            var UpWick = 0.0;
            var DownWick = 0.0;
            var Body = Math.Abs(Bars.OpenPrices.Last(x) - Bars.ClosePrices.Last(x));
            if (Bullish(x))
            {
                UpWick = Bars.HighPrices.Last(x) - Bars.ClosePrices.Last(x);
                DownWick = Bars.OpenPrices.Last(x) - Bars.LowPrices.Last(x);
            }
            if (!Bullish(x))
            {
                UpWick = Bars.HighPrices.Last(x) - Bars.OpenPrices.Last(x);
                DownWick = Bars.ClosePrices.Last(x) - Bars.LowPrices.Last(x);
            }
            var BiggerWick = Math.Max(UpWick, DownWick);
            if (BiggerWick > 3 * Body)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        protected override void OnBar()
        {
            if (Entry_1)
            {
                //Buy
                if (Bullish(1) && !Bullish(2) && !Bullish(3) && !Bullish(4) && UnderSMA(1) && UnderSMA(2) && UnderSMA(3) && UnderSMA(4) && Bars.ClosePrices.Last(1) > Bars.OpenPrices.Last(2))
                {
                    ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), "Buy 1", SL, TP);
                }
                //Sell
                if (!Bullish(1) && Bullish(2) && Bullish(3) && Bullish(4) && !UnderSMA(1) && !UnderSMA(2) && !UnderSMA(3) && !UnderSMA(4) && Bars.ClosePrices.Last(1) < Bars.OpenPrices.Last(2))
                {
                    ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), "Sell 1", SL, TP);
                }

            }

            if (Entry_2)
            {
                //Buy
                if (Bullish(1) && WickLogic(1) && Bars.LowPrices.Last(1) < Bars.LowPrices.Last(2) && Bars.LowPrices.Last(1) < Bars.LowPrices.Last(3))
                {
                    ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), "Buy 2", SL, TP);
                }

                //Sell
                if (!Bullish(1) && WickLogic(1) && Bars.HighPrices.Last(1) > Bars.HighPrices.Last(2) && Bars.HighPrices.Last(1) > Bars.HighPrices.Last(3))
                {
                    ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), "Sell 2", SL, TP);
                }
            }

        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
