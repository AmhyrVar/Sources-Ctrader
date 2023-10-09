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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.FullAccess)]
    public class RenkoStrategy7v3022 : Robot
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
        [Parameter("Number of bars from EMA to close", DefaultValue = 7, MinValue = 1)]
        public int Zbars { get; set; }

        [Parameter("Send Telegram Notifications", Group = "Telegram Notifications")]
        public bool SendNotifications { get; set; }

        [Parameter("Bot Token", DefaultValue = "", Group = "Telegram Notifications")]
        public string BotToken { get; set; }

        [Parameter("Chat ID", DefaultValue = "", Group = "Telegram Notifications")]
        public string ChatID { get; set; }

        [Parameter("Sunday stop from", DefaultValue = 17, Group = "No trade time")]
        public int SundayStop { get; set; }
        [Parameter("Sunday stop to", DefaultValue = 19, Group = "No trade time")]
        public int SundayStopTo { get; set; }

        [Parameter("Weekdays stop from", DefaultValue = 17, Group = "No trade time")]
        public int Weektop { get; set; }
        [Parameter("Weekdays stop to", DefaultValue = 18, Group = "No trade time")]
        public int WeekStopTo { get; set; }


        #endregion Parameters

        #region Variables
        private ExponentialMovingAverage _ema;
        public bool _allowBuy;
        public bool _allowSell;
        public bool label0_closed;
        public double l0_closeprice;
        public bool stop;
        public int barseeker;

        public int barstop;
        //Range high and low definition
       
        public bool RangeState;
        
        //list tuple tradetype bool and price level (false for usual true for trigger range)
        private List<Tuple<TradeType, bool, double>> ZTriggers = new List<Tuple<TradeType, bool, double>>();


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
            RangeState = false;


        }
      

     // On a l'entrée en Range trigger par la variable RangeState
     // On a les trade dans le Range 
     //On code la sortie du Range

      protected override void OnTick()
        {
            var p = Positions.FindAll("stop",SymbolName);
            foreach (var pos in p)
            {
                if (pos.TradeType == TradeType.Buy && Symbol.Bid > _ema.Result.LastValue)
                {
                    ClosePosition(pos);
                }
                if (pos.TradeType == TradeType.Sell && Symbol.Ask < _ema.Result.LastValue)
                {
                    ClosePosition(pos);
                }
            }
        }

        protected override void OnBar()
        {
            var p = PendingOrders.Where(order => order.Label == "stop");

            if (barstop == 0 && p.Count() > 0)
            {
                barstop++;
            }

            if (barstop == 16)
            {
                foreach (var pos in p)
                {
                    CancelPendingOrder(pos);
                }
            }

            if (RangeState && (Oversixteen() || Undersixteen()))
            {
                Print("Exit range");
                RangeState = false;
            }

           if (ZTriggers.Count >= 2 && !RangeState )
            {
                Print($"TC {ZTriggers.Count} Trigger 1 {ZTriggers[0].Item1} 2 {ZTriggers[0].Item2} 3 {ZTriggers[0].Item3}");
                if ( ZTriggers[ZTriggers.Count -1].Item2 ==  ZTriggers[ZTriggers.Count - 2].Item2  && ZTriggers[ZTriggers.Count - 1].Item2 == true)
                {
                    RangeState = true;
                   
                }

            }

            if (RangeState && TimeCheck())
            {

                if (EightOverEma() && Positions.Count(p => p.SymbolName == SymbolName && p.Label.Contains("buy") && p.TradeType == TradeType.Buy) == 0)
                {
                    var label = "buy";
                    ExecuteMarketOrderAsync(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), label);
                    PlaceStopOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), Bars.OpenPrices.Last(3), "stop");
                    //close it when price action touches ema at tick
                    Print("We have 8 bars over EMA here");
                    barstop = 0;
                }

                if (EightUnderEma() && Positions.Count(p => p.SymbolName == SymbolName && p.Label.Contains("sell") && p.TradeType == TradeType.Sell) == 0)
                {
                    var label = "sell";
                    Print("We have 8 bars Under EMA here");
                    ExecuteMarketOrderAsync(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), label);
                    PlaceStopOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), Bars.OpenPrices.Last(3), "stop");
                    barstop = 0;
                }
            }



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

            if (Bars.OpenPrices.Last(1) > _ema.Result.Last(1)&& Bars.ClosePrices.Last(1) > _ema.Result.Last(1) && Positions.Count(p => p.SymbolName == SymbolName && p.Label.Contains("buy") && p.TradeType == TradeType.Buy) == 0)
            {
                _allowSell = true;
                
            }
            if (Bars.OpenPrices.Last(1) < _ema.Result.Last(1) && Bars.ClosePrices.Last(1) < _ema.Result.Last(1) && Positions.Count(p => p.SymbolName == SymbolName && p.Label.Contains("sell") &&  p.TradeType == TradeType.Sell) == 0)
            {
                _allowBuy = true;
            }

            Chart.DrawIcon(Bars.Count.ToString(), ChartIconType.Circle, Bars.OpenTimes.Last(1),_ema.Result.Last(1),Color.Red);  
          
            if (Positions.Count(p => p.SymbolName == SymbolName && p.TradeType == TradeType.Buy) == 0)
            {
                if (CanBuy() && !RangeState && TimeCheck())
                {                   
                    {
                        
                        foreach (var position in Positions.Where(p => p.SymbolName == SymbolName && p.TradeType == TradeType.Sell))
                            position.Close();

                        if (_allowBuy)
                        {
                            var label = "";
                            for (int i = 0; i < SimultaneousTrades; i++)
                            {
                                label = "buy" + i;
                               

                                if (label ==  "buy" + 0)
                                {
                                 
                            var level = _ema.Result.Last(1) + Zbars* (Math.Abs(Bars.OpenPrices.Last(1) - Bars.ClosePrices.Last(1)));
                                    var tp0 = (level - Bars.ClosePrices.Last(1)) / Symbol.PipSize;
                                    ExecuteMarketOrderAsync(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), label, StopLoss, tp0);
                                   
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
                if (CanSell() && !RangeState && TimeCheck())
                {
                    {
                     
                        foreach (var position in Positions.Where(p => p.SymbolName == SymbolName && p.TradeType == TradeType.Buy))
                            position.Close();
                        if (_allowSell)
                        {
                            var label = "";
                            for (int i = 0; i < SimultaneousTrades; i++)


                            {
                               


                               label = "sell" + i;
                              

                                if (label == "sell" + 0)
                                {
                                  
                                    var level = _ema.Result.Last(1) - Zbars * (Math.Abs(Bars.OpenPrices.Last(1) - Bars.ClosePrices.Last(1)));
                                    var tp0 = ( Bars.ClosePrices.Last(1)-level) / Symbol.PipSize;
                                    ExecuteMarketOrderAsync(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(Volume), label, StopLoss, tp0);
                                    stop = false;
                                }

                                else
                                {
                                   
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

        private int getHour_Start()
        {
            if (Server.Time.DayOfWeek == DayOfWeek.Sunday)
            {
                return SundayStop;
            }
            else { return Weektop; }
        }
        private int getHour_Stop()
        {
            if (Server.Time.DayOfWeek == DayOfWeek.Sunday)
            {
                return SundayStopTo;
            }
            else { return WeekStopTo; }
        }
        private bool TimeCheck()
        {

            if (Server.Time.Hour >= getHour_Start() && Server.Time.Hour < getHour_Stop())
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private bool Oversixteen()
        {
            var close = false;
            var i = 0;
            var bars = 0;
            var listofOpen = new List<double>();
            while (!close)
            {

                if (Bars.ClosePrices.Last(i) > _ema.Result.Last(i) && Bars.OpenPrices.Last(i) > _ema.Result.Last(i) && Bars.ClosePrices.Last(i) > Bars.OpenPrices.Last(i))
                {


                    if (!listofOpen.Contains(Bars.OpenPrices.Last(i)))
                    {
                        listofOpen.Add(Bars.OpenPrices.Last(i));

                        bars++;
                    }

                    i++;

                }
                else if (Bars.ClosePrices.Last(i) > _ema.Result.Last(i) && Bars.OpenPrices.Last(i) > _ema.Result.Last(i) && Bars.ClosePrices.Last(i) < Bars.OpenPrices.Last(i))
                {
                    i++;

                }
                else { close = true; }
            }

            if (bars == 17)
            {
                Print("The bars are ************************");
                foreach (var bar in listofOpen)
                {
                    Print(bar);
                }
                return true;
            }

            else
            {
                return false;
            }
        }
        private bool EightOverEma()
        {
            var close = false;
            var i = 0;
            var bars = 0;
            var listofOpen = new List<double>();
            while (!close)
            {
                
                if (Bars.ClosePrices.Last(i) > _ema.Result.Last(i) && Bars.OpenPrices.Last(i) > _ema.Result.Last(i) && Bars.ClosePrices.Last(i) > Bars.OpenPrices.Last(i))
                {
                    
                   
                    if (!listofOpen.Contains(Bars.OpenPrices.Last(i)))
                    {
                        listofOpen.Add(Bars.OpenPrices.Last(i));

                        bars++;
                    }

                    i++;

                }
                else if (Bars.ClosePrices.Last(i) > _ema.Result.Last(i) && Bars.OpenPrices.Last(i) > _ema.Result.Last(i) && Bars.ClosePrices.Last(i) < Bars.OpenPrices.Last(i))
                {
                    i++;
                    
                }
                else { close = true; }
            }

            if (bars == 9)
            {
                Print("The bars are ************************");
                foreach (var bar in listofOpen)
                {
                    Print(bar);
                }
                return true;
            }
                
            else
            {
                return false;
            }
        }
        private bool Undersixteen()
        {
            var close = false;
            var i = 0;
            var bars = 0;
            var listofOpen = new List<double>();
            while (!close)
            {
                if (Bars.ClosePrices.Last(i) < _ema.Result.Last(i) && Bars.OpenPrices.Last(i) < _ema.Result.Last(i) && Bars.ClosePrices.Last(i) < Bars.OpenPrices.Last(i))
                {

                    if (!listofOpen.Contains(Bars.OpenPrices.Last(i)))
                    {
                        listofOpen.Add(Bars.OpenPrices.Last(i));

                        bars++;
                    }
                    i++;
                }
                else if (Bars.ClosePrices.Last(i) < _ema.Result.Last(i) && Bars.OpenPrices.Last(i) < _ema.Result.Last(i) && Bars.ClosePrices.Last(i) > Bars.OpenPrices.Last(i))
                {
                    i++;

                }
                else { close = true; }
            }

            if (bars == 17)
            {
                Print("The bars are ************************");
                foreach (var bar in listofOpen)
                {
                    Print(bar);
                }
                return true;
            }

            else
            {
                return false;
            }
        }
        private bool EightUnderEma()
        {
            var close = false;
            var i = 0;
            var bars = 0;
            var listofOpen = new List<double>();
            while (!close)
            {
                if (Bars.ClosePrices.Last(i) < _ema.Result.Last(i) && Bars.OpenPrices.Last(i) < _ema.Result.Last(i) && Bars.ClosePrices.Last(i) < Bars.OpenPrices.Last(i))
                {
                    
                    if (!listofOpen.Contains(Bars.OpenPrices.Last(i)))
                    {
                        listofOpen.Add(Bars.OpenPrices.Last(i));

                        bars++;
                    }
                    i++;
                }
                else if (Bars.ClosePrices.Last(i) < _ema.Result.Last(i) && Bars.OpenPrices.Last(i) < _ema.Result.Last(i) && Bars.ClosePrices.Last(i) > Bars.OpenPrices.Last(i))
                {
                    i++;

                }
                else { close = true; }
            }

            if (bars == 9)
            {
                Print("The bars are ************************");
                foreach (var bar in listofOpen)
                {
                    Print(bar);
                }
                return true;
            }
               
            else
            {
                return false;
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
            if (obj.Position.Label == "buy0" || obj.Position.Label == "sell0")
            {
                stop = true;
                barseeker = 0;
                var barsize = Math.Abs(Convert.ToDouble(Bars.OpenPrices.Last(1) - Bars.ClosePrices.Last(1))) / Symbol.PipSize;


                if (obj.Position.TradeType == TradeType.Buy)
                {
                    var i = 0;
                    while (Bars.ClosePrices.Last(i) > _ema.Result.Last(i) && Bars.OpenPrices.Last(i) > _ema.Result.Last(i))
                    {
                        i++;
                    }
                    Print("I Buy = " + i);
                    if (i >= Zbars)
                    {
                        ZTriggers.Add(new Tuple<TradeType, bool, double>(TradeType.Buy, false, (double)obj.Position.TakeProfit));
                        
                    }
                    else { ZTriggers.Add(new Tuple<TradeType, bool, double>(TradeType.Buy, true, (double)obj.Position.TakeProfit)); }
                }


                if (obj.Position.TradeType == TradeType.Sell)
                {
                    var i = 0;
                    while (Bars.ClosePrices.Last(i) < _ema.Result.Last(i) && Bars.OpenPrices.Last(i) < _ema.Result.Last(i))
                    {
                        i++;
                    }
                    Print("I Sell = " + i);
                    if (i>= Zbars)
                    {
                        ZTriggers.Add(new Tuple<TradeType, bool, double>(TradeType.Sell, false, (double)obj.Position.TakeProfit));
                    }
                    else { ZTriggers.Add(new Tuple<TradeType, bool, double>(TradeType.Sell, true, (double)obj.Position.TakeProfit)); }
                }


                /*if (obj.Position.Pips >= Zbars*barsize)
                {
                    Print("Business as usual");
                    ZTriggers.Append(false);
                }
                else
                {
                    Print($"Missed Z pos pips {obj.Position.Pips} the Z pips are { Zbars * barsize}");
                    ZTriggers.Append(true);
                }*/
            }

              

           

          
          

        }


    
        protected override void OnStop()
        {
          
        }

        #endregion
    }
}