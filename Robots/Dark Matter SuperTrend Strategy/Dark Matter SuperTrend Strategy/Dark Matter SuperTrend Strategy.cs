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
    public class DarkMatterSuperTrendStrategy : Robot
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
        [Parameter("Total Orders", DefaultValue = 30, Group = "Grid Settings")]
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
        [Parameter("Trigger (pips) ", DefaultValue = 20, Group = "SuperTrend")]
        public double BETriggerPips { get; set; }
        [Parameter("Steps (pips) ", DefaultValue = 10, Group = "SuperTrend")]
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

        [Parameter("Trade Buy", DefaultValue = true, Group = "Risk Management")]
        public bool Buy { get; set; }

        [Parameter("Trade Sell", DefaultValue = true, Group = "Risk Management")]
        public bool Sell { get; set; }

        [Parameter("Volume Type", Group = "Risk Management")]
        public VolumeType VolType { get; set; }






        [Parameter("Max Buy Trades", DefaultValue = 1, Group = "Risk Management")]
        public int MaxBuyPositions { get; set; }

        [Parameter("Max Sell Trades", DefaultValue = 1, Group = "Risk Management")]
        public int MaxSellPositions { get; set; }

        [Parameter("Max Trades Per Day", DefaultValue = 10, Group = "Risk Management")]
        public int MaxTradesPerDay { get; set; }

       

        

       
        [Parameter("Use Break Even", DefaultValue = false, Group = "Break Even")]
        public bool MoveToBE { get; set; }

        [Parameter("Trigger (pips)", DefaultValue = 5, Group = "Break Even")]
        public double TriggerBEPips { get; set; }

        [Parameter("Add to Break Even", DefaultValue = 1, Group = "Break Even")]
        public double PipsToAddToBE { get; set; }

       


        [Parameter("Use Partial TP", DefaultValue = false, Group = "Partial TP")]
        public bool UsePartialTP { get; set; }

        [Parameter("Trigger (pips)", DefaultValue = 5, Group = "Partial TP")]
        public double PartialTPTrigger { get; set; }

        [Parameter("Partial TP (%)", DefaultValue = 50, Group = "Partial TP")]
        public double PartialTP { get; set; }

        [Parameter("Use News Manager", DefaultValue = false, Group = "News Manager")]
        public bool IncludeNewsReleasePause { get; set; }

        [Parameter("Close Positions", DefaultValue = false, Group = "News Manager")]
        public bool ClosePositionsOnNewsRelease { get; set; }

   

        [Parameter("Use Trade Entry Periods", Group = "Trade Entry Periods", DefaultValue = false)]
        public bool UseTradeEntryPeriods { get; set; }

        [Parameter("Sunday", Group = "Trade Entry Periods", DefaultValue = true)]
        public bool Sunday { get; set; }

        [Parameter("Sunday (From)", Group = "Trade Entry Periods", DefaultValue = "11:00")]
        public string SundayFrom { get; set; }

        [Parameter("Sunday (To)", Group = "Trade Entry Periods", DefaultValue = "16:00")]
        public string SundayTo { get; set; }

        [Parameter("Monday", Group = "Trade Entry Periods", DefaultValue = true)]
        public bool Monday { get; set; }

        [Parameter("Monday (From)", Group = "Trade Entry Periods", DefaultValue = "11:00")]
        public string MondayFrom { get; set; }

        [Parameter("Monday (To)", Group = "Trade Entry Periods", DefaultValue = "16:00")]
        public string MondayTo { get; set; }

        [Parameter("Tuesday", Group = "Trade Entry Periods", DefaultValue = true)]
        public bool Tuesday { get; set; }

        [Parameter("Tuesday (From)", Group = "Trade Entry Periods", DefaultValue = "11:00")]
        public string TuesdayFrom { get; set; }

        [Parameter("Tuesday (To)", Group = "Trade Entry Periods", DefaultValue = "16:00")]
        public string TuesdayTo { get; set; }

        [Parameter("Wednesday", Group = "Trade Entry Periods", DefaultValue = true)]
        public bool Wednesday { get; set; }

        [Parameter("Wednesday (From)", Group = "Trade Entry Periods", DefaultValue = "11:00")]
        public string WednesdayFrom { get; set; }

        [Parameter("Wednesday (To)", Group = "Trade Entry Periods", DefaultValue = "16:00")]
        public string WednesdayTo { get; set; }

        [Parameter("Thursday", Group = "Trade Entry Periods", DefaultValue = true)]
        public bool Thursday { get; set; }

        [Parameter("Thursday (From)", Group = "Trade Entry Periods", DefaultValue = "11:00")]
        public string ThursdayFrom { get; set; }

        [Parameter("Thursday (To)", Group = "Trade Entry Periods", DefaultValue = "16:00")]
        public string ThursdayTo { get; set; }

        [Parameter("Friday", Group = "Trade Entry Periods", DefaultValue = true)]
        public bool Friday { get; set; }

        [Parameter("Friday (From)", Group = "Trade Entry Periods", DefaultValue = "11:00")]
        public string FridayFrom { get; set; }

        [Parameter("Friday (To)", Group = "Trade Entry Periods", DefaultValue = "16:00")]
        public string FridayTo { get; set; }

        [Parameter("Saturday", Group = "Trade Entry Periods", DefaultValue = true)]
        public bool Saturday { get; set; }

        [Parameter("Saturday (From)", Group = "Trade Entry Periods", DefaultValue = "11:00")]
        public string SaturdayFrom { get; set; }

        [Parameter("Saturday (To)", Group = "Trade Entry Periods", DefaultValue = "16:00")]
        public string SaturdayTo { get; set; }

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
            timer.Elapsed += OnTimedEventForNews;
            timer.Interval = 60000.0;
            timer.Enabled = true;

            // Handle the bar opened event for daily bars
            MarketData.GetBars(TimeFrame.Daily).BarOpened += BasecBot_BarOpened;

            // Handle the position opened and position closed events
            Positions.Opened += Positions_Opened;
            Positions.Closed += Positions_Closed;
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

        private void BasecBot_BarOpened(BarOpenedEventArgs obj)
        {

            // reset Daily Trades counter to 0 on the creation of each new daily bar
            _noOfDailyTrades = 0;
        }

        // Method that handles the OnBar event
        protected override void OnBar()
        {
            if (_isStop)
            {
                return;
            }

            // We check if the Supertrend indicator has reversed to an upwards trend
            if (double.IsNaN(_superTrend.UpTrend.Last(2)) && double.IsNaN(_superTrend.DownTrend.Last(1)))
            {

                // If Close on reversal is selected, then we close all positions of the opposite direction
                if (CloseOnReversal)
                    foreach (var position in Positions.Where(x => x.Label == Instance && x.TradeType == TradeType.Sell))
                        position.Close();

                // We execute the trade with a Buy direction
                Trade(TradeType.Buy);
            }

            // We check if the Supertrend indicator has reversed to an downwards trend
            if (double.IsNaN(_superTrend.DownTrend.Last(2)) && double.IsNaN(_superTrend.UpTrend.Last(1)))
            {

                // If Close on reversal is selected, then we close all positions of the opposite direction
                if (CloseOnReversal)
                    foreach (var position in Positions.Where(x => x.Label == Instance && x.TradeType == TradeType.Buy))
                        position.Close();

                // We execute the trade with a Sell direction
                Trade(TradeType.Sell);
            }
        }

        // Method that executes the trading logic
        private new void Trade(TradeType tradeType)
        {
            // If trading is not paused due to News events
            if (!_isPaused)
            {

                // If trading is enabled i.e. we are not not outside trading hours
                if (TradingEnabled())
                {

                    // If spread is below the max set spread
                    if ((Symbol.Ask - Symbol.Bid) / Symbol.PipSize < MaxSpread)
                    {

                        // If maximum daily trades have not been reached
                        if (_noOfDailyTrades < MaxTradesPerDay)
                        {
                            // If the number positions is lower that the max allowed number of positions for the selected direction
                            if (Positions.Count(p => p.SymbolName == SymbolName && p.Label == Instance && p.TradeType == tradeType) < (tradeType == TradeType.Buy ? MaxBuyPositions : MaxSellPositions))
                            {

                                // If number of positions is lower than open positions
                                if (Positions.Count(p => p.SymbolName == SymbolName && p.Label == Instance) < MaxOpenPositions)
                                {

                                    // If trading is allowed for the selected direction
                                    if (tradeType == TradeType.Buy ? Buy : Sell)
                                        ExecuteMarketRangeOrderAsync(tradeType, SymbolName, GetVolume(), MaxSlippage, tradeType == TradeType.Buy ? Symbol.Ask : Symbol.Bid, Instance, GetStopLoss(), GetTakeProfit());

                                    // If notofications are enabled, a signal message is sent to the relevant Telegram account
                                    if (SendNotifications)
                                    {
                                        var message = tradeType == TradeType.Buy ? "Buy Signal" : "Sell Signal" + Environment.NewLine;
                                        message += "Dark Matter SuperTrend Strategy" + Environment.NewLine;
                                        SendTelegramMessage(message, ChatID, BotToken);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Print("Max Trades Per Day Reached");
                        }
                    }
                    else
                    {
                        Print("Max Spread Reached");
                    }
                }
                else
                {
                    Print("Trading is Paused due to News");
                }
            }
            else
            {
                Print("Trading Disabled");
            }
        }

        // Methods handling the OnTick Event
        protected override void OnTick()
        {

            // If Max Drawdown is set to true, then we execute the following code block
            if (UseMaxDrawdown)
            {

                // if current equity is higher that previously recorded max equity, then we record a new max equity value
                _maxEquity = Math.Max(Account.Equity, _maxEquity);

                // If max equity drawdown is higher than the max allowed equity drawdowm
                if (_maxEquity - Account.Equity > MaxDrawdown / 100.0 * _maxEquity)
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

            // If Break Even is set to true, then we execute the following code block
            if (MoveToBE)
            {

                // we iterate through all the instance's positions
                foreach (var position in Positions.Where(x => x.Label == Instance))
                {

                    // If position's pips is above the break even trigger pips
                    if (position.Pips > TriggerBEPips)
                    {

                        // We check the position's trade type and excute the relevant code block
                        if (position.TradeType == TradeType.Buy)
                        {

                            // We calculate the stop loss based on the Pips To Add parameter
                            var stopLoss = Math.Round(position.EntryPrice + PipsToAddToBE * Symbol.PipSize, Symbol.Digits);

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
                            var stopLoss = Math.Round(position.EntryPrice - PipsToAddToBE * Symbol.PipSize, Symbol.Digits);

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

            // If Trailing Stop Loss is set to true, then we execute the following code block
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

            // If Partial Take Profit is set to true, then we execute the following code block
            if (UsePartialTP)
            {

                // we iterate through all the instance's positions
                foreach (var position in Positions.Where(x => x.Label == Instance))
                {

                    // If position's pips is above the partial take profit trigger and the position has not taken any partial take profit yet
                    if (position.Pips > PartialTPTrigger && !_positionsPT.Contains(position))
                    {

                        // We modify the volume based on the % of the partial TP 
                        position.ModifyVolume(Symbol.NormalizeVolumeInUnits(position.VolumeInUnits * ((100 - PartialTP) / 100.0), RoundingMode.Down));

                        //We add the position to the relevant list
                        _positionsPT.Add(position);

                        Print("Partial Take Profit Triggered");
                    }
                }
            }
        }

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
        private double GetVolume()
        {

            // Depending on the selected volume typw, we perform the relevant volume calculation.
            switch (VolType)
            {

                // We calculate the volume based on the balance percentage
                case VolumeType.BalancePercentage:
                    {
                        var maxAmountRisked = Account.Balance * (Volume / 100);
                        return Symbol.NormalizeVolumeInUnits(maxAmountRisked / (GetStopLoss() * Symbol.PipValue), RoundingMode.Down);
                    }

                // We calculate the volume based on the equity percentage
                case VolumeType.EquityPercentage:
                    {
                        var maxAmountRisked = Account.Equity * (Volume / 100);
                        return Symbol.NormalizeVolumeInUnits(maxAmountRisked / (GetStopLoss() * Symbol.PipValue), RoundingMode.Down);
                    }

                // We convert the lots to volume
                case VolumeType.Lots:
                    return Symbol.QuantityToVolumeInUnits(Volume);

                // We return the quantity as volume
                case VolumeType.Units:
                    return Volume;

                // We calculate the volume based on the cash amount
                case VolumeType.CashAmount:
                    {
                        return Symbol.NormalizeVolumeInUnits(Volume / (GetStopLoss() * Symbol.PipValue), RoundingMode.Down);
                    }

                // We calculate the volume based on the leverage
                case VolumeType.Leverage:
                    {
                        return Symbol.NormalizeVolumeInUnits(Account.Equity / Symbol.Bid * Volume);
                    }
                default:

                    return Volume;
            }
        }

        // Checking if trading is within trading hours
        private bool TradingEnabled()
        {

            // If the feature is disabled, retrun true
            if (!UseTradeEntryPeriods)
                return true;

            switch (Server.Time.DayOfWeek)
            {

                // Check Monday trading time
                case DayOfWeek.Monday:
                    {
                        var @from = TimeSpan.ParseExact(MondayFrom, "hh\\:mm", null);
                        var to = TimeSpan.ParseExact(MondayTo, "hh\\:mm", null);
                        return Monday && Server.Time.TimeOfDay >= @from && Server.Time.TimeOfDay < to;
                    }

                // Check Tuesday trading time
                case DayOfWeek.Tuesday:
                    {
                        var @from = TimeSpan.ParseExact(TuesdayFrom, "hh\\:mm", null);
                        var to = TimeSpan.ParseExact(TuesdayTo, "hh\\:mm", null);
                        return Tuesday && Server.Time.TimeOfDay >= @from && Server.Time.TimeOfDay < to;
                    }

                // Check Wednesday trading time
                case DayOfWeek.Wednesday:
                    {
                        var @from = TimeSpan.ParseExact(WednesdayFrom, "hh\\:mm", null);
                        var to = TimeSpan.ParseExact(WednesdayTo, "hh\\:mm", null);
                        return Wednesday && Server.Time.TimeOfDay >= @from && Server.Time.TimeOfDay < to;
                    }

                // Check Thurday trading time
                case DayOfWeek.Thursday:
                    {
                        var @from = TimeSpan.ParseExact(ThursdayFrom, "hh\\:mm", null);
                        var to = TimeSpan.ParseExact(ThursdayTo, "hh\\:mm", null);
                        return Thursday && Server.Time.TimeOfDay >= @from && Server.Time.TimeOfDay < to;
                    }

                // Check Friday trading time
                case DayOfWeek.Friday:
                    {
                        var @from = TimeSpan.ParseExact(FridayFrom, "hh\\:mm", null);
                        var to = TimeSpan.ParseExact(FridayTo, "hh\\:mm", null);
                        return Friday && Server.Time.TimeOfDay >= @from && Server.Time.TimeOfDay < to;
                    }


                // Check Saturday trading time
                case DayOfWeek.Saturday:
                    {
                        var @from = TimeSpan.ParseExact(SaturdayFrom, "hh\\:mm", null);
                        var to = TimeSpan.ParseExact(SaturdayTo, "hh\\:mm", null);
                        return Friday && Server.Time.TimeOfDay >= @from && Server.Time.TimeOfDay < to;
                    }


                // Check Sunday trading time
                case DayOfWeek.Sunday:
                    {
                        var @from = TimeSpan.ParseExact(SundayFrom, "hh\\:mm", null);
                        var to = TimeSpan.ParseExact(SundayTo, "hh\\:mm", null);
                        return Friday && Server.Time.TimeOfDay >= @from && Server.Time.TimeOfDay < to;
                    }
            }
            return false;
        }

        // Methods that handles the timer elapsed event
        private void OnTimedEventForNews(object sender, ElapsedEventArgs ev)
        {

            // We check if the cBot should be paused for trading news
            if (IncludeNewsReleasePause)
            {

                // We initialize the list of positions and set a flag to false
                var list = Positions.FindAll(Instance, SymbolName).ToList();
                bool flag = false;


                try
                {
                    // We initialuze the news alerts
                   

                    // We iterate through the currency list
               
                    if (!flag)
                    {

                        // if the flag is false we resule the cBot execution, by setting the flag to pause
                        _isPaused = false;
                    }
                } catch (Exception ex)
                {
                    Print("Failed reading registry for news release manager:  " + ex.InnerException.ToString(), new object[0]);
                }
            }
        }

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
