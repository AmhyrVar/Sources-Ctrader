using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.GMTStandardTime, AccessRights = AccessRights.None)]
    public class Gerbil : Robot
    {
        [Parameter("Trade Start Hour", DefaultValue = 23, MinValue = 0, Step = 1)]
        public int TradeStart { get; set; }
        [Parameter("Trade End Hour", DefaultValue = 1, MinValue = 0, Step = 1)]
        public int TradeEnd { get; set; }
        [Parameter("Take Profit (Pips)", DefaultValue = 6, MinValue = 1, Step = 1)]
        public int TakeProfit { get; set; }
        [Parameter("Stop Loss (Pips)", DefaultValue = 150, MinValue = 1, Step = 1)]
        public int StopLoss { get; set; }
        [Parameter("Calculate Volume by Percentage?", DefaultValue = false)]
        public bool RiskPercent { get; set; }
        [Parameter("Quantity (%Risk or Lots)", DefaultValue = 0.01, MinValue = 0.01, Step = 0.01)]
        public double Quantity { get; set; }
        [Parameter("RSI Source")]
        public DataSeries RSISource { get; set; }
        [Parameter("RSI Period", DefaultValue = 7, MinValue = 1, Step = 1)]
        public int RSIPeriods { get; set; }
        [Parameter("RSI Overbought Level", DefaultValue = 80, MinValue = 1, Step = 1)]
        public int RSIOverB { get; set; }
        [Parameter("RSI Oversold Level", DefaultValue = 40, MinValue = 1, Step = 1)]
        public int RSIOverS { get; set; }
        [Parameter("ATR Periods", DefaultValue = 15, MinValue = 1, Step = 1)]
        public int ATRPeriods { get; set; }
        [Parameter("ATR From", DefaultValue = 10, MinValue = 1, Step = 1)]
        public int ATRFrom { get; set; }
        [Parameter("ATR To", DefaultValue = 100, MinValue = 1, Step = 1)]
        public int ATRTo { get; set; }
        [Parameter("Max Positions", DefaultValue = 1, MinValue = 1, Step = 1)]
        public int MaxPos { get; set; }
        [Parameter("Max DD Positions", DefaultValue = 0, MinValue = 0, Step = 1)]
        public int MaxDDPos { get; set; }
        [Parameter("KillHours", DefaultValue = 0, MinValue = 0, Step = 1)]
        public int KillHours { get; set; }

        private RelativeStrengthIndex rsi;
        private AverageTrueRange atr;
        private int DDPos = 0;

        protected override void OnStart()
        {
            rsi = Indicators.RelativeStrengthIndex(RSISource, RSIPeriods);
            atr = Indicators.AverageTrueRange(ATRPeriods, MovingAverageType.Simple);
        }

        protected override void OnBar()
        {
            if (Time.Hour >= TradeStart || Time.Hour < TradeEnd)
            {
                var atrVal = atr.Result.LastValue * 100000;
                if (atrVal > ATRFrom && atrVal < ATRTo)
                {
                    if (Positions.Count < MaxPos)
                    {
                        if (rsi.Result.LastValue < RSIOverS)
                        {
                            Open(TradeType.Buy);
                        }
                        else if (rsi.Result.LastValue > RSIOverB)
                        {
                            Open(TradeType.Sell);
                        }
                    }
                }
            }
            if (KillHours != 0)
            {
                foreach (var position in Positions.FindAll("SampleRSI", Symbol))
                {
                    if (Time > position.EntryTime.AddMinutes(KillHours * 60))
                        ClosePosition(position);
                }
            }
        }


        private void Open(TradeType tradeType)
        {
            //var position = Positions.Find("SampleRSI", Symbol, tradeType);
            var volumeInUnits = CalculateVolume();

            ExecuteMarketOrder(tradeType, Symbol, volumeInUnits, "SampleRSI", StopLoss, TakeProfit);
        }



        double CalculateVolume()
        {
            if (!RiskPercent)
            {
                return (Symbol.QuantityToVolumeInUnits(Quantity));
            }
            else
            {
                // Calculate the total risk allowed per trade.
                double riskPerTrade = (Account.Balance * Quantity) / 100;
                double totalSLPipValue = (StopLoss + Symbol.Spread) * Symbol.PipValue;
                double calculatedVolume = riskPerTrade / totalSLPipValue;

                double normalizedCalculatedVolume = Symbol.NormalizeVolumeInUnits(calculatedVolume, RoundingMode.ToNearest);
                return normalizedCalculatedVolume;
            }
        }


    }
}
