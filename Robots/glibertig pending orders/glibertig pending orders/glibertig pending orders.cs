﻿using System;
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

        [Parameter("Equity drop", DefaultValue = 200)]
        public double Drop_per { get; set; }

        [Parameter("SL in pips", DefaultValue = 50)]
        public double SL { get; set; }

        [Parameter("TP Ratio", DefaultValue = 2)]
        public double Ratio { get; set; }

        [Parameter("Breakeven Ratio", DefaultValue = 0.5)]
        public double BERatio { get; set; }

        [Parameter("Candles to restart", DefaultValue = 5)]
        public int C_Restart { get; set; }

        public int CR_State = 0;

        public bool trigger = true;
        public bool relaunch = false;

        public DateTime MinTime;
        public DateTime MaxTime;



        public double AccBalance;

        Dictionary<PendingOrder, DateTime> PendingDict = new Dictionary<PendingOrder, DateTime>();

        protected override void OnStart()
        {
            AccBalance = Account.Balance;
            Print("Account balance = " + AccBalance);
        }


        protected override void OnTick()
        {


            if (Account.Equity <= (AccBalance - Drop_per))
            {
                Print("Drawdown");

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
                relaunch = true;
                Print("Restart with " + AccBalance);
            }
        }

        protected override void OnBar()
        {

            if (relaunch)
            {
                CR_State++;

                if (CR_State == C_Restart)
                {
                    CR_State = 0;
                    trigger = true;
                    relaunch = false;

                }

            }

            foreach (var po in Positions)
            {

                var TPinPips = Math.Abs(Convert.ToDouble(po.TakeProfit - po.EntryPrice) / Symbol.PipSize);




                if (po.Label == "LimitPlacer" && po.SymbolName == SymbolName && po.Pips >= TPinPips * BERatio && po.TradeType == TradeType.Buy && po.StopLoss < po.EntryPrice)
                {
                    // Print("Breakeven");
                    po.ModifyStopLossPrice(po.EntryPrice + (1 / Symbol.PipSize));

                }
                if (po.Label == "LimitPlacer" && po.SymbolName == SymbolName && po.Pips >= TPinPips * BERatio && po.TradeType == TradeType.Sell && po.StopLoss > po.EntryPrice)
                {
                    //Print("Breakeven");
                    po.ModifyStopLossPrice(po.EntryPrice - (1 / Symbol.PipSize));

                }

            }
            foreach (KeyValuePair<PendingOrder, DateTime> PendingPair in PendingDict)
            {



                if (Bars.OpenTimes.Last(Candles_Cancel) > PendingPair.Value)
                {

                    if (PendingPair.Key != null)
                    {
                        CancelPendingOrder(PendingPair.Key);

                    }


                }
            }

            if (!trigger && Bars.OpenTimes.Last(Candles_nbr) > MinTime && Bars.OpenTimes.Last(Candles_nbr) > MaxTime)
            {

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


            var x = Math.Round((Account.Balance * Balance_per) / (100 * SL * Symbol.PipValue * 1000));


            return Convert.ToInt32(x * 1000);

        }

        protected override void OnStop()
        {

        }
    }
}
