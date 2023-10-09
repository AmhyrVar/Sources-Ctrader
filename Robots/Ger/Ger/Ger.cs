using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;


using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class Ger : Robot
    {
        [Parameter(DefaultValue = "Hello world!")]
        public string Message { get; set; }
        
        //470 122 829
        
          private SuperProfit superProfit;
     








        protected override void OnStart()
        {

            double close = Bars.ClosePrices.LastValue;
            superProfit = Indicators.GetIndicator<SuperProfit>(10);
        
       
            // To learn more about cTrader Automate visit our Help Center:
            // https://help.ctrader.com/ctrader-automate

            Print(Message);
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