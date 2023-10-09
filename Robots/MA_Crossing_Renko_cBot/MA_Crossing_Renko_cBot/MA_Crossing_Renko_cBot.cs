using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MA_Crossing_cBot : Robot
    {

        [Parameter(DefaultValue = "MA_Crossing_Renko_cBot")]
        public string cBotLabel { get; set; }

        [Parameter("Currency pair", DefaultValue = "EURUSD")]
        public string TradeSymbol { get; set; }

        [Parameter("Lot Size", DefaultValue = 0.1, MinValue = 0.01, Step = 0.01)]
        public double LotSize { get; set; }

        [Parameter("MA Type", DefaultValue = MovingAverageType.Exponential)]
        public MovingAverageType MAType { get; set; }

        [Parameter("MA Period", DefaultValue = 14)]
        public int MAPeriod { get; set; }

        [Parameter("Signal candle,in bars", DefaultValue = 1, MinValue = 0)]
        public int SignalCandle { get; set; }

        [Parameter("Fixed TP", DefaultValue = false)]
        public bool UseTP { get; set; }

        [Parameter("TP Level", DefaultValue = 40.0)]
        public double TakeProfit { get; set; }

        [Parameter("TrailingStop", DefaultValue = false)]
        public bool UseTS { get; set; }

        [Parameter("Trailing Trigger", DefaultValue = 15.0)]
        public double TrailingTrigger { get; set; }

        [Parameter("Trailing Pips", DefaultValue = 4.0)]
        public double TrailingPips { get; set; }

        private MovingAverage MA;
        protected override void OnStart()
        {
            //check symbol
            Symbol CurrentSymbol = Symbols.GetSymbol(TradeSymbol);

            if (CurrentSymbol == null)
            {
                Print("Currency pair is not supported,please check!");
                OnStop();
            }

            MA = Indicators.MovingAverage(MarketSeries.Close, MAPeriod, MAType);
        }

        protected override void OnBar()
        {
            if (MA.Result.Last(SignalCandle) < MarketSeries.Close.Last(SignalCandle) && MA.Result.Last(SignalCandle) > MarketSeries.Low.Last(SignalCandle) && Positions.FindAll(cBotLabel, TradeSymbol, TradeType.Buy).Length == 0)
            {
                ClosePosition(TradeType.Sell);
                OpenMarketOrder(TradeType.Buy, TradeSymbol, LotSize);
            }
            else if (MA.Result.Last(SignalCandle) > MarketSeries.Close.Last(SignalCandle) && MA.Result.Last(SignalCandle) < MarketSeries.High.Last(SignalCandle) && Positions.FindAll(cBotLabel, TradeSymbol, TradeType.Sell).Length == 0)
            {
                ClosePosition(TradeType.Buy);
                OpenMarketOrder(TradeType.Sell, TradeSymbol, LotSize);
            }
        }

        protected override void OnTick()
        {
            if (UseTS == true && Positions.FindAll(cBotLabel, TradeSymbol).Length > 0)
                DoTrailingStop();
        }

        private void ClosePosition(TradeType tradeType)
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

        private void OpenMarketOrder(TradeType tradeType, string strSymbol, double dLots)
        {
            var volumeInUnits = Symbol.QuantityToVolumeInUnits(dLots);
            volumeInUnits = Symbol.NormalizeVolumeInUnits(volumeInUnits, RoundingMode.Down);
            double TP_in_pips = 0.0;
            if (UseTP == true)
                TP_in_pips = TakeProfit;

            var result = ExecuteMarketOrder(tradeType, strSymbol, volumeInUnits, cBotLabel, 0, TP_in_pips);
            if (!result.IsSuccessful)
            {
                Print("Execute Market Order Error: {0}", result.Error.Value);
                OnStop();
            }
        }

        private void DoTrailingStop()
        {
            var cBotPositions = Positions.FindAll(cBotLabel, TradeSymbol);

            foreach (var position in cBotPositions)
            {
                if (position.TradeType == TradeType.Buy && position.Pips >= TrailingTrigger && position.HasTrailingStop == false)
                {
                    var NewSL = position.TradeType == TradeType.Buy ? (position.EntryPrice + (TrailingPips * Symbol.PipSize)) : (position.EntryPrice - (TrailingPips * Symbol.PipSize));
                    ModifyPosition(position, NewSL, position.TakeProfit, true);
                }
            }

        }

        protected override void OnStop()
        {

        }
    }
}
