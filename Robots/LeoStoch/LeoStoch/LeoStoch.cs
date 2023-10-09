using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class LeoStoch : Robot
    {
        [Parameter("kPeriods", DefaultValue = 9)]
        public int kPeriods { get; set; }
        [Parameter("kSlowing", DefaultValue = 9)]
        public int kSlowing { get; set; }
        [Parameter("dPeriods", DefaultValue = 3)]
        public int dPeriods { get; set; }

        [Parameter("maType", DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType maType { get; set; }

        private StochasticOscillator stoch;

        [Parameter("volume", DefaultValue = 0.01)]
        public double volume { get; set; }

        protected override void OnStart()
        {

            stoch = Indicators.StochasticOscillator(kPeriods, kSlowing, dPeriods, maType);
        }
        /*
            It is and indicator/robot that sells and buys on price reversion.
            It analyses stochastic oscillator and buys when the candle breaks the minimum of previous ascending candles with stochastic level above 80. 
            For sales it does when price breaks maximum of descending candles with stochastic at level below 20. 
            It closes the positions when the stochastic reaches the opposite level (20 or 80).
       */

        protected override void OnTick()
        {
            var Lpos = Positions.FindAll("StochBuy", SymbolName);
            var Spos = Positions.FindAll("StochSell", SymbolName);

            if (Lpos.Length == 0 && Bars.ClosePrices.Last(1) > Bars.HighPrices.Last(2) && stoch.PercentD.LastValue < 20 && stoch.PercentK.LastValue < 20 && Math.Min(Bars.LowPrices.Last(1), Bars.LowPrices.Last(2)) < Math.Min(Bars.LowPrices.Last(2), Bars.LowPrices.Last(3)))
            {
                ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "StochBuy");
            }
            if (Spos.Length == 0 && Bars.ClosePrices.Last(1) < Bars.LowPrices.Last(2) && stoch.PercentD.LastValue > 80 && stoch.PercentK.LastValue > 80 && Math.Max(Bars.HighPrices.Last(1), Bars.HighPrices.Last(2)) > Math.Max(Bars.HighPrices.Last(2), Bars.HighPrices.Last(3)))
            {
                ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "StochSell");
            }



            foreach (var po in Lpos)
            {
                if (stoch.PercentD.LastValue > 80)
                {
                    ClosePosition(po);
                }
            }

            foreach (var po in Spos)
            {
                if (stoch.PercentD.LastValue < 20)
                {
                    ClosePosition(po);
                }
            }

        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
