using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Tuesday : Robot
    {
        [Parameter(DefaultValue = 10.0)]
        public double AwayPips { get; set; }

        [Parameter(DefaultValue = 2)]
        public double RiskPercent { get; set; }

        [Parameter(DefaultValue = 50)]
        public double TP_Percentage { get; set; }


        [Parameter(DefaultValue = 25)]
        public double SL_Percentage { get; set; }


        [Parameter(DefaultValue = 3)]
        public int Entries { get; set; }


        [Parameter(DefaultValue = 100)]
        public double Range { get; set; }




        public bool State = false;

        public double Highstuff;
        public double Lowstuff;

        public int Counter = 0;

        //NY Close in UTC = 10pm

        protected override void OnStart()
        {
            PendingOrders.Filled += PendingOrders_Filled;
            Positions.Closed += Positions_Closed;

            if (!IsBacktesting)
            {
                Stop();
            }
        }

        private bool CheckRrange(double R)
        {
            if (R > Range)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private void Positions_Closed(PositionClosedEventArgs obj)
        {
            if (obj.Reason == PositionCloseReason.StopLoss && obj.Position.Pips < 0)
            {
                Counter++;
                State = false;
                Print("Counter is " + Counter);
            }
        }
        private void PendingOrders_Filled(PendingOrderFilledEventArgs obj)
        {
            Print(string.Format("PendingOrders.Filled event received. (Order #{0} filled. Position is #{1})", obj.PendingOrder.Id, obj.Position.Id));
            foreach (var pending in PendingOrders)
            {
                if (pending.Label == "GJ")
                {
                    CancelPendingOrder(pending);
                }
            }
        }
        protected override void OnBar()
        {


            if (Server.Time.DayOfWeek == DayOfWeek.Friday && Server.Time.Hour >= 20 && Counter != 0)
            {
                Print("Friday Node");

                foreach (var pending in PendingOrders)
                {
                    if (pending.Label == "GJ")
                    {
                        Print("Cancelling pending " + pending);
                        CancelPendingOrder(pending);
                    }
                }

                foreach (var po in Positions)
                {
                    if (po.Label == "GJ")
                    {
                        Print("Closing position " + po);
                        ClosePosition(po);
                    }
                }



                Counter = 0;
                State = false;
            }


            if (Counter > 1 && Counter < Entries + 1 && !State)
            {
                var highestpips = Highstuff + (AwayPips * Symbol.PipSize);
                var lowestpips = Lowstuff - (AwayPips * Symbol.PipSize);
                var delta = (Highstuff - Lowstuff) / Symbol.PipSize;


                //Print("Highestpips++ " + highestpips);
                //Print("Lowestpips++ " + lowestpips);
                //Print("Delta pips++ " + delta);

                var SL = delta * 0.25;
                var TP = delta * 0.5;
                if (CheckRrange(delta))
                {
                    PlaceStopLimitOrder(TradeType.Buy, SymbolName, GetVolume(SL), highestpips, 5, "GJ", SL, TP);
                    PlaceStopLimitOrder(TradeType.Sell, SymbolName, GetVolume(SL), lowestpips, 5, "GJ", SL, TP);
                }
                //Print("Highest++ " + Highstuff);
                ///Print("Lowest++ " + Lowstuff);

                Counter++;
                State = true;

                Print("Second entry");
            }


            if (Counter == 0 && Server.Time.DayOfWeek == DayOfWeek.Wednesday && !State)
            {
                Highstuff = Bars.HighPrices.Last(1);
                for (int i = 2; i <= 24; i++)
                {
                    if (Bars.HighPrices.Last(i) > Highstuff)
                        Highstuff = Bars.HighPrices.Last(i);
                }

                Lowstuff = Bars.LowPrices.Last(1);
                for (int i = 2; i <= 24; i++)
                {
                    if (Bars.LowPrices.Last(i) < Lowstuff)
                        Lowstuff = Bars.LowPrices.Last(i);
                }



                var highestpips = Highstuff + (AwayPips * Symbol.PipSize);
                var lowestpips = Lowstuff - (AwayPips * Symbol.PipSize);
                var delta = (Highstuff - Lowstuff) / Symbol.PipSize;

                var HighBorne = Chart.DrawHorizontalLine("High", Highstuff, Color.Blue, 5);

                var LowBorne = Chart.DrawHorizontalLine("Low", Lowstuff, Color.Red, 5);

                //  Print("Highestpips " + highestpips);
                //  Print("Lowestpips " + lowestpips);
                // Print("Delta pips " + delta);

                var SL = delta * SL_Percentage / 100;
                var TP = delta * TP_Percentage / 100;
                if (CheckRrange(delta))
                {
                    PlaceStopLimitOrder(TradeType.Buy, SymbolName, GetVolume(SL), highestpips, 5, "GJ", SL, TP);
                    PlaceStopLimitOrder(TradeType.Sell, SymbolName, GetVolume(SL), lowestpips, 5, "GJ", SL, TP);
                    //Print("Highest " + Highstuff);
                    //Print("Lowest " + Lowstuff);
                }
                State = true;
                Counter = 1;
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


    }
}
