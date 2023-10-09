using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class Pendingorders : Robot
    {
        [Parameter("CSV directory", DefaultValue = "C:\\direction.csv")]
        public string CSV_File { get; set; }

        List<string> listA = new List<string>();
        List<string> listB = new List<string>();

        protected override void OnStart()
        {
            //delete this
            PlaceStopOrder(TradeType.Buy, SymbolName, 10000, 1.22, "myLimitOrder");
            PlaceStopOrder(TradeType.Sell, SymbolName, 10000, 1.19, "myStopOrder");

            PlaceStopOrder(TradeType.Buy, "GBPUSD", 10000, 1.4, "myLimitOrder");
            PlaceStopOrder(TradeType.Sell, "GBPUSD", 10000, 1.1, "myStopOrder");


        }

        protected override void OnBar()
        {

            //make this a parameter
            using (var reader = new StreamReader(CSV_File))
            {

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    listA.Add(values[0]);
                    listB.Add(values[1]);
                }
            }


            //Print("listA 0 contains " + listA[0]);
            //EURUSD AKA SymbolName
            //Print("listB 0 contains " + listB[0]);
            //buy

            //On to Pending Orders Now

            //iterate listA
            //if there's a pending order with a Symbol on listA treat it
            // either way continue iteration

            //when buy only allow buys when sell only allow sells when neutral do nothing

            var count = 0;
            while (count < listA.Count)
            {
                //Print(listA[count]);
                // listA[count] iterates throu symbols

                //now let's get Pending Orders symbols
                var POs = PendingOrders;
                //iterate through pending orders and get their symbols

                var POcount = 0;
                while (POcount < POs.Count)
                {
                    //Print("Pending Order number " + (POcount + 1) + " is on " + POs[POcount].SymbolName);
                    //POs[POcount].SymbolName  symbol of the pending order
                    if (POs[POcount].SymbolName == listA[count])
                    {
                        //Print("There's a position in " + POs[POcount].SymbolName + " it's direction is " + listB[count]);
                        if (listB[count] == "buy" && POs[POcount].TradeType == TradeType.Sell)
                        {
                            CancelPendingOrder(POs[POcount]);
                        }
                        if (listB[count] == "sell" && POs[POcount].TradeType == TradeType.Buy)
                        {
                            CancelPendingOrder(POs[POcount]);
                        }
                    }

                    POcount++;
                }






                count++;




            }

            listA.Clear();
            listB.Clear();

        }





        protected override void OnStop()
        {

        }
    }
}
