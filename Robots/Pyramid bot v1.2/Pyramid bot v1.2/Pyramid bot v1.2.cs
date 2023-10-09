/*
 *      Dark Matter SuperTrend Strategy
 *
 *      Changelog:
 *          v1.0.0 (December 15, 2021)
 *              - First release
 */

using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Timers;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class Pyramidbotv12 : Robot
    {
        #region Parameters

        #region GridSettings
        [Parameter("Instance Name", DefaultValue = "001", Group = "Grid Settings")]
        public string Instance { get; set; }
        [Parameter("Include Buy Stop?", DefaultValue = true, Group = "Grid Settings")]
        public bool IncludeBuyStop { get; set; }
        [Parameter("Include Sell Stop?", DefaultValue = true, Group = "Grid Settings")]
        public bool IncludeSellStop { get; set; }
        [Parameter("Grid Step (pips)", DefaultValue = 20, Group = "Grid Settings")]
        public double GridStepPips { get; set; }
        [Parameter("Total Orders", DefaultValue = 5, Group = "Grid Settings")]
        public int TotalOrders { get; set; }
         [Parameter("Buy & Sell Gap (pips)", DefaultValue = 40, Group = "Grid Settings")]
        public int BuySellGap { get; set; }
        #endregion

#region TradeControl
        [Parameter("Volume", DefaultValue = 0.01, MinValue = 0.01, MaxValue = 100, Step = 0.01, Group = "Trade Control")]
        public double Volume { get; set; }

        [Parameter("Stop Loss", DefaultValue = 50, MinValue = 0, Step = 0.1, Group = "Trade Control")]
        public double StopLoss { get; set; }

        [Parameter("Take Profit", DefaultValue = 100, MinValue = 0, Step = 0.1, Group = "Trade Control")]
        public double TakeProfit { get; set; }
        [Parameter("Max Spread", DefaultValue = 5, Group = "Trade Control")]
        public double MaxSpread { get; set; }

        [Parameter("Max Slippage", DefaultValue = 10, Group = "Trade Control")]
        public double MaxSlippage { get; set; }


        [Parameter("Max Open Trades", DefaultValue = 100, Group = "Trade Control")]
        public int MaxOpenPositions { get; set; }
        [Parameter("Max Losing Trades", DefaultValue = 10, Group = "Trade Control")]
        public int MaxLosingTrades { get; set; }

        #endregion
        #region TradingSession
        [Parameter("Activate ?", DefaultValue = true, Group = "Trading Session")]
        public bool TradingSessionActivate { get; set; }
        [Parameter("Trading Starts (hrs)", Group = "Trading Session", DefaultValue = "11:00")]
        public string TradingStarts { get; set; }
        [Parameter("Trading Stops (hrs)", Group = "Trading Session", DefaultValue = "11:00")]
        public string TradingStops { get; set; }


        [Parameter("Close All Winners?", DefaultValue = true, Group = "Trading Session")]
        public bool CloseAllWinners { get; set; }
        [Parameter("Close All Losers?", DefaultValue = true, Group = "Trading Session")]
        public bool CloseAllLosers{ get; set; }
        #endregion

        #region SuperTrend
        [Parameter("Activate ?" , DefaultValue = true, Group = "SuperTrend")]
        public bool ActivateSUperTrend { get; set; }
        [Parameter("ATR Length", DefaultValue = 10, Group = "SuperTrend")]
        public int Periods { get; set; }
        [Parameter("Factor", DefaultValue = 3, Group = "SuperTrend")]
        public double Multiplier { get; set; }
        [Parameter("Close on Reversal?", DefaultValue = false, Group = "SuperTrend")]
        public bool CloseOnReversal { get; set; }
        #endregion

        #region BreakEven
        [Parameter("Activate ?", DefaultValue = true, Group = "Break Even")]
        public bool ActivateBreakEven { get; set; }
        [Parameter("Trigger (pips) ", DefaultValue = 20, Group = "Break Even")]
        public double BETriggerPips { get; set; }
        [Parameter("Steps (pips) ", DefaultValue = 10, Group = "Break Even")]
        public double BEStepsPips { get; set; }
        #endregion
        #region TrailingStop
        [Parameter("Use Trailing Stop", DefaultValue = false, Group = "Trailing Stop")]
        public bool UseTSL { get; set; }

        [Parameter("Trigger (pips)", DefaultValue = 20, Group = "Trailing Stop")]
        public int TSLTrigger { get; set; }

        [Parameter("Distance (pips)", DefaultValue = 10, Group = "Trailing Stop")]
        public int TSLDistance { get; set; }

        #endregion
        #region EquityDrawdownStop
        [Parameter("Activate ?", DefaultValue = false, Group = "Equity Drawdown Stop")]
        public bool UseMaxDrawdown { get; set; }

        [Parameter("Max Drawdown(%)", DefaultValue = 10, Group = "Equity Drawdown Stop")]
        public double MaxDrawdown { get; set; }
        [Parameter("Close positions?", DefaultValue = false, Group = "Equity Drawdown Stop")]
        public bool EquityStopClosePosition { get; set; }

        #endregion
        #region TelegramNotifications
        [Parameter("Telegram Alerts?", Group = "Telegram Notifications")]
        public bool SendNotifications { get; set; }

        [Parameter("Bot Token", DefaultValue = "", Group = "Telegram Notifications")]
        public string BotToken { get; set; }

        [Parameter("Chat ID", DefaultValue = "", Group = "Telegram Notifications")]
        public string ChatID { get; set; }
        #endregion

       
        #endregion Parameters

        #region Private Variables

        private bool _isPaused;
        private double _maxEquity;
        public int _noOfDailyTrades;
        private List<Position> _positionsPT;

        private Supertrend _superTrend;

        private bool _isStop = false;
        private static bool _validationComplete = false;
        #endregion Private Variables

        #region Methods

        public enum VolumeType
        {
            BalancePercentage,
            EquityPercentage,
            Lots,
            Units,
            CashAmount,
            Leverage
        }

        // Method handling the OnStart event. Triggered when the cBot starts
        protected override void OnStart()
        {
           

            // Declare Telegram protocol for Tls2 security
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Set the current account equity
            _maxEquity = Account.Equity;

            // Initialize positions list
            _positionsPT = new List<Position>();

            // Initialize Supertrend indicator
            _superTrend = Indicators.Supertrend(Periods, Multiplier);

            // Initialize _isPaused variable to false
            _isPaused = false;

            // Initialize a timer to monitor the news events
            System.Timers.Timer timer = new System.Timers.Timer();

            timer.Interval = 60000.0;
            timer.Enabled = true;

            

            // Handle the position opened and position closed events
            Positions.Opened += Positions_Opened;
            Positions.Closed += Positions_Closed;

            //If supertrend set to yes Place the pending orders according to superTrend direction
            //Bullish = only Buy Stop orders 
            Supertrend_Grid();

        }

        protected override void OnTick()
        {

            if (UseMaxDrawdown)
            {

                // if current equity is higher that previously recorded max equity, then we record a new max equity value
                _maxEquity = Math.Max(Account.Equity, _maxEquity);

                // If max equity drawdown is higher than the max allowed equity drawdowm
                if (_maxEquity - Account.Equity > MaxDrawdown / 100.0 * _maxEquity && EquityStopClosePosition)
                {
                    // All positions are closed
                    foreach (var position in Positions.Where(x => x.SymbolName == Symbol.Name && x.Label == Instance))
                    {
                        position.Close();
                    }
                    Print("cBot stopped due to Max Equity DD");
                    // The cBot is stopped
                    Stop();
                }
            }

            if (ActivateBreakEven)
            {

                // we iterate through all the instance's positions
                foreach (var position in Positions.Where(x => x.Label == Instance))
                {

                    // If position's pips is above the break even trigger pips
                    if (position.Pips > BETriggerPips)
                    {

                        // We check the position's trade type and excute the relevant code block
                        if (position.TradeType == TradeType.Buy)
                        {

                            // We calculate the stop loss based on the Pips To Add parameter
                            var stopLoss = Math.Round(position.EntryPrice + BEStepsPips * Symbol.PipSize, Symbol.Digits);

                            if (stopLoss < position.StopLoss)
                            {
                                // If the stop loss is lower to the existing stop loss, we modify the stop loss price 
                                position.ModifyStopLossPrice(stopLoss);

                                Print("Stop Loss Moved to Break Even");
                            }
                        }
                        else
                        {

                            // We calculate the stop loss based on the Pips To Add parameter
                            var stopLoss = Math.Round(position.EntryPrice - BEStepsPips * Symbol.PipSize, Symbol.Digits);

                            if (stopLoss > position.StopLoss)
                            {
                                // If the stop loss is higher to the existing stop loss, we modify the stop loss price 
                                position.ModifyStopLossPrice(stopLoss);

                                Print("Stop Loss Moved to Break Even");
                            }
                        }
                    }
                }
            }

            if (UseTSL)
            {

                // we iterate through all the instance's positions
                foreach (var position in Positions.Where(x => x.Label == Instance))
                {

                    // If position's pips is above the trailing stop loss pips and the position has not trailing stop loss set
                    if (position.Pips > TSLTrigger && !position.HasTrailingStop)
                    {

                        // We check the position's trade type and excute the relevant code block
                        if (position.TradeType == TradeType.Buy)
                        {

                            // We calculate the stop loss based on the TSL Distance To Add parameter
                            var stopLoss = Symbol.Bid - (TSLDistance * Symbol.PipSize);

                            // We modify the stop loss price
                            position.ModifyStopLossPrice(stopLoss);

                            // We set the trailing stop loss to true
                            position.ModifyTrailingStop(true);

                            Print("Trailing Stop Loss Triggered");
                        }
                        else
                        {
                            // We calculate the stop loss based on the TSL Distance To Add parameter
                            var sl = Symbol.Ask + (TSLDistance * Symbol.PipSize);

                            // We modify the stop loss price
                            position.ModifyStopLossPrice(sl);
                            position.ModifyTrailingStop(true);

                            Print("Trailing Stop Loss Triggered");
                        }
                    }
                }
            }

           if (!TradingEnabled())
            {
                if (CloseAllLosers)
                { 
                    foreach (var po in Positions)
                    {
                        if (po.NetProfit > 0)
                        {
                            ClosePosition(po);
                        }
                    }
                }
                if (CloseAllLosers)
                {
                    foreach (var po in Positions)
                    {
                        if (po.NetProfit < 0)
                        {
                            ClosePosition(po);
                        }
                    }
                }
                //Question should we close all pendings ?
                foreach (var ord in PendingOrders)
                {
                    CancelPendingOrder(ord);
                }

            }


        }
            private void SuperTrend_Reverse(TradeType TypeToClose)
        {
            foreach (var po in Positions)
            {
                //QUESTION do we close on reversal only positions or both positions and Pendings 

                
                if (po.TradeType == TypeToClose && CloseOnReversal)
                {
                    ClosePosition(po);
                }
               
            }
            foreach (var ord in PendingOrders)

                if (ord.TradeType == TypeToClose)
                {
                    CancelPendingOrder(ord);
                }

        }
        private bool TradingEnabled()
        {
            if (!TradingSessionActivate)
                return true;
            var @from = TimeSpan.ParseExact(TradingStarts, "hh\\:mm", null);
            var to = TimeSpan.ParseExact(TradingStops, "hh\\:mm", null);
            return  Server.Time.TimeOfDay >= @from && Server.Time.TimeOfDay < to;

        }
        private void Supertrend_Grid()
        {
            Print($"Supergrid called" );
            if (TradingEnabled() && IncludeBuyStop && double.IsNaN(_superTrend.UpTrend.Last(2)) && double.IsNaN(_superTrend.DownTrend.Last(1)))
            {

                SuperTrend_Reverse(TradeType.Sell);
                var pendingOrdersWithMyLabel = PendingOrders.Where(order => order.SymbolName == SymbolName && order.Label.Equals(Instance, System.StringComparison.OrdinalIgnoreCase));
                var TriggerPrice = Symbol.Bid + GridStepPips * Symbol.PipSize;
                while (pendingOrdersWithMyLabel.Count() < TotalOrders)
                {
                    // Calculate the stop loss and take profit levels
                   

                    // Place a Buy Stop order
                    PlaceStopOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), TriggerPrice,Instance,StopLoss,TakeProfit);
                 
                    TriggerPrice += GridStepPips * Symbol.PipSize;
                    Print("Buy is placed");

                }

            }
            //NODE maybe displace Include sellstop so it can close opposite direction
            if (TradingEnabled() && IncludeSellStop && double.IsNaN(_superTrend.DownTrend.Last(2)) && double.IsNaN(_superTrend.UpTrend.Last(1)))
            {
                SuperTrend_Reverse(TradeType.Buy);
                var pendingOrdersWithMyLabel = PendingOrders.Where(order => order.SymbolName == SymbolName && order.Label.Equals(Instance, System.StringComparison.OrdinalIgnoreCase));
                var TriggerPrice = Symbol.Ask - GridStepPips * Symbol.PipSize;
                while (pendingOrdersWithMyLabel.Count() < TotalOrders)
                {
                    // Calculate the stop loss and take profit levels


                    // Place a Buy Stop order
                    PlaceStopOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), TriggerPrice, Instance, StopLoss, TakeProfit);

                    TriggerPrice -= GridStepPips * Symbol.PipSize;
                    Print("Sell is placed");

                }
            }
        }

        // Method that handles the Position Closed event. Triggered whenever a position is closed
        private void Positions_Closed(PositionClosedEventArgs obj)
        {
            if (obj.Position.SymbolName == SymbolName && obj.Position.Label == Instance)
            {

                // If send notifications is set to true, then a message is constructed and sent
                if (SendNotifications)
                {

                    // Constructing the message
                    var message = "Position Closed" + Environment.NewLine;
                    message += "Base Strategy" + Environment.NewLine;
                    message += "Symbol: " + obj.Position.SymbolName.ToString() + Environment.NewLine;
                    message += "Volume: " + obj.Position.VolumeInUnits.ToString() + Environment.NewLine;
                    message += "Direction: " + obj.Position.TradeType.ToString() + Environment.NewLine;
                    message += "Equity: " + Account.Equity + Environment.NewLine;
                    message += "Net Profit: " + obj.Position.NetProfit + Environment.NewLine;
                    message += "Account Type: " + (Account.IsLive ? "Live" : "Demo") + Environment.NewLine;

                    // Sending the message about the closed position
                    SendTelegramMessage(message, ChatID, BotToken);
                }
            }
        }

        // Method that handles the Position Opened event. Triggered whenever a position is opened
        private void Positions_Opened(PositionOpenedEventArgs obj)
        {
            if (obj.Position.SymbolName == SymbolName && obj.Position.Label == Instance)
            {
               if (ActivateSUperTrend)
                {
                    if(obj.Position.TradeType == TradeType.Buy)
                    {
                        double maxEntryPrice = 0.0;
                        foreach (var po in PendingOrders)
                        {
                            if (po.TargetPrice > maxEntryPrice)
                            {
                                maxEntryPrice = po.TargetPrice;
                            }
                        }
                        Print("Created**********************");
                        var TriggerPrice = maxEntryPrice + GridStepPips * Symbol.PipSize;
                        PlaceStopOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), TriggerPrice, Instance, StopLoss, TakeProfit);
                    }
                    if (obj.Position.TradeType == TradeType.Sell)
                    {
                        double minEntryPrice = double.PositiveInfinity;
                        foreach (var po in PendingOrders)
                        {
                            if (po.TargetPrice < minEntryPrice)
                            {
                               minEntryPrice = po.TargetPrice;
                            }
                        }
                        Print("Created**********************");
                        var TriggerPrice = minEntryPrice - GridStepPips * Symbol.PipSize;
                        PlaceStopOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), TriggerPrice, Instance, StopLoss, TakeProfit);
                    }

                }

                // Increasing the counter of the daily trades
                _noOfDailyTrades++;

                // If send notifications is set to true, then a message is constructed and sent
                if (SendNotifications)
                {

                    // Constructing the message
                    var message = "Position Opened" + Environment.NewLine;
                    message += "Base Strategy" + Environment.NewLine;
                    message += "Symbol: " + obj.Position.SymbolName.ToString() + Environment.NewLine;
                    message += "Volume: " + obj.Position.VolumeInUnits.ToString() + Environment.NewLine;
                    message += "Direction: " + obj.Position.TradeType.ToString() + Environment.NewLine;
                    message += "Entry Price: " + obj.Position.EntryPrice + Environment.NewLine;
                    message += "Account Type: " + (Account.IsLive ? "Live" : "Demo") + Environment.NewLine;

                    // Sending the message about the open position
                    SendTelegramMessage(message, ChatID, BotToken);
                }
            }
        }

      

        // Method that handles the OnBar event
        protected override void OnBar()
        {
            Supertrend_Grid();

        }

        // Method that executes the trading logic
    

        // Methods handling the OnTick Event
    

        // Method sending Telegram Messages
        private bool SendTelegramMessage(string message, string chatID, string botToken)
        {

            // If cBot is backtesting, we exit the method
            if (IsBacktesting)
                return false;
            try
            {

                // We initialize the telegram service
                TelegramService telegram = new TelegramService();

                // We send te Telegram message
                string result = telegram.SendTelegram(chatID, botToken.Trim(), message);

                // If the result contains ERROR, we return false, indicating that the message has not been sent
                if (result.Contains("ERROR"))
                {
                    result = result.Replace("ERROR:", string.Empty);
                    return false;
                }
                else
                {
                    Print(result);
                }

                return true;

                // We handle possible exceptions, by printing them in the log and returning a false value
            } catch (Exception ex)
            {
                Print("SENDING TELEGRAM ERROR: " + ex.Message);
                return false;
            }
        }

        // Calculating Volume
    

        // Checking if trading is within trading hours
  

        // Methods that handles the timer elapsed event
      
        // Calculating Stop Loss
        private double GetStopLoss()
        {
            return StopLoss;
        }

        // Calculating Take Profit
        private double GetTakeProfit()
        {
            return TakeProfit;
        }

        #endregion Methods
    }
}
