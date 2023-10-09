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



        [Parameter("Bollinger Bands Source ")]
        public DataSeries BB_Source { get; set; }
        [Parameter("Bollinger Bands Periods", DefaultValue = 20)]
        public int BB_Period { get; set; }

        [Parameter("Bollinger Standard Dev", DefaultValue = 2)]
        public int BB_StD { get; set; }
        [Parameter("Bollinger MA Type", DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType BB_MA_Type { get; set; }





        BollingerBands BB;


        private Des_Squeeze_Play_Indicator Cindy;

        protected override void OnStart()
        {

            BB = Indicators.BollingerBands(BB_Source, BB_Period, BB_StD, BB_MA_Type);





        }

        protected bool LPos()
        {
            var Lpos = Positions.FindAll("Buy", SymbolName);
            if (Lpos.Length == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        protected bool SPos()
        {
            var Spos = Positions.FindAll("Sell", SymbolName);
            if (Spos.Length == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        protected override void OnBar()
        {
            //Custom logic
            var Longpos = Positions.FindAll("LongBB", SymbolName);
            var Shortpos = Positions.FindAll("ShortBB", SymbolName);
            //Closing buys 
            if (Bars.ClosePrices.Last(1) > BB.Top.Last(1) && Longpos.Length > 0)
            {
                foreach (var po in Longpos)
                {
                    ClosePosition(po);
                    Print("Closing a long pos");
                }
            }
            //Closing sells
            if (Bars.ClosePrices.Last(1) < BB.Bottom.Last(1) && Shortpos.Length > 0)
            {
                foreach (var po in Shortpos)
                {
                    ClosePosition(po);
                    Print("Closing a short pos");
                }
            }


            // top scenario
            if (Bars.ClosePrices.Last(1) > BB.Top.Last(1) && (Bars.ClosePrices.Last(1) - BB.Top.Last(1)) >= (Bars.ClosePrices.Last(1) - Bars.OpenPrices.Last(1)) * 0.7)
            {
                ExecuteMarketOrder(TradeType.Buy, SymbolName, volume, "LongBB");
                ExecuteMarketOrder(TradeType.Sell, SymbolName, volume, "ShortBB");
                Print("Top Scenario");
            }
            //bot scenario
            if (Bars.ClosePrices.Last(1) < BB.Bottom.Last(1) && (BB.Top.Last(1) - Bars.ClosePrices.Last(1)) >= (Bars.OpenPrices.Last(1) - Bars.ClosePrices.Last(1)) * 0.7)
            {
                ExecuteMarketOrder(TradeType.Buy, SymbolName, volume, "LongBB");
                ExecuteMarketOrder(TradeType.Sell, SymbolName, volume, "ShortBB");
                Print("Bottom Scenario");
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
