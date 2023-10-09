using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class RSIMonaco : Robot
    {
        [Parameter("Minimum Ratio Shadow Over Body", DefaultValue = 2.0)]
        public double bodyshadowratio { get; set; }


        RelativeStrengthIndex RSI;

        protected override void OnStart()
        {
            RSI = Indicators.RelativeStrengthIndex(Bars.ClosePrices, 14);

        }
        private bool bullishEC()
        {
            if (Bars.ClosePrices.Last(1) >= Bars.OpenPrices.Last(2) && Bars.OpenPrices.Last(2) > Bars.ClosePrices.Last(2))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool bearishEC()
        {
            if (Bars.ClosePrices.Last(1) <= Bars.OpenPrices.Last(2) && Bars.OpenPrices.Last(2) < Bars.ClosePrices.Last(2))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool hammervalid()
        {
            if (Bars.ClosePrices.Last(2) >= Bars.ClosePrices.Last(1) && Bars.OpenPrices.Last(2) >= Bars.ClosePrices.Last(1) && Bars.ClosePrices.Last(2) >= Bars.OpenPrices.Last(1) && Bars.OpenPrices.Last(2) >= Bars.OpenPrices.Last(1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool shootingstarvalid()
        {
            if (Bars.ClosePrices.Last(2) <= Bars.ClosePrices.Last(1) && Bars.OpenPrices.Last(2) <= Bars.ClosePrices.Last(1) && Bars.ClosePrices.Last(2) < Bars.OpenPrices.Last(1) && Bars.OpenPrices.Last(2) <= Bars.OpenPrices.Last(1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool hammer()
        {



            if (hammervalid() && (csbody() > 0) && (((Bars.ClosePrices.Last(1) == Bars.HighPrices.Last(1)) && Math.Abs(Bars.OpenPrices.Last(1) - Bars.LowPrices.Last(1)) > csbody() * bodyshadowratio) || ((Bars.OpenPrices.Last(1) == Bars.HighPrices.Last(1)) && Math.Abs(Bars.ClosePrices.Last(1) - Bars.LowPrices.Last(1)) > csbody() * bodyshadowratio)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool shootingstar()
        {
            if (shootingstarvalid() && (csbody() > 0) && (((Bars.ClosePrices.Last(1) == Bars.LowPrices.Last(1)) && Math.Abs(Bars.OpenPrices.Last(1) - Bars.HighPrices.Last(1)) > csbody() * bodyshadowratio) || ((Bars.OpenPrices.Last(1) == Bars.LowPrices.Last(1)) && Math.Abs(Bars.ClosePrices.Last(1) - Bars.HighPrices.Last(1)) > csbody() * bodyshadowratio)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private double csbody()
        {
            return Math.Abs(Bars.OpenPrices.Last(1) - Bars.ClosePrices.Last(1));
        }

        protected override void OnBar()
        {
            if (RSI.Result.HasCrossedAbove(25, 5) && bullishEC() && hammer())
            {
                ExecuteMarketOrder(TradeType.Buy, SymbolName, 1000, "RSI");
            }
            if (RSI.Result.HasCrossedBelow(75, 5) && bearishEC() && shootingstar())
            {
                ExecuteMarketOrder(TradeType.Sell, SymbolName, 1000, "RSI");
            }
            if (RSI.Result.HasCrossedBelow(75, 1))
            {
                foreach (var po in Positions)
                {
                    if (po.SymbolName == SymbolName && po.Label == "RSI" && po.TradeType == TradeType.Buy)
                    {
                        ClosePosition(po);
                    }
                }
            }
            if (RSI.Result.HasCrossedAbove(25, 1))
            {
                foreach (var po in Positions)
                {
                    if (po.SymbolName == SymbolName && po.Label == "RSI" && po.TradeType == TradeType.Sell)
                    {
                        ClosePosition(po);
                    }
                }
            }
        }


    }
}
