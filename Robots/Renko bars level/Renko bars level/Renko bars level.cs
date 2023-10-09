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
    public class Renkobarslevel : Robot
    {
        [Parameter("Signal bars", DefaultValue = 2)]
        public int Signal_Bars { get; set; }

        [Parameter("TP Bars", DefaultValue = 3)]
        public int Bars_TP { get; set; }
        [Parameter("SL Bars", DefaultValue = 1)]
        public int Bars_SL { get; set; }



        [Parameter("Volume", DefaultValue = 0.1)]
        public double volume { get; set; }

        [Parameter("Buy", DefaultValue = true)]
        public bool allowbuy { get; set; }

        [Parameter("Sell", DefaultValue = true)]
        public bool allowsell { get; set; }

        bool close = false;

        IDictionary<Position, double> PriceLevels = new Dictionary<Position, double>();

        protected override void OnStart()
        {
            Positions.Closed += PositionsOnClosed;
        }

        private void PositionsOnClosed(PositionClosedEventArgs args)
        {
            var position = args.Position;
            PriceLevels.Remove(position);
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

            close = false;
            Checktrade();







            if (Short() && allowsell)
            {
                var a = ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Renko");

                var newsl = Bars.ClosePrices.Last(1) + (GetSL() * Symbol.PipSize);
                var newtp = Bars.ClosePrices.Last(1) - (GetTP() * Symbol.PipSize);
                ModifyPosition(a.Position, newsl, newtp, false);

                PriceLevels.Add(a.Position, Bars.ClosePrices.Last(1));
            }
            if (Long() && allowbuy)
            {
                var a = ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Renko", GetSL(), GetTP());

                var newsl = Bars.ClosePrices.Last(1) - (GetSL() * Symbol.PipSize);
                var newtp = Bars.ClosePrices.Last(1) + (GetTP() * Symbol.PipSize);
                ModifyPosition(a.Position, newsl, newtp, false);

                PriceLevels.Add(a.Position, Bars.ClosePrices.Last(1));
            }
        }


        public bool Long()
        {


            if (Bars.OpenPrices.Last(1) < Bars.ClosePrices.Last(1))
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

            if (Bars.OpenPrices.Last(1) > Bars.ClosePrices.Last(1))
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
            return Math.Round((Bars_SL * Math.Abs(Bars.OpenPrices.Last(1) - Bars.ClosePrices.Last(1))) / Symbol.PipSize);
        }

        public double GetTP()
        {
            return Math.Round((Bars_TP * Math.Abs(Bars.OpenPrices.Last(1) - Bars.ClosePrices.Last(1))) / Symbol.PipSize);
        }
    }
}
