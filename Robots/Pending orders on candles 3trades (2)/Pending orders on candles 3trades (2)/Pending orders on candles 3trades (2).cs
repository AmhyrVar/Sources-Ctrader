using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class Pendingordersoncandles3trades : Robot
    {
        [Parameter("TP multiplier", DefaultValue = 4)]
        public double TPmultiplier { get; set; }

        [Parameter("Risk percent", DefaultValue = 2)]
        public double RiskPercent { get; set; }

        [Parameter("Breakeven Ratio", DefaultValue = 0.5)]
        public double BERatio { get; set; }


        [Parameter("Target Ratio", DefaultValue = 0.5)]
        public double TargetRatio { get; set; }

        public int DecimalPrecision;


        public int OnTheBuy = 0;
        public int OnTheSell = 0;

        public int OnTheBuy1 = 0;
        public int OnTheSell1 = 0;

        public int OnTheBuy2 = 0;
        public int OnTheSell2 = 0;

        //NODE
        public bool OnTrade = false;
        public bool OnTrade1 = false;
        public bool OnTrade2 = false;


        public bool protectedpo = false;
        public bool protectedpo1 = false;
        public bool protectedpo2 = false;

        public bool cantrade1 = false;
        public bool cantrade2 = false;
        protected override void OnStart()
        {
            Positions.Closed += OnPositionsClosed;
            DecimalPrecision = Symbol.PipSize.ToString().Split(',').Count() > 1 ? Symbol.PipSize.ToString().Split(',').ToList().ElementAt(1).Length : 0;


        }
        void OnPositionsClosed(PositionClosedEventArgs obj)
        {
            if (obj.Position.Label == "LimitBot")
            {
                OnTrade = false;
                OnTheBuy = 0;
                OnTheSell = 0;
                protectedpo = false;
            }
            if (obj.Position.Label == "LimitBot1")
            {
                OnTrade1 = false;
                OnTheBuy1 = 0;
                OnTheSell1 = 0;
                protectedpo1 = false;
            }
            if (obj.Position.Label == "LimitBot2")
            {
                OnTrade2 = false;
                OnTheBuy2 = 0;
                OnTheSell2 = 0;
                protectedpo2 = false;
            }

        }

        protected override void OnTick()
        {
            foreach (var po in Positions)
            {

                var TPinPips = Math.Abs(Convert.ToDouble(po.TakeProfit - po.EntryPrice) / Symbol.PipSize);




                if (po.Label == "LimitBot" && po.SymbolName == SymbolName && po.Pips >= TPinPips * BERatio && !protectedpo)
                {
                    Print("Breakeven");
                    po.ModifyStopLossPips(po.EntryPrice);
                    protectedpo = true;
                }
                if (po.Label == "LimitBot1" && po.SymbolName == SymbolName && po.Pips >= TPinPips * BERatio && !protectedpo1)
                {
                    Print("Breakeven");
                    po.ModifyStopLossPips(po.EntryPrice);
                    protectedpo = true;
                }
                if (po.Label == "LimitBot2" && po.SymbolName == SymbolName && po.Pips >= TPinPips * BERatio && !protectedpo2)
                {
                    Print("Breakeven");
                    po.ModifyStopLossPips(po.EntryPrice);
                    protectedpo = true;
                }
            }
        }
        protected override void OnBar()
        {
            //Breakeven if 50% TP

            if (OnTrade)
            {
                cantrade1 = true;
            }
            if (OnTrade1)
            {
                cantrade2 = true;
            }


            if (!OnTrade)
            {
                cantrade1 = false;
            }
            if (!OnTrade1)
            {
                cantrade2 = false;
            }

            //A bot that after the close of a bar on X Timeframe creates a limit order at 50 % of the bar at the same direction of the bar, if the bar satisfies this:
            if (OnTrade)
            {

                if (OnTheBuy == 1 || OnTheSell == 1)
                {
                    foreach (var order in PendingOrders)
                    {
                        if (order.Label == "LimitBot" && order.SymbolName == SymbolName)
                        {
                            CancelPendingOrder(order);
                            OnTheBuy = 0;
                            OnTheSell = 0;
                            OnTrade = false;


                        }
                    }

                }
                if (OnTheBuy > 0)
                {
                    OnTheBuy--;

                }
                if (OnTheSell > 0)
                {
                    OnTheSell--;

                }
            }

            if (OnTrade1)
            {

                if (OnTheBuy1 == 1 || OnTheSell1 == 1)
                {
                    foreach (var order in PendingOrders)
                    {
                        if (order.Label == "LimitBot1" && order.SymbolName == SymbolName)
                        {
                            CancelPendingOrder(order);
                            OnTheBuy1 = 0;
                            OnTheSell1 = 0;
                            OnTrade1 = false;


                        }
                    }

                }
                if (OnTheBuy1 > 0)
                {
                    OnTheBuy1--;

                }
                if (OnTheSell1 > 0)
                {
                    OnTheSell1--;

                }
            }

            if (OnTrade2)
            {

                if (OnTheBuy2 == 1 || OnTheSell2 == 1)
                {
                    foreach (var order in PendingOrders)
                    {
                        if (order.Label == "LimitBot2" && order.SymbolName == SymbolName)
                        {
                            CancelPendingOrder(order);
                            OnTheBuy2 = 0;
                            OnTheSell2 = 0;
                            OnTrade2 = false;


                        }
                    }

                }
                if (OnTheBuy2 > 0)
                {
                    OnTheBuy2--;

                }
                if (OnTheSell2 > 0)
                {
                    OnTheSell2--;

                }
            }

            //make this a parameter
            // -bar body at least 60% of bar's lenghth.
            // -Bar high/low goes beyond the previous bar
            //  -bar close goes beyond the close of previous bar.

            //  Stop loss: 1 pip beyond the low/high of the bar

            //  Take profit: X times the Risk
            //  Cancel order if after 4 bars the trade doesnt trigger.

            if (!OnTrade && Math.Abs(Bars.ClosePrices.Last(1) - Bars.OpenPrices.Last(1)) >= (Bars.HighPrices.Last(1) - Bars.LowPrices.Last(1)) * 0.6)            /* Body >= 60% bar length */
            {
                //long
                if (Bars.OpenPrices.Last(1) < Bars.ClosePrices.Last(1) && Bars.HighPrices.Last(1) > Bars.HighPrices.Last(2) && Bars.ClosePrices.Last(1) > Bars.ClosePrices.Last(2))
                {
                    var targetprice = Bars.LowPrices.Last(1) + ((Bars.HighPrices.Last(1) - Bars.LowPrices.Last(1)) * TargetRatio);
                    var SLpips = 1 + ((Bars.HighPrices.Last(1) - Bars.LowPrices.Last(1)) / 2) / Symbol.PipSize;
                    var TPpips = SLpips * TPmultiplier;


                    PlaceLimitOrder(TradeType.Buy, SymbolName, GetVolume(SLpips), targetprice, "LimitBot", SLpips, TPpips);


                    OnTrade = true;
                    OnTheBuy = 4;

                }

                //short
                if (Bars.OpenPrices.Last(1) > Bars.ClosePrices.Last(1) && Bars.HighPrices.Last(1) < Bars.HighPrices.Last(2) && Bars.ClosePrices.Last(1) < Bars.ClosePrices.Last(2))
                {
                    var targetprice = Bars.HighPrices.Last(1) - (Bars.HighPrices.Last(1) - Bars.LowPrices.Last(1)) * TargetRatio;
                    var SLpips = 1 + ((Bars.HighPrices.Last(1) - Bars.LowPrices.Last(1)) / 2) / Symbol.PipSize;
                    var TPpips = SLpips * TPmultiplier;

                    PlaceLimitOrder(TradeType.Sell, SymbolName, GetVolume(SLpips), targetprice, "LimitBot", SLpips, TPpips);

                    OnTrade = true;
                    OnTheSell = 4;
                }
            }

            //same for sell

            if (!OnTrade1 && cantrade1 && Math.Abs(Bars.ClosePrices.Last(1) - Bars.OpenPrices.Last(1)) >= (Bars.HighPrices.Last(1) - Bars.LowPrices.Last(1)) * 0.6)            /* Body >= 60% bar length */
            {
                if (Bars.OpenPrices.Last(1) < Bars.ClosePrices.Last(1) && Bars.HighPrices.Last(1) > Bars.HighPrices.Last(2) && Bars.ClosePrices.Last(1) > Bars.ClosePrices.Last(2))
                {
                    var targetprice = Bars.LowPrices.Last(1) + ((Bars.HighPrices.Last(1) - Bars.LowPrices.Last(1)) * TargetRatio);
                    var SLpips = 1 + ((Bars.HighPrices.Last(1) - Bars.LowPrices.Last(1)) / 2) / Symbol.PipSize;
                    var TPpips = SLpips * TPmultiplier;

                    PlaceLimitOrder(TradeType.Buy, SymbolName, GetVolume(SLpips), targetprice, "LimitBot1", SLpips, TPpips);


                    OnTrade1 = true;
                    OnTheBuy1 = 4;

                }

                //short
                if (Bars.OpenPrices.Last(1) > Bars.ClosePrices.Last(1) && Bars.HighPrices.Last(1) < Bars.HighPrices.Last(2) && Bars.ClosePrices.Last(1) < Bars.ClosePrices.Last(2))
                {
                    var targetprice = Bars.HighPrices.Last(1) - (Bars.HighPrices.Last(1) - Bars.LowPrices.Last(1)) * TargetRatio;
                    var SLpips = 1 + ((Bars.HighPrices.Last(1) - Bars.LowPrices.Last(1)) / 2) / Symbol.PipSize;
                    var TPpips = SLpips * TPmultiplier;

                    PlaceLimitOrder(TradeType.Sell, SymbolName, GetVolume(SLpips), targetprice, "LimitBot1", SLpips, TPpips);

                    OnTrade1 = true;
                    OnTheSell1 = 4;
                }
            }


            if (!OnTrade2 && cantrade2 && Math.Abs(Bars.ClosePrices.Last(1) - Bars.OpenPrices.Last(1)) >= (Bars.HighPrices.Last(1) - Bars.LowPrices.Last(1)) * 0.6)            /* Body >= 60% bar length */
            {
                if (Bars.OpenPrices.Last(1) < Bars.ClosePrices.Last(1) && Bars.HighPrices.Last(1) > Bars.HighPrices.Last(2) && Bars.ClosePrices.Last(1) > Bars.ClosePrices.Last(2))
                {
                    var targetprice = Bars.LowPrices.Last(1) + ((Bars.HighPrices.Last(1) - Bars.LowPrices.Last(1)) * TargetRatio);
                    var SLpips = 1 + ((Bars.HighPrices.Last(1) - Bars.LowPrices.Last(1)) / 2) / Symbol.PipSize;
                    var TPpips = SLpips * TPmultiplier;

                    PlaceLimitOrder(TradeType.Buy, SymbolName, GetVolume(SLpips), targetprice, "LimitBot2", SLpips, TPpips);


                    OnTrade2 = true;
                    OnTheBuy2 = 4;

                }

                //short
                if (Bars.OpenPrices.Last(1) > Bars.ClosePrices.Last(1) && Bars.HighPrices.Last(1) < Bars.HighPrices.Last(2) && Bars.ClosePrices.Last(1) < Bars.ClosePrices.Last(2))
                {
                    var targetprice = Bars.HighPrices.Last(1) - (Bars.HighPrices.Last(1) - Bars.LowPrices.Last(1)) * TargetRatio;
                    var SLpips = 1 + ((Bars.HighPrices.Last(1) - Bars.LowPrices.Last(1)) / 2) / Symbol.PipSize;
                    var TPpips = SLpips * TPmultiplier;

                    PlaceLimitOrder(TradeType.Sell, SymbolName, GetVolume(SLpips), targetprice, "LimitBot2", SLpips, TPpips);

                    OnTrade2 = true;
                    OnTheSell2 = 4;
                }
            }






        }


        protected double GetVolume(double SL)
        {


            var x = 1.0;



            var RawRisk = Account.Balance * RiskPercent / 100;
            x = Math.Round((RawRisk) / (SL * Symbol.PipValue * Symbol.VolumeInUnitsMin));


            if (Symbol.VolumeInUnitsMin > 1)
            {
                return Convert.ToInt32(x * Symbol.VolumeInUnitsMin);
            }
            else
            {
                return (x * Symbol.VolumeInUnitsMin);
            }




        }


        protected override void OnStop()
        {

        }
    }
}
