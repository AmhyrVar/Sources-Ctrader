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

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class hendrixmscbot : Robot
    {

        [Parameter("Take Profit", DefaultValue = 50, Group = "Position management")]
        public double TP { get; set; }
        [Parameter("Stop Loss", DefaultValue = 25, Group = "Position management")]
        public double SL { get; set; }

        [Parameter("Maximum spread", DefaultValue = 25, Group = "Position management")]
        public double Spread { get; set; }

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





        private ExponentialMovingAverage _ema;
        private Rsioma _rsioma;
        private Smi _smi;

        private bool CrossOver;
        private int CrossOverPeriod;
        private int CrossOverCount;

        private bool CrossUnder;
        private int CrossUnderPeriod;
        private int CrossUnderCount;

        protected override void OnStart()
        {

            if (!IsBacktesting && Server.TimeInUtc.DayOfYear > 45)
            {
                Stop();
            }
            _ema = Indicators.ExponentialMovingAverage(Bars.ClosePrices, Periods);
            _rsioma = Indicators.GetIndicator<Rsioma>(RSIPeriods, RSource, MAPeriods, MaType, Source);
            _smi = Indicators.GetIndicator<Smi>(length, mult, lengthKC, multKC);

            CrossOver = false;
            CrossOverPeriod = 5;
            CrossOverCount = 0;

            CrossUnder = false;
            CrossUnderPeriod = 5;
            CrossUnderCount = 0;

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

        protected override void OnBar()
        {
            //buy zone
            //Condition 1 : dark red bar
            //Condition 2 : RSI crossover Trigger last 5 bars
            //Print("Recap, isGreen " + isDarkGreen() + " Cross under " + CrossUnder + " Under Ema " + (Bars.ClosePrices.Last(1) < _ema.Result.Last(1)) + " Abs stuff " + (Math.Abs(GetMinRed()) > GetMaxGreen()));

            //Print("Bar Balise DR "+_smi.BearCon.Last(1)  + " LR " + _smi.BearExp.Last(1) + " DG " + _smi.BullCon.Last(1) + " LG "+ _smi.BullExp.Last(1));

            if (isDarkGreen() && CrossUnder && (Bars.ClosePrices.Last(1) < _ema.Result.Last(1)))
            {
              //  Print("Sell MinRed should be big " + Math.Abs(GetMinRed()) + " Max Green " + GetMaxGreen());
               
            }

            if (isDarkRed() && CrossOver && (Bars.ClosePrices.Last(1) > _ema.Result.Last(1)))
            {
              //  Print("Buy MinRed should be smol " + Math.Abs(GetMinRed()) + " Max Green " + GetMaxGreen());

            }

            if (CrossOver)
            {

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


            if (_rsioma.Rsi.HasCrossedAbove(_rsioma.Trigger, 1))
            {
                CrossUnder = false;
                CrossOver = true;
                CrossOverCount = 0;
            }


            if (_rsioma.Rsi.HasCrossedBelow(_rsioma.Trigger, 1))
            {
                CrossUnder = true;
                CrossOver = false;
                CrossUnderCount = 0;
            }

            var Bpo = Positions.FindAll("Buy", SymbolName);
            if (isDarkRed()
            && CrossOver
            && Bars.ClosePrices.Last(1) > _ema.Result.Last(1) && Math.Abs(GetMinRed()) < GetMaxGreen() && Bpo.Length == 0
            )

            {
                ExecuteMarketOrder(TradeType.Buy, SymbolName, 1000, "Buy", 25, 50);
                //Print("Buy red < green : RED" + Math.Abs(GetMinRed()) + " ---GREEN " + GetMaxGreen());
                //Print("Buy Balise DR " + _smi.BearCon.Last(1) + " LR " + _smi.BearExp.Last(1) + " DG " + _smi.BullCon.Last(1) + " LG " + _smi.BullExp.Last(1));
                CrossUnder = false;

            }



            var Spo = Positions.FindAll("Sell", SymbolName);
            if (isDarkGreen()
            && CrossUnder //Convert to crossunder
            && Bars.ClosePrices.Last(1) < _ema.Result.Last(1) && Math.Abs(GetMinRed()) > GetMaxGreen() && Spo.Length == 0
            )

            {
               // Print("Sell red > green : RED" + Math.Abs(GetMinRed()) + " ---GREEN " + GetMaxGreen());
                ExecuteMarketOrder(TradeType.Sell, SymbolName, 1000, "Sell", 25, 50);
                CrossUnder = false;


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