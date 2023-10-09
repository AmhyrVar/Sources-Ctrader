using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class addbv33 : Robot
    {
        //Detect High and Low between two times
        //Define the two times 
        [Parameter("Start hour", DefaultValue = 2)]
        public int StartHour { get; set; }
        [Parameter("Start minute", DefaultValue = 30)]
        public int StartMinute { get; set; }
        [Parameter("To Hour", DefaultValue = 3)]
        public int StopHour { get; set; }
        [Parameter("To Minute", DefaultValue = 0)]
        public int StopMinute { get; set; }

        [Parameter("Take profit in pips", DefaultValue = 100)]
        public double TP { get; set; }

        [Parameter("position 1 volume", DefaultValue = 0.01)]
        public double vol1 { get; set; }
        [Parameter("position 2 volume", DefaultValue = 0.01)]
        public double vol2 { get; set; }
        [Parameter("position 3 volume", DefaultValue = 0.01)]
        public double vol3 { get; set; }

        [Parameter("Spread", DefaultValue = 5.0)]
        public double SSpread { get; set; }


        private DateTime _startTime;
        private DateTime _stopTime;

        public bool hooked;
        public double high;
        public double low;
        public int tradestate;
        public TradeType first_direction;


        public DateTime StartDay;
        public bool canT1;
        public bool canT2;
        public bool canT3;

        protected override void OnStart()
        {
            hooked = false;
            tradestate = 0;
            _startTime = Server.Time.Date.AddHours(StartHour);
            _startTime = _startTime.AddMinutes(StartMinute);
            // Stop Time is the next day at 06:00:00
            _stopTime = Server.Time.Date.AddHours(StopHour);
            _stopTime = _stopTime.AddMinutes(StopMinute);

            StartDay = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day);


            Positions.Closed += OnPositionsClosed;
            canT1 = true;
            canT2 = false;
            canT3 = false;
         


        }
        private void OnPositionsClosed(PositionClosedEventArgs args)
        {

            if (args.Position.Label == "bot1" && args.Position.NetProfit < 0)
            {
                canT2 = true;
            }
            if (args.Position.Label == "bot2" && args.Position.NetProfit < 0)
            {
                canT3 = true;
            }
        }
        private void CheckNewDay()
        {
            var thisDay = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day);
            if (thisDay != StartDay)
            {
                StartDay = thisDay;
                canT1 = true;
                canT2 = false;
                canT3 = false;

            }
        }
        protected override void OnBar()
        {
            var _stopTimes = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StopHour, StopMinute, 0);
            var _startTimes = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StartHour, StartMinute, 0);

            CheckNewDay();

            if (Server.TimeInUtc < _stopTimes  && hooked)
            {
                hooked = false;
                tradestate = 0;
                
               
            }
            var x = 0;
            if (!hooked && Server.TimeInUtc > _stopTimes)
            {
                low = double.PositiveInfinity;
                high = 0.0;
                while(Bars.OpenTimes.Last(x)>=_startTimes)
                {
                 
                    if (Bars.OpenTimes.Last(x) < _stopTimes )
                    {
                       
                        if (Bars.LowPrices.Last(x) < low)
                        {
                            low =Bars.LowPrices.Last(x);
                        }
                        if (Bars.HighPrices.Last(x) > high)
                        {
                            high = Bars.HighPrices.Last(x);
                        }

                    }

                    x++;
                
                }

                hooked = true;

                Print("log High " + high + " high with spread "+ (high+5)+ " low " + low);
                var a = Chart.DrawHorizontalLine("buy", high, Color.Green, 5);
                var b = Chart.DrawHorizontalLine("sell", low, Color.Red, 5);


            }

            
        }

        protected override void OnTick()
        {
            var _stopTimes = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StopHour, StopMinute, 0);
            var _startTimes = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StartHour, StartMinute, 0);

            if (hooked && Server.TimeInUtc > _stopTimes.AddMinutes(5) && tradestate == 0 && canT1 && vol1 !=0)
            {
                if (Symbol.Ask > high+ SSpread)
                {
                    var e = ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(vol1), "bot1", null, TP);
                    ModifyPosition(e.Position, low, e.Position.TakeProfit);
                    first_direction = TradeType.Buy;
                    tradestate = 1;
                    canT1 = false;
                }
                if (Symbol.Bid < low- SSpread)
                {
                    var e = ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(vol1), "bot1", null, TP);
                    ModifyPosition(e.Position, high, e.Position.TakeProfit);
                    first_direction = TradeType.Sell;
                    tradestate = 1;
                    canT1 = false;
                }
            }


            if (hooked &&  tradestate == 1 && canT2 && vol2 != 0)
            {
                if (Symbol.Ask > high + SSpread && first_direction == TradeType.Sell)
                {
                    var e = ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(vol2), "bot2", null, TP);
                    ModifyPosition(e.Position, low, e.Position.TakeProfit);
                   
                    tradestate = 2;
                    canT2 = false;
                }
                if (Symbol.Bid < low- SSpread && first_direction == TradeType.Buy)
                {
                    var e = ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits( vol2), "bot2", null, TP);
                    ModifyPosition(e.Position, high, e.Position.TakeProfit);
                   
                    tradestate = 2;
                    canT2 = false;
                }
            }


            if (hooked  && tradestate == 2&& canT3 && vol3 !=0)
            {
                if (Symbol.Ask > high + SSpread && first_direction == TradeType.Buy)
                {
                    var e = ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(vol3), "bot3", null, TP);
                    ModifyPosition(e.Position, low, e.Position.TakeProfit);
                   
                    tradestate = 3;
                    canT3 = false;
                }
                if (Symbol.Bid < low - SSpread  && first_direction == TradeType.Sell)
                {
                    var e = ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(vol3), "bot3", null, TP);
                    ModifyPosition(e.Position, high, e.Position.TakeProfit);
                   
                    tradestate = 3;
                    canT3 = false;
                }
            }


        }
    }
}