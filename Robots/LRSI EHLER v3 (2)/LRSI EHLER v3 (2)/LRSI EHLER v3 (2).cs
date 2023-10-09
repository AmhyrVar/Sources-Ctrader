using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class LRSIEHLERv3 : Robot
    {
        [Parameter(DefaultValue = 0.2)]
        public double gamma { get; set; }

        [Parameter("Source")]
        public DataSeries Source { get; set; }

        [Parameter("Alpha", DefaultValue = 0.07)]
        public double Alpha { get; set; }

        [Parameter("Cut Off", DefaultValue = 8)]
        public double CutOff { get; set; }

        [Parameter("MA Method", DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType MA_Method { get; set; }
        [Parameter("Average true range Period", DefaultValue = 14)]
        public int Average_true_range_Period { get; set; }

        [Parameter(DefaultValue = 0.1)]
        public double volume { get; set; }
        [Parameter("Sl multiplier", DefaultValue = 1)]
        public double SLm { get; set; }
        [Parameter("TP multiplier", DefaultValue = 2)]
        public double TPm { get; set; }

        [Parameter(DefaultValue = true)]
        public bool TrailStop { get; set; }


        [Parameter(DefaultValue = true)]
        public bool OnBar_Bot { get; set; }

        [Parameter(DefaultValue = false)]
        public bool Hedging { get; set; }

        [Parameter(DefaultValue = false)]
        public bool Repeater { get; set; }


        Laguerre_RSI LRSI;
        EhlersSmoothedAdaptiveMomentum ESAM;
        AverageTrueRange ATR;

        protected override void OnStart()
        {
            //BB = Indicators.BollingerBands(BB_Source, BB_Period, BB_StD, BB_MA_Type);
            ATR = Indicators.AverageTrueRange(Average_true_range_Period, MA_Method);
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
                if (ESAM.Sam.Last(1) > 0 && LRSI.laguerrersi.HasCrossedAbove(LRSI.oversold, 1))
                {
                    if (SP.Length == 0 && LP.Length == 0 && !Repeater && !Hedging)
                    {
                        if (TrailStop)
                        {
                            ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", ATR.Result.LastValue * SLm / Symbol.PipSize, ATR.Result.LastValue * TPm, null, true);
                        }
                        else
                        {
                            ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", ATR.Result.LastValue * SLm / Symbol.PipSize, ATR.Result.LastValue * TPm);
                        }
                    }
                    else if (Repeater && !Hedging && SP.Length == 0)
                    {
                        if (TrailStop)
                        {
                            ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", ATR.Result.LastValue * SLm / Symbol.PipSize, ATR.Result.LastValue * TPm, null, true);
                        }
                        else
                        {
                            ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", ATR.Result.LastValue * SLm / Symbol.PipSize, ATR.Result.LastValue * TPm);
                        }
                    }
                    else if (Hedging && Repeater)
                    {
                        if (TrailStop)
                        {
                            ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", ATR.Result.LastValue * SLm / Symbol.PipSize, ATR.Result.LastValue * TPm, null, true);
                        }
                        else
                        {
                            ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", ATR.Result.LastValue * SLm / Symbol.PipSize, ATR.Result.LastValue * TPm);
                        }
                    }
                    else if (Hedging && !Repeater && LP.Length == 0)
                    {
                        if (TrailStop)
                        {
                            ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", ATR.Result.LastValue * SLm / Symbol.PipSize, ATR.Result.LastValue * TPm, null, true);
                        }
                        else
                        {
                            ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", ATR.Result.LastValue * SLm / Symbol.PipSize, ATR.Result.LastValue * TPm);
                        }
                    }


                    //---------------------




                }
                if (ESAM.Sam.Last(1) < 0 && LRSI.laguerrersi.HasCrossedBelow(LRSI.overbought, 1))
                {
                    if (SP.Length == 0 && LP.Length == 0 && !Repeater && !Hedging)
                    {
                        if (TrailStop)
                        {
                            ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", ATR.Result.LastValue * SLm / Symbol.PipSize, ATR.Result.LastValue * TPm, null, true);
                        }
                        else
                        {
                            ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", ATR.Result.LastValue * SLm / Symbol.PipSize, ATR.Result.LastValue * TPm);
                        }
                    }
                    else if (Repeater && !Hedging && LP.Length == 0)
                    {
                        if (TrailStop)
                        {
                            ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", ATR.Result.LastValue * SLm / Symbol.PipSize, ATR.Result.LastValue * TPm, null, true);
                        }
                        else
                        {
                            ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", ATR.Result.LastValue * SLm / Symbol.PipSize, ATR.Result.LastValue * TPm);
                        }
                    }
                    else if (Hedging && Repeater)
                    {
                        if (TrailStop)
                        {
                            ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", ATR.Result.LastValue * SLm / Symbol.PipSize, ATR.Result.LastValue * TPm, null, true);
                        }
                        else
                        {
                            ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", ATR.Result.LastValue * SLm / Symbol.PipSize, ATR.Result.LastValue * TPm);
                        }
                    }
                    else if (Hedging && !Repeater && SP.Length == 0)
                    {
                        if (TrailStop)
                        {
                            ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", ATR.Result.LastValue * SLm / Symbol.PipSize, ATR.Result.LastValue * TPm, null, true);
                        }
                        else
                        {
                            ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", ATR.Result.LastValue * SLm / Symbol.PipSize, ATR.Result.LastValue * TPm);
                        }
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
                        ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", ATR.Result.LastValue * SLm / Symbol.PipSize, ATR.Result.LastValue * TPm, null, true);
                    }
                    else
                    {
                        ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", ATR.Result.LastValue * SLm / Symbol.PipSize, ATR.Result.LastValue * TPm);
                    }

                }
                if (SP.Length == 0 && ESAM.Sam.Last(1) < 0 && LRSI.laguerrersi.HasCrossedBelow(LRSI.overbought, 1))
                {
                    if (TrailStop)
                    {
                        ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", ATR.Result.LastValue * SLm / Symbol.PipSize, ATR.Result.LastValue * TPm, null, true);
                    }
                    else
                    {
                        ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", ATR.Result.LastValue * SLm / Symbol.PipSize, ATR.Result.LastValue * TPm);
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
