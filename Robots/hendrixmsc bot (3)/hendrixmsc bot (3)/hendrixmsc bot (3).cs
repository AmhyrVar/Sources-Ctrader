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
    public class hendrixmscbot3 : Robot
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

        List<double> Redswitch = new List<double>();
        List<double> Greenswitch = new List<double>();
        private bool GreenTrigger;
        private bool RedTrigger;


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

            GreenTrigger = false;
            RedTrigger = false; 
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

            var LR = _smi.BearExp.LastValue; //LR
            var DR = _smi.BearCon.LastValue; //DR
            var LG= _smi.BullExp.LastValue; //LG
            var DG  = _smi.BullCon.LastValue; //DG

           if (GreenTrigger  )
            {
                Print("Green trigger called ");
                if (LR != Greenswitch[0] || DR != Greenswitch[1] || LG != Greenswitch[2]) {
                    GreenTrigger = false;
                    Print($"Called LR {LR}DR {DR} LG {LG}");
                    Print($"OG vals are LR {Greenswitch[0]}DR {Greenswitch[1]} LG {Greenswitch[2]}");//$"Hello {name}";
                    Greenswitch.Clear();
                } }

          /*  if (RedTrigger)
            {
                Print("Red trigger called ");
                if (LR != Redswitch[0] || DG != Redswitch[1] || LG != Redswitch[2])
                {
                    RedTrigger = false;
                    Redswitch.Clear();
                }
            }*/


            //buy zone
            //Condition 1 : dark red bar
            //Condition 2 : RSI crossover Trigger last 5 bars
          

         

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
            if (isDarkRed() && !RedTrigger 
            && CrossOver &&  _rsioma.Rsi.LastValue >  _rsioma.Trigger.LastValue
            && Bars.ClosePrices.Last(1) > _ema.Result.Last(1) && Math.Abs(GetMinRed()) < GetMaxGreen() && Bpo.Length == 0
            )

            {
                ExecuteMarketOrder(TradeType.Buy, SymbolName, 1000, "Buy", 25, 50);
            
                CrossUnder = false;

                Redswitch.Clear();

                //  Print("Red switchsss " + Redswitch);
                 Redswitch.Add(_smi.BearExp.LastValue);
                  Redswitch.Add(_smi.BullCon.LastValue);
                  Redswitch.Add(_smi.BullExp.LastValue);


                  Print("Red switch " + (Redswitch[0]== _smi.BearExp.LastValue));
                 RedTrigger = true;

            }



            var Spo = Positions.FindAll("Sell", SymbolName);
            if (isDarkGreen()&& !GreenTrigger
            && CrossUnder && _rsioma.Rsi.LastValue <  _rsioma.Trigger.LastValue//_rsioma.Rsi.HasCrossedAbove(_rsioma.Trigger
            && Bars.ClosePrices.Last(1) < _ema.Result.Last(1) && Math.Abs(GetMinRed()) > GetMaxGreen() && Spo.Length == 0
            )

            {
              
                ExecuteMarketOrder(TradeType.Sell, SymbolName, 1000, "Sell", 25, 50);
                CrossUnder = false;

                Greenswitch.Clear();

                Greenswitch.Add(_smi.BearExp.LastValue); //LR
               Greenswitch.Add(_smi.BearCon.LastValue); //DR
                Greenswitch.Add( _smi.BullExp.LastValue); //LG



               Print("Green switch " + (Greenswitch[0] == _smi.BearExp.LastValue));

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