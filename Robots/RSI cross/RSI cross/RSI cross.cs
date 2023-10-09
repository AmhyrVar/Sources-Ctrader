using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class RSIcross : Robot
    {
        [Parameter("Source", Group = "RSI")]
        public DataSeries Source { get; set; }

        [Parameter("Periods", Group = "RSI", DefaultValue = 14)]
        public int Periods { get; set; }

       

        private RelativeStrengthIndex _RSI;

        protected override void OnStart()
        {
            // To learn more about cTrader Automate visit our Help Center:
            // https://help.ctrader.com/ctrader-automate
            _RSI = Indicators.RelativeStrengthIndex(Source, Periods);

        }

        protected override void OnTick()
        {
            // Handle price updates here
        }

        protected override void OnStop()
        {
            // Handle cBot stop here
        }
    }
}