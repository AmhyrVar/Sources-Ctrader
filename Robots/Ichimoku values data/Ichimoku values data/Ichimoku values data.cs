using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Ichimokuvaluesdata : Robot
    {
        IchimokuKinkoHyo ichimoku30;

        protected override void OnStart()
        {
            Print("Open " + Bars.OpenPrices.Last(1) + "Close " + Bars.ClosePrices.Last(1));
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
