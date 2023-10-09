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
    public class Potspy3 : Robot
    {

        [Parameter("Entry Offset Pips", DefaultValue = 2, Group = "Entry Params")]
        public double entryPriceOffset { get; set; }

        [Parameter("Volume", DefaultValue = 0.01, Group = "Entry Params")]
        public double volume { get; set; }

        //Initialiser 3 EMAs
        #region EMA Params
        [Parameter("EMA1 Period" ,DefaultValue = 20, Group = "EMA parameters")]
        public int Periods1 { get; set; }
        [Parameter("EMA2 Period", DefaultValue = 40, Group = "EMA parameters")]
        public int Periods2 { get; set; }
        [Parameter("EMA3 Period", DefaultValue = 60, Group = "EMA parameters")]
        public int Periods3 { get; set; }

        [Parameter("EMAs Source",  Group = "EMA parameters")]
        public DataSeries Ema_Source { get; set; }
        
        private ExponentialMovingAverage _ema1;
        private ExponentialMovingAverage _ema2;
        private ExponentialMovingAverage _ema3;
        #endregion

        #region TimeTrade
        [Parameter("Start Trade Time", DefaultValue = "08:00")]
        public string TradeTime { get; set; }
        [Parameter("Stop Trade Time", DefaultValue = "16:00")]
        public string CancelTime { get; set; }



        private int StartHour;
        private int StartMinute;
        private int StopHour;
        private int StopMinute;


        #endregion

        #region ATRParams
        private AverageTrueRange _aTR_SL;
        private AverageTrueRange _aTR_TP;

        [Parameter("ATR Periods SL", DefaultValue = 14 ,Group = "ATR parameters")]
        public int Atr_Periods_SL { get; set; }
        [Parameter("ATR Periods TP", DefaultValue = 16, Group = "ATR parameters")]
        public int Atr_Periods_TP { get; set; }

        [Parameter("ATR Periods", DefaultValue = MovingAverageType.Simple, Group = "ATR parameters")]
        public MovingAverageType AtrMaType { get; set; }


        #endregion

        #region SL params
        [Parameter("ATR SL Multiplier", DefaultValue = 2.0, Group = "SL parameters")]
        public double AtrSlMult { get; set; }


      

        #endregion

        #region TP Params

       
        [Parameter("ATR TP Multiplier", DefaultValue = 1.7, Group = "TP parameters")]
        public double AtrTPMult { get; set; }

        [Parameter("R TP Multiplier", DefaultValue = 2, Group = "TP parameters")]
        public double RTPMult { get; set; }

      

       


     



        Dictionary<Position, double> My_Positions = new Dictionary<Position, double>(); // quand les positions s'ouvrent et qu'elles ont un TP et le bool est true on les strip du tp et on le met dans le dict
        List<Position> PartiallyClosed = new List<Position>();
        #endregion

        protected override void OnStart()
        {

            #region Initialize Emas & ATR
            _ema1 = Indicators.ExponentialMovingAverage(Ema_Source, Periods1);
            _ema2 = Indicators.ExponentialMovingAverage(Ema_Source, Periods2);
            _ema3 = Indicators.ExponentialMovingAverage(Ema_Source, Periods3);
            _aTR_SL = Indicators.AverageTrueRange(Atr_Periods_SL, AtrMaType);
            _aTR_TP = Indicators.AverageTrueRange(Atr_Periods_TP, AtrMaType);

            #endregion

            #region TimeSpan trading Params
            var startTime = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StartHour, StartMinute, 0);



            string[] parts = TradeTime.Split(':');

            StartHour = int.Parse(parts[0]);
            StartMinute = int.Parse(parts[1]);

            string[] partss = CancelTime.Split(':');
            StopHour = int.Parse(partss[0]);
            StopMinute = int.Parse(partss[1]);
            #endregion


            #region Event handling

            Positions.Closed += PositionsOnClosed;
            Positions.Opened += PositionsOnOpened;

            #endregion

        }
        private void PositionsOnClosed(PositionClosedEventArgs args) //check if not affected by partial close

        {
            var position = args.Position;
            My_Positions.Remove(position);
        }
        private void PositionsOnOpened(PositionOpenedEventArgs args) //check if not affected by partial close

        {
          //  var position = args.Position;
          //  My_Positions.Add(position, Math.Abs(Convert.ToDouble(position.EntryPrice - position.TakeProfit)) / Symbol.PipSize);
          //  ModifyPosition(position, position.StopLoss, null);
        }


        protected override void OnTick()
        {
         
       
         
        
        }

        protected override void OnBar()
        {
            var stopTimes = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StopHour, StopMinute, 0);
            if (Server.TimeInUtc >= stopTimes)
            {
                var pends = PendingOrders.Where(order => order.SymbolName == SymbolName && order.Label.Equals("Pend", System.StringComparison.OrdinalIgnoreCase));
                var po = Positions.FindAll("Pend", SymbolName);
                if (pends.Count() == 0)
                {
                    foreach (var posss in pends)
                    {
                        CancelPendingOrder(posss);
                    }
                }
                if (po.Length == 0)
                {
                    foreach(var pos in po)
                    {
                        ClosePosition(pos);
                    }
                }
            }

            if (!IsBacktesting)
            { Stop(); }






            if (CheckTime() && NoRunPo()) // Trade only on checktime 
            {
                //long conditions 
                if (CheckEmaLong() && CandleTouchedEma2(TradeType.Buy))
                {
                    //Long

                    //DESTROy
                    
                   

                    //double entryPrice = Symbol.Ask - Symbol.PipSize * entryPriceOffset; // calculate the entry price based on the current Ask price and the offset
                    double EntryLevel = Symbol.Ask - Symbol.PipSize * entryPriceOffset;
                    Print($"entry {EntryLevel} ask {Symbol.Ask}");

                    if (Symbol.Ask < EntryLevel)
                    {
                        ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume),  "Pend", GetSlPips(TradeType.Buy, Symbol.Ask) , GetTP());
                        Print($"Last bar low {Bars.LowPrices.Last(1)} Last bar close {Bars.ClosePrices.Last(1)} EMA2 {_ema2.Result.Last(1)} Entry level {EntryLevel}");
                    }

                    if (Symbol.Ask >= EntryLevel)
                    {
                         PlaceLimitOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), RoundPrice(EntryLevel, TradeType.Buy), "Pend", GetSlPips(TradeType.Buy, RoundPrice(EntryLevel, TradeType.Buy)), GetTP());
                        Print($"Last bar low {Bars.LowPrices.Last(1)} Last bar close {Bars.ClosePrices.Last(1)} EMA2 {_ema2.Result.Last(1)} Entry level {EntryLevel}");

                    }
                }

                //short conditions
                if (CheckEmaShort() && CandleTouchedEma2(TradeType.Sell))
                {
                    //Short
                }
            }
        }

        private double GetTP()
        {
            var TpPips = (_aTR_TP.Result.Last(1) / Symbol.PipValue) * AtrTPMult * RTPMult;
            Print($"TP is {TpPips}");
           
                return TpPips;
            }  
        

            private double RoundPrice(double price, TradeType tradeType)
            {
                var _symbolInfo = Symbols.GetSymbolInfo(SymbolName);
                var multiplier = Math.Pow(10, _symbolInfo.Digits);

                if (tradeType == TradeType.Buy)
                    return Math.Ceiling(price * multiplier) / multiplier;

                return Math.Floor(price * multiplier) / multiplier;
            }

            private bool CheckEmaLong()
        {   
            if (_ema1.Result.Last(1) > _ema2.Result.Last(1) && _ema2.Result.Last(1) > _ema3.Result.Last(1))
            { return true; }            
            else
            {
                return false;
            }
        }
        private bool NoRunPo()
        {
           
                var pends = PendingOrders.Where(order =>  order.SymbolName == SymbolName && order.Label.Equals("Pend", System.StringComparison.OrdinalIgnoreCase));
            var po = Positions.FindAll("Pend", SymbolName);
            if (pends.Count() == 0 && po.Length == 0)
            { return true; }
            else { return false; }
        }
        private bool CheckEmaShort()
        {
            if (_ema1.Result.Last(1) < _ema2.Result.Last(1) && _ema2.Result.Last(1) < _ema3.Result.Last(1))
            { return true; }
            else
            {
                return false;
            }
        }

        private bool CheckTime()
        {
            var startTime = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StartHour, StartMinute, 0);
            var stopTime = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StopHour, StopMinute, 0);

            if (Server.TimeInUtc > startTime && Server.TimeInUtc < stopTime)
            { return true; }

            else { return false; }
        }

        private bool CandleTouchedEma2(TradeType tt)
        { 
        if (tt == TradeType.Buy && Bars.LowPrices.Last(1) < _ema2.Result.Last(1) && Bars.ClosePrices.Last(1) > _ema2.Result.Last(1))
            { return true;}
            if (tt == TradeType.Sell && Bars.HighPrices.Last(1) > _ema2.Result.Last(1) && Bars.ClosePrices.Last(1) < _ema2.Result.Last(1))
            { return true; }
            else { return false; }
        }

        private double GetSlPips(TradeType TT,double RefPrice )
        {
            var SlMult = (_aTR_SL.Result.Last(1) / Symbol.PipValue) * AtrSlMult;

            if (TT == TradeType.Buy)
            {
               
                
                    return (RefPrice - (Bars.ClosePrices.Last(1) - (_aTR_SL.Result.Last(1) * AtrSlMult)))/ Symbol.PipValue;
                
            }

            else
            {
              
                    return ((Bars.ClosePrices.Last(1) + (_aTR_SL.Result.Last(1) * AtrSlMult)) - RefPrice)/ Symbol.PipValue;
                
            }


        }
    }
}