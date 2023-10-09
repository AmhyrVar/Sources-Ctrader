//To use Math
using System;
//To use Positions.Count() method
using System.Linq;
using cAlgo.API;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class SmartGridV3Decimal : Robot
    {
        [Parameter("Maximum open buy position?", Group = "Basic Setup", DefaultValue = 8, MinValue = 0)]
        public int MaxOpenBuy { get; set; }

        [Parameter("Maximum open Sell position?", Group = "Basic Setup", DefaultValue = 8, MinValue = 0)]
        public int MaxOpenSell { get; set; }

        //NODE

        [Parameter("Pip step", Group = "Basic Setup", DefaultValue = 10, MinValue = 1)]
        public double PipStep { get; set; }

        public double BPipStep;
        public double SPipStep;

        [Parameter("Pip multiplier", Group = "Basic Setup", DefaultValue = 2, MinValue = 1)]
        public double PipMult { get; set; }
        //END NODE 

        [Parameter("Stop loss pips", Group = "Basic Setup", DefaultValue = 100, MinValue = 10, Step = 10)]
        public double StopLossPips { get; set; }

        [Parameter("First order volume", Group = "Basic Setup", DefaultValue = 1000, MinValue = 1, Step = 1)]
        public double FirstVolume { get; set; }

        [Parameter("Max spread allowed to open position", Group = "Basic Setup", DefaultValue = 3.0)]
        public double MaxSpread { get; set; }

        [Parameter("Target profit for each group of trade", Group = "Basic Setup", DefaultValue = 3, MinValue = 1)]
        public int AverageTakeProfit { get; set; }

        [Parameter("Debug flag, set to No on real account to avoid closing all positions when stoping this cBot", Group = "Advanced Setup", DefaultValue = false)]
        public bool IfCloseAllPositionsOnStop { get; set; }

        [Parameter("Volume exponent", Group = "Advanced Setup", DefaultValue = 1.0, MinValue = 0.1, MaxValue = 5.0)]
        public double VolumeExponent { get; set; }

        private string ThiscBotLabel;
        private DateTime LastBuyTradeTime;
        private DateTime LastSellTradeTime;

        // cBot initialization
        protected override void OnStart()
        {

            BPipStep = PipStep;
            SPipStep = PipStep;
            Print("Initial Buy PipStep " + BPipStep);
            Print("Initial Sell PipStep " + SPipStep);

            // Set position label to cBot name
            ThiscBotLabel = this.GetType().Name;
            // Normalize volume in case a wrong volume was entered
            if (FirstVolume != (FirstVolume = Symbol.NormalizeVolumeInUnits(FirstVolume)))
            {
                Print("Volume entered incorrectly, volume has been changed to ", FirstVolume);
            }
        }

        // Error handling
        protected override void OnError(Error error)
        {
            Print("Error occured, error code: ", error.Code);
        }

        protected override void OnTick()
        {
            // Close all buy positions if all buy positions' target profit is met
            if (Positions.Count(x => x.TradeType == TradeType.Buy && x.SymbolName == SymbolName && x.Label == ThiscBotLabel) > 0)
            {
                if (Positions.Where(x => x.TradeType == TradeType.Buy && x.SymbolName == SymbolName && x.Label == ThiscBotLabel).Average(x => x.NetProfit) >= FirstVolume * AverageTakeProfit * Symbol.PipSize)
                {
                    Print("Profit is " + Positions.Where(x => x.TradeType == TradeType.Buy && x.SymbolName == SymbolName && x.Label == ThiscBotLabel).Average(x => x.NetProfit));
                    foreach (var position in Positions)
                    {
                        if (position.TradeType == TradeType.Buy && position.SymbolName == SymbolName && position.Label == ThiscBotLabel)
                            ClosePosition(position);
                        BPipStep = PipStep;
                    }
                }
            }
            // Close all sell positions if all sell positions' target profit is met
            if (Positions.Count(x => x.TradeType == TradeType.Sell && x.SymbolName == SymbolName && x.Label == ThiscBotLabel) > 0)
            {
                if (Positions.Where(x => x.TradeType == TradeType.Sell && x.SymbolName == SymbolName && x.Label == ThiscBotLabel).Average(x => x.NetProfit) >= FirstVolume * AverageTakeProfit * Symbol.PipSize)
                {
                    Print("Profit is " + Positions.Where(x => x.TradeType == TradeType.Sell && x.SymbolName == SymbolName && x.Label == ThiscBotLabel).Average(x => x.NetProfit));
                    foreach (var position in Positions)
                    {
                        if (position.TradeType == TradeType.Sell && position.SymbolName == SymbolName && position.Label == ThiscBotLabel)
                            ClosePosition(position);
                        SPipStep = PipStep;
                    }
                }
            }
            // Conditions check before process trade
            if (Symbol.Spread / Symbol.PipSize <= MaxSpread)
            {
                if (Positions.Count(x => x.TradeType == TradeType.Buy && x.SymbolName == SymbolName && x.Label == ThiscBotLabel) < MaxOpenBuy)
                    ProcessBuy();
                if (Positions.Count(x => x.TradeType == TradeType.Sell && x.SymbolName == SymbolName && x.Label == ThiscBotLabel) < MaxOpenSell)
                    ProcessSell();
            }

            //if (!this.IsBacktesting)
            DisplayStatusOnChart();
        }

        protected override void OnStop()
        {
            Chart.RemoveAllObjects();
            // Close all open positions opened by this cBot on stop
            if (this.IsBacktesting || IfCloseAllPositionsOnStop)
            {
                foreach (var position in Positions)
                {
                    if (position.SymbolName == SymbolName && position.Label == ThiscBotLabel)
                        ClosePosition(position);
                }
            }
        }

        private void ProcessBuy()
        {
            //I1


            if (Positions.Count(x => x.TradeType == TradeType.Buy && x.SymbolName == SymbolName && x.Label == ThiscBotLabel) == 0 && Bars.ClosePrices.Last(1) > Bars.ClosePrices.Last(2))
            {
                ExecuteMarketOrder(TradeType.Buy, SymbolName, FirstVolume, ThiscBotLabel, StopLossPips, null);
                LastBuyTradeTime = Bars.OpenTimes.Last(0);
                Print("NOOOOOOOODE BUY");
                Print("Buy pipstep " + BPipStep);

            }
            if (Positions.Count(x => x.TradeType == TradeType.Buy && x.SymbolName == SymbolName && x.Label == ThiscBotLabel) > 0)
            {
                if (Symbol.Ask < (Positions.Where(x => x.TradeType == TradeType.Buy && x.SymbolName == SymbolName && x.Label == ThiscBotLabel).Min(x => x.EntryPrice) - BPipStep * Symbol.PipSize) && LastBuyTradeTime != Bars.OpenTimes.Last(0))
                {
                    ExecuteMarketOrder(TradeType.Buy, SymbolName, CalculateVolume(TradeType.Buy), ThiscBotLabel, StopLossPips, null);
                    LastBuyTradeTime = Bars.OpenTimes.Last(0);
                    BPipStep = BPipStep * PipMult;
                    Print("Buy pipstep " + BPipStep);



                }
            }



        }

        private void ProcessSell()
        {

            if (Positions.Count(x => x.TradeType == TradeType.Sell && x.SymbolName == SymbolName && x.Label == ThiscBotLabel) == 0 && Bars.ClosePrices.Last(2) > Bars.ClosePrices.Last(1))
            {
                Print("Sell pipstep " + SPipStep);
                ExecuteMarketOrder(TradeType.Sell, SymbolName, FirstVolume, ThiscBotLabel, StopLossPips, null);
                LastSellTradeTime = Bars.OpenTimes.Last(0);
                Print("NOOOOOOOODE SELL");
            }
            if (Positions.Count(x => x.TradeType == TradeType.Sell && x.SymbolName == SymbolName && x.Label == ThiscBotLabel) > 0)
            {
                if (Symbol.Bid > (Positions.Where(x => x.TradeType == TradeType.Sell && x.SymbolName == SymbolName && x.Label == ThiscBotLabel).Max(x => x.EntryPrice) + SPipStep * Symbol.PipSize) && LastSellTradeTime != Bars.OpenTimes.Last(0))
                {
                    ExecuteMarketOrder(TradeType.Sell, SymbolName, CalculateVolume(TradeType.Sell), ThiscBotLabel, StopLossPips, null);
                    LastSellTradeTime = Bars.OpenTimes.Last(0);
                    Print("Sell pipstep " + SPipStep);
                    SPipStep = SPipStep * PipMult;
                }
            }


        }

        private double CalculateVolume(TradeType tradeType)
        {
            return Symbol.NormalizeVolumeInUnits(FirstVolume * Math.Pow(VolumeExponent, Positions.Count(x => x.TradeType == tradeType && x.SymbolName == SymbolName && x.Label == ThiscBotLabel)));
        }

        private void DisplayStatusOnChart()
        {
            if (Positions.Count(x => x.TradeType == TradeType.Buy && x.SymbolName == SymbolName && x.Label == ThiscBotLabel) > 1)
            {
                var y = Positions.Where(x => x.TradeType == TradeType.Buy && x.SymbolName == SymbolName && x.Label == ThiscBotLabel).Average(x => x.EntryPrice);
                Chart.DrawHorizontalLine("bpoint", y, Color.Yellow, 2, LineStyle.Dots);
            }
            else
                Chart.RemoveObject("bpoint");
            if (Positions.Count(x => x.TradeType == TradeType.Sell && x.SymbolName == SymbolName && x.Label == ThiscBotLabel) > 1)
            {
                var z = Positions.Where(x => x.TradeType == TradeType.Sell && x.SymbolName == SymbolName && x.Label == ThiscBotLabel).Average(x => x.EntryPrice);
                Chart.DrawHorizontalLine("spoint", z, Color.HotPink, 2, LineStyle.Dots);
            }
            else
                Chart.RemoveObject("spoint");
            Chart.DrawStaticText("pan", GenerateStatusText(), VerticalAlignment.Top, HorizontalAlignment.Left, Color.Tomato);
        }

        private string GenerateStatusText()
        {
            var statusText = "";
            var buyPositions = "";
            var sellPositions = "";
            var spread = "";
            var buyDistance = "";
            var sellDistance = "";
            spread = "\nSpread = " + Math.Round(Symbol.Spread / Symbol.PipSize, 1);
            buyPositions = "\nBuy Positions = " + Positions.Count(x => x.TradeType == TradeType.Buy && x.SymbolName == SymbolName && x.Label == ThiscBotLabel);
            sellPositions = "\nSell Positions = " + Positions.Count(x => x.TradeType == TradeType.Sell && x.SymbolName == SymbolName && x.Label == ThiscBotLabel);
            if (Positions.Count(x => x.TradeType == TradeType.Buy && x.SymbolName == SymbolName && x.Label == ThiscBotLabel) > 0)
            {
                var averageBuyFromCurrent = Math.Round((Positions.Where(x => x.TradeType == TradeType.Buy && x.SymbolName == SymbolName && x.Label == ThiscBotLabel).Average(x => x.EntryPrice) - Symbol.Bid) / Symbol.PipSize, 1);
                buyDistance = "\nBuy Target Away = " + averageBuyFromCurrent;
            }
            if (Positions.Count(x => x.TradeType == TradeType.Sell && x.SymbolName == SymbolName && x.Label == ThiscBotLabel) > 0)
            {
                var averageSellFromCurrent = Math.Round((Symbol.Ask - Positions.Where(x => x.TradeType == TradeType.Sell && x.SymbolName == SymbolName && x.Label == ThiscBotLabel).Average(x => x.EntryPrice)) / Symbol.PipSize, 1);
                sellDistance = "\nSell Target Away = " + averageSellFromCurrent;
            }
            if (Symbol.Spread / Symbol.PipSize > MaxSpread)
                statusText = "MAX SPREAD EXCEED";
            else
                statusText = ThiscBotLabel + buyPositions + spread + sellPositions + buyDistance + sellDistance;
            return (statusText);
        }
    }
}
