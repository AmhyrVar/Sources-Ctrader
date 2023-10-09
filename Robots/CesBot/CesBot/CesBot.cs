using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Cesare_bot : Robot
    {
        private TradeType Direction = TradeType.Buy;

        [Parameter("SMA1 Period", DefaultValue = 15)]
        public int PeriodsSma1 { get; set; }
        [Parameter("SMA2 Period", DefaultValue = 10)]
        public int PeriodsSma2 { get; set; }

        [Parameter("SMA Source")]
        public DataSeries Source { get; set; }

        private SimpleMovingAverage _sma1 { get; set; }
        private SimpleMovingAverage _sma2 { get; set; }

        [Parameter("SL", DefaultValue = 20)]
        public int SL { get; set; }

        [Parameter("TP", DefaultValue = 20)]
        public int TP { get; set; }

        [Parameter("starting LotSize", DefaultValue = 0.01)]
        public int StartLS { get; set; }


        protected override void OnStart()
        {


            _sma1 = Indicators.SimpleMovingAverage(Source, PeriodsSma1); //shift =3
            _sma2 = Indicators.SimpleMovingAverage(Source, PeriodsSma2); // shift = 2

            Print("Decimal ", Symbol.Digits);


            Positions.Closed += PositionsOnClosed;

            CheckDirection();

            ExecuteMarketOrder(Direction, SymbolName, Symbol.QuantityToVolumeInUnits(0.03), "CesareLive", SL, TP);











            // Fill the list


        }

        private void PositionsOnClosed(PositionClosedEventArgs args)
        {
            if (args.Position.Label == "CesareLive")
            {
                ClosePendings();
                //maybe close noise positions too LO1 LO2 etc
                CheckDirection();
                ExecuteMarketOrder(Direction, SymbolName, Symbol.QuantityToVolumeInUnits(0.03), "CesareLive", SL, TP);



                //DIagnosis






            }
        }

        private void CheckDirection()
        {
            if (_sma1.Result.LastValue > _sma2.Result.LastValue)
            {
                Direction = TradeType.Buy;
            }
            else
            {
                Direction = TradeType.Sell;
            }
        }

        private void ClosePendings()
        {
            foreach (var po in PendingOrders)
            {
                if (po.Label.Contains("LO") && po.SymbolName == SymbolName)
                {

                    CancelPendingOrder(po);

                }
            }
        }



    }
}
