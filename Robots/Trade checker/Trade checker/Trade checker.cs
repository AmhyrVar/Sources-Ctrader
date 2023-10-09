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
    public class Tradechecker : Robot
    {
        [Parameter(DefaultValue = "Hello world!")]
        public string Message { get; set; }
        
        [Parameter(DefaultValue = 500 )]
        public double Limit { get; set; }
        
        public double DailyTotal;
        public DayOfWeek _NowDay;

        protected override void OnStart()
        {
             Positions.Closed += PositionsOnClosed;
             DailyTotal = 0;
            _NowDay =Server.Time.DayOfWeek;
        }
        private void PositionsOnClosed(PositionClosedEventArgs args)
{
    var pos = args.Position;
    // etc...
    Print("Position opened at {0}", pos.GrossProfit);
    
    DailyTotal = DailyTotal + pos.NetProfit;
}

        protected override void OnBar()
        {
            if (Server.Time.DayOfWeek != _NowDay) //Restart DailyTotal when day changes
            {
                DailyTotal = 0;
            }
            var Activedown = 0.0;
            foreach (var po in Positions)
            {
                Activedown = Activedown + po.NetProfit;
            }
            var Drawdown = DailyTotal+Activedown;
            if (Drawdown >= -Limit)
            {
                foreach (var po in Positions)
                {
                    ClosePosition(po);
                }
                Stop();
            }
        }

        protected override void OnStop()
        {
            // Handle cBot stop here
        }
    }
}