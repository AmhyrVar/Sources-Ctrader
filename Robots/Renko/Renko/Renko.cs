using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Renko : Robot
    {
        [Parameter("Bars", DefaultValue = 2)]
        public int BarsNumber { get; set; }

        [Parameter("Take Profit", DefaultValue = 10)]
        public double TP { get; set; }


        [Parameter("Lots", DefaultValue = 0.01)]
        public double Lots { get; set; }

        public double Volume;

        public bool stopped = false;
        public int lastdir = 2;
        public bool initial = false;
        //1 for long 0 for short
        protected override void OnStart()
        {
            Volume = (Symbol.QuantityToVolumeInUnits(Lots));

            Positions.Closed += OnPositionsClosed;
        }

        private void OnPositionsClosed(PositionClosedEventArgs args)
        {
            var pos = args.Position;
            if (args.Reason == PositionCloseReason.TakeProfit)
            {
                stopped = true;
                if (args.Position.TradeType == TradeType.Buy)
                {
                    lastdir = 1;
                }
                else
                {
                    lastdir = 0;
                }
            }
            // etc...

        }

        protected override void OnBar()
        {
            var SellPo = Positions.FindAll("Renko", SymbolName, TradeType.Sell);
            var BuyPo = Positions.FindAll("Renko", SymbolName, TradeType.Buy);


            if (ShortCheck(BarsNumber) && SellPo.Length == 0 && !initial)
            {
                if (stopped == false)
                {
                    ExecuteMarketOrder(TradeType.Sell, SymbolName, Volume, "Renko", null, TP);
                }
                initial = true;
                Print("Initial Sell");
            }
            if (LongCheck(BarsNumber) && BuyPo.Length == 0 && !initial)
            {
                if (stopped == false)
                {
                    ExecuteMarketOrder(TradeType.Buy, SymbolName, Volume, "Renko", null, TP);
                }
                initial = true;
                Print("Initial buy");
            }

            if (SellPo.Length > 0 && Bars.ClosePrices.Last(1) > Bars.OpenPrices.Last(1) && Bars.ClosePrices.Last(2) > Bars.OpenPrices.Last(2))
            {
                foreach (var po in SellPo)
                {
                    //ClosePosition(po);
                    ReversePosition(po);
                    Print("Reverse to buy");

                }
            }

            if (BuyPo.Length > 0 && Bars.ClosePrices.Last(1) < Bars.OpenPrices.Last(1) && Bars.ClosePrices.Last(2) < Bars.OpenPrices.Last(2))
            {
                foreach (var po in BuyPo)
                {
                    //ClosePosition(po);
                    ReversePosition(po);
                    Print("Reverse to sell");
                }
            }
            if (ShortCheck(BarsNumber) && SellPo.Length == 0)
            {
                /*if (stopped == false)
                { ExecuteMarketOrder(TradeType.Sell, SymbolName, Volume, "Renko Sell", null, TP); }*/
                if (stopped && lastdir == 1)
                {
                    ExecuteMarketOrder(TradeType.Sell, SymbolName, Volume, "Renko Sell", null, TP);
                    stopped = false;
                    Print("After tp sell");
                }

            }

            if (LongCheck(BarsNumber) && BuyPo.Length == 0)
            {
                /*if (stopped == false)
                { ExecuteMarketOrder(TradeType.Buy, SymbolName, Volume, "Renko Buy", null, TP); }*/
                if (stopped && lastdir == 0)
                {
                    ExecuteMarketOrder(TradeType.Buy, SymbolName, Volume, "Renko Buy", null, TP);
                    stopped = false;
                    Print("After tp buy");
                }
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
