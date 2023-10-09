/*
 *
 *    Renko Strategy
 *
 *    Customer: [Anthony Deric] [dericanthony123@yahoo.com]
 *    Developer: ClickAlgo <development@clickalgo.com>, Donald Davies <donald@clickalgo.com>
 *
 *    Changelog:
 *      v1.0.0 (October 07, 2022)
 *          - First release
 *      v1.1.0 (October 14, 2022)
 *          - Bug Fixes
 *      v1.2.0 (December 19, 2022)
 *          - Bug Fixes
 */

using cAlgo.API;
using cAlgo.API.Indicators;
using System;
using System.Linq;
using System.Net;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.FullAccess)]
    public class RenkoStrategy72 : Robot
    {
        #region Parameters
        [Parameter(DefaultValue = 0.01)]
        public EntryType Type { get; set; }

        [Parameter(DefaultValue = 0.01)]
        public double Volume { get; set; }

        [Parameter(DefaultValue = 10)]
        public double TakeProfit { get; set; }

        [Parameter(DefaultValue = 10)]
        public double StopLoss { get; set; }

        [Parameter(DefaultValue = 14)]
        public int Periods { get; set; }

        [Parameter("Simultaneous trades", DefaultValue = 10, MinValue = 1)]
        public int SimultaneousTrades { get; set; }

        [Parameter("Send Telegram Notifications", Group = "Telegram Notifications")]
        public bool SendNotifications { get; set; }

        [Parameter("Bot Token", DefaultValue = "", Group = "Telegram Notifications")]
        public string BotToken { get; set; }

        [Parameter("Chat ID", DefaultValue = "", Group = "Telegram Notifications")]
        public string ChatID { get; set; }

        #endregion Parameters

        #region Variables
        private ExponentialMovingAverage _ema;
        public bool _allowBuy;
        public bool _allowSell;

        public enum EntryType
        {
            OneBrick,
            ThreeBricks
        }

        #endregion

        #region Methods

        protected override void OnStart()
        {            
            // Declare Telegram protocol for Tls2 security
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            _ema = Indicators.ExponentialMovingAverage(Bars.ClosePrices, Periods);
            _allowBuy = false;
            _allowSell = false;
        }

        protected override void OnBar()
        {
            if (Bars.OpenPrices.Last(1) > _ema.Result.Last(1) && Positions.Count(p => p.SymbolName == SymbolName && p.TradeType == TradeType.Buy) == 0)
            {
                _allowSell = true;
            }
            if (Bars.OpenPrices.Last(1) < _ema.Result.Last(1) && Positions.Count(p => p.SymbolName == SymbolName && p.TradeType == TradeType.Sell) == 0)
            {
                _allowBuy = true;
            }

            Chart.DrawIcon(Bars.Count.ToString(), ChartIconType.Circle, Bars.OpenTimes.Last(1),_ema.Result.Last(1),Color.Red);  
            Print("EMA Last: " + _ema.Result.Last(1));

            // Handle price updates here
            if (Positions.Count(p => p.SymbolName == SymbolName && p.TradeType == TradeType.Buy) == 0)
            {
                if (CanBuy())
                {                   
                    {
                        Print("Bar Open Price: " + Bars.OpenPrices.Last(3));
                        Print("EMA: " + _ema.Result.Last(3));
                        Print("Three bars above.");
                        
                        foreach (var position in Positions.Where(p => p.SymbolName == SymbolName && p.TradeType == TradeType.Sell))
                            position.Close();

                        if (_allowBuy)
                        {
                            for (int i = 0; i < SimultaneousTrades; i++)
                            {
                                ExecuteMarketOrderAsync(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), "", StopLoss, TakeProfit);
                            }
                            // If send notifications is set to true, then a message is constructed and sent
                            if (SendNotifications)
                            {
                                // Constructing the message
                                var message = "Buy Position Opened" + Environment.NewLine;
                                message += "Symbol: " + SymbolName + Environment.NewLine;
                                // Sending the message about the open position
                                SendTelegramMessage(message, ChatID, BotToken);
                            }
                        }
                        _allowBuy = false;
                    }
                }
            }

            if (Positions.Count(p => p.SymbolName == SymbolName && p.TradeType == TradeType.Sell) == 0)
            {
                if (CanSell())
                {
                    {
                        Print("Bar Open Price: " + Bars.OpenPrices.Last(3));
                        Print("EMA: " + _ema.Result.Last(3));
                        Print("Three bars below.");
                        foreach (var position in Positions.Where(p => p.SymbolName == SymbolName && p.TradeType == TradeType.Buy))
                            position.Close();
                        if (_allowSell)
                        {
                            for (int i = 0; i < SimultaneousTrades; i++)
                            {
                                ExecuteMarketOrderAsync(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), "", StopLoss, TakeProfit);
                            }
                            // If send notifications is set to true, then a message is constructed and sent
                            if (SendNotifications)
                            {
                                // Constructing the message
                                var message = "Sell Position Opened" + Environment.NewLine;
                                message += "Symbol: " + SymbolName + Environment.NewLine;
                                // Sending the message about the open position
                                SendTelegramMessage(message, ChatID, BotToken);
                            }
                        }
                        _allowSell = false;
                    }
                }
            }

        }

        private bool CanSell()
        {
            if (Type == EntryType.ThreeBricks)
            {
                return Bars.ClosePrices.Last(1) < Bars.ClosePrices.Last(2) && Bars.ClosePrices.Last(2) < Bars.ClosePrices.Last(3) && Bars.ClosePrices.Last(3) < Bars.ClosePrices.Last(4) && Bars.OpenPrices.Last(3) < _ema.Result.Last(3); ;
            }
            else
            {
                return Bars.ClosePrices.Last(1) < Bars.ClosePrices.Last(2) && Bars.OpenPrices.Last(1) < _ema.Result.Last(1); 
            }
        }

        private bool CanBuy()
        {
            if (Type == EntryType.ThreeBricks)
            {
                return Bars.ClosePrices.Last(1) > Bars.ClosePrices.Last(2) && Bars.ClosePrices.Last(2) > Bars.ClosePrices.Last(3) && Bars.ClosePrices.Last(3) > Bars.ClosePrices.Last(4) && Bars.OpenPrices.Last(3) > _ema.Result.Last(3);
            }
            else
            {
                return Bars.ClosePrices.Last(1) > Bars.ClosePrices.Last(2) && Bars.OpenPrices.Last(1) > _ema.Result.Last(1);
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
            }
            catch (Exception ex)
            {
                Print("SENDING TELEGRAM ERROR: " + ex.Message);
                return false;
            }
        }

        protected override void OnStop()
        {
            // Handle cBot stop here
        }

        #endregion
    }
}