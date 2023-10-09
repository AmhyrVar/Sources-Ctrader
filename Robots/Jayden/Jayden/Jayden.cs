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
    public class Jayden : Robot
    {
        /*
         * 
        it triggered 2 candles after the confirmation   14 Feb 2023, 21:11 EURUSD m1
  
         * */

        [Parameter("Trade Time", DefaultValue = "08:00")]
        public string TradeTime { get; set; }
        [Parameter("Stop Time", DefaultValue = "16:00")]
        public string CancelTime { get; set; }

        [Parameter("Risk %", DefaultValue = 2, Group = "Position management")]
        public double Balance_per { get; set; }
        [Parameter("Max spread", DefaultValue = 0.1, Group = "Position management")]
        public double MaxSpread { get; set; }

        [Parameter("Min pipsize confirmation candle", DefaultValue = 10, Group = "Confirmation candle params")]
        public double ConfCandlePips { get; set; }



        private int StartHour;
        private int StartMinute;
        private int StopHour;
        private int StopMinute;


        private FractalChaosBands _fcb;
        protected override void OnStart()
        {
            _fcb = Indicators.FractalChaosBands();


            string[] parts = TradeTime.Split(':');

            StartHour = int.Parse(parts[0]);
            StartMinute = int.Parse(parts[1]);

            string[] partss = CancelTime.Split(':');
            StopHour = int.Parse(partss[0]);
            StopMinute = int.Parse(partss[1]);
        }

        protected double GetVolume(double SL)
        {

            var maxAmountRisked = Account.Equity * (Balance_per / 100);

            return Symbol.NormalizeVolumeInUnits(maxAmountRisked / (SL * Symbol.PipValue), RoundingMode.Down);



        }
        private bool CheckTime()
        {
            var startTime = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StartHour, StartMinute, 0);
            var stopTime = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StopHour, StopMinute, 0);

            if (Server.TimeInUtc > startTime && Server.TimeInUtc < stopTime)
            { return true; }

            else { return false; }
        }
        private static double GetPriceAdjustmentByTradeType(TradeType tradeType, double priceDifference)
        {
            if (tradeType == TradeType.Buy)
                return priceDifference;

            return -priceDifference;
        }

        protected override void OnTick()
        {
            foreach (var position in Positions)
            {
                var tp_pips = Math.Abs (Convert.ToDouble(position.EntryPrice - position.TakeProfit) /Symbol.PipSize );
                if (position.SymbolName == SymbolName && position.Pips >= (tp_pips/2) ) // change bepips
                {

                    if (position.TradeType == TradeType.Buy && position.StopLoss < position.EntryPrice)
                    {
                        var _symbolInfo = Symbols.GetSymbolInfo(position.SymbolName);
                        var desiredNetProfitInDepositAsset = 1 * _symbolInfo.PipValue * position.VolumeInUnits;
                        var desiredGrossProfitInDepositAsset = desiredNetProfitInDepositAsset - position.Commissions * 2 - position.Swap;
                        var quoteToDepositRate = _symbolInfo.PipValue / _symbolInfo.PipSize;
                        var priceDifference = desiredGrossProfitInDepositAsset / (position.VolumeInUnits * quoteToDepositRate);

                        var priceAdjustment = GetPriceAdjustmentByTradeType(position.TradeType, priceDifference);
                        var breakEvenLevel = position.EntryPrice + priceAdjustment;
                        var roundedBreakEvenLevel = RoundPrice(breakEvenLevel, position.TradeType);

                        ModifyPosition(position, roundedBreakEvenLevel, position.TakeProfit);
                        Print("BE is made");

                    }
                    if (position.TradeType == TradeType.Sell && position.StopLoss > position.EntryPrice)
                    {
                        var _symbolInfo = Symbols.GetSymbolInfo(position.SymbolName);
                        var desiredNetProfitInDepositAsset = 1 * _symbolInfo.PipValue * position.VolumeInUnits;
                        var desiredGrossProfitInDepositAsset = desiredNetProfitInDepositAsset - position.Commissions * 2 - position.Swap;
                        var quoteToDepositRate = _symbolInfo.PipValue / _symbolInfo.PipSize;
                        var priceDifference = desiredGrossProfitInDepositAsset / (position.VolumeInUnits * quoteToDepositRate);

                        var priceAdjustment = GetPriceAdjustmentByTradeType(position.TradeType, priceDifference);
                        var breakEvenLevel = position.EntryPrice + priceAdjustment;
                        var roundedBreakEvenLevel = RoundPrice(breakEvenLevel, position.TradeType);

                        ModifyPosition(position, roundedBreakEvenLevel, position.TakeProfit);
                        Print("BE is made");

                    }

                }
            }
        }

        protected override void OnBar()
        {
        


            var Pends = PendingOrders.Where(order => order.Label.Equals("Pend", System.StringComparison.OrdinalIgnoreCase) && order.SymbolName.Equals(SymbolName));
            var pos = Positions.FindAll("Pend",SymbolName);

            foreach (var p in Pends)
            {
                CancelPendingOrder(p); 
            }

            //1.Confirmation candle.Open from<fractal chaos band high and close > fractal chaos band high.Fractal chaos band indicator is built in ctrader.

            if (Pends.Count() == 0 && pos.Length == 0 && Confirmation_candle() && Symbol.Spread < MaxSpread && ConCandleBigger())
            {
                if (CheckTime() && Bars.OpenPrices.Last(1) < _fcb.High.Last(1) && Bars.ClosePrices.Last(1) > _fcb.High.Last(1) && Bars.OpenPrices.Last(0) <= Bars.ClosePrices.Last(1))
                {

                    //Math.Abs(Convert.ToDouble(po.EntryPrice - po.StopLoss)) / Symbol.PipSize)

                    var sl = Math.Abs(Convert.ToDouble((Bars.HighPrices.Last(1) - Bars.LowPrices.Last(1)) / Symbol.PipSize));
                    sl = sl + 0.1 * sl;
                    Print($"SL is {sl}");

                    if (Symbol.Ask < Bars.HighPrices.Last(1))
                    {
                        PlaceStopOrder(TradeType.Buy, SymbolName, GetVolume(sl), RoundPrice(Bars.HighPrices.Last(1), TradeType.Buy), "Pend", sl, 2 * sl);
                    }

                    if (Symbol.Ask > Bars.HighPrices.Last(1))
                    {
                        PlaceLimitOrder(TradeType.Buy, SymbolName, GetVolume(sl), RoundPrice(Bars.HighPrices.Last(1), TradeType.Buy), "Pend", sl, 2 * sl);
                    }
                }

                if (CheckTime() && Bars.OpenPrices.Last(1) > _fcb.Low.Last(1) && Bars.ClosePrices.Last(1) < _fcb.Low.Last(1) && Bars.OpenPrices.Last(0) >= Bars.ClosePrices.Last(1))
                {
                    var sl = Math.Abs(Convert.ToDouble((Bars.HighPrices.Last(1) - Bars.LowPrices.Last(1)) / Symbol.PipSize));
                    sl = sl + 0.1 * sl;

                    Print($"SL is {sl}");
                    if (Symbol.Bid > Bars.LowPrices.Last(1))
                    {
                        PlaceStopOrder(TradeType.Sell, SymbolName, GetVolume(sl), RoundPrice(Bars.LowPrices.Last(1), TradeType.Sell), "Pend", sl, 2 * sl);
                    }

                    if (Symbol.Bid < Bars.LowPrices.Last(1))
                    {
                        PlaceLimitOrder(TradeType.Sell, SymbolName, GetVolume(sl), RoundPrice(Bars.LowPrices.Last(1), TradeType.Sell), "Pend", sl, 2 * sl);
                    }
                }
            }
        }

       
        private double RoundPrice(double price, TradeType tradeType)
        {
            var _symbolInfo = Symbols.GetSymbolInfo(SymbolName);
            var multiplier = Math.Pow(10, _symbolInfo.Digits);

            if (tradeType == TradeType.Buy)
                return Math.Ceiling(price * multiplier) / multiplier;

            return Math.Floor(price * multiplier) / multiplier;
        }
        private bool Confirmation_candle()
        {
            var pips = Math.Abs(Convert.ToDouble((Bars.HighPrices.Last(1) - Bars.LowPrices.Last(1)) / Symbol.PipSize));
            if (pips >= ConfCandlePips)
                return true;
            else { return false; }
        }
        private bool ConCandleBigger()
        {
            var pips = Math.Abs(Convert.ToDouble((Bars.HighPrices.Last(1) - Bars.LowPrices.Last(1)) / Symbol.PipSize));
            var pips2 = Math.Abs(Convert.ToDouble((Bars.HighPrices.Last(2) - Bars.LowPrices.Last(2)) / Symbol.PipSize));
            if (pips > pips2)
                return true;
            else { return false; }
        }

        protected override void OnStop()
        {
            // Handle cBot stop here
        }
    }
}