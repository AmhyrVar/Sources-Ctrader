﻿using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class trail : Robot
    {
        [Parameter(DefaultValue = 0.0)]
        public double Parameter { get; set; }

        protected override void OnStart()
        {
            ExecuteMarketOrder(TradeType.Buy,SymbolName,1000,"CustomPOs",500,500,"nada",true,stopLoss
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
