using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using CsvHelper;

using System.Collections.Generic;
using System.IO;


namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class Ichimokuanalysis : Robot
    {


        [Parameter(DefaultValue = 9)]
        public int periodFast { get; set; }

        [Parameter(DefaultValue = 26)]
        public int periodMedium { get; set; }

        [Parameter(DefaultValue = 52)]
        public int periodSlow { get; set; }


        [Parameter("Lot size", DefaultValue = 0.01)]
        public double Volume { get; set; }

        [Parameter("tpmult", DefaultValue = 5)]
        public double mult { get; set; }


        [Parameter("Risk %", DefaultValue = 5)]
        public double RiskP { get; set; }

        public double AccStartBal;

        public Position GlobalID;

        IchimokuKinkoHyo ichimoku;
        IchimokuKinkoHyo ichimokuH1;
        IchimokuKinkoHyo ichimokuH4;

        List<OpenTrade> records = new List<OpenTrade>();
        List<CloseTrade> Closerecords = new List<CloseTrade>();


        protected override void OnStart()
        {
            Positions.Closed += PositionsOnClosed;

            ichimoku = Indicators.IchimokuKinkoHyo(periodFast, periodMedium, periodSlow);
            ichimokuH1 = Indicators.IchimokuKinkoHyo(periodFast * 4, periodMedium * 4, periodSlow * 4);
            ichimokuH4 = Indicators.IchimokuKinkoHyo(periodFast * 8, periodMedium * 8, periodSlow * 8);

            AccStartBal = Account.Balance;


        }

        private void PositionsOnClosed(PositionClosedEventArgs args)
        {
            Print("po pips" + args.Position.Id);
            WriteClosed(args.Position);
        }

        public class CloseTrade
        {
            public string id { get; set; }

            public double NetRealized { get; set; }
        }
        public class OpenTrade
        {
            public string id { get; set; }
            //tradeID type ?
            public double price { get; set; }
            //closeprice
            public double M15T { get; set; }
            public double M15K { get; set; }
            public double M15SSA { get; set; }
            public double M15SSB { get; set; }

            public double M15Chikou { get; set; }
            public double M15CPrice { get; set; }
            public double M15CKijun { get; set; }
            public double M15CTenkan { get; set; }
            public double M15CSSA { get; set; }
            public double M15CSSB { get; set; }

            public double H1T { get; set; }
            public double H1K { get; set; }
            public double H1SSA { get; set; }
            public double H1SSB { get; set; }
            public double H1Chikou { get; set; }
            public double H1CPrice { get; set; }
            public double H1CKijun { get; set; }
            public double H1CTenkan { get; set; }
            public double H1CSSA { get; set; }
            public double H1CSSB { get; set; }

            public double H4T { get; set; }
            public double H4K { get; set; }
            public double H4SSA { get; set; }
            public double H4SSB { get; set; }

            public double H4Chikou { get; set; }
            public double H4CPrice { get; set; }
            public double H4CKijun { get; set; }
            public double H4CTenkan { get; set; }
            public double H4CSSA { get; set; }
            public double H4CSSB { get; set; }



        }

        public void WriteClosed(Position po)
        {
            Print("Writing closed, initiated");

            CloseTrade record = new CloseTrade 
            {
                id = po.Id.ToString(),
                NetRealized = po.NetProfit

            };


            Closerecords.Add(record);


            using (var writer = new StreamWriter("F:\\\\closestuff.csv"))
            {
                using (var csv = new CsvWriter(writer))
                {
                    csv.WriteRecords(Closerecords);

                }
            }


            Print("Done Writing To CSV File");
            Print("count " + Closerecords.Count);


        }

        public void WriteToCSVFile()
        {
            Print("Writing initiated");





            OpenTrade record = new OpenTrade 
            {
                id = GlobalID.Id.ToString(),
                price = Bars.ClosePrices.Last(1),
                M15T = ichimoku.TenkanSen.Last(1),
                M15K = ichimoku.KijunSen.Last(1),
                M15SSA = ichimoku.SenkouSpanA.Last(26),
                M15SSB = ichimoku.SenkouSpanB.Last(26),

                M15Chikou = ichimoku.ChikouSpan.Last(1),
                M15CPrice = Bars.ClosePrices.Last(26),
                M15CKijun = ichimoku.KijunSen.Last(26),
                M15CTenkan = ichimoku.TenkanSen.Last(26),
                M15CSSA = ichimoku.SenkouSpanA.Last(52),
                M15CSSB = ichimoku.SenkouSpanB.Last(52),

                H1T = ichimokuH1.TenkanSen.Last(1),
                H1K = ichimokuH1.KijunSen.Last(1),
                H1SSA = ichimokuH1.SenkouSpanA.Last(104),
                H1SSB = ichimokuH1.SenkouSpanA.Last(104),
                H1Chikou = ichimokuH1.ChikouSpan.Last(1),
                H1CPrice = Bars.ClosePrices.Last(104),
                H1CKijun = ichimokuH1.KijunSen.Last(104),
                H1CTenkan = ichimokuH1.TenkanSen.Last(104),
                H1CSSA = ichimokuH1.SenkouSpanA.Last(52 * 4),
                H1CSSB = ichimokuH1.SenkouSpanB.Last(52 * 4),


                H4T = ichimokuH4.TenkanSen.Last(1),
                H4K = ichimokuH4.KijunSen.Last(1),
                H4SSA = ichimokuH4.SenkouSpanA.Last(104 * 4),
                H4SSB = ichimokuH4.SenkouSpanA.Last(104 * 4),
                H4Chikou = ichimokuH4.ChikouSpan.Last(1),
                H4CPrice = Bars.ClosePrices.Last(104 * 4),
                H4CKijun = ichimokuH4.KijunSen.Last(104 * 4),
                H4CTenkan = ichimokuH4.TenkanSen.Last(104 * 4),
                H4CSSA = ichimokuH4.SenkouSpanA.Last(52 * 4 * 4),
                H4CSSB = ichimokuH4.SenkouSpanB.Last(52 * 4 * 4)

            };

            records.Add(record);


            using (var writer = new StreamWriter("F:\\\\stuff.csv"))
            {
                using (var csv = new CsvWriter(writer))
                {
                    csv.WriteRecords(records);

                }
            }


            Print("Done Writing To CSV File");
            Print("count " + records.Count);
        }

        protected int ChikouUp()
        {

            if (ichimoku.ChikouSpan.LastValue > ichimoku.SenkouSpanA.Last(52) && ichimoku.ChikouSpan.LastValue > ichimoku.SenkouSpanB.Last(52) && ichimoku.ChikouSpan.LastValue > ichimoku.KijunSen.Last(26) && ichimoku.ChikouSpan.LastValue > ichimoku.TenkanSen.Last(26) && ichimoku.ChikouSpan.LastValue > Bars.ClosePrices.Last(26) && ichimoku.ChikouSpan.LastValue > Bars.OpenPrices.Last(26))
            {
                return 1;
            }

            if (ichimoku.ChikouSpan.LastValue < ichimoku.SenkouSpanA.Last(52) && ichimoku.ChikouSpan.LastValue < ichimoku.SenkouSpanB.Last(52) && ichimoku.ChikouSpan.LastValue < ichimoku.KijunSen.Last(26) && ichimoku.ChikouSpan.LastValue < ichimoku.TenkanSen.Last(26) && ichimoku.ChikouSpan.LastValue < Bars.ClosePrices.Last(26) && ichimoku.ChikouSpan.LastValue < Bars.OpenPrices.Last(26))
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }
        protected int PriceUp()
        {
            if (Bars.ClosePrices.Last(1) > ichimoku.SenkouSpanA.Last(26) && Bars.ClosePrices.Last(1) > ichimoku.SenkouSpanB.Last(26) && Bars.ClosePrices.Last(1) > ichimoku.TenkanSen.Last(1) && Bars.ClosePrices.Last(1) > ichimoku.KijunSen.Last(1))
            {
                return 1;
            }
            if (Bars.ClosePrices.Last(1) < ichimoku.SenkouSpanA.Last(26) && Bars.ClosePrices.Last(1) < ichimoku.SenkouSpanB.Last(26) && Bars.ClosePrices.Last(1) < ichimoku.TenkanSen.Last(1) && Bars.ClosePrices.Last(1) < ichimoku.KijunSen.Last(1))
            {
                return 2;
            }
            else
            {
                return 3;
            }

        }

        protected double GetVolume(double SL)
        {





            var x = 1.0;



            var RawRisk = Account.Balance * RiskP / 100;
            x = Math.Round((RawRisk) / (SL * Symbol.PipValue * Symbol.VolumeInUnitsMin));


            if (Symbol.VolumeInUnitsMin > 1)
            {
                return Convert.ToInt32(x * Symbol.VolumeInUnitsMin);
            }
            else
            {
                return (x * Symbol.VolumeInUnitsMin);
            }







        }

        protected double GetSL()
        {
            var SL = Math.Abs(Bars.ClosePrices.Last(1) - ichimoku.KijunSen.Last(1)) / Symbol.PipSize;
            Print("SL is " + Math.Round(SL, Symbol.Digits));
            if (SL < 1)
            {
                return 1;
            }
            else
            {
                return Math.Round(SL, Symbol.Digits);
            }

        }

        protected double GetTP(double SL)
        {
            return mult * SL;
        }
        protected override void OnBar()
        {
            var Lpos = Positions.FindAll("Ichi", SymbolName, TradeType.Buy);
            var Spos = Positions.FindAll("Ichi", SymbolName, TradeType.Sell);
            //Buy
            if (ChikouUp() == 1 && PriceUp() == 1 && Lpos.Length == 0)
            {
                var t = ExecuteMarketOrder(TradeType.Buy, SymbolName, GetVolume(GetSL()), "Ichi", GetSL(), GetTP(GetSL()));
                Print("onto trade  " + t);
                if (t.IsSuccessful)
                {
                    GlobalID = t.Position;
                    WriteToCSVFile();
                }
            }
            if (ChikouUp() == 2 && PriceUp() == 2 && Spos.Length == 0)
            {
                var t = ExecuteMarketOrder(TradeType.Sell, SymbolName, GetVolume(GetSL()), "Ichi", GetSL(), GetTP(GetSL()));
                Print("onto trade  " + t);
                if (t.IsSuccessful)
                {
                    GlobalID = t.Position;
                    WriteToCSVFile();
                }

            }
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}

