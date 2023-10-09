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
    public class Kalmanbot : Robot
    {
        [Parameter(DefaultValue = 10000)]
        public double Volume { get; set; }
         [Parameter(DefaultValue = 10)]
        public double TP{ get; set; }
         [Parameter(DefaultValue = 10)]
        public double SL{ get; set; }
         
         private  KalmanFilterLoxx KalmanFilter;

        protected override void OnStart()
        {
           KalmanFilter = Indicators.GetIndicator<KalmanFilterLoxx>(1.0,1.0);
         
        }

        protected override void OnBar()
        {
            var Gpo = Positions.FindAll("Green",SymbolName);
             var Rpo = Positions.FindAll("Red",SymbolName);
            if (!double.IsNaN(KalmanFilter.kfiltG.LastValue) &&  Gpo.Length == 0)
            {
                ExecuteMarketOrder(TradeType.Buy,SymbolName,Volume,"Green",SL,TP);
                /*foreach (var element in Rpo)
                {
                    ClosePosition(element);
                }*/
            }
            
            if (!double.IsNaN(KalmanFilter.kfiltR.LastValue) &&  Rpo.Length == 0)
            {
                ExecuteMarketOrder(TradeType.Sell,SymbolName,Volume,"Red",SL,TP);
               /* foreach (var element in Gpo)
                {
                    ClosePosition(element);
                }*/
                
            }
        }

        protected override void OnStop()
        {
            // Handle cBot stop here
        }
    }
}