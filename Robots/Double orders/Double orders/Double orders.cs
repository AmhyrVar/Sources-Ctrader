using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using System.Collections.Generic;
namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Doubleorders : Robot
    {

        protected override void OnStart()
        {
            List<Position> arrpos = new List<Position>();
            foreach (var po in Positions)
            {
                if (po.SymbolName == SymbolName)
                {

                    arrpos.Add(po);
                }


            }

            List<Position> SortedList = arrpos.OrderBy(o => o.VolumeInUnits).ToList();
            foreach (var pos in SortedList)
            {
                Print("modifying po " + pos);
                Print("modifying vol " + pos.VolumeInUnits);
                pos.ModifyVolume(pos.VolumeInUnits * 2);
                Print("To vol " + pos.VolumeInUnits);

            }
            Stop();
        }


    }
}
