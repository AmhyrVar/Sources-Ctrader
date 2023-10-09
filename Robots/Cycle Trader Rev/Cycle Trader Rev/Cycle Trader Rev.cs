using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class CycleTraderRev : Robot
    {


        public bool TradeState = false;

        [Parameter("Use flat Risk Amount", DefaultValue = true)]
        public bool FlatRisk { get; set; }

        [Parameter("Risk Amount", DefaultValue = 250)]
        public double RawRisk { get; set; }


        [Parameter("Risk %", DefaultValue = 5)]
        public double RiskP { get; set; }



        // % from balance

        [Parameter("Limit Range", DefaultValue = 5)]
        public double LR { get; set; }

        [Parameter("Long Trades", DefaultValue = true)]
        public bool LT { get; set; }

        [Parameter("Short Trades", DefaultValue = true)]
        public bool ST { get; set; }


        //substract 2 

        [Parameter("Reference time", DefaultValue = "08:00:00")]
        public string TradeTime { get; set; }

        [Parameter(" Distance from reference candle", DefaultValue = 50)]
        public double DistancePips { get; set; }



        [Parameter("Take Profit", DefaultValue = 50)]
        public double TP { get; set; }
        [Parameter("Stop Loss", DefaultValue = 100)]
        public double SL { get; set; }

        [Parameter("Use Candlebars to cancel", DefaultValue = true)]
        public bool CancleBars { get; set; }

        [Parameter("Candles to cancel", DefaultValue = 5)]
        public int CancelCandle { get; set; }


        public int CandleTick = 0;

        [Parameter("Breakeven Pips", DefaultValue = 10)]
        public double BEPips { get; set; }


        public int DecimalPrecision;

        public bool check1;
        public bool check2;






        protected override void OnStart()
        {
            Positions.Opened += PositionsOnOpened;
            Positions.Closed += PositionsClosed;





            DecimalPrecision = Symbol.PipSize.ToString().Split(',').Count() > 1 ? Symbol.PipSize.ToString().Split(',').ToList().ElementAt(1).Length : 0;


            var Int_TradeHour = Convert.ToInt32(TradeTime.Substring(0, 2));




            Print("Sliced hour" + Int_TradeHour);

            if (Int_TradeHour == 1)
            {
                Int_TradeHour = 23;
            }
            if (Int_TradeHour == 0)
            {
                Int_TradeHour = 22;
            }
            else
            {
                Int_TradeHour = Int_TradeHour - 2;
            }


            Print("ModTradeHour" + Int_TradeHour);

            var hourstring = "00";

            if (Int_TradeHour < 10)
            {
                hourstring = "0" + Int_TradeHour.ToString();
            }

            else
            {
                hourstring = Int_TradeHour.ToString();
            }

            Print("String hour------------ " + hourstring);


            var FinalString = TradeTime.Remove(0, 2);

            FinalString = hourstring + FinalString;

            Print("Final Format-------------------" + FinalString);



            TradeTime = FinalString;





        }

        protected override void OnBar()
        {
            if ((Bars.OpenTimes.Last(1).ToString()).Contains(TradeTime))
            {

                Print("TradeState" + TradeState);
                Print("Check1 " + check1);
                Print("Check2 " + check2);


            }

            if ((Bars.OpenTimes.Last(1).ToString()).Contains(TradeTime) && TradeState == false)
            {


                //Print("OpenTime" + Bars.OpenTimes.Last(1));

                //Print("TradeTime" + TradeTime);

                //Math.Round(SLSSL, DecimalPrecision);
                var TargetBuy = Math.Round((Bars.ClosePrices.LastValue + DistancePips * Symbol.PipSize), DecimalPrecision);
                var TargetSell = Math.Round((Bars.ClosePrices.LastValue - DistancePips * Symbol.PipSize), DecimalPrecision);



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


            if (TradeState == true)
            {
                CandleTick = CandleTick + 1;
            }

            foreach (var PO in PendingOrders)
            {
                if (CancleBars && PO.SymbolName == SymbolName && CandleTick == CancelCandle + 1)
                {
                    //Print("Cancel time" + Time);
                    PO.Cancel();


                }

            }

            if (CancleBars && CandleTick == CancelCandle + 1)
            {
                TradeState = false;
                CandleTick = 0;
            }
        }

        protected override void OnTick()
        {


            foreach (var ActivePosition in Positions)
            {
                if ((ActivePosition.Label == "StopLimitBuy" || ActivePosition.Label == "StopLimitSell") && ActivePosition.SymbolName == SymbolName && ActivePosition.Pips >= BEPips && ActivePosition.StopLoss != ActivePosition.EntryPrice)
                {
                    ModifyPosition(ActivePosition, ActivePosition.EntryPrice, ActivePosition.TakeProfit);
                    Print(ActivePosition.StopLoss);
                }

            }

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
                    }

                }

            }
        }







        protected double GetVolume(double SL)
        {
            var x = 1.0;
            if (FlatRisk)
            {
                x = Math.Round((RawRisk) / (SL * Symbol.PipValue * Symbol.VolumeInUnitsMin));
            }

            if (FlatRisk != true)
            {
                RawRisk = Account.Balance * RiskP / 100;
                x = Math.Round((RawRisk) / (SL * Symbol.PipValue * Symbol.VolumeInUnitsMin));
            }

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
