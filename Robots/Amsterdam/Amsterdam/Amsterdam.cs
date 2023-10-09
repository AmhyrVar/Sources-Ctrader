using System;
using cAlgo.API;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class SimpleBot : Robot
    {
        [Parameter("Profit target in pips", DefaultValue = 10)]
        public double _profitTarget { get; set; }

        [Parameter("SL in pips", DefaultValue = 10)]
        public double _SL{ get; set; }

        [Parameter("Risk %", DefaultValue = 2)]
           public  double p { get; set; }

        private int _sellTime = 3; // hour

        //Volume param
       /* [Parameter("Volume", DefaultValue = 1, MinValue = 0.01, Step = 0.01)]
        public double Volume { get; set; }*/
        protected override void OnStart()
        {
            // nothing to do here
        }

        protected int GetVolume(double SL)
        {

            // x which is 1 = (balance * risk%)/(SL*pipvalue*1000) ROUND TO INT
            var x = Math.Round(( Account.Balance* p) / (100 * SL * Symbol.PipValue * 1000));
            //Account.Balance
            //Convert.ToInt32(double)
            Print("X is "+x);
            return Convert.ToInt32(x * 1000);

        }

        protected override void OnTick()
        {

            TimeSpan amsterdamTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")).TimeOfDay;
            // Get the current time and day of the week
            var time = Server.Time.TimeOfDay;
           
            var dayOfWeek = Server.Time.DayOfWeek;
            var Amsterdam_positions = Positions.FindAll("Amsterdam", SymbolName);

            // Check if it's a day to trade (not Friday or Saturday)
            if (dayOfWeek == DayOfWeek.Friday || dayOfWeek == DayOfWeek.Saturday)
                return;

            // Buy at 23:00 Amsterdam time
            if (time.Hours == 23 && time.Minutes == 0 && Amsterdam_positions.Length == 0)
            {
                Print("Volume of order is " + Symbol.VolumeInUnitsToQuantity(GetVolume(_SL)) + "Max is " + Symbol.VolumeInUnitsToQuantity( Symbol.VolumeInUnitsMax));
                ExecuteMarketOrder(TradeType.Buy, SymbolName, GetVolume(_SL), "Amsterdam", _SL, _profitTarget);
                amsterdamTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")).TimeOfDay;
                //Print("Amstertime" + amsterdamTime + " used time is H " + time.Hours);
            }
            // Sell when x.x pips in profit

            if (Amsterdam_positions.Length > 0)
            {
                foreach (var position in Amsterdam_positions)
                { // If profit target hasn't been reached, sell at 03:00
                    if (time.Hours == _sellTime && time.Minutes == 0 )
                        ClosePosition(position);
                }
                   
            }
            
        }
    }
}
