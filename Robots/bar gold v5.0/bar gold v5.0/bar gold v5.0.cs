﻿using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Barsgoldatrupdate : Robot
    {
        [Parameter("password", DefaultValue = "")]
        public string Pwd { get; set; }

        IchimokuKinkoHyo ichimoku;
        [Parameter("Tenkan Sen Periods", DefaultValue = 9)]
        public int Tenkan { get; set; }
        [Parameter("Kijun Sen Periods", DefaultValue = 26)]
        public int Kijun { get; set; }
        [Parameter("Senkou Span B Periods", DefaultValue = 52)]
        public int SSB { get; set; }

        MovingAverage MA;


        [Parameter(DefaultValue = true)]
        public bool Default_Strategy { get; set; }


        [Parameter(DefaultValue = 0.1)]
        public double volume { get; set; }



        [Parameter(DefaultValue = true)]
        public bool ichimokusignal { get; set; }


        [Parameter("Source MA")]
        public DataSeries SourceMA { get; set; }

        [Parameter("Period MA", DefaultValue = 50)]
        public int PMA { get; set; }

        [Parameter("Type MA", DefaultValue = MovingAverageType.Exponential)]
        public MovingAverageType TMA { get; set; }



        [Parameter("Only short position")]
        public bool S1 { get; set; }
        //done
        [Parameter("S&L fixed distance")]
        public bool S2 { get; set; }
        [Parameter("S&L Updated Trailing stop")]
        public bool S3 { get; set; }
        [Parameter("S&L trailing on MA")]
        public bool S4 { get; set; }
        //done
        // 0 for idle 
        public double BuyDelta;
        public double SellDelta;



        protected override void OnStart()
        {
            ichimoku = Indicators.IchimokuKinkoHyo(Tenkan, Kijun, SSB);


            MA = Indicators.MovingAverage(SourceMA, PMA, TMA);
            if (Pwd != "GBPZAR")
            {
                Stop();
            }
        }



        protected override void OnBar()
        {
            if ((Bars.ClosePrices.Last(1) > ichimoku.SenkouSpanA.Last(Kijun) && Bars.ClosePrices.Last(1) < ichimoku.SenkouSpanB.Last(Kijun)) || (Bars.ClosePrices.Last(1) < ichimoku.SenkouSpanA.Last(Kijun) && Bars.ClosePrices.Last(1) > ichimoku.SenkouSpanB.Last(Kijun)))
            {
                ichimokusignal = true;
            }
            if ((Bars.OpenPrices.Last(1) > ichimoku.SenkouSpanA.Last(Kijun) && Bars.OpenPrices.Last(1) < ichimoku.SenkouSpanB.Last(Kijun)) || (Bars.OpenPrices.Last(1) < ichimoku.SenkouSpanA.Last(Kijun) && Bars.OpenPrices.Last(1) > ichimoku.SenkouSpanB.Last(Kijun)))
            {
                ichimokusignal = true;
            }

            var case_1_pos = Positions.FindAll("Case1", SymbolName);

            var case_2_pos = Positions.FindAll("Case2", SymbolName);
            var case_3_pos = Positions.FindAll("Case3", SymbolName);
            var case_4_pos = Positions.FindAll("Case4", SymbolName);


            var case_1_post = Positions.FindAll("Case1t", SymbolName);

            var case_2_post = Positions.FindAll("Case2t", SymbolName);
            var case_3_post = Positions.FindAll("Case3t", SymbolName);
            var case_4_post = Positions.FindAll("Case4t", SymbolName);







            if (case_1_pos.Length > 0)
            {
                foreach (var po in case_1_pos)
                {
                    if (Bars.ClosePrices.Last(1) > Bars.OpenPrices.Last(1))
                    {
                        ClosePosition(po);
                        Print("Bullish close Case1");
                    }
                }
            }

            if (case_2_pos.Length > 0)
            {
                foreach (var po in case_2_pos)
                {
                    if (Bars.ClosePrices.Last(1) > Bars.OpenPrices.Last(1))
                    {
                        ClosePosition(po);
                        Print("Bullish close Case2");
                    }
                }
            }


            if (case_3_pos.Length > 0)
            {
                foreach (var po in case_3_pos)
                {
                    if (Bars.ClosePrices.Last(1) < Bars.OpenPrices.Last(1))
                    {
                        ClosePosition(po);
                        Print("Bearish close Case3");
                    }
                }
            }


            if (case_4_pos.Length > 0)
            {
                foreach (var po in case_4_pos)
                {
                    if (Bars.ClosePrices.Last(1) < Bars.OpenPrices.Last(1))
                    {
                        ClosePosition(po);
                        Print("Bearish close Case4");
                    }
                }
            }


            if (S4)
            {

                if (case_1_post.Length > 0)
                {
                    foreach (var po in case_1_post)
                    {
                        ModifyPosition(po, MA.Result.LastValue, null);
                    }
                }
                if (case_2_post.Length > 0)
                {
                    foreach (var po in case_2_post)
                    {
                        ModifyPosition(po, MA.Result.LastValue, null);
                    }
                }
                if (case_3_post.Length > 0)
                {
                    foreach (var po in case_3_post)
                    {
                        ModifyPosition(po, MA.Result.LastValue, null);
                    }
                }
                if (case_4_post.Length > 0)
                {
                    foreach (var po in case_4_post)
                    {
                        ModifyPosition(po, MA.Result.LastValue, null);
                    }
                }
            }

            if (S3)
            {

                if (case_1_post.Length > 0)
                {
                    foreach (var po in case_1_post)
                    {
                        if (po.StopLoss < Bars.LowPrices.Last(1))
                        {
                            ModifyPosition(po, Bars.LowPrices.Last(1), null);
                        }
                    }
                }
                if (case_2_post.Length > 0)
                {
                    foreach (var po in case_2_post)
                    {
                        if (po.StopLoss < Bars.LowPrices.Last(1))
                        {
                            ModifyPosition(po, Bars.LowPrices.Last(1), null);
                        }
                    }
                }
                if (case_3_post.Length > 0)
                {
                    foreach (var po in case_3_post)
                    {
                        if (po.StopLoss > Bars.HighPrices.Last(1))
                        {
                            ModifyPosition(po, Bars.HighPrices.Last(1), null);
                        }
                    }
                }
                if (case_4_post.Length > 0)
                {
                    foreach (var po in case_4_post)
                    {
                        if (po.StopLoss > Bars.HighPrices.Last(1))
                        {
                            ModifyPosition(po, Bars.HighPrices.Last(1), null);
                        }
                    }
                }
            }

            if (S2)
            {

                if (case_1_post.Length > 0)
                {
                    foreach (var po in case_1_post)
                    {
                        if ((Bars.HighPrices.Last(1) - po.StopLoss) > BuyDelta)
                        {
                            var newSL = (Bars.HighPrices.Last(1) - BuyDelta);
                            ModifyPosition(po, newSL, null);

                            Print("SL modified");
                        }
                    }
                }
                if (case_2_post.Length > 0)
                {
                    foreach (var po in case_2_post)
                    {
                        if ((Bars.HighPrices.Last(1) - po.StopLoss) > BuyDelta)
                        {

                            var newSL = (Bars.HighPrices.Last(1) - BuyDelta);
                            ModifyPosition(po, newSL, null);
                            Print("SL modified");
                        }
                    }
                }
                if (case_3_post.Length > 0)
                {
                    foreach (var po in case_3_post)
                    {
                        if ((po.StopLoss - Bars.LowPrices.Last(1)) > SellDelta)
                        {
                            var newSL = Bars.LowPrices.Last(1) + SellDelta;
                            ModifyPosition(po, newSL, null);
                            Print("SL modified");
                        }
                    }
                }
                if (case_4_post.Length > 0)
                {
                    foreach (var po in case_4_post)
                    {
                        if ((po.StopLoss - Bars.LowPrices.Last(1)) > SellDelta)
                        {
                            var newSL = Bars.LowPrices.Last(1) + SellDelta;
                            ModifyPosition(po, newSL, null);
                            Print("SL modified");
                        }
                    }
                }
            }


            if (ichimokusignal && Bars.ClosePrices.Last(1) > ichimoku.SenkouSpanA.Last(Kijun) && Bars.ClosePrices.Last(1) > ichimoku.SenkouSpanB.Last(Kijun) && Bars.OpenPrices.Last(1) > ichimoku.SenkouSpanA.Last(Kijun) && Bars.OpenPrices.Last(1) > ichimoku.SenkouSpanB.Last(Kijun) && Bars.ClosePrices.Last(1) > Bars.OpenPrices.Last(1) && Bars.HighPrices.Last(1) > Bars.HighPrices.Last(2) && Bars.LowPrices.Last(1) > Bars.LowPrices.Last(2))
            {

                var SLpips = (Symbol.Ask - Bars.LowPrices.Last(2)) / Symbol.PipSize;
                if (S1)
                {
                    ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Case1", SLpips, null);
                }

                if (S2 || S3 || S4)
                {
                    ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Case1", SLpips, null);
                    ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Case1t", SLpips, null);
                    var po = Positions.Find("Case1t", SymbolName);
                    BuyDelta = Convert.ToDouble(po.EntryPrice - po.StopLoss);
                }

                Print("Open Case1");
                Print("SSB " + ichimoku.SenkouSpanB.Last(26) + " SSA " + ichimoku.SenkouSpanA.Last(26));

                ichimokusignal = false;
            }



            if (ichimokusignal && Bars.ClosePrices.Last(1) > ichimoku.SenkouSpanA.Last(Kijun) && Bars.ClosePrices.Last(1) > ichimoku.SenkouSpanB.Last(Kijun) && Bars.OpenPrices.Last(1) > ichimoku.SenkouSpanA.Last(Kijun) && Bars.OpenPrices.Last(1) > ichimoku.SenkouSpanB.Last(Kijun) && Bars.ClosePrices.Last(1) > Bars.OpenPrices.Last(1) && Bars.HighPrices.Last(1) < Bars.HighPrices.Last(2) && Bars.LowPrices.Last(1) < Bars.LowPrices.Last(2))
            {

                var SLpips = (Symbol.Ask - Bars.LowPrices.Last(1)) / Symbol.PipSize;
                if (S1)
                {
                    ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Case2", SLpips, null);
                }

                if (S2 || S3 || S4)
                {
                    ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Case2", SLpips, null);
                    ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Case2t", SLpips, null);
                    var po = Positions.Find("Case2t", SymbolName);
                    BuyDelta = Convert.ToDouble(po.EntryPrice - po.StopLoss);

                    Print("Buy delta is " + BuyDelta);
                }

                Print("Open Case2");

                ichimokusignal = false;
            }

            if (ichimokusignal && Bars.ClosePrices.Last(1) < ichimoku.SenkouSpanA.Last(Kijun) && Bars.ClosePrices.Last(1) < ichimoku.SenkouSpanB.Last(Kijun) && Bars.OpenPrices.Last(1) < ichimoku.SenkouSpanA.Last(Kijun) && Bars.OpenPrices.Last(1) < ichimoku.SenkouSpanB.Last(Kijun) && Bars.ClosePrices.Last(1) < Bars.OpenPrices.Last(1) && Bars.HighPrices.Last(1) > Bars.HighPrices.Last(2) && Bars.LowPrices.Last(1) > Bars.LowPrices.Last(2))
            {

                var SLpips = (Bars.HighPrices.Last(1) - Symbol.Bid) / Symbol.PipSize;
                if (S1)
                {
                    ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Case3", SLpips, null);
                }
                if (S2 || S3 || S4)
                {
                    ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Case3", SLpips, null);
                    ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Case3t", SLpips, null);
                    var po = Positions.Find("Case3t", SymbolName);
                    SellDelta = Convert.ToDouble(po.StopLoss - po.EntryPrice);
                    Print("SL Distance ************************************" + (SellDelta));
                }


                Print("Open Case3");
                ichimokusignal = false;
            }



            if (ichimokusignal && Bars.ClosePrices.Last(1) < ichimoku.SenkouSpanA.Last(Kijun) && Bars.ClosePrices.Last(1) < ichimoku.SenkouSpanB.Last(Kijun) && Bars.OpenPrices.Last(1) < ichimoku.SenkouSpanA.Last(Kijun) && Bars.OpenPrices.Last(1) < ichimoku.SenkouSpanB.Last(Kijun) && Bars.ClosePrices.Last(1) < Bars.OpenPrices.Last(1) && Bars.HighPrices.Last(1) < Bars.HighPrices.Last(2) && Bars.LowPrices.Last(1) < Bars.LowPrices.Last(2))
            {

                var SLpips = (Bars.HighPrices.Last(2) - Symbol.Bid) / Symbol.PipSize;
                if (S1)
                {
                    ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Case4", SLpips, null);
                }
                if (S2 || S3 || S4)
                {
                    ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Case4", SLpips, null);

                    ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Case4t", SLpips, null);
                    var po = Positions.Find("Case4t", SymbolName);
                    SellDelta = Convert.ToDouble(po.StopLoss - po.EntryPrice);
                }
                Print("Open Case4");

                ichimokusignal = false;
            }

        }






    }
}
