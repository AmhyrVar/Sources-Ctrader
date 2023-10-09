using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class TimeLord : Robot
    {


        protected override void OnStart()
        {

        }

        protected override void OnBar()
        {
            Print(Bars.OpenTimes.LastValue);
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
