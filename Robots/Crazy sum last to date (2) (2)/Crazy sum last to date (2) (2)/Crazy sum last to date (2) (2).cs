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





//Time Hendrixapproved

#endregion

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class Crazysumlasttodate22 : Robot
    {

        [Parameter("Lots", DefaultValue = 1)]
        public double Lots { get; set; }

        [Parameter("EMA Period", DefaultValue = 50)]
        public int PeriodsEma1 { get; set; }
        [Parameter("SMA Period", DefaultValue = 50)]
        public int PeriodsSma1 { get; set; }
        [Parameter("Take Profit", DefaultValue = 10)]
        public int TP { get; set; }
        [Parameter("Stop Loss", DefaultValue = 10)]
        public double SL { get; set; }


        [Parameter("Trade Time 1", DefaultValue = "09:00")]
        public string TradeTime1 { get; set; }
        [Parameter("Stop Time 1", DefaultValue = "10:00")]
        public string CancelTime1 { get; set; }
        [Parameter("Trade Time 2", DefaultValue = "13:00")]
        public string TradeTime2 { get; set; }
        [Parameter("Stop Time 2", DefaultValue = "16:00")]
        public string CancelTime2 { get; set; }


        private ExponentialMovingAverage _ema1 { get; set; }
        private SimpleMovingAverage _sma1 { get; set; }

        private bool AllowTrade;


        private int StartHour1;
        private int StartMinute1;
        private int StopHour1;
        private int StopMinute1;

        private int StartHour2;
        private int StartMinute2;
        private int StopHour2;
        private int StopMinute2;

        private double cross;

        protected override void OnStart()
        {
            _ema1 = Indicators.ExponentialMovingAverage(Bars.ClosePrices, PeriodsEma1);
            _sma1 = Indicators.SimpleMovingAverage(Bars.ClosePrices, PeriodsSma1);
            AllowTrade = true;
            Positions.Closed += PositionsOnClosed;


            string[] parts1 = TradeTime1.Split(':');

            StartHour1 = int.Parse(parts1[0]);
            StartMinute1 = int.Parse(parts1[1]);

            string[] partss1 = CancelTime1.Split(':');
            StopHour1 = int.Parse(partss1[0]);
            StopMinute1 = int.Parse(partss1[1]);

            string[] parts = TradeTime2.Split(':');

            StartHour2 = int.Parse(parts[0]);
            StartMinute2 = int.Parse(parts[1]);

            string[] partss = CancelTime2.Split(':');
            StopHour2 = int.Parse(partss[0]);
            StopMinute2 = int.Parse(partss[1]);
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
            
           

            if (_sma1.Result.LastValue == _ema1.Result.LastValue && Po.Length > 0 && _sma1.Result.LastValue != cross)    
            {
                foreach (var po in Po)
                {
                    ClosePosition(po);
                    Print("Close one");
                }
            }
            if (Po.Length != 0 && (_sma1.Result.HasCrossedAbove(_ema1.Result,0) || _sma1.Result.HasCrossedBelow(_ema1.Result,0)) && _sma1.Result.LastValue != cross)
            {
                foreach (var po in Po)
                {
                    ClosePosition(po);
                    Print("Close two");
                }
            }

            if (Bpo.Length > 1 || Spo.Length > 1)
            {
                Print("**************ERROR**********");
            }
            //If the EMA goes below the SMA a SELL position is taken
            if (_ema1.Result.HasCrossedAbove(_sma1.Result, 1) && Po.Length == 0 && CheckTime())
            {
                cross = _sma1.Result.Last(1);
                ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(Lots), "cs",SL,TP);
                AllowTrade = false;
            }

            if (_ema1.Result.HasCrossedBelow(_sma1.Result, 1) && Po.Length == 0 && CheckTime())
            {
                cross = _sma1.Result.Last(1);
                ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(Lots), "cs", SL, TP);
                AllowTrade = false;
            }
        }

        private bool CheckTime()
        {
            var startTime1 = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StartHour1, StartMinute1, 0);
            var stopTime1 = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StopHour1, StopMinute1, 0);

            var startTime2 = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StartHour2, StartMinute2, 0);
            var stopTime2 = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StopHour2, StopMinute2, 0);

            if ((Server.TimeInUtc > startTime1 && Server.TimeInUtc < stopTime1)||(Server.TimeInUtc > startTime2 && Server.TimeInUtc < stopTime2))
            { return true; }

            else { return false; }
        }

        protected override void OnStop()
        {
            // Handle cBot stop here
        }
    }
}