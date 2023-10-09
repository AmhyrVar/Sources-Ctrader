using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Renkobarstrader : Robot
    {
        [Parameter("Signal bars", DefaultValue = 2)]
        public int Signal_Bars { get; set; }

        [Parameter("TP Bars", DefaultValue = 3)]
        public int Bars_TP { get; set; }
        [Parameter("SL Bars", DefaultValue = 1)]
        public int Bars_SL { get; set; }



        [Parameter("Volume", DefaultValue = 0.1)]
        public double volume { get; set; }

        bool close = false;


        protected override void OnStart()
        {
            Print((Bars_TP * Math.Abs(Bars.OpenPrices.Last(1) - Bars.ClosePrices.Last(1))) / Symbol.PipSize);
        }

        public void Checktrade()
        {
            var pos = Positions.FindAll("Renko", SymbolName);
            foreach (var po in pos)
            {
                if (po.EntryPrice == Bars.ClosePrices.Last(1))
                {
                    close = true;
                }
            }
        }

        protected override void OnBar()
        {
            Print("New Brick header ");
            close = false;
            Checktrade();




            Print("New Brick Logic");

            if (Short() && close == false)
            {
                ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Renko", GetSL(), GetTP());
            }
            if (Long() && close == false)
            {
                ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Renko", GetSL(), GetTP());
            }
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
        public bool Long()
        {
            var Signal = 0;
            var i = Signal_Bars;
            // condition
            while (i > 0)
            {
                if (Bars.OpenPrices.Last(i) < Bars.ClosePrices.Last(i))
                {
                    Signal = Signal + 1;
                    i = i - 1;
                }


            }

            if (Signal == Signal_Bars && Bars.OpenPrices.Last(Signal_Bars + 1) > Bars.ClosePrices.Last(Signal_Bars + 1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Short()
        {
            var Signal = 0;
            var i = Signal_Bars;
            // condition
            while (i > 0)
            {
                if (Bars.OpenPrices.Last(i) > Bars.ClosePrices.Last(i))
                {
                    Signal = Signal + 1;
                    i = i - 1;
                }


            }

            if (Signal == Signal_Bars && Bars.OpenPrices.Last(Signal_Bars + 1) < Bars.ClosePrices.Last(Signal_Bars + 1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public double GetSL()
        {
            return (Bars_SL * Math.Abs(Bars.OpenPrices.Last(1) - Bars.ClosePrices.Last(1))) / Symbol.PipSize;
        }

        public double GetTP()
        {
            return (Bars_TP * Math.Abs(Bars.OpenPrices.Last(1) - Bars.ClosePrices.Last(1))) / Symbol.PipSize;
        }
    }
}
