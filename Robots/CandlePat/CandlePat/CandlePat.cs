using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class CandlePat : Robot
    {
        [Parameter(DefaultValue = 0.1)]
        public double Volume { get; set; }

        [Parameter("Direction",DefaultValue = TradeType.Buy)]
        public TradeType Direction { get; set; }

        [Parameter("Number of trades",DefaultValue =2)]
        public int Trade_Number { get; set; }

        [Parameter("TP",DefaultValue = 20)]
        public double TP{ get; set; }


        [Parameter("SL", DefaultValue = 10)]
        public double SL { get; set; }

        [Parameter("Max Spread", DefaultValue = 1)]
        public double Max_Sppread { get; set; }





        protected override void OnStart()
        {
            // Put your initialization logic here
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
