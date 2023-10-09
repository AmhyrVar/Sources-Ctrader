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
    public class andrea2 : Robot
    {
      

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

        private int StartHour;
        private int StartMinute;
        private int StopHour;
        private int StopMinute;
        protected override void OnStart()
        {
           


            string[] parts = TradeTime.Split(':');

            StartHour = int.Parse(parts[0]);
            StartMinute = int.Parse(parts[1]);

            string[] partss = CancelTime.Split(':');
            StopHour = int.Parse(partss[0]);
            StopMinute = int.Parse(partss[1]);
        }


        protected override void OnBar()
        {
            var Lpo = Positions.FindAll("Buy", SymbolName);
            var Spo = Positions.FindAll("Sell", SymbolName);
            if (GreenSignal() && CheckTime() && Lpo.Length == 0)
            {
                foreach (var po in Positions)
                {
                    if (po.TradeType == TradeType.Sell && po.SymbolName == SymbolName && po.Label == "Sell")
                    {
                        ClosePosition(po);
                    }
                }
                ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), "Buy", SL, TP);
            }
            if (RedSignal() && CheckTime() && Spo.Length == 0)
            {
                foreach (var po in Positions)
                {
                    if (po.TradeType == TradeType.Buy && po.SymbolName == SymbolName && po.Label == "Buy")
                    {
                        ClosePosition(po);
                    }
                }

                ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), "Sell", SL, TP);
            }
        }
        private bool GreenSignal()
        {
            if (Bars.OpenPrices.Last(1) < Bars.ClosePrices.Last(1))
            { return true; }
            else { return false; }
        }
        protected override void OnTick()
        {


            SetTrailingStop();



        }

        private bool RedSignal()
        {
            if (Bars.OpenPrices.Last(1) > Bars.ClosePrices.Last(1))
            { return true; }
            else { return false; }
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

        private bool CheckTime()
        {
            var startTime = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StartHour, StartMinute, 0);
            var stopTime = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StopHour, StopMinute, 0);

            if (Server.TimeInUtc > startTime && Server.TimeInUtc < stopTime)
            { return true; }
            

            else { return false; }
        }

        protected override void OnStop()
        {
            /*
             * 
             * Go long when the bar that closed is Green
                Short when the bar is Red
                TP/SL Trailing stoploss
                In case of opposite signal we close the trade and open opposite
                    Option to have a trading time window expressed with two time strings hh:mm the bot won�t trade outside
             * 
             * 
             * 
             * 
             * 
             * 
             * 
             * 
             * */
        }
    }
}