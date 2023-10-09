using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class RENKO_TREND_TRADER : Robot
    {

        [Parameter(DefaultValue = "RENKO_TREND_TRADER")]
        public string cBotLabel { get; set; }

        [Parameter("Currency pair", DefaultValue = "EURUSD")]
        public string TradeSymbol { get; set; }

        [Parameter("Lot Size", DefaultValue = 0.5, MinValue = 0.01, Step = 0.01)]
        public double LotSize { get; set; }

        [Parameter("Trade entry - Use green/red candle", DefaultValue = true)]
        public bool UseCandle { get; set; }

        [Parameter("Trade entry - Use candle above/below MAs", DefaultValue = true)]
        public bool UseCandleMAs { get; set; }

        [Parameter("MA01 Type", DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType MAType1 { get; set; }

        [Parameter("MA01 Period", DefaultValue = 16)]
        public int MAPeriod1 { get; set; }

        [Parameter("MA02 Type", DefaultValue = MovingAverageType.Exponential)]
        public MovingAverageType MAType2 { get; set; }

        [Parameter("MA02 Period", DefaultValue = 8)]
        public int MAPeriod2 { get; set; }

        [Parameter("Trade entry - Use ADX", DefaultValue = true)]
        public bool UseADX { get; set; }

        [Parameter("ADX Period", DefaultValue = 6)]
        public int ADXPeriod { get; set; }

        [Parameter("ADX Level", DefaultValue = 32)]
        public int ADXLevel { get; set; }

        [Parameter("Multiply trades", DefaultValue = false)]
        public bool UseMT { get; set; }

        [Parameter("StopLoss in pips", DefaultValue = 40.0)]
        public double StopLoss { get; set; }

        [Parameter("TakeProfit in pips", DefaultValue = 40.0)]
        public double TakeProfit { get; set; }

        [Parameter("Run in backtesting mode", DefaultValue = false)]
        public bool UseBacktesting { get; set; }

        [Parameter("Renko brick in pips", DefaultValue = 10)]
        public double RenkoPips { get; set; }

        [Parameter("Renko bricks to show", DefaultValue = 100)]
        public int BricksToShow { get; set; }

        Symbol CurrentSymbol;
        private MovingAverage MA1;
        private MovingAverage MA2;
        //private DirectionalMovementSystem DMS;
        private MarketSeries TMS;
        private Renko renko;
        private DMS DMS;
        private double renko_bar_close;

        protected override void OnStart()
        {
            //check symbol
            CurrentSymbol = Symbols.GetSymbol(TradeSymbol);

            if (CurrentSymbol == null)
            {
                Print("Currency pair is not supported,please check!");
                OnStop();
            }
            if (UseBacktesting == false)
            {
                TMS = MarketData.GetSeries(TradeSymbol, MarketSeries.TimeFrame);
                MA1 = Indicators.MovingAverage(TMS.Close, MAPeriod1, MAType1);
                MA2 = Indicators.MovingAverage(TMS.Close, MAPeriod2, MAType2);
                DMS = Indicators.GetIndicator<DMS>(TMS.High, TMS.Low, TMS.Close, ADXPeriod, MovingAverageType.WilderSmoothing);
            }
            else
            {
                renko = Indicators.GetIndicator<Renko>(RenkoPips, BricksToShow, 3, "SeaGreen", "Tomato");
                renko_bar_close = renko.Close.Last(1);
                MA1 = Indicators.MovingAverage(renko.Close, MAPeriod1, MAType1);
                MA2 = Indicators.MovingAverage(renko.Close, MAPeriod2, MAType2);
                DMS = Indicators.GetIndicator<DMS>(renko.High, renko.Low, renko.Close, ADXPeriod, MovingAverageType.WilderSmoothing);
            }
        }

        protected override void OnBar()
        {
            if (UseBacktesting == false)
            {
                DoTrade(TMS.Open.Last(1), TMS.Close.Last(1));
            }
        }

        protected override void OnTick()
        {
            if (UseBacktesting == true)
            {
                if (renko.Close.Last(1) != renko_bar_close)
                {
                    renko_bar_close = renko.Close.Last(1);
                    Print("Renko last bar close = {0}", renko_bar_close);
                    DoTrade(renko.Open.Last(1), renko.Close.Last(1));
                }
            }
        }

        private void DoTrade(double lastBarOpen, double lastBarClose)
        {
            if (IsTradePossible() == true && ((UseMT == false && Positions.FindAll(cBotLabel, TradeSymbol).Length == 0) || (UseMT == true)))
            {
                if (((UseCandle == true && lastBarClose > lastBarOpen) || UseCandle == false) && ((UseCandleMAs == true && lastBarClose > MA1.Result.Last(1) && lastBarClose > MA2.Result.Last(1)) || (UseCandleMAs == false)))
                {
                    OpenMarketOrder(TradeType.Buy, LotSize);
                }
                else if (((UseCandle == true && lastBarClose < lastBarOpen) || UseCandle == false) && ((UseCandleMAs == true && lastBarClose < MA1.Result.Last(1) && lastBarClose < MA2.Result.Last(1)) || (UseCandleMAs == false)))
                {
                    OpenMarketOrder(TradeType.Sell, LotSize);
                }
            }
            if (lastBarClose < lastBarOpen)
                ClosePositions(TradeType.Buy);
            else if (lastBarClose > lastBarOpen)
                ClosePositions(TradeType.Sell);

        }

        private void ClosePositions(TradeType tradeType)
        {
            foreach (var position in Positions.FindAll(cBotLabel, TradeSymbol, tradeType))
            {
                var result = ClosePosition(position);
                if (!result.IsSuccessful)
                {
                    Print("Closing market order error: {0}", result.Error);
                    OnStop();
                }
            }
        }

        private void OpenMarketOrder(TradeType tradeType, double dLots)
        {
            var volumeInUnits = CurrentSymbol.QuantityToVolumeInUnits(dLots);
            volumeInUnits = CurrentSymbol.NormalizeVolumeInUnits(volumeInUnits, RoundingMode.Down);

            //in final version need add attempts counter
            var result = ExecuteMarketOrder(tradeType, CurrentSymbol.Name, volumeInUnits, cBotLabel, StopLoss, TakeProfit);
            if (!result.IsSuccessful)
            {
                Print("Execute Market Order Error: {0}", result.Error.Value);
                OnStop();
            }
        }

        private bool IsTradePossible()
        {

            if (UseADX == true && DMS.ADX.Last(0) < ADXLevel)
            {
                Print("No trade - ADX is low - {0}", DMS.ADX.Last(0));
                return false;
            }

                        /*
            if (DMS.ADX.Last(1) > DMS.ADX.Last(0))
            {
                Print("No trade - ADX is go down - current {0} previous - ", DMS.ADX.Last(0), DMS.ADX.Last(1));
                return false;
            }
            */

return true;
        }

        protected override void OnStop()
        {

        }
    }
}
