using System;
using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Minitron : Robot
    {


        [Parameter(DefaultValue = 1000)]
        public int Volume { get; set; }

        private Position pos;
        private bool IsPosOpen = false;

        int PSSA;
        int PSSB;
        int PK;
        int PT;
        int KSSB;
        int KSSA;
        int TSSB;
        int TSSA;
        int TK;
        [Parameter(DefaultValue = 0)]
        public int PerceptronTrigger { get; set; }








        IchimokuKinkoHyo ichimoku;

        private int perceptron()
        {


            return PSSA + PSSB + PK + PT + KSSB + KSSA + TSSB + TSSA + TK;

        }


        protected override void OnStart()
        {
            ichimoku = Indicators.IchimokuKinkoHyo(9, 26, 52);
            double close = Bars.ClosePrices.LastValue;

            if (close > ichimoku.SenkouSpanA.Last(26))
            {
                PSSA = 1;
            }
            if (close < ichimoku.SenkouSpanA.Last(26))
            {
                PSSA = 0;
            }



            if (close > ichimoku.SenkouSpanB.Last(26))
            {
                PSSB = 2;
            }
            if (close < ichimoku.SenkouSpanB.Last(26))
            {
                PSSB = 0;
            }


            if (close > ichimoku.KijunSen.Last(0))
            {
                PK = 4;
            }
            if (close < ichimoku.KijunSen.Last(0))
            {
                PK = 0;
            }


            if (close > ichimoku.TenkanSen.Last(0))
            {
                PT = 8;
            }
            if (close < ichimoku.TenkanSen.Last(0))
            {
                PT = 0;
            }


            if (ichimoku.KijunSen.Last(0) > ichimoku.SenkouSpanB.Last(26))
            {
                KSSB = 16;
            }
            if (ichimoku.KijunSen.Last(0) < ichimoku.SenkouSpanB.Last(26))
            {
                KSSB = 0;
            }


            if (ichimoku.KijunSen.Last(0) > ichimoku.SenkouSpanA.Last(26))
            {
                KSSA = 32;
            }
            if (ichimoku.KijunSen.Last(0) < ichimoku.SenkouSpanA.Last(26))
            {
                KSSA = 0;
            }


            if (ichimoku.TenkanSen.Last(0) > ichimoku.SenkouSpanB.Last(26))
            {
                TSSB = 64;
            }
            if (ichimoku.TenkanSen.Last(0) < ichimoku.SenkouSpanB.Last(26))
            {
                TSSB = 0;
            }


            if (ichimoku.TenkanSen.Last(0) > ichimoku.SenkouSpanA.Last(26))
            {
                TSSA = 128;
            }
            if (ichimoku.TenkanSen.Last(0) < ichimoku.SenkouSpanA.Last(26))
            {
                TSSA = 0;
            }


            if (ichimoku.TenkanSen.Last(0) > ichimoku.KijunSen.Last(0))
            {
                TK = 256;
            }
            if (ichimoku.TenkanSen.Last(0) < ichimoku.KijunSen.Last(0))
            {
                TK = 0;
            }


        }

        protected override void OnBar()
        {
            int per = perceptron();
            if (!IsPosOpen)
            {
                if (per == PerceptronTrigger)
                {
                    ExecuteMarketOrder(TradeType.Buy, Symbol, Volume, "Buy");
                }

            }











        }
    }
}





