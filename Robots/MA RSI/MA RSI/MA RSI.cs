using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class EMACross_RSI : Robot
    {
        [Parameter("Source")]
        public DataSeries SourceSeries { get; set; }
        [Parameter("RSI Periods", DefaultValue = 14)]
        public int Periods { get; set; }


        [Parameter("Slow Periods", DefaultValue = 30)]
        public int SlowPeriods { get; set; }

        [Parameter("Fast Periods", DefaultValue = 14)]
        public int FastPeriods { get; set; }

        [Parameter("RSI High", DefaultValue = 60)]
        public int RSI_HI { get; set; }
        [Parameter("RSI Low", DefaultValue = 40)]
        public int RSI_LO { get; set; }

        [Parameter("Stop Loss", DefaultValue = 50)]
        public double SL { get; set; }
        [Parameter("Take Profit", DefaultValue = 150)]
        public double TP { get; set; }
        [Parameter("Quantity (Lots)", DefaultValue = 1, MinValue = 0.01, Step = 0.01)]
        public double Quantity { get; set; }
        private RelativeStrengthIndex rsi;



        private ExponentialMovingAverage slowMa;
        private ExponentialMovingAverage fastMa;

        private const string label = "EMA&RSI";
        private Position longPosition;
        private Position shortPosition;

        protected override void OnStart()
        {
            rsi = Indicators.RelativeStrengthIndex(SourceSeries, Periods);
            fastMa = Indicators.ExponentialMovingAverage(SourceSeries, FastPeriods);
            slowMa = Indicators.ExponentialMovingAverage(SourceSeries, SlowPeriods);



        }

        protected override void OnBar()
        {

            longPosition = Positions.Find(label, Symbol, TradeType.Buy);
            shortPosition = Positions.Find(label, Symbol, TradeType.Sell);


            if (slowMa.Result.Last(1) < fastMa.Result.Last(1) && longPosition == null && rsi.Result.Last(1) <= RSI_HI)
            {
                ExecuteMarketOrder(TradeType.Buy, Symbol, VolumeInUnits, label, SL, TP);

            }
            else if (slowMa.Result.Last(1) > fastMa.Result.Last(1) && shortPosition == null && rsi.Result.Last(1) > RSI_LO)
            {
                ExecuteMarketOrder(TradeType.Sell, Symbol, VolumeInUnits, label, SL, TP);

            }


        }

        private long VolumeInUnits
        {
            get { return Symbol.QuantityToVolume(Quantity); }
        }
    }
}
