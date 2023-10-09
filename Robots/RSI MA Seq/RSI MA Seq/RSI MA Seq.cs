using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class RSIMASeq : Robot
    {

        [Parameter("Source")]
        public DataSeries SourceSeries { get; set; }
        [Parameter("RSI Periods", DefaultValue = 14)]
        public int Periods { get; set; }
        private RelativeStrengthIndex _rsi;

        protected override void OnStart()
        {
            _rsi = Indicators.RelativeStrengthIndex(SourceSeries, Periods);
        }

        private string RSI_State(int candle)
        {
            if (_rsi.Result.Last(candle) > 75)
            {
                return "OverBought";
            }
            if (_rsi.Result.Last(candle) < 25)
            {
                return "OverSold";
            }
            else
            {
                return "Null";
            }
        }



        protected override void OnTick()
        {
            // Put your core logic here
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
