using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class barcoloralgo : Robot
    {
        [Parameter("MA Periode", DefaultValue = 24)]
        public int Period { get; set; }

        [Parameter("MA Type", DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType MaType { get; set; }

        [Parameter("volume", DefaultValue = 0.01)]
        public double volume { get; set; }
        [Parameter("TP", DefaultValue = 50)]
        public double TP { get; set; }

        [Parameter("SL", DefaultValue = 25)]
        public double SL { get; set; }


        ColorMA CMA;

        protected override void OnStart()
        {
            CMA = Indicators.GetIndicator<ColorMA>(Period, MaType);

        }

        protected bool GreenCon()
        {
            if (Bars.LowPrices.Last(1) > CMA.Result.Last(1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        protected bool RedCon()
        {
            if (Bars.HighPrices.Last(1) < CMA.Result.Last(1))
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
            var buypo = Positions.FindAll("BarSMA", SymbolName, TradeType.Buy);
            var sellpo = Positions.FindAll("BarSMA", SymbolName, TradeType.Sell);
            //Closecon
            if (!GreenCon() && !RedCon())
            {
                foreach (var po in Positions)
                {
                    if (po.Label == "BarSMA" && po.SymbolName == SymbolName)
                    {
                        ClosePosition(po);
                    }
                }
            }
            //Buycon
            if (buypo.Length == 0 && GreenCon())
            {
                ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "BarSMA", SL, TP);
            }
            //Sell Con
            if (sellpo.Length == 0 && RedCon())
            {
                ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "BarSMA", SL, TP);
            }

        }

        protected override void OnStop()
        {
            //Alert and we're good to go

        }
    }
}
