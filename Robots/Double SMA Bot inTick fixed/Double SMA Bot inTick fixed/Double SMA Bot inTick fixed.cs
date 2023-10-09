using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class DoubleSMABotinTickfixed : Robot
    {

        [Parameter("Volume", DefaultValue = 1000)]
        public int volume { get; set; }

        [Parameter("SMA1 Period", DefaultValue = 3)]
        public int PeriodsSma1 { get; set; }
        [Parameter("SMA2 Period", DefaultValue = 3)]
        public int PeriodsSma2 { get; set; }

        [Parameter("SMA1 Pips", DefaultValue = 5)]
        public int SMA1_Pips { get; set; }
        [Parameter("SMA2 Pips", DefaultValue = 5)]
        public int SMA2_Pips { get; set; }

        [Parameter("Take Profit", DefaultValue = 5)]
        public int TP { get; set; }
        [Parameter("Stop Loss", DefaultValue = 3)]
        public int SL { get; set; }


        private SimpleMovingAverage _sma1 { get; set; }
        private SimpleMovingAverage _sma2 { get; set; }


        protected override void OnStart()
        {



            _sma1 = Indicators.SimpleMovingAverage(Bars.HighPrices, PeriodsSma1);
            _sma2 = Indicators.SimpleMovingAverage(Bars.LowPrices, PeriodsSma2);
        }

        protected override void OnTick()
        {
            //Print(Symbol.Bid - (SMA2_Pips / 10000));
            if (IsShortPoOpen() && (Symbol.Bid - (SMA1_Pips / 10000)) > _sma1.Result.LastValue)
            {
                ExecuteMarketOrder(TradeType.Sell, SymbolName, volume, "DSMA", SL, TP);
                Print("Buy sma1 = " + _sma1.Result.LastValue);
                Print("ASk we bought" + Symbol.Bid);

                Print("delta = " + (Symbol.Bid - _sma1.Result.LastValue));
            }

            if (IsLongPoOpen() && (Symbol.Ask) < _sma2.Result.LastValue - (SMA2_Pips / 10000))
            {
                ExecuteMarketOrder(TradeType.Buy, SymbolName, volume, "DSMA", SL, TP);


                Print("Buy sma1 = " + _sma2.Result.LastValue);
                Print("ASk we bought" + Symbol.Ask);

                Print("delta sell = " + (_sma2.Result.LastValue - Symbol.Ask));
            }
        }

        protected bool IsLongPoOpen()
        {
            var pos = Positions.FindAll("DSMA", SymbolName, TradeType.Buy);

            if (pos.Length == 0)
            {
                return true;

            }
            else
            {
                return false;

            }
        }

        protected bool IsShortPoOpen()
        {
            var pos = Positions.FindAll("DSMA", SymbolName, TradeType.Sell);

            if (pos.Length == 0)
            {
                return true;

            }
            else
            {
                return false;

            }
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
