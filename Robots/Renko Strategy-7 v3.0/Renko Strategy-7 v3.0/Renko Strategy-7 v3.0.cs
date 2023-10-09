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
    public class RenkoStrategy7v30 : Robot
    {
        #region Parameters
        
        

        [Parameter(DefaultValue = 0.01)]
        public double Volume { get; set; }

        [Parameter(DefaultValue = 10)]
        public double TakeProfit { get; set; }

        [Parameter(DefaultValue = 10)]
        public double StopLoss { get; set; }

        [Parameter(DefaultValue = 14)]
        public int Periods { get; set; }

        [Parameter("Simultaneous trades", DefaultValue = 2, MinValue = 1)]
        public int SimultaneousTrades { get; set; }
        [Parameter("Number of bars from EMA to close", DefaultValue = 2, MinValue = 1)]
        public int Zbars { get; set; }

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
        public bool label0_closed;
        public double l0_closeprice;
        public bool stop;
        public int barseeker;

      
        #endregion

        #region Methods

        protected override void OnStart()
        {
            barseeker = 0;
            label0_closed = false;
            stop = false;
            // Declare Telegram protocol for Tls2 security
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            _ema = Indicators.ExponentialMovingAverage(Bars.ClosePrices, Periods);
            _allowBuy = false;
            _allowSell = false;

            Positions.Closed += Positions_Closed;
        }
      

     
        protected override void OnBar()
        {
            foreach (var po in Positions)
            {
                if(po.TradeType == TradeType.Buy && po.SymbolName ==SymbolName && po.Label.Contains("buy")&& Bars.OpenPrices.Last(1) < _ema.Result.Last(1) && Bars.ClosePrices.Last(1) < _ema.Result.Last(1) )
                {
                    ClosePosition(po);
                }

                if (po.TradeType == TradeType.Sell && po.SymbolName == SymbolName && po.Label.Contains("sell") && Bars.OpenPrices.Last(1) > _ema.Result.Last(1) && Bars.ClosePrices.Last(1) > _ema.Result.Last(1))
                {
                    ClosePosition(po);
                }
             
            }

            if (Bars.OpenPrices.Last(1) > _ema.Result.Last(1)&& Bars.ClosePrices.Last(1) > _ema.Result.Last(1) && Positions.Count(p => p.SymbolName == SymbolName && p.TradeType == TradeType.Buy) == 0)
            {
                _allowSell = true;
                
            }
            if (Bars.OpenPrices.Last(1) < _ema.Result.Last(1) && Bars.ClosePrices.Last(1) < _ema.Result.Last(1) && Positions.Count(p => p.SymbolName == SymbolName && p.TradeType == TradeType.Sell) == 0)
            {
                _allowBuy = true;
            }

            Chart.DrawIcon(Bars.Count.ToString(), ChartIconType.Circle, Bars.OpenTimes.Last(1),_ema.Result.Last(1),Color.Red);  
          //  Print("EMA Last: " + _ema.Result.Last(1));

            // Handle price updates here
            if (Positions.Count(p => p.SymbolName == SymbolName && p.TradeType == TradeType.Buy) == 0)
            {
                if (CanBuy())
                {                   
                    {
                       // Print("Bar Open Price: " + Bars.OpenPrices.Last(3));
                       // Print("EMA: " + _ema.Result.Last(3));
                       // Print("Three bars above.");
                        
                        foreach (var position in Positions.Where(p => p.SymbolName == SymbolName && p.TradeType == TradeType.Sell))
                            position.Close();

                        if (_allowBuy)
                        {
                            var label = "";
                            for (int i = 0; i < SimultaneousTrades; i++)
                            {
                                label = "buy" + i;
                               Print("Label is *********** " + label);

                                if (label ==  "buy" + 0)
                                {
                                  // var barPips = (Math.Abs(Bars.OpenPrices.Last(1) - Bars.ClosePrices.Last(1))) / Symbol.PipSize;
                            var level = _ema.Result.Last(1) + Zbars* (Math.Abs(Bars.OpenPrices.Last(1) - Bars.ClosePrices.Last(1)));
                                    var tp0 = (level - Bars.ClosePrices.Last(1)) / Symbol.PipSize;
                                    ExecuteMarketOrderAsync(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), label, StopLoss, tp0);
                                    //ModifyPosition(a.TradeResult.Position, a.TradeResult.Position.VolumeInUnits, tp0);
                                    stop = false;
                                }

                                if (label != "buy" + 0)
                                {
                                    ExecuteMarketOrderAsync(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), label, StopLoss, TakeProfit);
                                    stop = false;
                                }
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
                       // Print("Bar Open Price: " + Bars.OpenPrices.Last(3));
                       // Print("EMA: " + _ema.Result.Last(3));
                      //  Print("Three bars below.");
                        foreach (var position in Positions.Where(p => p.SymbolName == SymbolName && p.TradeType == TradeType.Buy))
                            position.Close();
                        if (_allowSell)
                        {
                            var label = "";
                            for (int i = 0; i < SimultaneousTrades; i++)


                            {
                               


                               label = "sell" + i;
                                Print("Label is *********** " + label);

                                if (label == "sell" + 0)
                                {
                                    Print("Sell0");
                                    var level = _ema.Result.Last(1) - Zbars * (Math.Abs(Bars.OpenPrices.Last(1) - Bars.ClosePrices.Last(1)));
                                    var tp0 = ( Bars.ClosePrices.Last(1)-level) / Symbol.PipSize;
                                    ExecuteMarketOrderAsync(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), label, StopLoss, tp0);
                                    stop = false;
                                }

                                else
                                {
                                    Print("Sell1");
                                    ExecuteMarketOrderAsync(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), label, StopLoss, TakeProfit);
                                    stop = false;
                                }
                               
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
            
                return Bars.OpenPrices.Last(1) < _ema.Result.Last(1) && Bars.ClosePrices.Last(1) < _ema.Result.Last(1) && Bars.OpenPrices.Last(1) > Bars.ClosePrices.Last(1);
            
        }

        private bool CanBuy()
        {
           
                return Bars.OpenPrices.Last(1) > _ema.Result.Last(1) && Bars.ClosePrices.Last(1) > _ema.Result.Last(1) && Bars.OpenPrices.Last(1) < Bars.ClosePrices.Last(1);
            
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
                   // Print(result);
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

        private void Positions_Closed(PositionClosedEventArgs obj)
        {
            if (obj.Position.Label == "buy1" || obj.Position.Label == "sell1")
            {
                stop = true;
                barseeker = 0;
            }

               // Print("AMIR PO Closed label " + obj.Position.Label);

            if (obj.Position.Label == "buy2" && obj.Position.SymbolName == SymbolName)
            {
                label0_closed = false;
                l0_closeprice = double.NaN;
            }

            if (obj.Position.Label == "buy0" && obj.Position.SymbolName == SymbolName)
            {

                label0_closed = true;
                foreach (var po in Positions)
                {
                    if (po.Label == "buy1" && po.SymbolName == SymbolName)
                    {
                        //GetTP(po);
                     

                    }
                }
            }
            if (obj.Position.Label == "sell0" && obj.Position.SymbolName == SymbolName)
            {
                //Print("Sell0 Closed");
                label0_closed = true;
                foreach (var po in Positions)
                {
                    if (po.Label == "sell1" && po.SymbolName == SymbolName)
                    {
                        //Print("Called for sell 1 "+po);
                        //GetTP(po);


                    }
                }
            }

        }


        private void GetTP(Position po)
        {

            Print("AMIR GETPO Called to amend " + po.Label);
            if (po.TradeType == TradeType.Buy)
            {
                Print("Get TP called internal");
                HistoricalTrade _hist = History.FindLast("buy0");
                l0_closeprice = _hist.ClosingPrice;
               
               
                var x = barseeker+1;
                
                while (stop == false && Positions.FindAll("buy1").Length != 0)
                {
                    
                   
                   
                    if (Bars.ClosePrices.Last(x) > Bars.OpenPrices.Last(x) && Math.Max(Bars.ClosePrices.Last(x - 1), Bars.OpenPrices.Last(x - 1)) < Bars.ClosePrices.Last(x)
                      && Math.Max(Bars.ClosePrices.Last(x + 1), Bars.OpenPrices.Last(x + 1)) < Bars.ClosePrices.Last(x) &&
                      Bars.ClosePrices.Last(x) > l0_closeprice && Bars.ClosePrices.Last(x) > _ema.Result.Last(x) && Bars.ClosePrices.Last(x + 1) > _ema.Result.Last(x + 1) && Bars.OpenPrices.Last(x + 1) > _ema.Result.Last(x + 1)
                      && Bars.ClosePrices.Last(x - 1) > _ema.Result.Last(x - 1) && Bars.OpenPrices.Last(x - 1) > _ema.Result.Last(x - 1)
                      )
                    {

                        ModifyPosition(po, po.StopLoss, Math.Round(Bars.ClosePrices.Last(x), Symbol.Digits)); //Math.Round(DefaultSTL,Symbol.Digits
                        stop = true;
                        Print(" AAA TP MODIF BUY");
                    }
                    if (x >= 100000)
                    {
                        stop = true;
                    }

                    x++;
                }

            }
            if (po.TradeType == TradeType.Sell)
            {
                HistoricalTrade _hist = History.FindLast("sell0");
                l0_closeprice = _hist.ClosingPrice;


                
                var x = barseeker+1;
               while (stop == false && Positions.FindAll("sell1").Length != 0)
                {
                   
                    if (Bars.ClosePrices.Last(x) < Bars.OpenPrices.Last(x)
                        && Math.Min(Bars.ClosePrices.Last(x - 1), Bars.OpenPrices.Last(x - 1)) > Bars.ClosePrices.Last(x)
                        && Math.Min(Bars.ClosePrices.Last(x + 1), Bars.OpenPrices.Last(x + 1)) > Bars.ClosePrices.Last(x)
                         && Bars.ClosePrices.Last(x) < l0_closeprice
                        && Bars.ClosePrices.Last(x) < _ema.Result.Last(x)
                        && Bars.ClosePrices.Last(x + 1) < _ema.Result.Last(x + 1)
                        && Bars.OpenPrices.Last(x + 1) < _ema.Result.Last(x + 1)
                        && Bars.ClosePrices.Last(x - 1) < _ema.Result.Last(x - 1)
                        && Bars.OpenPrices.Last(x - 1) < _ema.Result.Last(x - 1)
                        )
                    {
                      //  Print("THOSE WORK");
                        //po.ModifyTakeProfitPrice(Bars.ClosePrices.Last(x));
                        ModifyPosition(po, po.StopLoss,Math.Round( Bars.ClosePrices.Last(x),Symbol.Digits)); //Math.Round(DefaultSTL,Symbol.Digits
                        Print(" AAA TP MODIF SELL");
                        stop = true;
                    }



                     if (x >= 100000)
                    {
                        stop = true;
                    }

                    x++;
                }

            }

        }
        protected override void OnStop()
        {
            // Handle cBot stop here
        }

        #endregion
    }
}