using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class retracearrow : Robot
    {
        [Parameter("Inventory Retracement Percentage %", DefaultValue = 45)]
        public int z { get; set; }

        protected override void OnStart()
        {
            // Put your initialization logic here
        }

        protected override void OnBar()
        {
            var a = Math.Abs(Bars.HighPrices.Last(0) - Bars.LowPrices.Last(0));
            // Candle Body
            var b = Math.Abs(Bars.ClosePrices.Last(0) - Bars.OpenPrices.Last(0));
            // Percent to Decimal
            var c = z / 100;

            var rv = def_rv(a, b, c);

            var x = Bars.LowPrices.LastValue + (c * a);
            var y = Bars.HighPrices.LastValue - (c * a);

            var sl = def_sl(rv, y);
            var ss = def_ss(rv, x);

            if (sl)
            {
                Chart.DrawIcon(Bars.OpenTimes.LastValue.ToString(), ChartIconType.DownArrow, Bars.OpenTimes.LastValue, Bars.HighPrices.LastValue, Color.Blue);
                Print("sl");

            }
            if (ss)
            {
                Chart.DrawIcon(Bars.OpenTimes.LastValue.ToString(), ChartIconType.UpArrow, Bars.OpenTimes.LastValue, Bars.LowPrices.LastValue, Color.Red);
                Print("ss");
            }

        }

        public bool def_rv(double a, double b, double c)
        {
            if (b < c * a)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool def_sl(bool rv, double y)
        {
            if (rv && Bars.HighPrices.LastValue > y && Bars.ClosePrices.LastValue < y && Bars.OpenPrices.LastValue < y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool def_ss(bool rv, double x)
        {
            if (rv && Bars.LowPrices.LastValue < x && Bars.ClosePrices.LastValue > x && Bars.OpenPrices.LastValue > x)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
