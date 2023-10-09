using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class NewYorkCycle : Robot
    {


        public bool TradeState = false;




        [Parameter("Risk %", DefaultValue = 5)]
        public double RiskP { get; set; }



        // % from balance


        public double LR = 5;


        public bool LT = true;


        public bool ST = true;


        //substract 2 

        [Parameter("Trade Time", DefaultValue = "13:30:00")]
        public string TradeTime { get; set; }

        [Parameter(" Distance from reference candle", DefaultValue = 50)]
        public double DistancePips { get; set; }



        [Parameter("Take Profit", DefaultValue = 50)]
        public double TP { get; set; }
        [Parameter("Stop Loss", DefaultValue = 100)]
        public double SL { get; set; }





        public int DecimalPrecision;

        public bool check1;
        public bool check2;






        protected override void OnStart()
        {
            Positions.Opened += PositionsOnOpened;
            Positions.Closed += PositionsClosed;





            DecimalPrecision = Symbol.PipSize.ToString().Split(',').Count() > 1 ? Symbol.PipSize.ToString().Split(',').ToList().ElementAt(1).Length : 0;








        }

        protected override void OnBar()
        {


            if ((Bars.OpenTimes.LastValue.ToString()).Contains(TradeTime) && TradeState == false)
            {


                Print("We should start");


                if (Bars.OpenPrices.Last(1) < Bars.ClosePrices.Last(1))
                {

                    Print("OpenTime" + Bars.OpenTimes.LastValue);
                    Print("Bullish candle" + Bars.OpenTimes.LastValue);

                    //Print("TradeTime" + TradeTime);

                    //Math.Round(SLSSL, DecimalPrecision);
                    var TargetBuy = Math.Round((Bars.ClosePrices.Last(1) + DistancePips * Symbol.PipSize), DecimalPrecision);
                    var TargetSell = Math.Round((Bars.OpenPrices.Last(1) - DistancePips * Symbol.PipSize), DecimalPrecision);

                    Print("Target Buy " + TargetBuy);
                    Print("Target Sell " + TargetSell);
                    Print("Close " + Bars.ClosePrices.Last(1));




                    //placer les pending orders à DistancePips interval avec TakeProfit et StopLoss in pips
                    if (LT == true)
                    {
                        //Print("Node Buy");
                        PlaceStopLimitOrder(TradeType.Buy, SymbolName, GetVolume(SL), TargetBuy, LR, "StopLimitBuy", SL, TP);
                    }

                    if (ST == true)
                    {
                        //Print("Node Sell");
                        PlaceStopLimitOrder(TradeType.Sell, SymbolName, GetVolume(SL), TargetSell, LR, "StopLimitSell", SL, TP);
                    }


                    //TradeState = true;

                }

                if (Bars.OpenPrices.Last(1) > Bars.ClosePrices.Last(1))
                {

                    Print("We should start");

                    Print("OpenTime" + Bars.OpenTimes.LastValue);
                    Print("Bearish candle" + Bars.OpenTimes.LastValue);

                    //Print("TradeTime" + TradeTime);

                    //Math.Round(SLSSL, DecimalPrecision);
                    var TargetBuy = Math.Round((Bars.OpenPrices.Last(1) + DistancePips * Symbol.PipSize), DecimalPrecision);
                    var TargetSell = Math.Round((Bars.ClosePrices.Last(1) - DistancePips * Symbol.PipSize), DecimalPrecision);

                    Print("Target Buy " + TargetBuy);
                    Print("Target Sell " + TargetSell);
                    Print("Close " + Bars.ClosePrices.Last(1));



                    //placer les pending orders à DistancePips interval avec TakeProfit et StopLoss in pips
                    if (LT == true)
                    {
                        //Print("Node Buy");
                        PlaceStopLimitOrder(TradeType.Buy, SymbolName, GetVolume(SL), TargetBuy, LR, "StopLimitBuy", SL, TP);
                    }

                    if (ST == true)
                    {
                        //Print("Node Sell");
                        PlaceStopLimitOrder(TradeType.Sell, SymbolName, GetVolume(SL), TargetSell, LR, "StopLimitSell", SL, TP);
                    }


                    //TradeState = true;

                }
            }





        }

        protected override void OnTick()
        {




            if (PendingOrders.Count == 0)
            {
                check1 = false;

            }

            if (Positions.Count == 0)
            {
                check2 = false;

            }


            foreach (var PO in PendingOrders)
            {

                //Print("Pendingcount" + PendingOrders.Count);


                if (PendingOrders.Count > 0 && PO.SymbolName == SymbolName && (PO.Label == "StopLimitBuy" || PO.Label == "StopLimitSell"))
                {
                    check1 = true;
                    //Print("PO length" + PO.SymbolName.Count());
                }

                else
                {
                    check1 = false;
                }





            }


            foreach (var Pos in Positions)
            {
                if (Pos.SymbolName == SymbolName && (Pos.Label == "StopLimitBuy" || Pos.Label == "StopLimitSell"))
                {
                    check2 = true;
                }

                else
                {
                    check2 = false;
                }





            }

            if (check1 == false && check2 == false)
            {
                TradeState = false;
            }

            else
            {
                TradeState = true;
            }












        }

        protected override void OnStop()
        {
            foreach (var PO in PendingOrders)
            {
                if (PO.SymbolName == SymbolName)
                {
                    PO.Cancel();
                }

            }
        }

        private void PositionsClosed(PositionClosedEventArgs args)
        {



            if ((args.Position.Label == "StopLimitBuy" || args.Position.Label == "StopLimitSell") && args.Position.SymbolName == SymbolName)
            {
                TradeState = false;
            }

        }

        private void PositionsOnOpened(PositionOpenedEventArgs args)
        {




            if (args.Position.TradeType == TradeType.Buy && args.Position.Label == "StopLimitBuy" && args.Position.SymbolName == SymbolName)
            {

                foreach (var PO in PendingOrders)
                {
                    if (PO.TradeType == TradeType.Sell)
                    {
                        PO.Cancel();
                        Print("Node2");
                    }

                }

            }

            if (args.Position.TradeType == TradeType.Sell && args.Position.Label == "StopLimitSell" && args.Position.SymbolName == SymbolName)
            {

                foreach (var PO in PendingOrders)
                {
                    if (PO.TradeType == TradeType.Buy)
                    {
                        PO.Cancel();
                        Print("Node1");
                    }

                }

            }
        }







        protected double GetVolume(double SL)
        {
            var x = 1.0;



            var RawRisk = Account.Balance * RiskP / 100;
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





    }
}
