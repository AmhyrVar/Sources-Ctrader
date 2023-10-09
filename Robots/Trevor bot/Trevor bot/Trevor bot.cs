using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Trevorbot : Robot
    {
        [Parameter("AB Volume", DefaultValue = 0.1)]
        public double volume { get; set; }
        [Parameter("AB Stop Loss", DefaultValue = 10)]
        public double SL { get; set; }
        [Parameter("AB Take Profit", DefaultValue = 20)]
        public double TP { get; set; }


        [Parameter("C Volume", DefaultValue = 0.1)]
        public double C_volume { get; set; }

        [Parameter("Stop Loss C", DefaultValue = 10)]
        public double SL1 { get; set; }
        [Parameter("Take ProfitC", DefaultValue = 20)]
        public double TP1 { get; set; }

        [Parameter("Breakeven add", DefaultValue = 2)]
        public double BEADD { get; set; }


        [Parameter("Pips to trail", DefaultValue = 10)]
        public double Trail { get; set; }

        public bool IsCOpen = false;

        protected override void OnStart()
        {

            Positions.Closed += PositionsOnClosed;
            ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Short", SL, TP);
            ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Long", SL, TP);
        }

        private void PositionsOnClosed(PositionClosedEventArgs args)
        {

            switch (args.Reason)
            {
                case PositionCloseReason.StopLoss:

                    if (!IsCOpen && args.Position.TradeType == TradeType.Buy && args.Position.Label == "Long")
                    {
                        ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(C_volume), "SC", SL1, TP1);
                        IsCOpen = true;
                    }
                    if (!IsCOpen && args.Position.TradeType == TradeType.Sell && args.Position.Label == "Short")
                    {
                        ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(C_volume), "LC", SL1, TP1);
                        IsCOpen = true;
                    }
                    break;

            }
        }




        protected override void OnTick()
        {


            var LCpos = Positions.FindAll("LC", SymbolName);
            var SCpos = Positions.FindAll("SC", SymbolName);
            var Longpos = Positions.FindAll("Long", SymbolName);
            var Shortpos = Positions.FindAll("Short", SymbolName);


            if (SCpos.Length == 0 && LCpos.Length == 0 && Longpos.Length == 0 && Shortpos.Length == 0)
            {
                ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Short", SL, TP);
                ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Long", SL, TP);
                IsCOpen = false;
            }



            if (LCpos.Length > 0)
            {
                foreach (var po in LCpos)
                {
                    if (po.StopLoss < po.EntryPrice && po.Pips >= Trail)
                    {

                        po.ModifyStopLossPrice(Math.Round((po.EntryPrice + 2 * Symbol.PipSize), Symbol.Digits));
                    }

                }
            }
            if (Longpos.Length > 0)
            {
                foreach (var po in Longpos)
                {

                    if (po.StopLoss < po.EntryPrice && po.Pips >= Trail)
                    {

                        po.ModifyStopLossPrice(Math.Round((po.EntryPrice + 2 * Symbol.PipSize), Symbol.Digits));
                    }

                }
            }

            if (Shortpos.Length > 0)
            {
                foreach (var po in Shortpos)
                {
                    if (po.StopLoss > po.EntryPrice && po.Pips >= Trail)
                    {

                        po.ModifyStopLossPrice(Math.Round((po.EntryPrice - 2 * Symbol.PipSize), Symbol.Digits));
                    }

                }
            }
            if (SCpos.Length > 0)
            {
                foreach (var po in SCpos)
                {

                    if (po.StopLoss > po.EntryPrice && po.Pips >= Trail)
                    {

                        po.ModifyStopLossPrice(Math.Round((po.EntryPrice - 2 * Symbol.PipSize), Symbol.Digits));
                    }


                }
            }





            if (Longpos.Length > 0)
            {
                foreach (var po in Longpos)
                {

                    if (po.StopLoss != null && Math.Abs(Convert.ToDouble(po.EntryPrice - po.StopLoss)) / Symbol.PipSize < SL && (Symbol.Bid - po.StopLoss) / Symbol.PipSize > 2 * Trail)
                    {
                        Print(Math.Abs(Convert.ToDouble(po.EntryPrice - po.StopLoss)) * Symbol.PipSize);
                        po.ModifyStopLossPips(-GetSLPips(po.Pips, Convert.ToDouble(po.StopLoss), po.EntryPrice));
                    }

                }
            }
            if (LCpos.Length > 0)
            {
                foreach (var po in LCpos)
                {
                    if (po.StopLoss != null && Math.Abs(Convert.ToDouble(po.EntryPrice - po.StopLoss)) / Symbol.PipSize < SL && (Symbol.Bid - po.StopLoss) / Symbol.PipSize > 2 * Trail)
                    {
                        po.ModifyStopLossPips(-GetSLPips(po.Pips, Convert.ToDouble(po.StopLoss), po.EntryPrice));
                    }

                }
            }

            if (SCpos.Length > 0)
            {
                foreach (var po in SCpos)
                {
                    if (po.StopLoss != null && Math.Abs(Convert.ToDouble(po.EntryPrice - po.StopLoss)) / Symbol.PipSize < SL && (po.StopLoss - Symbol.Ask) / Symbol.PipSize > 2 * Trail)
                    {
                        po.ModifyStopLossPips(-GetSLPips(po.Pips, Convert.ToDouble(po.StopLoss), po.EntryPrice));
                    }

                }
            }

            if (Shortpos.Length > 0)
            {
                foreach (var po in Shortpos)
                {
                    if (po.StopLoss != null && Math.Abs(Convert.ToDouble(po.EntryPrice - po.StopLoss)) / Symbol.PipSize < SL && (po.StopLoss - Symbol.Ask) / Symbol.PipSize > 2 * Trail)
                    {
                        po.ModifyStopLossPips(-GetSLPips(po.Pips, Convert.ToDouble(po.StopLoss), po.EntryPrice));
                    }

                }
            }
        }


        private double GetSLPips(double PoPips, double PoSLPrice, double PoEntry)
        {
            var SLpips = Math.Abs(Convert.ToDouble(PoSLPrice - PoEntry));

            var x = Math.Floor(PoPips / Trail);

            return (x - 1) * Trail;
        }

    }
}
