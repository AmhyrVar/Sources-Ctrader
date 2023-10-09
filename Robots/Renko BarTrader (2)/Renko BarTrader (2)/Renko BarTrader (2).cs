using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

using System.IO;

using System.Collections;
using System.Collections.Generic;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class RenkoBarTrader : Robot
    {

        [Parameter("First signal bar", DefaultValue = 2)]
        public int FSB { get; set; }
        [Parameter("Donchian period", DefaultValue = 20)]
        public int DCP { get; set; }

        [Parameter("Breakeven by pips", DefaultValue = 5)]
        public double BEPips { get; set; }
        [Parameter("Next signal bar", DefaultValue = 2)]
        public int NSB { get; set; }


        [Parameter("Stop Loss bar", DefaultValue = 2)]
        public double Bars_SL { get; set; }
        [Parameter("Take Profit bar", DefaultValue = 5)]
        public double Bars_TP { get; set; }

        [Parameter("Lots", DefaultValue = 0.01)]
        public double Lots { get; set; }
        [Parameter("Trail first Order?", DefaultValue = true)]
        public bool TrailFirstOrder { get; set; }
        [Parameter("Trail second Order?", DefaultValue = true)]
        public bool TrailSecondOrder { get; set; }

        [Parameter("Use Channel ?", DefaultValue = true)]
        public bool ChannelUse { get; set; }

        [Parameter("Allow Buy?", DefaultValue = true)]
        public bool AllowBuy { get; set; }

        [Parameter("Allow Sell?", DefaultValue = true)]
        public bool AllowSell { get; set; }

        public double Volume;

        int CSB = 1;

        public bool Switch;
        //0 for short 1 for long use it for ChannelUse

        private DonchianChannel DC;
        IDictionary<Position, double> PriceLevels = new Dictionary<Position, double>();


        protected override void OnStart()
        {
            Volume = (Symbol.QuantityToVolumeInUnits(Lots));
           
            DC = Indicators.DonchianChannel(DCP);
            Positions.Closed += PositionsOnClosed;
        }

        private void PositionsOnClosed(PositionClosedEventArgs args)
        {
            var position = args.Position;
            PriceLevels.Remove(position);
        }
        public double GetSL()
        {
            return Math.Round((Bars_SL * Math.Abs(Bars.OpenPrices.Last(1) - Bars.ClosePrices.Last(1))) / Symbol.PipSize);
        }

        public double GetTP()
        {
            return Math.Round((Bars_TP * Math.Abs(Bars.OpenPrices.Last(1) - Bars.ClosePrices.Last(1))) / Symbol.PipSize);
        }
        public bool Checktrade()
        {
            var pos = Positions.FindAll("Renko", SymbolName);
            if (pos.Length == 0)
            {
                return true;
            }
            if (pos.Length != 0 && PriceLevels.Values.Contains(Bars.ClosePrices.Last(1)) == true)
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
            if (ChannelUse) //  public bool Switch;  0 for short 1 for long use it for ChannelUse
                            
            {
                if (FirstShort() && Checktrade())
                {
                    var a = ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(Lots), "Renko");

                    var newsl = Bars.ClosePrices.Last(1) + (GetSL() * Symbol.PipSize);
                    var newtp = Bars.ClosePrices.Last(1) - (GetTP() * Symbol.PipSize);
                    ModifyPosition(a.Position, newsl, newtp, AskTrail(1));
                    Switch = false;
                }
                if (FirstLong() && Checktrade())
                {
                    var a = ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(Lots), "Renko", GetSL(), GetTP());

                    var newsl = Bars.ClosePrices.Last(1) - (GetSL() * Symbol.PipSize);
                    var newtp = Bars.ClosePrices.Last(1) + (GetTP() * Symbol.PipSize);
                    ModifyPosition(a.Position, newsl, newtp, AskTrail(1));
                    Switch = true;
                }
                if (SecondShort() && Checktrade() && !Switch )
                {
                    var a = ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(Lots), "Renko");

                    var newsl = Bars.ClosePrices.Last(1) + (GetSL() * Symbol.PipSize);
                    var newtp = Bars.ClosePrices.Last(1) - (GetTP() * Symbol.PipSize);
                    ModifyPosition(a.Position, newsl, newtp, AskTrail(2));
                }
                if (SecondLong() && Checktrade() && Switch)
                {
                    var a = ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(Lots), "Renko", GetSL(), GetTP());

                    var newsl = Bars.ClosePrices.Last(1) - (GetSL() * Symbol.PipSize);
                    var newtp = Bars.ClosePrices.Last(1) + (GetTP() * Symbol.PipSize);
                    ModifyPosition(a.Position, newsl, newtp, AskTrail(2));
                }

            }


            if (!ChannelUse)
            {
                if (AllowBuy && SecondLong() && Checktrade())
                {
                    var a = ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(Lots), "Renko", GetSL(), GetTP());

                    var newsl = Bars.ClosePrices.Last(1) - (GetSL() * Symbol.PipSize);
                    var newtp = Bars.ClosePrices.Last(1) + (GetTP() * Symbol.PipSize);
                    ModifyPosition(a.Position, newsl, newtp, AskTrail(2));
                }
                if (AllowSell && SecondShort() && Checktrade())
                {
                    var a = ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(Lots), "Renko");

                    var newsl = Bars.ClosePrices.Last(1) + (GetSL() * Symbol.PipSize);
                    var newtp = Bars.ClosePrices.Last(1) - (GetTP() * Symbol.PipSize);
                    ModifyPosition(a.Position, newsl, newtp, AskTrail(2));
                }
            }
           /* Print("Open" + Bars.OpenPrices.Last(1));
            Print("Close" + Bars.ClosePrices.Last(1));
            if (Bars.OpenPrices.Last(1) > Bars.ClosePrices.Last(1))
            {
                ExecuteMarketOrder(TradeType.Sell, SymbolName, Volume, "Renko Trader", SL, TP);
            }
            if (Bars.OpenPrices.Last(1) < Bars.ClosePrices.Last(1))
            {
                ExecuteMarketOrder(TradeType.Buy, SymbolName, Volume, "Renko Trader", SL, TP);
            } */


        }
        public bool AskTrail(int order)
        {
            if (order == 1)
            {
                if (TrailFirstOrder)
                { return true; }
                else { return false; }

            }
            if (order == 2)
            {
                {
                    if (TrailSecondOrder)
                    { return true; }
                    else { return false; }
                }
            }
            else { return false; }
        }
        public bool SecondLong()
        {
            var Signal = 0;
            var i = CSB;
            // condition
            while (i > 0)
            {
                if (Bars.OpenPrices.Last(i) < Bars.ClosePrices.Last(i) && Bars.OpenPrices.Last(i) > DC.Middle.Last(i))
                {
                    Signal = Signal + 1;

                }
                i = i - 1;

            }

            if (Signal == CSB && Bars.OpenPrices.Last(CSB + 1) > Bars.ClosePrices.Last(CSB + 1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SecondShort()
        {
            var Signal = 0;
            var i = CSB;
            // condition
            while (i > 0)
            {
                if (Bars.OpenPrices.Last(i) > Bars.ClosePrices.Last(i) && Bars.OpenPrices.Last(i) < DC.Middle.Last(i))
                {
                    Signal = Signal + 1;

                }
                i = i - 1;

            }

            if (Signal == CSB && Bars.OpenPrices.Last(CSB + 1) < Bars.ClosePrices.Last(CSB + 1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool FirstLong()
        {
            var Signal = 0;
            var i = FSB;
            // condition
            while (i > 0)
            {
                if (Bars.OpenPrices.Last(i) < Bars.ClosePrices.Last(i)&& Bars.OpenPrices.Last(i) > DC.Middle.Last(i))
                {
                    Signal = Signal + 1;
                  
                }
                i = i - 1;

            }

            if (Signal == FSB && Bars.OpenPrices.Last(FSB + 1) > Bars.ClosePrices.Last(FSB + 1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool FirstShort()
        {
            var Signal = 0;
            var i = FSB;
            // condition
            while (i > 0)
            {
                if (Bars.OpenPrices.Last(i) > Bars.ClosePrices.Last(i) && Bars.OpenPrices.Last(i) < DC.Middle.Last(i))
                {
                    Signal = Signal + 1;
                    
                }
                i = i - 1;

            }

            if (Signal == FSB && Bars.OpenPrices.Last(FSB + 1) < Bars.ClosePrices.Last(FSB + 1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
