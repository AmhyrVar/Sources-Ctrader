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
    public class waqaarhussain2 : Robot
    {
        //+13 mins
        [Parameter("RSI Source", Group = "RSI parameters")]
        public DataSeries RsiSourceSeries { get; set; }
        [Parameter("RSI Periods", DefaultValue = 14, Group = "RSI parameters")]
        public int RsiPeriods { get; set; }

 


        [Parameter("EMA Periods", DefaultValue = 14, Group = "EMA parameters")]
        public int EMAPeriods { get; set; }
        [Parameter("EMA Source", Group = "EMA parameters")]
        public DataSeries EMASourceSeries { get; set; }


        [Parameter("Fractals Periods", DefaultValue = 5, Group = "Fractals parameters")]
        public int FracPeriods { get; set; }


        [Parameter("Lot size", DefaultValue = 0.01, Group = "Position management")]
        public double volume { get; set; }
        [Parameter("Use Take Profit", DefaultValue = false, Group = "Position management")]
        public bool Use_TP { get; set; }

        [Parameter("Take Profit", DefaultValue = 50, Group = "Position management")]
        public double TP { get; set; }
        [Parameter("Use Stop Loss", DefaultValue = false, Group = "Position management")]
        public bool Use_SL { get; set; }
        [Parameter("Stop Loss", DefaultValue = 25, Group = "Position management")]
        public double SL { get; set; }



        private RelativeStrengthIndex _rsi;
        private Fractals _fractals;
        private ExponentialMovingAverage _ema;



      
       
       
        protected override void OnStart()
        {

            _rsi = Indicators.RelativeStrengthIndex(RsiSourceSeries, RsiPeriods);
           _fractals = Indicators.Fractals(FracPeriods);
             _ema = Indicators.ExponentialMovingAverage(EMASourceSeries, EMAPeriods);

          

         
        }

        protected override void OnBar()
        {
           

            var Bpo = Positions.FindAll("Buy", SymbolName);
            var Spo = Positions.FindAll("Sell", SymbolName);


           

                if ( BuyCond() && Bpo.Length==0)
            {
                var p = ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Buy", getSL(), getTP());

                var DwnFrac = 0.0;
                foreach (int i in Enumerable.Range(0, _fractals.DownFractal.Count))
                {
                    double fractalValue = _fractals.DownFractal.Last(i);

                    if (fractalValue < p.Position.EntryPrice)
                    {
                       DwnFrac = RoundPrice(fractalValue, TradeType.Buy);
                        break;
                    }
                }


                var sl = RoundPrice(DwnFrac, TradeType.Buy);
                var tp = RoundPrice(((p.Position.EntryPrice - DwnFrac) + p.Position.EntryPrice),TradeType.Buy);
                Print($"Buying at { p.Position.EntryPrice} TP {tp} SL {sl} DownFrac lastvalue { DwnFrac}");
                if (!Use_TP && !Use_SL)
                { ModifyPosition(p.Position, sl, tp); }
                if (Use_TP && !Use_SL)
                { ModifyPosition(p.Position, sl, p.Position.TakeProfit); }

                if (Spo.Length > 0)
                {
                    foreach (var po in Spo)
                    {
                        ClosePosition(po);
                    }
                }
            }
            if (SellCond() && Spo.Length == 0)
            {
                

                var p = ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Sell", getSL(), getTP());

                var UpFrac = 0.0;
                foreach (int i in Enumerable.Range(0, _fractals.UpFractal.Count))
                {
                    double fractalValue = _fractals.UpFractal.Last(i);

                    if (fractalValue > p.Position.EntryPrice)
                    {
                        UpFrac = RoundPrice(fractalValue, TradeType.Buy);
                        break;
                    }
                }
                //itérer pour trouver le premier upfrac au dessus du prix de vente

                var sl = RoundPrice(UpFrac, TradeType.Sell);
                var tp = RoundPrice((p.Position.EntryPrice - (UpFrac - p.Position.EntryPrice) ), TradeType.Sell);

                Print($"Selling at {p.Position.EntryPrice} TP {tp} SL {sl} UPfractal lastvalue {UpFrac}");
               
                if (!Use_TP && !Use_SL)
                { ModifyPosition(p.Position, sl, tp); }
                if (Use_TP && !Use_SL)
                { ModifyPosition(p.Position, sl, p.Position.TakeProfit); }

                if (Bpo.Length>0)
                {
                    foreach(var po in Bpo)
                    {
                        ClosePosition(po);
                    }
                }

            }

        }
        private double getTP()
        {
            if (Use_TP)
            { return TP; }
            else { return double.NaN; }
        }
        private double getSL()
        {
            if (Use_SL)
            { return SL; }
            else { return double.NaN; }
        }

        private double RoundPrice(double price, TradeType tradeType)
        {
            var _symbolInfo = Symbols.GetSymbolInfo(SymbolName);
            var multiplier = Math.Pow(10, _symbolInfo.Digits);

            if (tradeType == TradeType.Buy)
                return Math.Ceiling(price * multiplier) / multiplier;

            return Math.Floor(price * multiplier) / multiplier;
        }

        private bool BuyCond()
        {   
            if (_rsi.Result.Last(2) < 30 && _rsi.Result.Last(1) > 30 && Bars.ClosePrices.Last(1) > _ema.Result.Last(1))
            { return true; }
            else { return false; }
        }

        private bool SellCond()
        {
            if (_rsi.Result.Last(2) > 70 && _rsi.Result.Last(1) < 70 && Bars.ClosePrices.Last(1) < _ema.Result.Last(1))
            { return true; }
            else { return false; }
        }
        protected override void OnStop()
        {
            
        }
    }
}