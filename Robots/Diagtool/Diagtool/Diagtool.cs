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
    public class Diagtool : Robot
    {
        [Parameter(DefaultValue = "Hello world!")]
        public string Message { get; set; }

        protected override void OnStart()
        {
            
        }

        protected override void OnBar()
        {
        Print($"{Bars.OpenTimes.LastValue}");
        }

        protected override void OnStop()
        {
            // Handle cBot stop here
        }
    }
}