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
    public class Ichimokucloudbot : Robot
    {
        [Parameter(DefaultValue = 0.01)]
        public double volume { get; set; }





        private Bars data1H;
        private Bars data4H;

        IchimokuKinkoHyo ichimoku1;
        IchimokuKinkoHyo ichimoku4;
        IchimokuKinkoHyo ichimoku30;



        protected override void OnStart()
        {
            data1H = MarketData.GetBars(TimeFrame.Hour);
            data4H = MarketData.GetBars(TimeFrame.Hour4);
            ichimoku30 = Indicators.IchimokuKinkoHyo(9, 25, 52);
            ichimoku1 = Indicators.IchimokuKinkoHyo(data1H, 9, 25, 52);
            ichimoku4 = Indicators.IchimokuKinkoHyo(data4H, 9, 25, 52);

        }

        private bool TimeCheck()
        {

            if (Server.Time.Hour >= 7 && Server.Time.Hour < 17)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void OnBar()
        {

            ichimoku4.SenkouSpanA.Last(0);

            //Entry rule( break any kumo)

            //bullish entries
            if (TimeCheck() && ichimoku4.SenkouSpanA.Last(25) > ichimoku4.SenkouSpanB.Last(25))            /*&& TimeCheck()*/
            {
                //kumo breakout M30 
                if ((ichimoku30.SenkouSpanA.Last(25) > ichimoku30.SenkouSpanB.Last(25) && Bars.OpenPrices.Last(1) > ichimoku30.SenkouSpanB.Last(25) && Bars.OpenPrices.Last(1) < ichimoku30.SenkouSpanA.Last(25) && Bars.ClosePrices.Last(1) > ichimoku30.SenkouSpanA.Last(25)) || (ichimoku30.SenkouSpanA.Last(25) < ichimoku30.SenkouSpanB.Last(25) && Bars.OpenPrices.Last(1) < ichimoku30.SenkouSpanB.Last(25) && Bars.OpenPrices.Last(1) > ichimoku30.SenkouSpanA.Last(25) && Bars.ClosePrices.Last(1) > ichimoku30.SenkouSpanB.Last(25)))
                {
                    //get tar price, look for the best cloud flat
                    // cloud flat = get array of ssa and ssb 
                    // tar price = value most repeated
                    List<double> list = new List<double>();


                    for (int i = 0; i < 25; i++)
                    {
                        list.Add(ichimoku30.SenkouSpanA.Last(25 - i));
                        list.Add(ichimoku30.SenkouSpanB.Last(25 - i));
                    }

                    var tarprice = list.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
                    Print("The flat is " + tarprice);

                    //SL outside of Kumo
                    //TP is 2x SL
                    var MinSSAB = Math.Min(ichimoku30.SenkouSpanA.Last(1), ichimoku30.SenkouSpanB.Last(1));
                    var SL = Math.Abs(Convert.ToDouble(Bars.ClosePrices.Last(1) - MinSSAB)) / Symbol.PipSize;
                    PlaceLimitOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), tarprice, "Ichi", SL, 2 * SL);

                    //Breakeven when 1/1
                }

                //H1
                if ((ichimoku1.SenkouSpanA.Last(25) > ichimoku1.SenkouSpanB.Last(25) && data1H.OpenPrices.Last(1) > ichimoku1.SenkouSpanB.Last(25) && data1H.OpenPrices.Last(1) < ichimoku1.SenkouSpanA.Last(25) && data1H.ClosePrices.Last(1) > ichimoku1.SenkouSpanA.Last(25)) || (ichimoku1.SenkouSpanA.Last(25) < ichimoku1.SenkouSpanB.Last(25) && data1H.OpenPrices.Last(1) < ichimoku1.SenkouSpanB.Last(25) && data1H.OpenPrices.Last(1) > ichimoku1.SenkouSpanA.Last(25) && data1H.ClosePrices.Last(1) > ichimoku1.SenkouSpanB.Last(25)))
                {
                    //get tar price, look for the best cloud flat
                    // cloud flat = get array of ssa and ssb 
                    // tar price = value most repeated
                    List<double> list = new List<double>();


                    for (int i = 0; i < 25; i++)
                    {
                        list.Add(ichimoku1.SenkouSpanA.Last(25 - i));
                        list.Add(ichimoku1.SenkouSpanB.Last(25 - i));
                    }

                    var tarprice = list.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
                    Print("The flat is " + tarprice);

                    //SL outside of Kumo
                    //TP is 2x SL
                    var MinSSAB = Math.Min(ichimoku1.SenkouSpanA.Last(1), ichimoku1.SenkouSpanB.Last(1));
                    var SL = Math.Abs(Convert.ToDouble(data1H.ClosePrices.Last(1) - MinSSAB)) / Symbol.PipSize;
                    PlaceLimitOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), tarprice, "Ichi", SL, 2 * SL);

                    //Breakeven when 1/1
                }

            }


            //Bearish 
            if (TimeCheck() && ichimoku4.SenkouSpanA.Last(25) < ichimoku4.SenkouSpanB.Last(25))            /*&& TimeCheck()*/
            {
                //kumo breakout M30 delete tradeh1 for 1h trading
                if ((ichimoku30.SenkouSpanA.Last(25) < ichimoku30.SenkouSpanB.Last(25) && Bars.OpenPrices.Last(1) < ichimoku30.SenkouSpanB.Last(25) && Bars.OpenPrices.Last(1) > ichimoku30.SenkouSpanA.Last(25) && Bars.ClosePrices.Last(1) < ichimoku30.SenkouSpanA.Last(25)) || (ichimoku30.SenkouSpanA.Last(25) > ichimoku30.SenkouSpanB.Last(25) && Bars.OpenPrices.Last(1) < ichimoku30.SenkouSpanA.Last(25) && Bars.OpenPrices.Last(1) > ichimoku30.SenkouSpanB.Last(25) && Bars.ClosePrices.Last(1) < ichimoku30.SenkouSpanB.Last(25)))
                {

                    //Print("");
                    //Print("-------------------------------------");

                    //get tar price, look for the best cloud flat
                    // cloud flat = get array of ssa and ssb 
                    // tar price = value most repeated
                    List<double> list = new List<double>();


                    for (int i = 0; i < 25; i++)
                    {
                        list.Add(ichimoku30.SenkouSpanA.Last(25 - i));
                        list.Add(ichimoku30.SenkouSpanB.Last(25 - i));
                    }

                    var tarprice = list.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
                    Print("The flat is " + tarprice);

                    //SL outside of Kumo
                    //TP is 2x SL
                    var MaxSSAB = Math.Min(ichimoku30.SenkouSpanA.Last(1), ichimoku30.SenkouSpanB.Last(1));
                    var SL = (Math.Abs(Convert.ToDouble(Bars.ClosePrices.Last(1) - MaxSSAB)) / Symbol.PipSize) + 2 * Symbol.PipSize;
                    PlaceLimitOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), tarprice, "Ichi", SL, 2 * SL);

                    //Diagnosis
                    //Chart.DrawVerticalLine("s", Bars.OpenTimes.Last(1), Color.White);

                    //Breakeven when 1/1
                }
                if ((ichimoku1.SenkouSpanA.Last(25) < ichimoku1.SenkouSpanB.Last(25) && data1H.OpenPrices.Last(1) < ichimoku1.SenkouSpanB.Last(25) && data1H.OpenPrices.Last(1) > ichimoku1.SenkouSpanA.Last(25) && data1H.ClosePrices.Last(1) < ichimoku1.SenkouSpanA.Last(25)) || (ichimoku1.SenkouSpanA.Last(25) > ichimoku1.SenkouSpanB.Last(25) && data1H.OpenPrices.Last(1) < ichimoku1.SenkouSpanA.Last(25) && data1H.OpenPrices.Last(1) > ichimoku1.SenkouSpanB.Last(25) && data1H.ClosePrices.Last(1) < ichimoku1.SenkouSpanB.Last(25)))
                {

                    //Print("");
                    //Print("-------------------------------------");

                    //get tar price, look for the best cloud flat
                    // cloud flat = get array of ssa and ssb 
                    // tar price = value most repeated
                    List<double> list = new List<double>();


                    for (int i = 0; i < 25; i++)
                    {
                        list.Add(ichimoku1.SenkouSpanA.Last(25 - i));
                        list.Add(ichimoku1.SenkouSpanB.Last(25 - i));
                    }

                    var tarprice = list.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
                    Print("The flat is " + tarprice);

                    //SL outside of Kumo
                    //TP is 2x SL
                    var MaxSSAB = Math.Max(ichimoku1.SenkouSpanA.Last(1), ichimoku1.SenkouSpanB.Last(1));
                    var SL = (Math.Abs(Convert.ToDouble(data1H.ClosePrices.Last(1) - MaxSSAB)) / Symbol.PipSize) + 2 * Symbol.PipSize;
                    PlaceLimitOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), tarprice, "Ichi", SL, 2 * SL);

                    //Diagnosis
                    //Chart.DrawVerticalLine("s", data1H.OpenTimes.Last(1), Color.White);

                    //Breakeven when 1/1
                }
            }

        }
        /*
             * 
             * 1. Check the H4 cloud: if it's bullish, looking for buy opportunities. if bearish, looking for selling opportunities

                . Find a Kumo breakout on m30 OR H1 in this time period: 7am UTC - 17pm UTC. ONLY the breaks consistent with the verse of Kumo in H4 are valid

                     When a breakout happened, put a sell limit in the first cloud flattening, as a retest of Kumo

                 Stop loss outside the Kumo, fixed R/R at 2:1 (it depends stop loss size)

                 When the position is at 1:1 level, put the position at BreakEven
             * 
             * 
             * 
             * 
             * 
             * */
        protected override void OnTick()
        {
            foreach (var po in Positions)
            {

                var TPinPips = Math.Abs(Convert.ToDouble(po.TakeProfit - po.EntryPrice) / Symbol.PipSize);




                if (po.Label == "Ichi" && po.SymbolName == SymbolName && po.Pips >= TPinPips * 0.5 && po.TradeType == TradeType.Buy && po.StopLoss < po.EntryPrice)
                {
                    Print("Breakeven");
                    po.ModifyStopLossPips(po.EntryPrice + (1 / Symbol.PipSize));

                }
                if (po.Label == "Ichi" && po.SymbolName == SymbolName && po.Pips >= TPinPips * 0.5 && po.TradeType == TradeType.Sell && po.StopLoss > po.EntryPrice)
                {
                    Print("Breakeven");
                    po.ModifyStopLossPips(po.EntryPrice - (1 / Symbol.PipSize));

                }

            }
        }
        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
