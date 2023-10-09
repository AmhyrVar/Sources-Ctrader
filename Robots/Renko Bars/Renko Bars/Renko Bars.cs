using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class RenkoBars : Robot
    {
        [Parameter("Bars", DefaultValue = 3)]
        public int BarsNumber { get; set; }

        [Parameter("Take Profit", DefaultValue = 10)]
        public double TP { get; set; }
        [Parameter("Stop Loss", DefaultValue = 50)]
        public double SL { get; set; }

        [Parameter("Lots", DefaultValue = 0.01)]
        public double Lots { get; set; }

        public double Volume;

        protected override void OnStart()
        {
            Volume = (Symbol.QuantityToVolumeInUnits(Lots));

        }

        protected override void OnBar()
        {
            var SellPo = Positions.FindAll("Renko Sell", SymbolName);
            var BuyPo = Positions.FindAll("Renko Buy", SymbolName);

            if (SellPo.Length > 0 && Bars.ClosePrices.Last(1) > Bars.OpenPrices.Last(1))
            {
                foreach (var po in SellPo)
                {
                    ClosePosition(po);
                }
            }

            if (BuyPo.Length > 0 && Bars.ClosePrices.Last(1) < Bars.OpenPrices.Last(1))
            {
                foreach (var po in BuyPo)
                {
                    ClosePosition(po);
                }
            }
            if (ShortCheck(BarsNumber))
            {
                ExecuteMarketOrder(TradeType.Sell, SymbolName, Volume, "Renko Sell", SL, TP);
            }

            if (LongCheck(BarsNumber))
            {
                ExecuteMarketOrder(TradeType.Buy, SymbolName, Volume, "Renko Buy", SL, TP);
            }
        }

        private bool LongCheck(int BN)
        {
            var Teferi = true;
            var c = 1;
            while (Teferi && c < BN + 1)
            {
                if (Bars.ClosePrices.Last(c) > Bars.OpenPrices.Last(c))
                {
                    c++;
                }
                else
                {
                    Teferi = false;
                }
            }

            return Teferi;
        }

        private bool ShortCheck(int BN)
        {
            var Teferi = true;
            var c = 1;
            while (Teferi && c < BN + 1)
            {
                if (Bars.ClosePrices.Last(c) < Bars.OpenPrices.Last(c))
                {
                    c++;
                }
                else
                {
                    Teferi = false;
                }
            }

            return Teferi;
        }
    }
}
