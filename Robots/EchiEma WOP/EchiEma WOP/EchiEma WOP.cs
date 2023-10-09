using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class EchiEmaWOP : Robot
    {
        [Parameter(DefaultValue = 0.0)]
        public double Parameter { get; set; }
        AverageTrueRange ATR;

        //call the indicators
        protected override void OnStart()
        {
            ATR = Indicators.AverageTrueRange(5, MovingAverageType.Simple);



            // Put your initialization logic here
            //call ATR stops with 5 periods MA simple Weight 3.5 true highlow yes
            // when we're under we short when over we long ratio TP/SL 2:1 M15
            //Call ATR too

        }
        /*RMA*/
        /*plot(rma(close, 15))

// same on pine, but much less efficient
pine_rma(x, y) =>
	alpha = y
    sum = 0.0
    sum := (x + (alpha - 1) * nz(sum[1])) / alpha
plot(pine_rma(close, 15))*/



        protected override void OnTick()
        {
            // Put your core logic here
        }
        /*Plot emaup and emadw, conversionLine and baseLine
            */

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
