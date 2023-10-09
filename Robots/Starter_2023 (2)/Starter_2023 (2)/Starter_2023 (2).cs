using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class Starter_20232 : Robot
    {
        [Parameter("Lot size", Group = "Normal mode", DefaultValue = 0.1, MinValue = 0.01, Step = 0.01)]
        public double Quantity { get; set; }
        

        [Parameter("Order type", DefaultValue = enmABC.Both, Group = "Normal mode")]

        public enmABC SellPC { get; set; }
        public enum enmABC
        {
            Both,
            Buy,
            Sell
        }

        [Parameter("Period",  Group = "Indicator Donchian", DefaultValue = 20)]
        public int DC_Bars { get; set; }

        [Parameter("Bar to open", Group = "Trade", DefaultValue = 2)]
        public int bar_To_Open { get; set; }
        [Parameter("Bar to close", Group = "Trade", DefaultValue = 2)]
        public int bar_To_Close { get; set; }
        [Parameter("SL bar", Group = "Trade", DefaultValue = 2)]
        public int sl_Bar { get; set; } //if 0 no SL
        [Parameter("Minimum profit", Group = "Trade", DefaultValue = 2)]
        public int min_Profit { get; set; }//in profit to close the trade


       


       


        private DonchianChannel DC;


        protected override void OnStart()
        {
            DC = Indicators.DonchianChannel(DC_Bars);

           



        }

        protected override void OnBar()
        {
            var Bpo = Positions.FindAll("artPo", SymbolName, TradeType.Buy);
            var Spo = Positions.FindAll("artPo", SymbolName, TradeType.Sell);
            
            if (Bpo.Length > 0)
            {
                foreach (var po in Bpo)
                {
                    if (CheckClose(TradeType.Buy, po))
                    {
                        ClosePosition(po);
                    }
                }
            }

            if (Spo.Length > 0)
            {
                foreach (var po in Spo)
                {
                    if (CheckClose(TradeType.Sell, po))
                    {
                        ClosePosition(po);
                    }
                }
            }

            if (Long()&& Bpo.Length == 0)
            {
                Open_Trade(TradeType.Buy);
            }

            if (Short()&& Spo.Length == 0)
            {
                Open_Trade(TradeType.Sell);
            }


        }

        public bool Short()
        {
            var Signal = 0;
            var i = bar_To_Open;
            // condition
            while (i > 0)
            {
                if (Bars.OpenPrices.Last(i) > Bars.ClosePrices.Last(i) && Bars.OpenPrices.Last(i) < DC.Middle.Last(i) && Bars.ClosePrices.Last(i) < DC.Middle.Last(i))
                {
                    
                    Signal = Signal + 1;

                }

                i = i - 1;
                
            }

            if (Signal == bar_To_Open && (SellPC == enmABC.Both || SellPC == enmABC.Sell) ) // if (Signal == bar_To_Open && Bars.OpenPrices.Last(bar_To_Open + 1) > Bars.ClosePrices.Last(bar_To_Open + 1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool Long()
        {
            var Signal = 0;
            var i = bar_To_Open;
            // condition
            while (i > 0)
            {
                if (Bars.OpenPrices.Last(i) < Bars.ClosePrices.Last(i) && Bars.OpenPrices.Last(i) > DC.Middle.Last(i) && Bars.ClosePrices.Last(i) > DC.Middle.Last(i))
                {

                    Signal = Signal + 1;

                }

                i = i - 1;

            }

            if (Signal == bar_To_Open && (SellPC == enmABC.Both || SellPC == enmABC.Buy)) // if (Signal == bar_To_Open && Bars.OpenPrices.Last(bar_To_Open + 1) > Bars.ClosePrices.Last(bar_To_Open + 1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool CheckClose(TradeType actual_TT, Position po)
        {
            var Signal = 0;
            var i = bar_To_Close;

            if (actual_TT == TradeType.Buy)
            {
                while (i > 0)
                {
                    if (Bars.OpenPrices.Last(i) > Bars.ClosePrices.Last(i) && Bars.OpenPrices.Last(i) < DC.Middle.Last(i) && Bars.ClosePrices.Last(i) < DC.Middle.Last(i))
                    {
                        
                        Signal = Signal + 1;

                    }

                    i = i - 1;
                    
                }
                var minPips = (Math.Abs(Bars.OpenPrices.Last(1)-Bars.ClosePrices.Last(1))/ Symbol.PipSize)*min_Profit;
                Print("minpips are " + minPips + "po Pips " + po.Pips + "Signal is " + Signal + "bars to close are " + bar_To_Close);
                if (Signal == bar_To_Close && po.Pips >= minPips)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }



            if (actual_TT == TradeType.Sell)
            {
                while (i > 0)
                {
                    if (Bars.OpenPrices.Last(i) < Bars.ClosePrices.Last(i) && Bars.OpenPrices.Last(i) > DC.Middle.Last(i) && Bars.ClosePrices.Last(i) > DC.Middle.Last(i))
                    {
                        
                        Signal = Signal + 1;

                    }

                    i = i - 1;
                    
                }
                var minPips = (Math.Abs(Bars.OpenPrices.Last(1) - Bars.ClosePrices.Last(1)) / Symbol.PipSize) * min_Profit;
                Print("minpips are " + minPips + "po Pips " + po.Pips + "Signal is " + Signal+ "bars to close are "+bar_To_Close);
                if (Signal == bar_To_Close && po.Pips >= minPips)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            else
            {
                return false;
            }
           

        }
        public void CheckProfit()
        {

        }
      
        public void Open_Trade(TradeType actual_TradeType)
        {
            if (SellPC == enmABC.Both)
            {
               var a =  ExecuteMarketOrder(actual_TradeType, SymbolName, Symbol.QuantityToVolumeInUnits(Quantity),"artPo"); 

                if (a.Position.TradeType == TradeType.Buy)
                {
                    var newSL = Bars.ClosePrices.Last(1) - (sl_Bar * (Bars.ClosePrices.Last(1) - Bars.OpenPrices.Last(1)));
                    a.Position.ModifyStopLossPrice(newSL);

                }

                if (a.Position.TradeType == TradeType.Sell)
                {
                    var newSL = Bars.ClosePrices.Last(1) + (sl_Bar * (Bars.OpenPrices.Last(1) - Bars.ClosePrices.Last(1) ));
                    a.Position.ModifyStopLossPrice(newSL);
                }
            }
            if (SellPC == enmABC.Buy)
            {
                var a = ExecuteMarketOrder(actual_TradeType, SymbolName, Symbol.QuantityToVolumeInUnits(Quantity), "artPo");

                if (a.Position.TradeType == TradeType.Buy)
                {
                    var newSL = Bars.ClosePrices.Last(1) - (sl_Bar * (Bars.ClosePrices.Last(1) - Bars.OpenPrices.Last(1)));
                    a.Position.ModifyStopLossPrice(newSL);

                }
            }
            if (SellPC == enmABC.Sell)
            {
                var a = ExecuteMarketOrder(actual_TradeType, SymbolName, Symbol.QuantityToVolumeInUnits(Quantity), "artPo");
                if (a.Position.TradeType == TradeType.Sell)
                {
                    var newSL = Bars.ClosePrices.Last(1) + (sl_Bar * (Bars.OpenPrices.Last(1) - Bars.ClosePrices.Last(1)));
                    a.Position.ModifyStopLossPrice(newSL);
                }
            }
        }
        protected override void OnStop()
        {
            
        }
    }
}