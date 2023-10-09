using System;
using System.Collections;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class ADXFractals : Robot
    {

        [Parameter("SMA Slow", DefaultValue = 100, MinValue = 50, MaxValue = 1000, Step = 50)]
        public int ma_slow_period { get; set; }

        [Parameter("SMA Fast", DefaultValue = 30, MinValue = 10, MaxValue = 100, Step = 10)]
        public int ma_fast_period { get; set; }

        [Parameter("MA Cross", DefaultValue = 30, MinValue = 10, MaxValue = 200, Step = 10)]
        public int param_ma_cross_period { get; set; }

        [Parameter("ADX Min Value", DefaultValue = 20, MinValue = 10, MaxValue = 100, Step = 1)]
        public int param_adx_min { get; set; }

        [Parameter("ADX Max Value", DefaultValue = 40, MinValue = 30, MaxValue = 100, Step = 5)]
        public int param_adx_max { get; set; }

        [Parameter("Fractal Period", DefaultValue = 10, MinValue = 10, MaxValue = 100, Step = 10)]
        public int param_fractal_period { get; set; }

        [Parameter("Fractal diff", DefaultValue = 0, MinValue = 0, MaxValue = 10, Step = 1)]
        public int param_fractal_diff { get; set; }

        [Parameter("Min SL", DefaultValue = 30, MinValue = 5, MaxValue = 100, Step = 1)]
        public int param_min_stop_loss { get; set; }

        [Parameter("Max SL", DefaultValue = 50, MinValue = 30, MaxValue = 50, Step = 5)]
        public int param_max_stop_loss { get; set; }

        [Parameter("Min TP", DefaultValue = 30, MinValue = 5, MaxValue = 50, Step = 5)]
        public int param_min_take_profit { get; set; }

        [Parameter("Max TP", DefaultValue = 50, MinValue = 30, MaxValue = 50, Step = 1)]
        public int param_max_take_profit { get; set; }

        [Parameter("Volume Units", DefaultValue = 100000, MinValue = 1000, MaxValue = 1000000, Step = 1000)]
        public int volume_units { get; set; }

        private MovingAverage i_MA_slow;
        private MovingAverage i_MA_fast;
        private DirectionalMovementSystem i_ADXR;
        private RelativeStrengthIndex i_RSI;
        private Fractals i_fractal;

        protected override void OnStart()
        {
            i_MA_slow = Indicators.MovingAverage(MarketSeries.Close, ma_slow_period, MovingAverageType.Simple);
            i_MA_fast = Indicators.MovingAverage(MarketSeries.Close, ma_fast_period, MovingAverageType.Simple);
            i_ADXR = Indicators.DirectionalMovementSystem(20);
            i_RSI = Indicators.RelativeStrengthIndex(MarketSeries.Close, 14);
            i_fractal = Indicators.GetIndicator<Fractals>(param_fractal_period);
        }

        protected override void OnTick()
        {
            if (Positions.FindAll("MA", SymbolName).Length == 0)
            {
                if (i_ADXR.ADX.LastValue > param_adx_min && i_ADXR.ADX.LastValue < param_adx_max)
                    if (i_RSI.Result.LastValue < 60)
                        if (i_MA_fast.Result.HasCrossedAbove(i_MA_slow.Result.LastValue, param_ma_cross_period))
                            if (Symbol.Bid > i_MA_fast.Result.LastValue)
                            {
                                Open_Buy_Order();
                                return;
                            }
                if (i_ADXR.ADX.LastValue > param_adx_min && i_ADXR.ADX.LastValue < param_adx_max)
                    if (i_RSI.Result.LastValue > 40)
                        if (i_MA_fast.Result.HasCrossedBelow(i_MA_slow.Result.LastValue, param_ma_cross_period))
                            if (Symbol.Bid < i_MA_fast.Result.LastValue)
                            {
                                Open_Sell_Order();
                                return;
                            }
            }
        }

        private void Open_Buy_Order()
        {
            double tp = 0, sl = 0;
            tp = getTakeProfit(TradeType.Buy);
            sl = getStopLoss(TradeType.Buy);
            if (tp > param_min_take_profit && tp < param_max_take_profit && sl > param_min_stop_loss && sl < param_max_stop_loss)
                ExecuteMarketOrder(TradeType.Buy, SymbolName, volume_units, "MA", sl + param_fractal_diff, tp - param_fractal_diff);
        }


        private void Open_Sell_Order()
        {
            double tp = 0, sl = 0;
            tp = getTakeProfit(TradeType.Sell);
            sl = getStopLoss(TradeType.Sell);
            if (tp > param_min_take_profit && tp < param_max_take_profit && sl > param_min_stop_loss && sl < param_max_stop_loss)
                ExecuteMarketOrder(TradeType.Sell, SymbolName, volume_units, "MA", sl + param_fractal_diff, tp - param_fractal_diff);
        }


        private double getTakeProfit(TradeType type)
        {
            double tp = 0;
            if (type == TradeType.Buy)
            {
                for (int i = i_fractal.UpFractal.Count; i > 0; i--)
                {
                    if (!double.IsNaN(i_fractal.UpFractal[i]))
                    {
                        if ((i_fractal.UpFractal[i] - Symbol.Bid) / Symbol.PipSize > param_min_take_profit)
                            if ((i_fractal.UpFractal[i] - Symbol.Bid) / Symbol.PipSize < param_max_take_profit)
                            {
                                tp = (i_fractal.UpFractal[i] - Symbol.Bid) / Symbol.PipSize;
                                break;
                            }
                    }
                }
            }

            if (type == TradeType.Sell)
            {
                for (int i = i_fractal.DownFractal.Count; i > 0; i--)
                {
                    if (!double.IsNaN(i_fractal.DownFractal[i]))
                    {
                        if ((Symbol.Bid - i_fractal.DownFractal[i]) / Symbol.PipSize > param_min_stop_loss)
                            if ((Symbol.Bid - i_fractal.DownFractal[i]) / Symbol.PipSize < param_max_stop_loss)
                            {
                                tp = (Symbol.Bid - i_fractal.DownFractal[i]) / Symbol.PipSize;
                                break;
                            }
                    }
                }
            }

            return tp;
        }

        private double getStopLoss(TradeType type)
        {
            double sl = 0;
            if (type == TradeType.Buy)
            {
                for (int i = i_fractal.DownFractal.Count; i > 0; i--)
                {
                    if (!double.IsNaN(i_fractal.DownFractal[i]))
                    {
                        if ((Symbol.Bid - i_fractal.DownFractal[i]) / Symbol.PipSize > param_min_stop_loss)
                            if ((Symbol.Bid - i_fractal.DownFractal[i]) / Symbol.PipSize < param_max_stop_loss)
                            {
                                sl = (Symbol.Bid - i_fractal.DownFractal[i]) / Symbol.PipSize;
                                break;
                            }
                    }
                }

            }

            if (type == TradeType.Sell)
            {
                for (int i = i_fractal.UpFractal.Count; i > 0; i--)
                {
                    if (!double.IsNaN(i_fractal.UpFractal[i]))
                    {
                        if ((i_fractal.UpFractal[i] - Symbol.Bid) / Symbol.PipSize > param_min_take_profit)
                            if ((i_fractal.UpFractal[i] - Symbol.Bid) / Symbol.PipSize < param_max_take_profit)
                            {
                                sl = (i_fractal.UpFractal[i] - Symbol.Bid) / Symbol.PipSize;
                                break;
                            }
                    }
                }
            }

            return sl;
        }
    }
}
