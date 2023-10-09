using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class RenkoBarTrader : Robot
    {
        [Parameter("Take Profit", DefaultValue = 10)]
        public double TP { get; set; }
        [Parameter("Stop Loss", DefaultValue = 50)]
        public double SL { get; set; }

        [Parameter("Lots", DefaultValue = 0.01)]
        public double Lots { get; set; }

        public double Volume;

        protected override void OnStart()
        {
            Volume = (Symbol.QuantityToVolumeInUnits(Lots));
            Print((Bars.OpenPrices.LastValue));
            Print(Bars.ClosePrices.LastValue);
        }

        protected override void OnBar()
        {

            Print("Open" + Bars.OpenPrices.Last(1));
            Print("Close" + Bars.ClosePrices.Last(1));
            if (Bars.OpenPrices.Last(1) > Bars.ClosePrices.Last(1))
            {
                ExecuteMarketOrder(TradeType.Sell, SymbolName, Volume, "Renko Trader", SL, TP);
            }
            if (Bars.OpenPrices.Last(1) < Bars.ClosePrices.Last(1))
            {
                ExecuteMarketOrder(TradeType.Buy, SymbolName, Volume, "Renko Trader", SL, TP);
            }
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
