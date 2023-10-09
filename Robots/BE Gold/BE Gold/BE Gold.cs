using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class BEGold : Robot
    {


        protected override void OnStart()
        {
            Print("Min Vol sym " + Symbol.VolumeInUnitsMin);
            Print("Digits " + Symbol.Digits);
            Print("step " + Symbol.VolumeInUnitsStep);
        }

        public int Round(double i, int v)
        {
            return (int)(Math.Round(i / v) * v);
        }

        protected override void OnBar()
        {
            var Opened_Orders_t = Positions.FindAll("", SymbolName);
            double popips = 0;
            foreach (var po in Opened_Orders_t)
            {
                popips = popips + po.Pips;
            }
            foreach (var po in Opened_Orders_t)
            {

                //Print("Modify node");
                //Break-even
                //po.ModifyStopLossPips(po.EntryPrice);


                Print("Min Vol sym " + Symbol.VolumeInUnitsMin);
                //Modify volume
                Print("Po vol units " + po.VolumeInUnits);
                Print("New vol " + Round(po.VolumeInUnits * 0.5, Convert.ToInt32(Symbol.VolumeInUnitsStep)));
                po.ModifyVolume(Round(po.VolumeInUnits * 0.5, Convert.ToInt32(Symbol.VolumeInUnitsStep)));
            }

        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
