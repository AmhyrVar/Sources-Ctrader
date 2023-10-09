using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Dailycandlev5 : Robot
    {
        [Parameter(DefaultValue = 2.0)]
        public double triggerpips { get; set; }

        [Parameter(DefaultValue = 2.0)]
        public double StopPips { get; set; }

        [Parameter(DefaultValue = 50)]
        public double TP { get; set; }

        [Parameter(DefaultValue = true)]
        public bool LongTrades { get; set; }
        [Parameter(DefaultValue = true)]
        public bool ShortTrades { get; set; }


        [Parameter(DefaultValue = 0.01)]
        public double Volume { get; set; }



        public bool BarScan;



        protected override void OnStart()
        {
            BarScan = true;
        }

        protected bool isPo()
        {
            var Pos = Positions.FindAll("DailyBar", SymbolName);
            if (Pos.Length > 0)
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
            var Pos = Positions.FindAll("DailyBar", SymbolName);
            if (Pos.Length > 0)
            {
                BarScan = false;
            }
            else
            {
                BarScan = true;
            }
        }
        protected override void OnTick()
        {


            if (BarScan && LongTrades && !isPo() && Symbol.Ask > Bars.HighPrices.Last(1) + triggerpips * Symbol.PipValue)
            {

                var Thresh = Bars.HighPrices.Last(1) + triggerpips * Symbol.PipValue;

                var StopLoss = Bars.LowPrices.Last(1) - StopPips * Symbol.PipSize;

                var SLpips = (Thresh - StopLoss) / Symbol.PipSize;



                ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), "DailyBar", SLpips, TP);
            }

            if (BarScan && ShortTrades && !isPo() && Symbol.Bid < Bars.LowPrices.Last(1) - triggerpips * Symbol.PipValue)
            {

                var Thresh = (Bars.LowPrices.Last(1) - triggerpips * Symbol.PipValue);

                var StopLoss = Bars.HighPrices.Last(1) + StopPips * Symbol.PipSize;

                var SLpips = (StopLoss - Thresh) / Symbol.PipSize;


                ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), "DailyBar", SLpips, TP);
            }
        }


    }
}
