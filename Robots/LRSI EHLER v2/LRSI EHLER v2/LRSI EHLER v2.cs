using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class LRSIEHLERv2 : Robot
    {
        [Parameter(DefaultValue = 0.2)]
        public double gamma { get; set; }

        [Parameter("Source")]
        public DataSeries Source { get; set; }

        [Parameter("Alpha", DefaultValue = 0.07)]
        public double Alpha { get; set; }

        [Parameter("Cut Off", DefaultValue = 8)]
        public double CutOff { get; set; }

        [Parameter(DefaultValue = 0.1)]
        public double volume { get; set; }
        [Parameter(DefaultValue = 20)]
        public double SL { get; set; }

        [Parameter(DefaultValue = 50)]
        public double TP { get; set; }

        [Parameter(DefaultValue = true)]
        public bool TrailStop { get; set; }


        [Parameter(DefaultValue = true)]
        public bool OnBar_Bot { get; set; }


        Laguerre_RSI LRSI;
        EhlersSmoothedAdaptiveMomentum ESAM;

        protected override void OnStart()
        {
            //BB = Indicators.BollingerBands(BB_Source, BB_Period, BB_StD, BB_MA_Type);
            LRSI = Indicators.GetIndicator<Laguerre_RSI>(gamma);
            ESAM = Indicators.GetIndicator<EhlersSmoothedAdaptiveMomentum>(Source, Alpha, CutOff);
        }

        protected override void OnBar()
        {
            if (OnBar_Bot)
            {
                var LP = Positions.FindAll("ESAMRSI", SymbolName, TradeType.Buy);
                var SP = Positions.FindAll("ESAMRSI", SymbolName, TradeType.Sell);
                // if Ehler green buy when LRSI crosses over red line
                if (LP.Length == 0 && ESAM.Sam.Last(1) > 0 && LRSI.laguerrersi.HasCrossedAbove(LRSI.oversold, 1))
                {

                    if (TrailStop)
                    {
                        ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", SL, TP, null, true);
                    }
                    else
                    {
                        ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", SL, TP);
                    }

                }
                if (SP.Length == 0 && ESAM.Sam.Last(1) < 0 && LRSI.laguerrersi.HasCrossedBelow(LRSI.overbought, 1))
                {
                    if (TrailStop)
                    {
                        ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", SL, TP, null, true);
                    }
                    else
                    {
                        ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", SL, TP);
                    }

                }
            }

            // if ehler red sell when LRSI crosses under green line
        }
        protected override void OnTick()
        {
            if (!OnBar_Bot)
            {
                var LP = Positions.FindAll("ESAMRSI", SymbolName, TradeType.Buy);
                var SP = Positions.FindAll("ESAMRSI", SymbolName, TradeType.Sell);
                // if Ehler green buy when LRSI crosses over red line
                if (LP.Length == 0 && ESAM.Sam.Last(1) > 0 && LRSI.laguerrersi.HasCrossedAbove(LRSI.oversold, 1))
                {

                    if (TrailStop)
                    {
                        ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", SL, TP, null, true);
                    }
                    else
                    {
                        ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", SL, TP);
                    }

                }
                if (SP.Length == 0 && ESAM.Sam.Last(1) < 0 && LRSI.laguerrersi.HasCrossedBelow(LRSI.overbought, 1))
                {
                    if (TrailStop)
                    {
                        ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", SL, TP, null, true);
                    }
                    else
                    {
                        ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", SL, TP);
                    }

                }
            }

        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
