using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Hooks : Robot
    {
        [Parameter(DefaultValue = 0.2)]
        public double gamma { get; set; }



        [Parameter(DefaultValue = 0.1)]
        public double volume { get; set; }
        [Parameter(DefaultValue = 50)]
        public double TP { get; set; }
        [Parameter(DefaultValue = 25)]
        public double SL { get; set; }

        [Parameter(DefaultValue = true)]
        public bool TrailStop { get; set; }


        [Parameter(DefaultValue = true)]
        public bool OnBar_Bot { get; set; }

        [Parameter(DefaultValue = false)]
        public bool Hedging { get; set; }

        [Parameter(DefaultValue = false)]
        public bool Repeater { get; set; }



        //AverageTrueRange ATR;
        Laguerre_RSI LRSI;
        //ATRStops ATR_Stops;
        //EhlersSmoothedAdaptiveMomentum ESAM;

        protected override void OnStart()
        {
            //BB = Indicators.BollingerBands(BB_Source, BB_Period, BB_StD, BB_MA_Type);
            //ATR_Stops = Indicators.GetIndicator<ATRStops>(MA_Method, Period, Weight, HighLow);
            LRSI = Indicators.GetIndicator<Laguerre_RSI>(gamma);
            //ATR = Indicators.AverageTrueRange(Average_true_range_Period, MA_Method);
            //ESAM = Indicators.GetIndicator<EhlersSmoothedAdaptiveMomentum>(Source, Alpha, CutOff);
        }

        protected override void OnBar()
        {


            if (OnBar_Bot)
            {
                var LP = Positions.FindAll("ESAMRSI", SymbolName, TradeType.Buy);
                var SP = Positions.FindAll("ESAMRSI", SymbolName, TradeType.Sell);
                // if Ehler green buy when LRSI crosses over red line
                if (LRSI.laguerrersi.HasCrossedAbove(LRSI.oversold, 1))
                {
                    if (SP.Length == 0 && LP.Length == 0 && !Repeater && !Hedging)
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
                    else if (Repeater && !Hedging && SP.Length == 0)
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
                    else if (Hedging && Repeater)
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
                    else if (Hedging && !Repeater && LP.Length == 0)
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


                    //---------------------




                }
                if (LRSI.laguerrersi.HasCrossedBelow(LRSI.overbought, 1))
                {


                    if (SP.Length == 0 && LP.Length == 0 && !Repeater && !Hedging)
                    {
                        if (TrailStop)
                        {

                            ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", SL, TP, null, true);
                            // Print("ATR sell" + ATR_Stops.Result.Last(1) + " Close last 1 " + Bars.ClosePrices.Last(1) + "Time " + Bars.OpenTimes.Last(1));
                        }
                        else
                        {
                            ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", SL, TP);
                        }
                    }
                    else if (Repeater && !Hedging && LP.Length == 0)
                    {
                        if (TrailStop)
                        {
                            ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", SL / Symbol.PipSize, TP / Symbol.PipSize, null, true);
                        }
                        else
                        {
                            ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", SL, TP);
                        }
                    }
                    else if (Hedging && Repeater)
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
                    else if (Hedging && !Repeater && SP.Length == 0)
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

            // if ehler red sell when LRSI crosses under green line
        }
        protected override void OnTick()
        {

            if (!OnBar_Bot)
            {
                var LP = Positions.FindAll("ESAMRSI", SymbolName, TradeType.Buy);
                var SP = Positions.FindAll("ESAMRSI", SymbolName, TradeType.Sell);
                // if Ehler green buy when LRSI crosses over red line
                if (LP.Length == 0 && LRSI.laguerrersi.HasCrossedAbove(LRSI.oversold, 1))
                {

                    if (TrailStop)
                    {
                        ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", SL / Symbol.PipSize, TP / Symbol.PipSize, null, true);
                    }
                    else
                    {
                        ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ESAMRSI", SL, TP);
                    }

                }
                if (SP.Length == 0 && LRSI.laguerrersi.HasCrossedBelow(LRSI.overbought, 1))
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
