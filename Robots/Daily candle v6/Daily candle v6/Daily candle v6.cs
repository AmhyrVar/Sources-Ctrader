using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Dailycandlev6 : Robot
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

        [Parameter(DefaultValue = 2)]
        public double stopLimitRangePips { get; set; }



        public bool BarScan;







        protected override void OnStart()
        {
            BarScan = true;
        }

        protected bool isPo()
        {
            var Pos = Positions.FindAll("DailyBar", SymbolName);
            var Pend = PendingOrders.Count(item => item.Label == "DailyBar" && item.SymbolName == SymbolName);



            if (Pos.Length == 0 && Pend == 0)
            {
                return false;
            }
            else
            {
                return true;

            }
        }
        protected override void OnBar()
        {



            foreach (var Pend in PendingOrders)
            {
                if (Pend.Label == "DailyBar" && Pend.SymbolName == SymbolName)
                {
                    CancelPendingOrder(Pend);
                }
            }



            BarScan = !isPo();



        }
        protected override void OnTick()
        {

            var Margin_Required = Symbol.Bid * Symbol.TickValue / Symbol.TickSize * Symbol.QuantityToVolumeInUnits(Volume) / Symbol.DynamicLeverage[0].Leverage;

            Print("Margin req " + Margin_Required);
            if (Account.Balance < Margin_Required)
            {
                Stop();

                Print("Not enough Balance to trade");
            }



            if (BarScan && LongTrades && !isPo() && Symbol.Ask < Bars.HighPrices.Last(1) + triggerpips * Symbol.PipValue)
            {

                var Thresh = Bars.HighPrices.Last(1) + triggerpips * Symbol.PipValue;

                var StopLoss = Bars.LowPrices.Last(1) - StopPips * Symbol.PipSize;

                var SLpips = (Thresh - StopLoss) / Symbol.PipSize;




                PlaceStopLimitOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), Thresh, stopLimitRangePips, "DailyBar", SLpips, TP);
            }

            if (BarScan && ShortTrades && !isPo() && Symbol.Bid < Bars.LowPrices.Last(1) - triggerpips * Symbol.PipValue)
            {

                var Thresh = (Bars.LowPrices.Last(1) - triggerpips * Symbol.PipValue);

                var StopLoss = Bars.HighPrices.Last(1) + StopPips * Symbol.PipSize;

                var SLpips = (StopLoss - Thresh) / Symbol.PipSize;



                PlaceStopLimitOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), Thresh, stopLimitRangePips, "DailyBar", SLpips, TP);
            }
        }


    }
}
