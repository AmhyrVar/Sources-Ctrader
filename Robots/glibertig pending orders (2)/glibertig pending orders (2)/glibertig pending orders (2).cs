using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using System.Collections.Generic;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class glibertigpendingorders : Robot
    {
        [Parameter("Number of Candles to watch", DefaultValue = 5)]
        public int Candles_nbr { get; set; }

        [Parameter("Number of Candles to cancel Pending", DefaultValue = 10)]
        public int Candles_Cancel { get; set; }

        [Parameter("Balance % position", DefaultValue = 2)]
        public double Balance_per { get; set; }

        [Parameter("Equity drop %", DefaultValue = 20)]
        public double Drop_per { get; set; }

        [Parameter("SL in pips", DefaultValue = 50)]
        public double SL { get; set; }

        [Parameter("TP Ratio", DefaultValue = 2)]
        public double Ratio { get; set; }

        [Parameter("Breakeven Ratio", DefaultValue = 0.5)]
        public double BERatio { get; set; }

        public bool trigger = true;

        public DateTime MinTime;
        public DateTime MaxTime;

        public double AccBalance;

        Dictionary<PendingOrder, DateTime> PendingDict = new Dictionary<PendingOrder, DateTime>();

        protected override void OnStart()
        {
            AccBalance = Account.Balance;
        }


        protected override void OnTick()
        {
            // Print("Thresh " + (AccBalance - (AccBalance * (Drop_per / 100))));
            //Print("Equity" + Account.Equity);
            if (Account.Equity < (AccBalance - (AccBalance * (Drop_per / 100))))
            {


                foreach (var po in Positions)
                {
                    if (po.Label == "LimitPlacer" && po.SymbolName == SymbolName)
                    {
                        ClosePosition(po);
                    }
                }

                foreach (var po in PendingOrders)
                {
                    if (po.Label == "LimitPlacer" && po.SymbolName == SymbolName)
                    {

                        CancelPendingOrder(po);
                    }
                }
                AccBalance = Account.Balance;
                trigger = true;
                Print("Restart");
            }
        }

        protected override void OnBar()
        {
            foreach (var po in Positions)
            {

                var TPinPips = Math.Abs(Convert.ToDouble(po.TakeProfit - po.EntryPrice) / Symbol.PipSize);




                if (po.Label == "LimitPlacer" && po.SymbolName == SymbolName && po.Pips >= TPinPips * BERatio && po.TradeType == TradeType.Buy && po.StopLoss < po.EntryPrice)
                {
                    // Print("Breakeven");
                    po.ModifyStopLossPips(po.EntryPrice + (1 / Symbol.PipSize));

                }
                if (po.Label == "LimitPlacer" && po.SymbolName == SymbolName && po.Pips >= TPinPips * BERatio && po.TradeType == TradeType.Sell && po.StopLoss > po.EntryPrice)
                {
                    //Print("Breakeven");
                    po.ModifyStopLossPips(po.EntryPrice - (1 / Symbol.PipSize));

                }

            }
            foreach (KeyValuePair<PendingOrder, DateTime> PendingPair in PendingDict)
            {


                //Print("Key: {0}, Value: {1}", PendingPair.Key, PendingPair.Value);
                if (Bars.OpenTimes.Last(Candles_Cancel) > PendingPair.Value)
                {
                    //Print("B4 " + PendingDict);
                    if (PendingPair.Key != null)
                    {
                        CancelPendingOrder(PendingPair.Key);

                    }

                    //PendingDict.Remove(PendingPair.Key);
                    //Print("After " + PendingDict);
                    //Print("Order Cancelled");
                }
            }

            if (!trigger && Bars.OpenTimes.Last(Candles_nbr) > MinTime && Bars.OpenTimes.Last(Candles_nbr) > MaxTime)
            {
                //Print("Node");
                //Print("Bars Open Last Candles_nbr" + Bars.OpenTimes.Last(Candles_nbr));
                trigger = true;
            }

            if (trigger)
            {
                var i = 1;
                var cMin = Bars.LowPrices.Last(1);
                var cMax = Bars.HighPrices.Last(1);

                for (i = 1; i < Candles_nbr + 1; i++)
                {
                    //Print("Iteration number " + i);
                    if (Bars.HighPrices.Last(i) > cMax)
                    {
                        cMax = Bars.HighPrices.Last(i);
                        MaxTime = Bars.OpenTimes.Last(i);
                    }
                    if (Bars.LowPrices.Last(i) < cMin)
                    {
                        cMin = Bars.LowPrices.Last(i);
                        MinTime = Bars.OpenTimes.Last(i);
                    }
                }

                var SLO = PlaceLimitOrder(TradeType.Sell, SymbolName, GetVolume(SL), cMax, "LimitPlacer", SL, Ratio * SL);
                var BLO = PlaceLimitOrder(TradeType.Buy, SymbolName, GetVolume(SL), cMin, "LimitPlacer", SL, Ratio * SL);

                var SSO = PlaceStopOrder(TradeType.Sell, SymbolName, GetVolume(SL), cMin, "LimitPlacer", SL, Ratio * SL);
                var BSO = PlaceStopOrder(TradeType.Buy, SymbolName, GetVolume(SL), cMax, "LimitPlacer", SL, Ratio * SL);
                trigger = false;
                //Print("MinTIme " + MinTime);
                //Print("MaxTime" + MaxTime);
                //Print("SLO " + SLO);
                //Print("BLO " + BLO);


                if (BLO.PendingOrder != null)
                {
                    PendingDict.Add(BLO.PendingOrder, Bars.OpenTimes.Last(1));
                }
                if (SLO.PendingOrder != null)
                {
                    PendingDict.Add(SLO.PendingOrder, Bars.OpenTimes.Last(1));
                }
                if (BSO.PendingOrder != null)
                {
                    PendingDict.Add(BSO.PendingOrder, Bars.OpenTimes.Last(1));
                }
                if (SSO.PendingOrder != null)
                {
                    PendingDict.Add(SSO.PendingOrder, Bars.OpenTimes.Last(1));
                }











            }
        }

        protected int GetVolume(double SL)
        {

            // x which is 1 = (balance * risk%)/(SL*pipvalue*1000) ROUND TO INT
            var x = Math.Round((Account.Balance * Balance_per) / (100 * SL * Symbol.PipValue * 1000));

            //Convert.ToInt32(double)
            //Print(x);
            return Convert.ToInt32(x * 1000);

        }

        protected override void OnStop()
        {
            // Breakeven
            // TPSL
            // Close pendings
            // More questions
        }
    }
}
