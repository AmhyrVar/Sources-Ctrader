﻿using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MovingAverageMartingalePro : Robot
    {
        [Parameter("Simulated balance", DefaultValue = 2000)]
        public double Sim_Bal { get; set; }

        [Parameter("Balance % position", DefaultValue = 10)]
        public double Balance_per { get; set; }
        [Parameter("Hour to start trading", DefaultValue = 8)]
        public int Hour_Start { get; set; }
        [Parameter("Hour to stop trading", DefaultValue = 18)]
        public int Hour_Stop { get; set; }
        [Parameter("TEMA Period", DefaultValue = 50)]
        public int Tema_Period { get; set; }
        [Parameter("TEMA Source")]
        public DataSeries Tema_Source { get; set; }

        [Parameter("DEMA Period", DefaultValue = 50)]
        public int Dema_Period { get; set; }
        [Parameter("DEMA Source")]
        public DataSeries Dema_Source { get; set; }
        [Parameter("Bars after crossing", DefaultValue = 1)]
        public int Bars_cross { get; set; }
        [Parameter("martingale multiplier", DefaultValue = 2)]
        public double Martingale_Multiplier { get; set; }
        [Parameter("Maximum orders", DefaultValue = 2)]
        public int MaxOrders { get; set; }

        [Parameter("TP", DefaultValue = 500)]
        public double TP { get; set; }
        [Parameter("SL", DefaultValue = 250)]
        public double SL { get; set; }

        [Parameter("Long Trades only", DefaultValue = false)]
        public bool OnlyLong { get; set; }
        [Parameter("Short Trades only", DefaultValue = false)]
        public bool OnlyShort { get; set; }

        [Parameter("Break-even pips", DefaultValue = 700)]
        public double BEpips { get; set; }
        [Parameter("Break-even pips to add", DefaultValue = 3)]
        public double BEAddPips { get; set; }
        [Parameter("Volume % left after BE", DefaultValue = 0.5)]
        public double VolMod { get; set; }
        /// <summary>
        /// Done
        /// </summary>














        private int Martingale_Status = 0;





        private double Bal_thresh;





        Tema _TEMA;
        DEMA _DEMA;

        protected override void OnStart()
        {
            _TEMA = Indicators.GetIndicator<Tema>(Tema_Source, Tema_Period);
            _DEMA = Indicators.GetIndicator<DEMA>(Dema_Source, Dema_Period);
            Positions.Closed += PositionsOnClosed;
            Bal_thresh = Account.Balance - Sim_Bal;


            if (Server.Time.Month > 4 && Server.Time.Year >= 2022)
            {
                Stop();
            }
        }
        private void PositionsOnClosed(PositionClosedEventArgs args)
        {
            switch (args.Reason)
            {
                case PositionCloseReason.StopLoss:
                    Martingale_Status++;
                    break;
                case PositionCloseReason.TakeProfit:
                    Martingale_Status = 0;
                    break;

            }

        }
        private bool TimeCheck()
        {

            if (Server.Time.Hour >= Hour_Start && Server.Time.Hour < Hour_Stop)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        protected int GetVolume(double SL)
        {
            //Print("Pipsize " + Symbol.PipSize);

            var x = Math.Round((Sim_Bal * Balance_per) / (100 * SL * Symbol.PipValue * Symbol.VolumeInUnitsMin));
            var next_risk = SL * Convert.ToInt32(x * Math.Pow(Martingale_Multiplier, Martingale_Status) * Symbol.VolumeInUnitsMin) * Symbol.PipValue;
            //Print("Next risk " + next_risk);
            if (Account.Balance - next_risk < Bal_thresh)
            {
                Print("No more trade ");
                return 0;
            }

            else
            {

                return Convert.ToInt32(x * Math.Pow(Martingale_Multiplier, Martingale_Status) * Symbol.VolumeInUnitsMin);
            }


        }

        protected override void OnBar()
        {
            var Opened_Orders = Positions.FindAll("TDEMA", SymbolName);

            if (_TEMA.Result.HasCrossedAbove(_DEMA.dema, Bars_cross - 1) && Opened_Orders.Length < MaxOrders && !OnlyShort && TimeCheck())
            {
                //Print("Buy " + GetVolume(SL) + "Martingale status " + Martingale_Status + " at " + Server.Time);
                ExecuteMarketOrder(TradeType.Buy, SymbolName, GetVolume(SL), "TDEMA", SL, TP);

            }
            if (_TEMA.Result.HasCrossedBelow(_DEMA.dema, Bars_cross - 1) && Opened_Orders.Length < MaxOrders && !OnlyLong && TimeCheck())
            {
                ExecuteMarketOrder(TradeType.Sell, SymbolName, GetVolume(SL), "TDEMA", SL, TP);
                //Print("Sell " + GetVolume(SL) + "Martingale status " + Martingale_Status);
            }
        }


        public int Round(double i, int v)
        {
            return (int)(Math.Round(i / v) * v);
        }

        protected override void OnTick()
        {
            var Opened_Orders_t = Positions.FindAll("TDEMA", SymbolName);
            double popips = 0;
            foreach (var po in Opened_Orders_t)
            {
                popips = popips + po.Pips;
            }
            if (popips >= BEpips)
            {
                foreach (var po in Opened_Orders_t)
                {

                    //Print("Modify node");
                    //Break-even
                    po.ModifyStopLossPips(po.EntryPrice + (BEAddPips / Symbol.PipSize));

                    //Modify volume
                    po.ModifyVolume(Round(po.VolumeInUnits * 0.5, Convert.ToInt32(Symbol.VolumeInUnitsStep)));
                }
            }
        }


    }
}
