using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class DonnyDarcov2 : Robot
    {
        [Parameter("SMA Period", DefaultValue = 50)]
        public int SMA_P { get; set; }
        [Parameter("SMA Source")]
        public DataSeries SMA_S { get; set; }
        [Parameter("Fixed Lot size", DefaultValue = false)]
        public bool FixedLotSize { get; set; }
        [Parameter("Custom LotSize", DefaultValue = 0.1)]
        public double CustomLot { get; set; }

        [Parameter("Channel Bars", DefaultValue = 100)]
        public int DC_Bars { get; set; }

        private DonchianChannel DC;
        private SimpleMovingAverage SMA;

        private bool direction;
        private bool state;

        //private int Volume;

        protected override void OnStart()
        {
            DC = Indicators.DonchianChannel(DC_Bars);
            SMA = Indicators.SimpleMovingAverage(SMA_S, SMA_P);
            if (Bars.ClosePrices.LastValue > SMA.Result.LastValue)
            {
                direction = true;
            }
            else
            {
                direction = false;
            }
        }
        protected double GetVol()
        {
            if (!FixedLotSize)
            {
                var f = Math.Round(Account.Balance / 100);
                var Vol = f * 0.01;
                Print(" f = " + f + "Vol = " + Vol);

                var Volume = Symbol.QuantityToVolumeInUnits(Vol);


                return Volume;

            }
            else
            {
                return Symbol.QuantityToVolumeInUnits(CustomLot);
            }

        }

        protected override void OnBar()
        {
            var pos = Positions.FindAll("Donny", SymbolName);



            if (direction == true && pos.Length == 0 && Bars.ClosePrices.Last(1) > DC.Top.Last(1) && Bars.ClosePrices.Last(1) > SMA.Result.Last(1))
            {
                Print("LONG");
                ExecuteMarketOrder(TradeType.Buy, SymbolName, GetVol(), "Donny");
                direction = false;
                state = true;
            }

            if (direction == false && pos.Length == 0 && Bars.ClosePrices.Last(1) < DC.Bottom.Last(1) && Bars.ClosePrices.Last(1) < SMA.Result.Last(1))
            {
                Print("Short");
                ExecuteMarketOrder(TradeType.Sell, SymbolName, GetVol(), "Donny");
                direction = true;
                state = true;
            }


            if (pos.Length != 0 && pos[0].TradeType == TradeType.Buy)
            {
                if (state == true && Bars.ClosePrices.Last(1) < DC.Middle.Last(1))
                {
                    var fifty = pos[0].VolumeInUnits / 2;
                    var result = fifty % 1000 >= 500 ? fifty + 1000 - fifty % 1000 : fifty - fifty % 1000;
                    pos[0].ModifyVolume(result);
                    state = false;

                }


                if (state == false && Bars.ClosePrices.Last(1) < DC.Bottom.Last(1))
                {
                    ClosePosition(pos[0]);


                }
            }

            if (pos.Length != 0 && pos[0].TradeType == TradeType.Sell)
            {
                if (state == true && Bars.ClosePrices.Last(1) > DC.Middle.Last(1))
                {
                    var fifty = pos[0].VolumeInUnits / 2;
                    var result = fifty % 1000 >= 500 ? fifty + 1000 - fifty % 1000 : fifty - fifty % 1000;
                    pos[0].ModifyVolume(result);
                    state = false;

                }
                if (state == false && Bars.ClosePrices.Last(1) > DC.Top.Last(1))
                {
                    ClosePosition(pos[0]);

                }
            }



        }


    }
}
