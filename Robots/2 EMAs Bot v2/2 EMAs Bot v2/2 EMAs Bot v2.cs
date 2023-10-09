using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class DoubleSMABotinTickfixed : Robot
    {

        [Parameter("Lots", DefaultValue = 0.01)]
        public double Lots { get; set; }

        [Parameter("Risk %", DefaultValue = 10)]
        public double Risk { get; set; }

        [Parameter("EMA1 Period", DefaultValue = 14)]
        public int PeriodsEma1 { get; set; }
        [Parameter("EMA2 Period", DefaultValue = 200)]
        public int PeriodsEma2 { get; set; }



        [Parameter("Take Profit", DefaultValue = 0)]
        public int TP { get; set; }
        [Parameter("Stop Loss", DefaultValue = 50)]
        public double SL { get; set; }


        private ExponentialMovingAverage _ema1 { get; set; }
        private ExponentialMovingAverage _ema2 { get; set; }

        public double Volume;


        protected override void OnStart()
        {
            if (Risk == 0)
            {
                Volume = (Symbol.QuantityToVolumeInUnits(Lots));
                Print("Volume is " + Volume);
            }

            if (Risk != 0)
            {
                Volume = GetVolume(SL);
            }
            _ema1 = Indicators.ExponentialMovingAverage(Bars.ClosePrices, PeriodsEma1);
            _ema2 = Indicators.ExponentialMovingAverage(Bars.ClosePrices, PeriodsEma2);
        }

        protected override void OnTick()
        {


            var spos = Positions.Find("2EMA", SymbolName, TradeType.Sell);
            var lpos = Positions.Find("2EMA", SymbolName, TradeType.Buy);
            //Node1
            if (_ema1.Result.HasCrossedAbove(_ema2.Result, 1) && lpos == null)
            {
                //Print("spos = " + spos);
                //Print("lpos =" + lpos);



                ExecuteMarketOrder(TradeType.Buy, SymbolName, Volume, "2EMA", SL, TP);

            }

            if (_ema1.Result.HasCrossedBelow(_ema2.Result, 1) && spos == null)
            {

                Print("We crossedBelow");
                Print("spos = " + spos);
                Print("lpos =" + lpos);




                ExecuteMarketOrder(TradeType.Sell, SymbolName, Volume, "2EMA", SL, TP);

            }


            if (_ema1.Result.HasCrossedBelow(_ema2.Result, 1) && lpos != null)
            {
                ClosePosition(lpos);
                Print("Buy Close");
            }

            if (_ema1.Result.HasCrossedAbove(_ema2.Result, 1) && spos != null)
            {
                ClosePosition(spos);
                Print("Sell Close");
            }



        }





        protected double GetVolume(double SL)
        {

            var RawRisk = Account.Equity * Risk / 100;
            Print("RawRisk = " + RawRisk);

            var x = Math.Round((RawRisk) / (SL * Symbol.PipValue * Symbol.VolumeInUnitsMin));

            //RawRisk = Account.Equity * Risk/100

            if (Symbol.VolumeInUnitsMin > 1)
            {
                return Convert.ToInt32(x * Symbol.VolumeInUnitsMin);
            }
            else
            {
                return (x * Symbol.VolumeInUnitsMin);
            }



        }
    }
}
