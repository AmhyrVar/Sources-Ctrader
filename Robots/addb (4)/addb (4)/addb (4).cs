using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class addb4 : Robot
    {
        //Detect High and Low between two times
        //Define the two times 
        [Parameter("Trade Time", DefaultValue = "1:30:00 PM")]
        public string TradeTime { get; set; }
        [Parameter("Cancel Time", DefaultValue = "2:00:00 PM")]
        public string CancelTime { get; set; }

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
          

            var dtday = new DateTime(Server.TimeInUtc.Day, Server.TimeInUtc.Month, Server.TimeInUtc.Year);

            Print(" dtday " + dtday);
            string thisday = dtday.ToString();
            Print(" thisday " + thisday);
            string dateString = thisday + TradeTime;
            Print(" dateString " + dateString);
            DateTime date = DateTime.ParseExact(dateString, "dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
            Print(" date " + date);
        }

        protected override void OnBar()
        {

            var dtday = new DateTime(Server.TimeInUtc.Day, Server.TimeInUtc.Month, Server.TimeInUtc.Year);
            string thisday = dtday.ToString();
            string startdateString = thisday + TradeTime;
            string stopdateString = thisday + CancelTime;
            DateTime startdate = DateTime.ParseExact(startdateString, "dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
            DateTime stopdate = DateTime.ParseExact(stopdateString, "dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

      

            if (Server.TimeInUtc < stopdate  && hooked)
            {
                hooked = false;
                tradestate = 0;
               
               
            }
            var x = 0;
            if (!hooked && Server.TimeInUtc > stopdate)
            {
                low = double.PositiveInfinity;
                high = 0.0;
                while(Bars.OpenTimes.Last(x)>= stopdate)
                {
                   // Print("While x " + x+" bars " + Bars.OpenTimes.Last(x) + " _stop " + _startTime);
                    if (Bars.OpenTimes.Last(x) < stopdate)
                    {
                       // Print("trigger");
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

         

            
            }

            
        }


    

    

        protected override void OnTick()
        {
            var dtday = new DateTime(Server.TimeInUtc.Day, Server.TimeInUtc.Month, Server.TimeInUtc.Year);
            string thisday = dtday.ToString();
            string startdateString = thisday + TradeTime;
            string stopdateString = thisday + CancelTime;
            DateTime startdate = DateTime.ParseExact(startdateString, "dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
            DateTime stopdate = DateTime.ParseExact(stopdateString, "dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

            if (hooked && Server.TimeInUtc > stopdate.AddMinutes(5) && tradestate == 0)
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




