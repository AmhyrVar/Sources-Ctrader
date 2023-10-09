﻿using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MACDEMA : Robot
    {

        [Parameter()]
        public DataSeries Source { get; set; }

        public MacdCrossOver _macd;
        public ExponentialMovingAverage _ema;
        public FractalChaosBands _frac;



        protected override void OnStart()
        {
            _ema = Indicators.ExponentialMovingAverage(Source, 200);

            _frac = Indicators.FractalChaosBands();
            _macd = Indicators.MacdCrossOver(26, 12, 9);
        }

        protected override void OnBar()
        {
            if (_macd.MACD.LastValue > 0 && Bars.ClosePrices.Last(1) < _ema.Result.Last(1) && _macd.MACD.HasCrossedBelow(_macd.Signal, 1))
            {
                var x = 0;
                while (_frac.High.Last(x) != _frac.High.Last(x + 1))
                {
                    x++;
                }
                var SL = _frac.High.Last(x);
                Print("SL level" + SL);
                var SLpips = (SL - Bars.ClosePrices.Last(1)) / Symbol.PipSize;
                Print("SL Pip" + SLpips);

                //calculate swing high with fractal bands
                ExecuteMarketOrder(TradeType.Sell, SymbolName, 1000, "sell", SLpips, SLpips * 1.5);
            }



        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}

/*
if (_macd.MACD.LastValue < 0 && Bars.ClosePrices.Last(1) > _ema.Result.Last(1) && _macd.MACD.HasCrossedAbove(_macd.Signal, 1))
{
    //calculate swing high with fractal bands
    ExecuteMarketOrder(TradeType.Buy, SymbolName,GetVolume(SL)1000, "sell", 100, 50);
}*/