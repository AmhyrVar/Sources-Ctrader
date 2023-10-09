using cAlgo.API;
using System;
using System.Globalization;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class NewcBot : Robot
    {
        [Parameter(DefaultValue = 0.01)]
        public double volume { get; set; }

        [Parameter(DefaultValue = 70)]
        public double TP { get; set; }


        [Parameter(DefaultValue = 50)]
        public double SL { get; set; }

        private TimeSpan _startTime, _endTime, _closeTime;

        [Parameter("Start Time", DefaultValue = "20:30:00", Group = "Time Filter")]
        public string StartTime { get; set; }

        [Parameter("End Time", DefaultValue = "21:10:00", Group = "Time Filter")]
        public string EndTime { get; set; }

        [Parameter("Close Time", DefaultValue = "20:50:00", Group = "Time Filter")]
        public string CloseTime { get; set; }

        [Parameter("Positions multiplier", DefaultValue = 5)]
        public int MPos { get; set; }


        [Parameter("Max Positions", DefaultValue = 150)]
        public int MaxPos { get; set; }



        protected override void OnStart()
        {
            if (!TimeSpan.TryParse(StartTime, CultureInfo.InvariantCulture, out _startTime))
            {
                Print("Invalid start time input");

                Stop();
            }

            if (!TimeSpan.TryParse(EndTime, CultureInfo.InvariantCulture, out _endTime))
            {
                Print("Invalid end time input");

                Stop();
            }
            if (!TimeSpan.TryParse(CloseTime, CultureInfo.InvariantCulture, out _closeTime))
            {
                Print("Invalid end time input");

                Stop();
            }

        }







        private DateTime CurrentTime
        {
            get { return Server.TimeInUtc.Add(-Application.UserTimeOffset); }
        }

        private bool IsTimeIncorrect
        {

            get { return ((CurrentTime.TimeOfDay >= _startTime) && (CurrentTime.TimeOfDay <= _endTime)); }
        }

        protected override void OnTick()
        {
            if (CurrentTime.TimeOfDay == _closeTime)
            {
                foreach (var pos in Positions)
                {
                    if (pos.Label == "MinuteMaid" && pos.SymbolName == SymbolName)
                    {
                        ClosePosition(pos);
                    }
                }
            }
        }
        protected override void OnBar()
        {


            if (!IsTimeIncorrect)
            {
                var PCount = 0;
                var totalPos = Positions.FindAll("MinuteMaid", SymbolName);

                while (PCount < MPos && totalPos.Length < MaxPos)
                {

                    ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "MinuteMaid", SL, TP);
                    ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "MinuteMaid", SL, TP);
                    PCount++;
                }
            }


        }

        protected override void OnStop()
        {

        }
    }
}
