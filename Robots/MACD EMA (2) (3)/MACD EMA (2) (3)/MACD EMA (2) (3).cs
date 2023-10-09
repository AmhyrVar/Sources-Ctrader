using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MACDEMA23 : Robot
    {

        [Parameter()]
        public DataSeries Source { get; set; }

        public MacdCrossOver _macd;
        public ExponentialMovingAverage _ema;
        

        
        [Parameter("Lots", DefaultValue = 0.01)]
        public double Lots { get; set; }

        [Parameter("Risk %", DefaultValue = 10)]
        public double Risk { get; set; }
        
         [Parameter("Take Profit", DefaultValue = 0)]
        public int TP { get; set; }
        [Parameter("Stop Loss", DefaultValue = 50)]
        public double SL { get; set; }
        
         [Parameter("MA Period", DefaultValue = 200)]
        public int MAPeriod { get; set; }
        
        [Parameter("MACD Period", DefaultValue = 9)]
        public int Period { get; set; }

        [Parameter("Long Cycle", DefaultValue = 26)]
        public int LongCycle { get; set; }

        [Parameter("Short Cycle", DefaultValue = 12)]
        public int ShortCycle { get; set; }
        
        
        
        public double Volume;
        
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
            
            _ema = Indicators.ExponentialMovingAverage(Source, MAPeriod);

           
            _macd = Indicators.MacdCrossOver(LongCycle, ShortCycle, Period);
        }

        protected override void OnBar()
        {
        
        var po = Positions.FindAll("MACDMA",SymbolName);
            if (po.Length ==0 && _macd.MACD.LastValue > 0 && Bars.ClosePrices.Last(1) < _ema.Result.Last(1) && _macd.MACD.HasCrossedBelow(_macd.Signal, 1))
            {
                
               
                
                

                //calculate swing high with fractal bands
                ExecuteMarketOrder(TradeType.Sell, SymbolName, Volume, "MACDMA",SL,TP);
            }
            
            if (po.Length ==0 && _macd.MACD.LastValue < 0 && Bars.ClosePrices.Last(1) > _ema.Result.Last(1) && _macd.MACD.HasCrossedAbove(_macd.Signal, 1))
            {
                
               
                
                

                //calculate swing high with fractal bands
                ExecuteMarketOrder(TradeType.Buy, SymbolName, Volume, "MACDMA",SL,TP);
            }
            
            
            //buy condition
            



        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}

