using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Super_trend_bot : Robot
    {
        [Parameter(DefaultValue = 0.0)]
        public double Parameter { get; set; }


        [Parameter(DefaultValue = 0.1)]
        public double volume { get; set; }

        [Parameter(DefaultValue = 50)]
        public double SL { get; set; }

        [Parameter(DefaultValue = 100)]
        public double TP { get; set; }

        private bool longtrades;

        Supertrend st;
        protected override void OnStart()
        {
            st = Indicators.Supertrend(10, 3);


            if (!double.IsNaN(st.DownTrend.Last(1)))
            {
                longtrades = true;

            }
            else
            {
                longtrades = false;
            }
        }

        protected override void OnBar()
        {

            //buy when last 2 red and last 1 green
            if (double.IsNaN(st.DownTrend.Last(1)) && double.IsNaN(st.UpTrend.Last(2)) && longtrades)
            {
                foreach (var po in Positions)
                {
                    if (po.TradeType == TradeType.Sell && po.Label == "Supertrend" && po.SymbolName == SymbolName)
                    {
                        ClosePosition(po);
                    }
                }
                ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Supertrend", SL, TP);
                longtrades = false;
            }

            if (double.IsNaN(st.DownTrend.Last(2)) && double.IsNaN(st.UpTrend.Last(1)) && !longtrades)
            {
                foreach (var po in Positions)
                {
                    if (po.TradeType == TradeType.Buy && po.Label == "Supertrend" && po.SymbolName == SymbolName)
                    {
                        ClosePosition(po);
                    }
                }
                ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Supertrend", SL, TP);
                longtrades = true;
            }

        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
