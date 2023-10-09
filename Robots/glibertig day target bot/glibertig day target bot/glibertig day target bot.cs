using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class glibertigdaytargetbot : Robot
    {
        [Parameter("Daily target", DefaultValue = 6)]
        public double Target { get; set; }

        [Parameter("TP Ratio", DefaultValue = 3)]
        public double TPRatio { get; set; }

        [Parameter("Daily max operations", DefaultValue = 5)]
        public int DailyMaxOps { get; set; }

        [Parameter("Trade Direction", DefaultValue = TradeType.Buy)]
        public TradeType TDirection { get; set; }


        [Parameter("Risked amount", DefaultValue = 5)]
        public double RiskAmount { get; set; }

        [Parameter("SL Pips", DefaultValue = 5)]
        public double SL { get; set; }

        public int Iteration = 0;
        public double InitialBalance;


        protected override void OnStop()
        {
            Print("Iterations done " + Iteration);
        }

        protected override void OnStart()
        {
            InitialBalance = Account.Balance;
            Positions.Closed += PositionsOnClosed;

            Iteration++;
            Print("First Volume " + GetVolume(SL));
            ExecuteMarketOrder(TDirection, SymbolName, GetVolume(SL), "Mart", SL, SL * TPRatio);


        }

        protected override void OnTick()
        {
            if (Account.Equity >= (InitialBalance + Target))
            {
                foreach (var po in Positions)
                {
                    if (po.Label == "Mart")
                    {
                        ClosePosition(po);
                    }
                }
                Stop();
            }
        }

        protected int GetVolume(double SL)
        {

            // x which is 1 = (balance * risk%)/(SL*pipvalue*1000) ROUND TO INT
            var x = Math.Round((RiskAmount * Iteration) / (SL * Symbol.PipValue * 1000));

            //Convert.ToInt32(double)
            Print(x);
            return Convert.ToInt32(x * 1000);

        }

        private void PositionsOnClosed(PositionClosedEventArgs args)
        {
            if (DailyMaxOps == Iteration)
            {
                Stop();
            }
            if (DailyMaxOps > Iteration && args.Position.Label == "Mart")
            {
                Iteration++;
                ExecuteMarketOrder(TDirection, SymbolName, GetVolume(SL), "Mart", SL, SL * TPRatio);

            }
            // the reason for closing can be captured. 
            switch (args.Reason)
            {
                case PositionCloseReason.StopLoss:
                    Print("Position closed as stop loss was hit");




                    break;
                case PositionCloseReason.StopOut:
                    Print("Position closed as it was stopped out");
                    break;
                case PositionCloseReason.TakeProfit:
                    Print("Position closed as take profit was hit");
                    break;
            }
        }
    }
}
