﻿using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class BBCandle : Robot
    {

        [Parameter(DefaultValue = 0.1)]
        public double volume { get; set; }

        [Parameter("Number of trades above/below", DefaultValue = 5)]
        public int nbrtrades { get; set; }

        [Parameter("wait for the pair of trades to resolve", DefaultValue = false)]
        public bool Wait { get; set; }

        [Parameter("Double the volume", DefaultValue = false)]
        public bool Double_Volume { get; set; }

        public int DV;



        [Parameter("Bollinger Bands Source ")]
        public DataSeries BB_Source { get; set; }
        [Parameter("Bollinger Bands Periods", DefaultValue = 20)]
        public int BB_Period { get; set; }

        [Parameter("Bollinger Standard Dev", DefaultValue = 2)]
        public int BB_StD { get; set; }
        [Parameter("Bollinger MA Type", DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType BB_MA_Type { get; set; }


        public bool waitrigger;


        BollingerBands BB;

        public int SnbrTrades;
        public int LnbrTrades;



        protected override void OnStart()
        {
            if (Double_Volume)
            {
                DV = 2;
            }
            if (!Double_Volume)
            {
                DV = 1;
            }


            SnbrTrades = nbrtrades;
            LnbrTrades = nbrtrades;
            BB = Indicators.BollingerBands(BB_Source, BB_Period, BB_StD, BB_MA_Type);

            if (Wait == false)
            {
                waitrigger = true;
            }

            if (Wait == true)
            {
                waitrigger = true;
            }



        }

        protected override void OnBar()
        {


            //Custom logic
            var Longpos = Positions.FindAll("LongBB", SymbolName);
            var Shortpos = Positions.FindAll("ShortBB", SymbolName);

            var Longposs = Positions.FindAll("LongBBs", SymbolName);
            var Shortposs = Positions.FindAll("ShortBBs", SymbolName);

            if (Longposs.Length == 0)
            {
                LnbrTrades = nbrtrades;
                Print("Restart Long");
            }
            if (Shortposs.Length == 0)
            {
                SnbrTrades = nbrtrades;
                Print("Restart short");
            }


            if (Wait && (Longpos.Length > 0 || Shortpos.Length > 0))
            {
                waitrigger = false;
            }

            if (Wait && (Longpos.Length == 0 && Shortpos.Length == 0))
            {
                waitrigger = true;
            }


            // top scenario
            if (Bars.ClosePrices.Last(1) > BB.Top.Last(1) && (Bars.ClosePrices.Last(1) - BB.Top.Last(1)) >= (Bars.ClosePrices.Last(1) - Bars.OpenPrices.Last(1)) * 0.7)
            {
                if (Longpos.Length > 0)
                {
                    foreach (var po in Longpos)
                    {
                        ClosePosition(po);

                    }
                }
                if (Longposs.Length > 0)
                {
                    foreach (var po in Longposs)
                    {
                        ClosePosition(po);

                    }
                }


                if (waitrigger)
                {
                    ExecuteMarketOrder(TradeType.Buy, SymbolName, DV * Symbol.QuantityToVolumeInUnits(volume), "LongBB");

                    if (SnbrTrades > 0)
                    {
                        ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ShortBBs");
                        SnbrTrades--;

                    }

                }
            }
            //bot scenario
            if (Bars.ClosePrices.Last(1) < BB.Bottom.Last(1) && (BB.Top.Last(1) - Bars.ClosePrices.Last(1)) >= (Bars.OpenPrices.Last(1) - Bars.ClosePrices.Last(1)) * 0.7)
            {


                if (Shortpos.Length > 0)
                {
                    foreach (var po in Shortpos)
                    {
                        ClosePosition(po);

                    }
                }
                if (Shortposs.Length > 0)
                {
                    foreach (var po in Shortposs)
                    {
                        ClosePosition(po);

                    }
                }

                if (waitrigger)
                {
                    if (LnbrTrades > 0)
                    {
                        ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "LongBBs");
                        LnbrTrades--;

                    }
                    ExecuteMarketOrder(TradeType.Sell, SymbolName, DV * Symbol.QuantityToVolumeInUnits(volume), "ShortBB");

                }
            }


            // var Longpos = Positions.FindAll("Buy", SymbolName);
            // var Shortpos = Positions.FindAll("Sell", SymbolName);





        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
