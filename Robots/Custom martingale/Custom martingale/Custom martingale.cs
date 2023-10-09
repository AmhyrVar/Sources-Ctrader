// -------------------------------------------------------------------------------------------------
//
//    This code is a cTrader Automate API example.
//
//    This cBot is intended to be used as a sample and does not guarantee any particular outcome or
//    profit of any kind. Use it at your own risk.
//    
//    All changes to this file might be lost on the next application update.
//    If you are going to modify this file please make a copy using the "Duplicate" command.
//
//    The "Sample Martingale cBot" creates a random Sell or Buy order. If the Stop loss is hit, a new 
//    order of the same type (Buy / Sell) is created with double the Initial Volume amount. The cBot will 
//    continue to double the volume amount for  all orders created until one of them hits the take Profit. 
//    After a Take Profit is hit, a new random Buy or Sell order is created with the Initial Volume amount.
//
// -------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Custommartingale : Robot
    {
        [Parameter("Initial Volume (Lots)", Group = "Volume", DefaultValue = 1, MinValue = 0.01, Step = 0.01)]
        public double InitialQuantity { get; set; }

        [Parameter("Slippage limit", Group = "Protection", DefaultValue = 5)]
        public double SlippageLimit { get; set; }

        [Parameter("Failsafe volume", Group = "Protection", DefaultValue = 0.8)]
        public double FailSafeVol { get; set; }

        [Parameter("Failsafe duration", Group = "Protection", DefaultValue = 2)]
        public int FailSafeCandles { get; set; }


        [Parameter("Martingale signal candels", Group = "Renko", DefaultValue = 1)]
        public int MSC { get; set; }

        [Parameter("Reverse-Martingale signal candels", Group = "Renko", DefaultValue = 0)]
        public int RMSC { get; set; }

        [Parameter("Martingale Multiplier", Group = "Martingale", DefaultValue = 2)]
        public int MM { get; set; }
        //------------------------------------------------

        [Parameter("Stop Loss", Group = "Protection", DefaultValue = 40)]
        public int StopLoss { get; set; }

        [Parameter("Take Profit", Group = "Protection", DefaultValue = 40)]
        public int TakeProfit { get; set; }


        private Random random = new Random();

        protected override void OnStart()
        {
            Positions.Closed += OnPositionsClosed;

            ExecuteOrder(InitialQuantity, GetRandomTradeType());
        }

        private void ExecuteOrder(double quantity, TradeType tradeType)
        {
            var volumeInUnits = Symbol.QuantityToVolumeInUnits(quantity);
            var result = ExecuteMarketOrder(tradeType, SymbolName, volumeInUnits, "Martingale", StopLoss, TakeProfit);

            if (result.Error == ErrorCode.NoMoney)
                Stop();
        }

        private void OnPositionsClosed(PositionClosedEventArgs args)
        {
            Print("Closed");
            var position = args.Position;

            if (position.Label != "Martingale" || position.SymbolName != SymbolName)
                return;

            if (position.GrossProfit > 0)
            {
                ExecuteOrder(InitialQuantity, GetRandomTradeType());
            }
            else
            {
                ExecuteOrder(position.Quantity * 2, position.TradeType);
            }
        }

        private TradeType GetRandomTradeType()
        {
            return random.Next(2) == 0 ? TradeType.Buy : TradeType.Sell;
        }
    }
}
