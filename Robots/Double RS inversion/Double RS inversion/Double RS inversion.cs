﻿using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;


/* 

 * pour le risque management 35 pips le sl 70 pips le tp 50 pips breakeven 

timeframe D1 j'aimerai essayé h4 aussi mais sa press pas h4.

la startegie : 
cest que quand 2 jour de suite les wick des bougie son a moins de 10 pips de difference 
on rentre 5 pips plus bas pour une vente si cest le haut des bougie qui se sont fermé a moins de 10 pips de différence 
et l'inverse pour un achat donc si les 2 bas de 2 bougie de suite se ferme a moins de 10pips on prend un achat 5 pips plus haut que le wick 
je t'envoie des lien pour que tu comprenne mieux. il faut aussi que le sell ou buy limite se supprime apres 24h si il est pas activé 

les pair EURAUD EURNZD GBPUSD GBPNZ GBPJPY GBPAUD

 * */

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class DoubleRSinversion : Robot
    {
        [Parameter(DefaultValue = 0.1)]
        public double volume { get; set; }
        [Parameter("TP", DefaultValue = 70)]
        public double TP { get; set; }
        [Parameter("SL", DefaultValue = 35)]
        public double SL { get; set; }
        [Parameter("Breakeven pips", DefaultValue = 50)]
        public double BEPips { get; set; }
        protected override void OnBar()
        {
            var upwick1 = Bars.HighPrices.Last(1);
            var upwick2 = Bars.HighPrices.Last(2);
            var lowwick1 = Bars.LowPrices.Last(1);
            var lowwick2 = Bars.LowPrices.Last(2);


            //short scenario
            if (Math.Abs(upwick1 - upwick2) / Symbol.PipSize <= 10)
            {

                DateTime expiry = Server.Time.AddDays(1);

                PlaceStopOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), Math.Min(upwick1, upwick2) + (5 * Symbol.PipSize), "Penwick", SL, TP, expiry);
            }
            if (Math.Abs(lowwick1 - lowwick2) / Symbol.PipSize <= 10)
            {
                //Print("trigger buy");
                DateTime expiry = Server.Time.AddDays(1);
                PlaceStopOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), Math.Max(lowwick1, lowwick2) - (5 * Symbol.PipSize), "Penwick", SL, TP, expiry);
            }
        }
        protected override void OnStart()
        {
            PendingOrders.Cancelled += PendingOrders_Cancelled;
        }
        private void PendingOrders_Cancelled(PendingOrderCancelledEventArgs obj)
        {
            Print("Cancel " + obj.Reason);
        }

        protected override void OnTick()
        {
            foreach (var po in Positions)
            {
                if (po.SymbolName == SymbolName && po.Label == "Penwick" && po.Pips >= BEPips)
                {
                    po.ModifyStopLossPrice(po.EntryPrice);
                }
            }
        }




    }
}
