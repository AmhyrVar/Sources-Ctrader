using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class timecheck : Robot
    {


        protected override void OnStart()
        {
            // Put your initialization logic here
        }

        protected override void OnTick()
        {
            Print(Bars.OpenTimes.LastValue);
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
