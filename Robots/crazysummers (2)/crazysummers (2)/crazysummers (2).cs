using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;



# region requirements

/*
 * 1. When price is above the 50ema line and comes back to touch it, a buy trade opens, with a stop loss of 10pips and take profit of 10pips.

2. When price is below the 50ema line and comes back to touch it, a sell trade opens, with a stop loss of 10pips and take profit of 10pips.

3. Timeframe: 30mins

4. Lot size: 1

5. Only one trade to open at a time, per currency pair.

6. When a trade closes, ignore the next 30min candle.****

7. Markets: GBPJPY, EURGBP

8. I will need to edit the script to be able to adjust to the markets.
 * 
 * 
 */

#endregion

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class crazysummers2 : Robot
    {

        [Parameter("Lots", DefaultValue = 1)]
        public double Lots { get; set; }

        [Parameter("EMA1 Period", DefaultValue = 50)]
        public int PeriodsEma1 { get; set; }
        [Parameter("Take Profit", DefaultValue = 10)]
        public int TP { get; set; }
        [Parameter("Stop Loss", DefaultValue = 10)]
        public double SL { get; set; }
        private ExponentialMovingAverage _ema1 { get; set; }

        private bool AllowTrade;


        protected override void OnStart()
        {
            _ema1 = Indicators.ExponentialMovingAverage(Bars.ClosePrices, PeriodsEma1);
            AllowTrade = true;
            Positions.Closed += PositionsOnClosed;
        }
        private void PositionsOnClosed(PositionClosedEventArgs args)
        {   
            AllowTrade = false;
        }
        protected override void OnBar()
        {
            AllowTrade = true;
        }
        protected override void OnTick()
        {
            var Po = Positions.FindAll("cs", SymbolName);
            var Bpo = Positions.FindAll("cs",SymbolName,TradeType.Buy);
            var Spo = Positions.FindAll("cs", SymbolName, TradeType.Sell);
            if (Bpo.Length > 1 || Spo.Length > 1)
            {
                Print("**************ERROR**********");
            }

            if (Bars.OpenPrices.Last(1)> _ema1.Result.Last(1) && Bars.ClosePrices.Last(1) > _ema1.Result.Last(1) && Symbol.Ask <= _ema1.Result.Last(1) && Po.Length == 0 && AllowTrade)
            {
                ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(Lots), "cs",SL,TP);
            }

            if (Bars.OpenPrices.Last(1) < _ema1.Result.Last(1)&& Bars.ClosePrices.Last(1) < _ema1.Result.Last(1) && Symbol.Bid >= _ema1.Result.Last(1) && Po.Length == 0 && AllowTrade)
            {
                ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(Lots), "cs", SL, TP);
            }
        }

        protected override void OnStop()
        {
            // Handle cBot stop here
        }
    }
}