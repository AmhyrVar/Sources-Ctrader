﻿using System;
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


        [Parameter("Custom LotSize", DefaultValue = 0.1)]
        public double CustomLot { get; set; }

        [Parameter("Channel Bars", DefaultValue = 100)]
        public int DC_Bars { get; set; }

        [Parameter("Max Positions", DefaultValue = 5)]
        public int MaxPos { get; set; }

        [Parameter("SL", DefaultValue = 20)]
        public int SL { get; set; }

        [Parameter("TP", DefaultValue = 50)]
        public int TP { get; set; }
        private DonchianChannel DC;




        //private int Volume;

        protected override void OnStart()
        {
            DC = Indicators.DonchianChannel(DC_Bars);

        }

        protected override void OnBar()
        {
            var pos = Positions.FindAll("Donny", SymbolName);



            if (pos.Length == MaxPos && Bars.ClosePrices.Last(1) > DC.Middle.Last(1) && Bars.OpenPrices.Last(1) < DC.Middle.Last(1))
            {
                Print("LONG");
                ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(CustomLot), "Donny", SL, TP);

            }

            if (pos.Length == MaxPos && Bars.ClosePrices.Last(1) > DC.Middle.Last(1) && Bars.OpenPrices.Last(1) < DC.Middle.Last(1))
            {
                Print("Short");
                ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(CustomLot), "Donny", SL, TP);

            }




        }


    }
}
