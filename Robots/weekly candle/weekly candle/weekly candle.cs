using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class weeklycandle : Robot
    {
        [Parameter("Volume", DefaultValue = 0.1)]
        public double volume { get; set; }



        protected override void OnStart()
        {
            Positions.Closed += PositionsOnClosed;
        }

        private void PositionsOnClosed(PositionClosedEventArgs args)
        {
           
            var position = args.Position;
            if (args.Reason == PositionCloseReason.StopLoss && args.Position.Label == "Week Bar" && args.Position.SymbolName == SymbolName)
            {

             

                if (position.TradeType == TradeType.Buy)
                {
                    
                    var t = ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Week Reverse");
                    ModifyPosition(t.Position, position.EntryPrice, null, false);
                }
                if (position.TradeType == TradeType.Sell)
                {
                   
                    var t = ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Week Reverse");
                    ModifyPosition(t.Position, position.EntryPrice, null, false);
                }
            }
        }

        protected override void OnBar()
        {
            var po = Positions.FindAll("Week Bar", SymbolName);
            foreach (var p in po)
            {
                ClosePosition(p);
            }
            var pos = Positions.FindAll("Week Reverse", SymbolName);
            foreach (var p in po)
            {
                ClosePosition(p);
            }

        }
        protected override void OnTick()
        {
            var po = Positions.FindAll("Week Bar", SymbolName);
            if (po.Length == 0 && Bars.HighPrices.Last(0) > Bars.HighPrices.Last(1) && Symbol.Bid < Bars.LowPrices.Last(1))
            {
                var t = ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Week Bar");
                ModifyPosition(t.Position, Bars.HighPrices.Last(1), null, false);
            }

            if (po.Length == 0 && Bars.LowPrices.Last(0) < Bars.LowPrices.Last(1) && Symbol.Ask > Bars.HighPrices.Last(1))
            {
                var t = ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Week Bar");
                ModifyPosition(t.Position, Bars.LowPrices.Last(1), null, false);
            }
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
