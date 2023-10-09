using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class MAcross3 : Robot
    {

        [Parameter("Use 200 SMA", DefaultValue = false)]
        public bool UseSMA { get; set; }

        [Parameter("SMA Period", DefaultValue = 200)]
        public int SMAPeriod { get; set; }
        [Parameter("SMA Source")]
        public DataSeries SMASource { get; set; }

        [Parameter("EMA Period", DefaultValue = 10)]
        public int EMAPeriod { get; set; }
        [Parameter("EMA Source")]
        public DataSeries EMASource { get; set; }


        [Parameter("small SMA Period", DefaultValue = 13)]
        public int SSMAPeriod { get; set; }
        [Parameter("Small SMA Source")]
        public DataSeries SSMASource { get; set; }





        [Parameter("LotSize", DefaultValue = 0.1)]
        public double Volume { get; set; }

        [Parameter("TP", DefaultValue = 100)]
        public double TP { get; set; }
        [Parameter("SL", DefaultValue = 50)]
        public double SL { get; set; }


        [Parameter("Trailing Stop Trigger (pips)", DefaultValue = 50, Group = "Position management")]
        public double TrailingStopTrigger { get; set; }

        [Parameter("Trailing Stop Step (pips)", DefaultValue = 10, Group = "Position management")]
        public double TrailingStopStep { get; set; }


        [Parameter("Trade Time", DefaultValue = "08:00")]
        public string TradeTime { get; set; }
        [Parameter("Stop Time", DefaultValue = "16:00")]
        public string CancelTime { get; set; }

        [Parameter("Use Time Window", DefaultValue = false)]
        public bool UseTimeOC { get; set; }



        //Continuous system EMA and Smoothed MA cross

        private SimpleMovingAverage _sma;
        private ExponentialMovingAverage _ema;
        private SimpleMovingAverage _ssma;

        private int StartHour;
        private int StartMinute;
        private int StopHour;
        private int StopMinute;
        protected override void OnStart()
        {
            _sma = Indicators.SimpleMovingAverage(SMASource, SMAPeriod);
            _ema = Indicators.ExponentialMovingAverage(EMASource, EMAPeriod);
            _ssma = Indicators.SimpleMovingAverage(SSMASource, SSMAPeriod);


            string[] parts = TradeTime.Split(':');

            StartHour = int.Parse(parts[0]);
            StartMinute = int.Parse(parts[1]);

            string[] partss = CancelTime.Split(':');
            StopHour = int.Parse(partss[0]);
            StopMinute = int.Parse(partss[1]);
        }

        private void SetTrailingStop()
        {
            var sellPositions = Positions.FindAll("Sell", SymbolName);

            foreach (Position position in sellPositions)
            {
                double distance = position.EntryPrice - Symbol.Ask;

                if (distance < TrailingStopTrigger * Symbol.PipSize)
                    continue;

                double newStopLossPrice = Symbol.Ask + TrailingStopStep * Symbol.PipSize;

                if (position.StopLoss == null || newStopLossPrice < position.StopLoss)
                {
                    ModifyPosition(position, newStopLossPrice, position.TakeProfit);
                }
            }

            var buyPositions = Positions.FindAll("Buy", SymbolName);

            foreach (Position position in buyPositions)
            {
                double distance = Symbol.Bid - position.EntryPrice;

                if (distance < TrailingStopTrigger * Symbol.PipSize)
                    continue;

                double newStopLossPrice = Symbol.Bid - TrailingStopStep * Symbol.PipSize;
                if (position.StopLoss == null || newStopLossPrice > position.StopLoss)
                {
                    ModifyPosition(position, newStopLossPrice, position.TakeProfit);
                }
            }
        }
        protected override void OnTick()
        {


            SetTrailingStop();



        }

        protected override void OnBar()
        {
         
            if (!CheckTime())
            {
                foreach (var po in Positions)
                {
                    if (po.TradeType == TradeType.Sell && po.SymbolName == SymbolName && po.Label == "Sell")
                    {
                        ClosePosition(po);
                    }
                    if (po.TradeType == TradeType.Buy && po.SymbolName == SymbolName && po.Label == "Buy")
                    {
                        ClosePosition(po);
                    }
                }
            }
            if (CheckTime())
            {
                if ( SMACheck(TradeType.Buy) && _ema.Result.Last(2) < _ssma.Result.Last(2) && _ema.Result.Last(1) > _ssma.Result.Last(1))
                {
                    foreach (var po in Positions)
                    {
                        if (po.TradeType == TradeType.Sell && po.SymbolName == SymbolName && po.Label == "Sell")
                        {
                            ClosePosition(po);
                        }
                    }


                    var pos = Positions.FindAll("Buy", SymbolName);

                    if (pos.Length == 0)
                    {
                        Print($"node  buy  emalast2 {_ema.Result.Last(2)}  sma2 {_ssma.Result.Last(2)} ema 1ast1 {_ema.Result.Last(1)} sma1 {_ssma.Result.Last(1)}   ");

                        

                        ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), "Buy", SL, TP);
                    }

                   
                }

                if (_ema.Result.Last(2) > _ssma.Result.Last(2) && _ema.Result.Last(1) < _ssma.Result.Last(1) && SMACheck(TradeType.Sell))
                {
                    foreach (var po in Positions)
                    {
                        if (po.TradeType == TradeType.Buy && po.SymbolName == SymbolName && po.Label == "Buy")
                        {
                            ClosePosition(po);
                        }
                    }

                    var pos = Positions.FindAll("Sell", SymbolName);

                    if (pos.Length == 0)
                    {
                        Print($"node  sell  emalast2 {_ema.Result.Last(2)}  sma2 {_ssma.Result.Last(2)} ema 1ast1 {_ema.Result.Last(1)} sma1  {_ssma.Result.Last(1)}   ");

                        ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), "Sell", SL, TP);
                    }

                   
                }



               

            }
            Diag();
        }

        bool SMACheck(TradeType tt)
        {
            if (!UseSMA)
            { return true; }
            if (UseSMA && tt == TradeType.Buy && Bars.ClosePrices.Last(1) > _sma.Result.Last(1) )
            { return true;}
            if (UseSMA && tt == TradeType.Sell && Bars.ClosePrices.Last(1) < _sma.Result.Last(1))
            { return true; }
            else { return false; }

        }

        private bool CheckTime()
        {
            var startTime = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StartHour, StartMinute, 0);
            var stopTime = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StopHour, StopMinute, 0);

            if (Server.TimeInUtc > startTime && Server.TimeInUtc < stopTime)
            { return true; }
            if (!UseTimeOC)
            { return true; }

            else { return false; }
        }

        private void Diag()
        {
            foreach (var po in Positions)
            {
                if (po.TradeType == TradeType.Buy && po.SymbolName == SymbolName && po.Label == "Buy" && _ema.Result.Last(1) < _ssma.Result.Last(1))
                { Print("Error buy"); }
                if (po.TradeType == TradeType.Sell && po.SymbolName == SymbolName && po.Label == "Sell" && _ema.Result.Last(1) > _ssma.Result.Last(1))
                { Print("Error sell"); }
            }
        }

        protected override void OnStop()
        {
            // Handle cBot stop here
        }
    }
}