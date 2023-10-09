using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using System.Net;

using System.IO;

using System.Collections;


namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class hendrixmscbot72 : Robot
    {

        [Parameter("Take Profit", DefaultValue = 50, Group = "Position management")]
        public double TP { get; set; }
        [Parameter("Stop Loss", DefaultValue = 25, Group = "Position management")]
        public double SL { get; set; }

       

        [Parameter("Risk %", DefaultValue = 2, Group = "Position management")]
        public double Balance_per { get; set; }

        [Parameter("Max spread", DefaultValue = 0.1, Group = "Position management")]
        public double MaxSpread { get; set; }

        [Parameter("Trailing Stop Trigger (pips)", DefaultValue = 50, Group = "Position management")]
        public double TrailingStopTrigger { get; set; }

        [Parameter("Trailing Stop Step (pips)", DefaultValue = 10, Group = "Position management")]
        public double TrailingStopStep { get; set; }


        [Parameter("Trade Time", DefaultValue = "08:00")]
        public string TradeTime { get; set; }
        [Parameter("Stop Time", DefaultValue = "16:00")]
        public string CancelTime { get; set; }




        [Parameter(DefaultValue = 14, Group = "EMA parameters")]
        public int Periods { get; set; }

        [Parameter("RSI Periods", DefaultValue = 14, Group = "RSIoma parameters")]

        public int RSIPeriods { get; set; }

        [Parameter("RSI Source", Group = "RSIoma parameters")]

        public DataSeries RSource { get; set; }

        [Parameter("MA Period", DefaultValue = 14, Group = "RSIoma parameters")]

        public int MAPeriods { get; set; }

        [Parameter("MA Type", DefaultValue = MovingAverageType.Simple, Group = "RSIoma parameters")]

        public MovingAverageType MaType { get; set; }

        [Parameter("MA Source", Group = " RSIoma parameters")]

        public DataSeries Source { get; set; }


        [Parameter("BB Period", DefaultValue = 20, Group = " SMI Parameters")]
        public int length { get; set; }
        [Parameter("BB Deviation", DefaultValue = 2, Group = " SMI Parameters")]
        public double mult { get; set; }
        [Parameter("KC Period", DefaultValue = 20, Group = " SMI Parameters")]
        public int lengthKC { get; set; }
        [Parameter("KC Deviation", DefaultValue = 1.5, Group = " SMI Parameters")]
        public double multKC { get; set; }

        [Parameter("RSI Counter", DefaultValue = 5, Group = " Counters")]
        public int CrossOverPeriod { get; set; }

        [Parameter("SMI Counter", DefaultValue = 3, Group = " Counters")]
        public int DarkBarPeriod { get; set; }


        private bool DarkBarSignalR;
        public bool DarkBarSignalG;
        private int DarkBarCount;


        private ExponentialMovingAverage _ema;
        private Rsioma _rsioma;
        private Smi _smi;

        private bool CrossOver;
       private int CrossOverCount;

        private bool CrossUnder;
        private int CrossUnderPeriod;
        private int CrossUnderCount;

        List<double> Redswitch = new List<double>();
        List<double> Greenswitch = new List<double>();
        private bool GreenTrigger;
        private bool RedTrigger;

        private int StartHour;
        private int StartMinute;
        private int StopHour;
        private int StopMinute;



        private bool CheckTime()
        {
            var startTime = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StartHour, StartMinute, 0);
            var stopTime = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StopHour, StopMinute, 0);

            if (Server.TimeInUtc > startTime && Server.TimeInUtc < stopTime)
            { return true; }

            else { return false; }
        }

        protected override void OnStart()
        {
            var startTime = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StartHour, StartMinute, 0);
          
            

            string[] parts = TradeTime.Split(':');

            StartHour= int.Parse(parts[0]);
            StartMinute = int.Parse(parts[1]);

            string[] partss = CancelTime.Split(':');
            StopHour = int.Parse(partss[0]);
            StopMinute = int.Parse(partss[1]);

            if (!IsBacktesting && Server.TimeInUtc.DayOfYear > 45)
            {
                Stop();
            }
            _ema = Indicators.ExponentialMovingAverage(Bars.ClosePrices, Periods);
            _rsioma = Indicators.GetIndicator<Rsioma>(RSIPeriods, RSource, MAPeriods, MaType, Source);
            _smi = Indicators.GetIndicator<Smi>(length, mult, lengthKC, multKC);

            CrossOver = false;
           
            CrossOverCount = 0;

            CrossUnder = false;
            CrossUnderPeriod = 5;
            CrossUnderCount = 0;

            GreenTrigger = false;
            RedTrigger = false;


               DarkBarSignalR = false;
            DarkBarSignalG = false;
         DarkBarCount = 0;
    }

        private double GetMaxGreen()
        {

            var lastgreen = 0.0;
            var x = 0;

            while (_smi.BullExp.Last(x) > 0 || _smi.BullCon.Last(x) > 0)
            {



                if (_smi.BullExp.Last(x) > 0 && _smi.BullExp.Last(x) > lastgreen)
                {
                    lastgreen = _smi.BullExp.Last(x);
                }


                x++;


            }

            return lastgreen;
        }

        private double GetMinRed()
        {

            var lastred = 0.0;
            var x = 0;

            while (_smi.BearExp.Last(x) < 0 || _smi.BearCon.Last(x) < 0)
            {



                if (_smi.BearExp.Last(x) < 0 && _smi.BearExp.Last(x) < lastred)
                {
                    lastred = _smi.BearExp.Last(x);
                }


                x++;


            }

            return lastred;
        }

        private static double GetPriceAdjustmentByTradeType(TradeType tradeType, double priceDifference)
        {
            if (tradeType == TradeType.Buy)
                return priceDifference;

            return -priceDifference;
        }
        protected double GetVolume(double SL)
        {

            var maxAmountRisked = Account.Equity * (Balance_per / 100);
           
            return Symbol.NormalizeVolumeInUnits(maxAmountRisked / (SL * Symbol.PipValue), RoundingMode.Down);

           

        }


        private double RoundPrice(double price, TradeType tradeType)
        {
            var _symbolInfo = Symbols.GetSymbolInfo(SymbolName);
            var multiplier = Math.Pow(10, _symbolInfo.Digits);

            if (tradeType == TradeType.Buy)
                return Math.Ceiling(price * multiplier) / multiplier;

            return Math.Floor(price * multiplier) / multiplier;
        }

        private void SetTrailingStop()
        {
            var sellPositions = Positions.FindAll("Sell", SymbolName);

            foreach (Position position in sellPositions)
            {
                double distance = position.EntryPrice - Symbol.Ask;

                if (distance < TrailingStopTrigger * Symbol.PipSize)
                    continue;

                double newStopLossPrice = Symbol.Ask + TrailingStopStep * Symbol.PipSize;

                if (position.StopLoss == null || newStopLossPrice < position.StopLoss)
                {
                    ModifyPosition(position, newStopLossPrice, position.TakeProfit);
                }
            }

            var buyPositions = Positions.FindAll("Buy", SymbolName);

            foreach (Position position in buyPositions)
            {
                double distance = Symbol.Bid - position.EntryPrice;

                if (distance < TrailingStopTrigger * Symbol.PipSize)
                    continue;

                double newStopLossPrice = Symbol.Bid - TrailingStopStep * Symbol.PipSize;
                if (position.StopLoss == null || newStopLossPrice > position.StopLoss)
                {
                    ModifyPosition(position, newStopLossPrice, position.TakeProfit);
                }
            }
        }
        protected override void OnTick()
        {


            SetTrailingStop();



        }



        protected override void OnBar()
        {

            if (isDarkRed() && Math.Abs(_smi.Result.Last(1)) < GetMaxGreen() && !DarkBarSignalR)
            {
                DarkBarSignalR = true;
                DarkBarCount = 0;
            }

            if ( isDarkGreen() && Math.Abs(GetMinRed()) > _smi.Result.Last(1) && !DarkBarSignalG)
            {
                DarkBarSignalG = true;
                DarkBarCount = 0;
            }
            if (!isDarkRed() && !isDarkGreen())
            {
                DarkBarCount++;
                if (DarkBarCount == DarkBarPeriod)
                {
                    DarkBarSignalR = false;
                    DarkBarSignalG = false;
                }
            }


            if (_rsioma.Rsi.Last(1) > _rsioma.Trigger.Last(1) && _rsioma.Rsi.Last(2) < _rsioma.Trigger.Last(2))
            {
                CrossUnder = false;
                CrossOver = true;
                CrossOverCount = 0;


            }


            if (_rsioma.Rsi.Last(1) < _rsioma.Trigger.Last(1) && _rsioma.Rsi.Last(2) > _rsioma.Trigger.Last(2))
            {
                CrossUnder = true;
                CrossOver = false;
                CrossUnderCount = 0;


            }


            if (CrossOver)
            {
                CrossUnder = false;
                CrossOverCount++;
                if (CrossOverCount == CrossOverPeriod)
                {
                    CrossOver = false;
                }
            }

            if (CrossUnder)
            {
                CrossOver = false;
                CrossUnderCount++;
                if (CrossUnderCount == CrossUnderPeriod)
                {
                    CrossUnder = false;
                }
            }


           
            var LR = _smi.BearExp.LastValue; 
            var DR = _smi.BearCon.LastValue; 
            var LG= _smi.BullExp.LastValue; 
            var DG  = _smi.BullCon.LastValue; 

           if (GreenTrigger  )
            {
                if (LR != Greenswitch[0] || DR != Greenswitch[1] || LG != Greenswitch[2]) {
                    GreenTrigger = false;
                 
                   
                    Greenswitch.Clear();
                } }

           if (RedTrigger)
            {
                
                if (LR != Redswitch[0] || DG != Redswitch[1] || LG != Redswitch[2])
                {
                    RedTrigger = false;
                    Redswitch.Clear();
                }
            }

         



        
            var Bpo = Positions.FindAll("Buy", SymbolName);

           

            if (isDarkRed() && !RedTrigger && Symbol.Spread < MaxSpread && CheckTime()
            && CrossOver 
            && Bars.ClosePrices.Last(1) > _ema.Result.Last(1) &&  DarkBarSignalR && Bpo.Length == 0
            )

            {
              
                ExecuteMarketOrder(TradeType.Buy, SymbolName, GetVolume(SL), "Buy", SL, TP);

                CrossUnder = false;

                Redswitch.Clear();

               
                 Redswitch.Add(_smi.BearExp.LastValue);
                  Redswitch.Add(_smi.BullCon.LastValue);
                  Redswitch.Add(_smi.BullExp.LastValue);


                
                 RedTrigger = true;

            }



            var Spo = Positions.FindAll("Sell", SymbolName);
            if (isDarkGreen()&& !GreenTrigger && Symbol.Spread<MaxSpread && CheckTime()
            && CrossUnder && _rsioma.Rsi.LastValue <  _rsioma.Trigger.LastValue
            && Bars.ClosePrices.Last(1) < _ema.Result.Last(1) && DarkBarSignalG  && Spo.Length == 0
            )

            {
              
                ExecuteMarketOrder(TradeType.Sell, SymbolName, GetVolume( SL), "Sell", SL, TP);
                CrossUnder = false;

                Greenswitch.Clear();

                Greenswitch.Add(_smi.BearExp.LastValue); 
               Greenswitch.Add(_smi.BearCon.LastValue); 
                Greenswitch.Add( _smi.BullExp.LastValue); 



             

              GreenTrigger = true;
            }




        }
        private bool isDarkRed()
        {
            if (_smi.Result.Last(1) < 0 && _smi.Result.Last(1) < 0 && _smi.Result.Last(1) > _smi.Result.Last(2))
            {

                return true;
            }
            else { return false; }
        }
        private bool isDarkGreen()
        {
            if (_smi.Result.Last(1) > 0 && _smi.Result.Last(2) > 0 && _smi.Result.Last(1) < _smi.Result.Last(2))
            {

                return true;
            }
            else { return false; }
        }

        protected override void OnStop()
        {

        }
    }
}