using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class PendingOrderonADXandMACD : Robot
    {

        [Parameter("Target TP", DefaultValue = 0)]
        public double p_target_tp { get; set; }

        [Parameter("Pivot SL", DefaultValue = 0)]
        public double p_pivot_sl { get; set; }

        [Parameter("Distance from Target/Pivot", DefaultValue = 20)]
        public double p_distance { get; set; }

        [Parameter("Amount to risk", DefaultValue = 100)]
        public double p_amount { get; set; }

        [Parameter("Open Trade on Start", DefaultValue = false)]
        public bool p_open_trade { get; set; }

        public DirectionalMovementSystem i_adx;
        public MacdCrossOver i_macd;

        public int tp = 0;
        public int sl = 0;

        protected override void OnStart()
        {

            i_adx = Indicators.DirectionalMovementSystem(14);
            i_macd = Indicators.MacdCrossOver(26, 12, 14);
            Chart.DrawHorizontalLine("Target", p_target_tp, Color.Green, 2, LineStyle.Solid);
            Chart.DrawHorizontalLine("Pivot", p_pivot_sl, Color.Red, 2, LineStyle.Solid);

            if (p_open_trade && Positions.FindAll("target", SymbolName).Length == 0)
            {
                //Sell
                if (p_target_tp < Symbol.Ask)
                {
                    tp = (int)Math.Round((Symbol.Ask - p_target_tp) / Symbol.PipSize) * 1;
                    sl = (int)Math.Round((p_pivot_sl - Symbol.Ask) / Symbol.PipSize) * 1;
                    Print("tp: " + tp);
                    ExecuteMarketOrder(TradeType.Sell, SymbolName, getVolume(TradeType.Sell), "target", sl, tp);
                }

                //Buy
                if (p_target_tp > Symbol.Ask)
                {
                    tp = (int)Math.Round((p_target_tp - Symbol.Ask) / Symbol.PipSize);
                    sl = (int)Math.Round((Symbol.Ask - p_pivot_sl) / Symbol.PipSize);
                    ExecuteMarketOrder(TradeType.Buy, SymbolName, getVolume(TradeType.Buy), "target", sl, tp);
                }
                Stop();
            }
        }

        protected override void OnTick()
        {
            if (Positions.FindAll("target", SymbolName).Length > 0)
                return;

            //Sell
            tp = (int)Math.Round((Symbol.Ask - p_target_tp) / Symbol.PipSize) * 1;
            sl = (int)Math.Round((p_pivot_sl - Symbol.Ask) / Symbol.PipSize) * 1;
            if (i_macd.Signal.HasCrossedAbove(i_macd.MACD, 30))
                if ((Symbol.Ask - p_target_tp) / Symbol.PipSize > p_distance)
                    if ((p_pivot_sl - Symbol.Ask) / Symbol.PipSize > p_distance)
                        if (i_adx.ADX.LastValue > 20)
                            if (MarketSeries.Close.IsFalling())
                                if (Symbol.Ask < p_pivot_sl)
                                {
                                    ExecuteMarketOrder(TradeType.Sell, SymbolName, getVolume(TradeType.Sell), "target", sl, tp);
                                    if (!IsBacktesting)
                                        Stop();
                                }

            //Buy
            tp = (int)Math.Round((p_target_tp - Symbol.Ask) / Symbol.PipSize);
            sl = (int)Math.Round((Symbol.Ask - p_pivot_sl) / Symbol.PipSize);
            if (i_macd.Signal.HasCrossedBelow(i_macd.MACD, 30))
                if ((p_target_tp - Symbol.Ask) / Symbol.PipSize > p_distance)
                    if ((Symbol.Ask - p_pivot_sl) / Symbol.PipSize > p_distance)
                        if (i_adx.ADX.LastValue > 20)
                            if (MarketSeries.Close.IsRising())
                                if (Symbol.Ask > p_pivot_sl)
                                {
                                    ExecuteMarketOrder(TradeType.Buy, SymbolName, getVolume(TradeType.Buy), "target", sl, tp);
                                    if (!IsBacktesting)
                                        Stop();
                                }
        }

        double getVolume(TradeType trade_type)
        {
            double sl_pips = 0;
            if (trade_type == TradeType.Sell)
                sl_pips = Math.Round((p_pivot_sl - Symbol.Ask) / Symbol.PipSize);
            else
                sl_pips = Math.Round((Symbol.Ask - p_pivot_sl) / Symbol.PipSize);

            double volume = p_amount / (sl_pips * Symbol.PipSize);
            double normalizedVolume = Symbol.NormalizeVolumeInUnits(volume);

            return Math.Abs(normalizedVolume);

        }
    }
}
