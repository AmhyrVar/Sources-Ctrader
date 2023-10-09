using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class scanner : Robot
    {

        public bool LongState = false;
        public bool ShortState = false;
        public bool TradeState = false;

        [Parameter("Risk amount", DefaultValue = 250)]
        public int RawRisk { get; set; }

        [Parameter("RiskReward Ratio", DefaultValue = 3)]
        public double RR { get; set; }

        [Parameter("Extra Pips SL", DefaultValue = 2)]
        public double EP { get; set; }

        [Parameter("Limit Range", DefaultValue = 5)]
        public double LR { get; set; }

        [Parameter("Long Trades", DefaultValue = true)]
        public bool LT { get; set; }

        [Parameter("Short Trades", DefaultValue = true)]
        public bool ST { get; set; }

        public int DecimalPrecision;




        protected override void OnStart()
        {
            Positions.Opened += PositionsOnOpened;
            Positions.Closed += PositionsClosed;



            DecimalPrecision = Symbol.PipSize.ToString().Split(',').Count() > 1 ? Symbol.PipSize.ToString().Split(',').ToList().ElementAt(1).Length : 0;




        }

        protected override void OnTick()
        {
            var lines = Chart.FindAllObjects<ChartHorizontalLine>();
            var SLBSL = 0.0;
            var SLSSL = 0.0;
            foreach (var line in lines)
            {
                if (line.Comment == "LONG" && LongState == false)
                {
                    SLSSL = line.Y;


                    LongState = true;
                }
                if (line.Comment == "SHORT" && ShortState == false)
                {
                    SLBSL = line.Y;


                    ShortState = true;
                }
            }
            if (TradeState == false)
            {

                SLSSL = Math.Round(SLSSL, DecimalPrecision);
                SLBSL = Math.Round(SLBSL, DecimalPrecision);

                var SLBSLv = ((SLSSL - SLBSL) / Symbol.PipSize) + EP;

                var SLSSLv = ((SLSSL - SLBSL) / Symbol.PipSize) + EP;

                SLSSLv = Convert.ToInt32(SLSSLv);
                SLBSLv = Convert.ToInt32(SLBSLv);


                var SLBTP = SLBSLv * RR;
                var SLSTP = SLSSLv * RR;









                if (LongState == true && LT == true)
                {
                    PlaceStopLimitOrder(TradeType.Buy, SymbolName, GetVolume(SLBSLv), SLSSL, LR, "StopLimitBuy", SLBSLv, SLBTP);
                }

                if (ShortState == true && ST == true)
                {

                    PlaceStopLimitOrder(TradeType.Sell, SymbolName, GetVolume(SLSSLv), SLBSL, LR, "StopLimitSell", SLSSLv, SLSTP);
                }
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

            var x = Math.Round((RawRisk) / (SL * Symbol.PipValue * Symbol.VolumeInUnitsMin));


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
