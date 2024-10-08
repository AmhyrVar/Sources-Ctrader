﻿using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class timeoftheday : Robot
    {
        [Parameter(DefaultValue = 0.0)]
        public double Parameter { get; set; }

        protected override void OnStart()
        {
            // Put your initialization logic here
        }

        protected override void OnBar()
        {
            Print(Server.Time.TimeOfDay);
            if (Server.Time.Hour == 22)
            {
                Print("true");
            }
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
