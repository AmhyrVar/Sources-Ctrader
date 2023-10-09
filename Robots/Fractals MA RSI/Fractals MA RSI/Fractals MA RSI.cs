using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;


namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class FractalsMARSI : Robot
    {
        [Parameter(DefaultValue = 0.0)]
        public double Parameter { get; set; }


        private Fractals i_fractal;

        protected override void OnStart()
        {
            i_fractal = Indicators.GetIndicator<Fractals>(10);
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
