using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class waqaarhussain : Robot
    {
        //+13 mins
        [Parameter("RSI Source", Group = "RSI parameters")]
        public DataSeries RsiSourceSeries { get; set; }
        [Parameter("RSI Periods", DefaultValue = 14, Group = "RSI parameters")]
        public int RsiPeriods { get; set; }

        [Parameter("RSI Cross look back", DefaultValue = 5, Group = "RSI parameters")]
        public int CrossPeriod { get; set; }


        [Parameter("EMA Periods", DefaultValue = 14, Group = "EMA parameters")]
        public int EMAPeriods { get; set; }

       
        [Parameter("Fractals Periods", DefaultValue = 5, Group = "Fractals parameters")]
        public int FracPeriods { get; set; }


        [Parameter("volume", DefaultValue = 0.01, Group = "Position management")]
        public double volume { get; set; }

        [Parameter("Take Profit", DefaultValue = 50, Group = "Position management")]
        public double TP { get; set; }
        [Parameter("Stop Loss", DefaultValue = 25, Group = "Position management")]
        public double SL { get; set; }



        private RelativeStrengthIndex _rsi;
        private Fractals _fractals;
        private ExponentialMovingAverage _ema;



        private bool CrossOver;
        private bool CrossUnder;

        private int CrossOverCount;
        private int CrossUnderCount;

       
       
        protected override void OnStart()
        {

            _rsi = Indicators.RelativeStrengthIndex(RsiSourceSeries, RsiPeriods);
           _fractals = Indicators.Fractals(FracPeriods);
             _ema = Indicators.ExponentialMovingAverage(_rsi.Result, EMAPeriods);

          

            CrossOver = false;
            CrossUnder = false;

            CrossOverCount = 0;
            CrossUnderCount = 0;
        }

        protected override void OnBar()
        {
            if (_rsi.Result.HasCrossedAbove(30, 1)) // &&// _rsi.Result.Last(2) < 30 && _rsi.Result.Last(1)>30
            {
                Print($"Buy before {_rsi.Result.Last(2)} After {_rsi.Result.Last(2)}");

                   CrossOver = true;

                CrossUnder = false;

                CrossOverCount = 0;
                CrossUnderCount = 0;
            }
            if (_rsi.Result.HasCrossedBelow(70, 1))//_rsi.Result.Last(2) > 70 && _rsi.Result.Last(1) < 70
            {

                Print($"Sell before {_rsi.Result.Last(2)} After {_rsi.Result.Last(2)}");
                CrossOver = false;
                CrossUnder = true;

                CrossOverCount = 0;
                CrossUnderCount = 0;
            }
            if (!_rsi.Result.HasCrossedAbove(30, 1) && !_rsi.Result.HasCrossedBelow(70, 1))
            {
                if (CrossOver)
                    CrossOverCount++;
                if (CrossUnder)
                    CrossUnderCount++;
            }
            if (CrossOverCount == CrossPeriod || CrossUnderCount == CrossPeriod)
            {
                CrossOver = false;
                CrossUnder = false;
                CrossOverCount = 0;
                CrossUnderCount = 0;
            }

            var Bpo = Positions.FindAll("Buy", SymbolName);
            var Spo = Positions.FindAll("Sell", SymbolName);
            if (CrossOver && _rsi.Result.HasCrossedAbove(_ema.Result,1) && Bpo.Length==0)
            {
                var p = ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Buy");


                var sl = RoundPrice(_fractals.DownFractal.LastValue, TradeType.Buy);



                var tp = RoundPrice(((p.Position.EntryPrice - _fractals.DownFractal.LastValue ) + p.Position.EntryPrice),TradeType.Buy);
                
                ModifyPosition(p.Position,sl ,tp);

                if (Spo.Length > 0)
                {
                    foreach (var po in Spo)
                    {
                        ClosePosition(po);
                    }
                }
            }
            if (CrossUnder && _rsi.Result.HasCrossedBelow(_ema.Result, 1) && Spo.Length == 0)
            {
                var p = ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Sell", SL, TP);

                var sl = RoundPrice(_fractals.UpFractal.LastValue, TradeType.Sell);
                var tp = RoundPrice((p.Position.EntryPrice - ( _fractals.UpFractal.LastValue - p.Position.EntryPrice) ), TradeType.Sell);
                ModifyPosition(p.Position, sl, tp);

                if (Bpo.Length>0)
                {
                    foreach(var po in Bpo)
                    {
                        ClosePosition(po);
                    }
                }

            }

        }


        //Round price of a TP/SL Level
        private double RoundPrice(double price, TradeType tradeType)
        {
            var _symbolInfo = Symbols.GetSymbolInfo(SymbolName);
            var multiplier = Math.Pow(10, _symbolInfo.Digits);

            if (tradeType == TradeType.Buy)
                return Math.Ceiling(price * multiplier) / multiplier;

            return Math.Floor(price * multiplier) / multiplier;
        }
        protected override void OnStop()
        {
            
        }
    }
}