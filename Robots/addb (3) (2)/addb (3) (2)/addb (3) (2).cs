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
    public class addb32 : Robot
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

        [Parameter("Volume in quantity", DefaultValue = 0.01)]
        public double vol { get; set; }


        private DateTime _startTime;
        private DateTime _stopTime;

        public bool hooked;
        public double high;
        public double low;
        public int tradestate;
        public TradeType first_direction;

        protected override void OnStart()
        {
            hooked = false;
            tradestate = 0;
            _startTime = Server.Time.Date.AddHours(StartHour);
            _startTime = _startTime.AddMinutes(StartMinute);
            // Stop Time is the next day at 06:00:00
            _stopTime = Server.Time.Date.AddHours(StopHour);
            _stopTime = _stopTime.AddMinutes(StopMinute);

         
        }

        protected override void OnBar()
        {
            var _stopTimes = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StopHour, StopMinute, 0);
            var _startTimes = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StartHour, StartMinute, 0);

            

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

                Print("log High " + high + " low " + low);
                var a = Chart.DrawHorizontalLine("buy", high, Color.Green, 5);
                var b = Chart.DrawHorizontalLine("sell", low, Color.Red, 5);


            }

            
        }

        protected override void OnTick()
        {
            var _stopTimes = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StopHour, StopMinute, 0);
            var _startTimes = new DateTime(Server.TimeInUtc.Year, Server.TimeInUtc.Month, Server.TimeInUtc.Day, StartHour, StartMinute, 0);

            if (hooked && Server.TimeInUtc > _stopTimes.AddMinutes(5) && tradestate == 0)
            {
                if (Symbol.Ask > high)
                {
                    var e = ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(vol), "bot1", null, TP);
                    ModifyPosition(e.Position, low, e.Position.TakeProfit);
                    first_direction = TradeType.Buy;
                    tradestate = 1;
                }
                if (Symbol.Bid < low)
                {
                    var e = ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(vol), "bot1", null, TP);
                    ModifyPosition(e.Position, high, e.Position.TakeProfit);
                    first_direction = TradeType.Sell;
                    tradestate = 1;
                }
            }


            if (hooked &&  tradestate == 1)
            {
                if (Symbol.Ask > high && first_direction == TradeType.Sell)
                {
                    var e = ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(2 * vol), "bot2", null, TP);
                    ModifyPosition(e.Position, low, e.Position.TakeProfit);
                   
                    tradestate = 2;
                }
                if (Symbol.Bid < low && first_direction == TradeType.Buy)
                {
                    var e = ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(2 * vol), "bot2", null, TP);
                    ModifyPosition(e.Position, high, e.Position.TakeProfit);
                   
                    tradestate = 2;
                }
            }


            if (hooked  && tradestate == 2)
            {
                if (Symbol.Ask > high && first_direction == TradeType.Buy)
                {
                    var e = ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(3 * vol), "bot3", null, TP);
                    ModifyPosition(e.Position, low, e.Position.TakeProfit);
                   
                    tradestate = 3;
                }
                if (Symbol.Bid < low && first_direction == TradeType.Sell)
                {
                    var e = ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(3*vol), "bot3", null, TP);
                    ModifyPosition(e.Position, high, e.Position.TakeProfit);
                   
                    tradestate = 3;
                }
            }


        }
    }
}