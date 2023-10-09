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
    public class TMARejection : Robot
    {
        [Parameter(DefaultValue = 20)]
        public int Period_20 { get; set; }

        [Parameter(DefaultValue = 50)]
        public int Period_50 { get; set; }

        [Parameter(DefaultValue = 200)]
        public int Period_200 { get; set; }

        [Parameter(DefaultValue = 0.1)]
        public double volume { get; set; }

        [Parameter("MA Source")]
        public DataSeries SourceMA { get; set; }

        [Parameter("TP 1 Multiplier",DefaultValue = 1)] // the profit mult of SL
        public double  Multiplier { get; set; }
        
        [Parameter("TP 1 % close", DefaultValue = 0.5)] // %close
        public double ClosePer { get; set; }

        private MovingAverage _MA20;
        private MovingAverage _MA50;
        private MovingAverage _MA200;

        Dictionary<Position, string> My_Positions = new Dictionary<Position, string>();
        
        protected override void OnStart()
        {
            //_MA20= Indicators.SimpleMovingAverage(Source, Period);
            _MA20 = Indicators.MovingAverage(SourceMA, Period_20, MovingAverageType.Simple);
            _MA50 = Indicators.MovingAverage(SourceMA, Period_50, MovingAverageType.Simple);
            _MA200 = Indicators.MovingAverage(SourceMA, Period_200, MovingAverageType.Simple);

            Positions.Closed += PositionsOnClosed;
        }
        private void PositionsOnClosed(PositionClosedEventArgs args) //check if not affected by partial close

        {
            var position = args.Position;
            My_Positions.Remove(position);
        }

        private bool LRejection(MovingAverage M1, MovingAverage M2)
        {
            if (M1.Result.Last(2) > M2.Result.Last(2) 
                && (Bars.OpenPrices.Last(2) > Bars.ClosePrices.Last(2)) 
                && (Bars.OpenPrices.Last(1) < Bars.ClosePrices.Last(1)) 
                && 
                (
                (Bars.OpenPrices.Last(2)>M1.Result.Last(2) && Bars.ClosePrices.Last(2) < M1.Result.Last(2))
                || ( (Bars.ClosePrices.Last(2) - M1.Result.Last(2)) < (Bars.OpenPrices.Last(2)-Bars.ClosePrices.Last(2)))
                )
                )
            {
                return true;
            }
            else { return false; }
        }


        private bool SRejection(MovingAverage M1, MovingAverage M2)
        {
            if (M1.Result.Last(2) < M2.Result.Last(2)
                && (Bars.OpenPrices.Last(2) < Bars.ClosePrices.Last(2))
                && (Bars.OpenPrices.Last(1) > Bars.ClosePrices.Last(1))
                &&
                (
                (Bars.OpenPrices.Last(2) < M1.Result.Last(2) && Bars.ClosePrices.Last(2) > M1.Result.Last(2))
                || ((  M1.Result.Last(2) - Bars.ClosePrices.Last(2) ) < (Bars.ClosePrices.Last(2) - Bars.OpenPrices.Last(2)))
                )
                )
            {
                return true;
            }
            else { return false; }
        }

        protected override void OnBar()
        {
            if(LRejection(_MA50,_MA200))
                {
                var SL_Pips=  Math.Abs(Convert.ToDouble(Symbol.Ask - Bars.ClosePrices.Last(2)) / Symbol.PipSize);
                var a = ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Rejection",SL_Pips+1,SL_Pips*Multiplier);
                My_Positions.Add(a.Position, "1");
            }
            if (LRejection(_MA20, _MA50))
            {
                var SL_Pips = Math.Abs(Convert.ToDouble(Symbol.Ask - Bars.ClosePrices.Last(2)) / Symbol.PipSize);
                var a = ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Rejection", SL_Pips + 1, SL_Pips * Multiplier);
                My_Positions.Add(a.Position, "1");
            }

            if (SRejection(_MA50, _MA200))
            {
                var SL_Pips = Math.Abs(Convert.ToDouble(Bars.ClosePrices.Last(2) - Symbol.Bid  ) / Symbol.PipSize);
                var a = ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Rejection", SL_Pips + 1, null);
                My_Positions.Add(a.Position, "1");
            }
            if (SRejection(_MA20, _MA50))
            {
                var SL_Pips = Math.Abs(Convert.ToDouble(Bars.ClosePrices.Last(2) - Symbol.Bid) / Symbol.PipSize);
                var a = ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Rejection", SL_Pips + 1, null);
                My_Positions.Add(a.Position, "1");
            }

            //Open rules are set, need to code the position management rules and we're good to go


        }

        protected override void OnTick()
        {
            if (My_Positions.Count !=0)
            {
                foreach (var k in My_Positions.Keys)
                {
                    var SLpips = Math.Abs(Convert.ToDouble(k.EntryPrice - k.StopLoss) / Symbol.PipSize);
                    if (k.Pips >= SLpips && My_Positions[k] == "1")
                    {
                        My_Positions[k] = "2";
                        k.ModifyVolume(k.VolumeInUnits * (1-ClosePer));
                        Print("Partial close");
                    }
                    if (k.TradeType == TradeType.Buy && My_Positions[k] == "2" && Bars.ClosePrices.Last(1) < Bars.OpenPrices.Last(1) )
                    {
                        ClosePosition(k);
                        Print("Final close");
                    }
                    if (k.TradeType == TradeType.Sell && My_Positions[k] == "2" && Bars.ClosePrices.Last(1) > Bars.OpenPrices.Last(1))
                    {
                        ClosePosition(k);
                        Print("Final close");
                    }
                }
            }
        }

        protected override void OnStop()
        {
            // Handle cBot stop here
        }
    }
}