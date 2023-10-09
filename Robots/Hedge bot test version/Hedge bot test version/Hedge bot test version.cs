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
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Hedgebottestversion : Robot
    {

        //Size modifier
        [Parameter("Volume modifier", DefaultValue = 1.5)]
        public double VM { get; set; }

        [Parameter("Profit/Loss ratio", DefaultValue = 1.5)]
        public double PLR { get; set; }




        //Delta pips to trigger the Hedge
        [Parameter("Pips to Hedge", DefaultValue = 50)]
        public double DP { get; set; }

        [Parameter("Take profit in pips", DefaultValue = 100)]
        public double TP { get; set; }

        [Parameter("Stop loss in pips", DefaultValue = 50)]
        public double SL { get; set; }


        [Parameter("Positions to hedge labed", DefaultValue = "myPos")]
        public string ToHedgeLabel { get; set; }


        [Parameter("Pips to trigger the Breakeven", DefaultValue = 10)]
        public double PipsToBE { get; set; }

        [Parameter("Pips to add to Breakeven", DefaultValue = 5)]
        public double PipsToAddBE { get; set; }



        List<Position> OPos = new List<Position>();

        List<PendingOrder> HPos = new List<PendingOrder>();




        protected override void OnStart()
        {
            Positions.Opened += PositionsOnOpened;
            Positions.Closed += PositionsClosed;



        }

        protected override void OnTick()
        {



            foreach (var position in Positions)
            {

                if (position.Label == "Hedge Position" && position.SymbolName == SymbolName && position.Pips >= PipsToBE)
                {

                    ModifyPosition(position, position.EntryPrice + 0.01 * PipsToAddBE, position.TakeProfit);
                }


            }


            var HedgeBuyPos = Positions.Find("Hedge Position", SymbolName, TradeType.Buy);

            var MySellPos = Positions.Find(ToHedgeLabel, SymbolName, TradeType.Sell);

            if (HedgeBuyPos != null && MySellPos != null && HedgeBuyPos.NetProfit > 0 && HedgeBuyPos.NetProfit / (-MySellPos.NetProfit) >= PLR)
            {
                Print("Hedge close with profit " + HedgeBuyPos.NetProfit + " initial pos profit " + MySellPos.NetProfit);
                ClosePosition(MySellPos);
                ClosePosition(HedgeBuyPos);

            }



            var HedgeSellPos = Positions.Find("Hedge Position", SymbolName, TradeType.Sell);

            var MyBuyPos = Positions.Find(ToHedgeLabel, SymbolName, TradeType.Buy);


            if (HedgeSellPos != null && MyBuyPos != null && HedgeSellPos.NetProfit > 0 && HedgeSellPos.NetProfit / (-MyBuyPos.NetProfit) >= PLR)
            {
                ClosePosition(MyBuyPos);
                ClosePosition(HedgeSellPos);

                Print("Hedge close");

            }





        }
        /* 
        
        
            var HedgeSellPos = Positions.Find("Hedge Position", SymbolName, TradeType.Sell);

            var MyBuyPos = Positions.Find(ToHedgeLabel, SymbolName, TradeType.Buy);


            if (HedgeSellPos.NetProfit / (-MyBuyPos.NetProfit) >= PLR)
            {
                ClosePosition(MyBuyPos);
                ClosePosition(HedgeSellPos);

                Print("Hedge close");

            }
            
            
            */




        private void PositionsClosed(PositionClosedEventArgs args)
        {



            if (args.Position.Label == ToHedgeLabel)
            {
                var ind = OPos.IndexOf(args.Position);


                CancelPendingOrder(HPos[ind]);
            }

        }

        private void PositionsOnOpened(PositionOpenedEventArgs args)
        {




            if (args.Position.TradeType == TradeType.Buy && args.Position.Label == ToHedgeLabel)
            {
                OPos.Add(args.Position);

                var HEP = args.Position.EntryPrice - (0.01 * DP);

                PlaceStopOrder(TradeType.Sell, args.Position.SymbolName, args.Position.VolumeInUnits * VM, HEP, "Hedge Position", SL, TP);









                foreach (var PO in PendingOrders)
                {
                    if (!HPos.Contains(PO))
                    {
                        HPos.Add(PO);
                    }

                }





            }

            if (args.Position.TradeType == TradeType.Sell && args.Position.Label == ToHedgeLabel)
            {

                OPos.Add(args.Position);

                var HEP = args.Position.EntryPrice + (0.01 * DP);

                PlaceStopOrder(TradeType.Buy, args.Position.SymbolName, args.Position.VolumeInUnits * VM, HEP, "Hedge Position", SL, TP);


                foreach (var PO in PendingOrders)
                {
                    if (!HPos.Contains(PO))
                    {
                        HPos.Add(PO);
                    }

                }

            }
        }

        protected override void OnBar()
        {

            if (Positions.Count == 0)
            {
                ExecuteMarketOrder(TradeType.Sell, SymbolName, 2, "myPos", 100, 20);
            }

        }




    }
}
