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
        [Parameter("Stop Loss", DefaultValue = 200)]
        public int StopLossPips { get; set; }

        [Parameter("Take Profit", DefaultValue = 25)]
        public int TakeProfitPips { get; set; }

        [Parameter("Switch Entry PSSA", DefaultValue = true)]
        public bool swe_PSSA { get; set; }


        [Parameter("Switch Entry PSSB", DefaultValue = true)]
        public bool swe_PSSB { get; set; }


        [Parameter("Switch Entry PK", DefaultValue = true)]
        public bool swe_PK { get; set; }


        [Parameter("Switch Entry PT", DefaultValue = true)]
        public bool swe_PT { get; set; }

        [Parameter("Switch Entry KSSB", DefaultValue = true)]
        public bool swe_KSSB { get; set; }


        [Parameter("Switch Entry KSSA", DefaultValue = true)]
        public bool swe_KSSA { get; set; }



        [Parameter("Switch Entry TSSB", DefaultValue = true)]
        public bool swe_TSSB { get; set; }



        [Parameter("Switch Entry TSSA", DefaultValue = true)]
        public bool swe_TSSA { get; set; }



        [Parameter("Switch Entry TK", DefaultValue = true)]
        public bool swe_TK { get; set; }







        IchimokuKinkoHyo ichimoku;


        protected override void OnStart()
        {
            ichimoku = Indicators.IchimokuKinkoHyo(9, 26, 52);


        }

        protected override void OnBar()
        {
            var positionsBuy = Positions.FindAll("Buy");
            var positionsSell = Positions.FindAll("Sell");
            var lastIndex = Bars.ClosePrices.Count;
            double close = Bars.ClosePrices.LastValue;
            // Over and Under neurones 9 neurones



            bool PSSA;
            if (close > ichimoku.SenkouSpanA.Last(26))
            {
                PSSA = true;
            }
            if (close < ichimoku.SenkouSpanA.Last(26))
            {
                PSSA = false;
            }





            bool PSSB;
            if (close > ichimoku.SenkouSpanB.Last(26))
            {
                PSSB = true;
            }
            if (close < ichimoku.SenkouSpanB.Last(26))
            {
                PSSB = false;
            }




            bool PK;
            if (close > ichimoku.KijunSen.Last(0))
            {
                PK = true;
            }
            if (close < ichimoku.KijunSen.Last(0))
            {
                PK = false;
            }




            bool PT;
            if (close > ichimoku.TenkanSen.Last(0))
            {
                PT = true;
            }
            if (close < ichimoku.TenkanSen.Last(0))
            {
                PT = false;
            }




            bool KSSB;
            if (ichimoku.KijunSen.Last(0) > ichimoku.SenkouSpanB.Last(26))
            {
                KSSB = true;
            }
            if (ichimoku.KijunSen.Last(0) < ichimoku.SenkouSpanB.Last(26))
            {
                KSSB = false;
            }




            bool KSSA;
            if (ichimoku.KijunSen.Last(0) > ichimoku.SenkouSpanA.Last(26))
            {
                KSSA = true;
            }
            if (ichimoku.KijunSen.Last(0) < ichimoku.SenkouSpanA.Last(26))
            {
                KSSA = false;
            }



            bool TSSB;
            if (ichimoku.TenkanSen.Last(0) > ichimoku.SenkouSpanB.Last(26))
            {
                TSSB = true;
            }
            if (ichimoku.TenkanSen.Last(0) < ichimoku.SenkouSpanB.Last(26))
            {
                TSSB = false;
            }




            bool TSSA;
            if (ichimoku.TenkanSen.Last(0) > ichimoku.SenkouSpanA.Last(26))
            {
                TSSA = true;
            }
            if (ichimoku.TenkanSen.Last(0) < ichimoku.SenkouSpanA.Last(26))
            {
                TSSA = false;
            }



            bool TK;
            if (ichimoku.TenkanSen.Last(0) > ichimoku.KijunSen.Last(0))
            {
                TK = true;
            }
            if (ichimoku.TenkanSen.Last(0) < ichimoku.KijunSen.Last(0))
            {
                TK = false;
            }

                        /* foreach (var position in Positions)
            {
                if (PT == swc_PT ) 
                {if (PK == swc_PK) && PSSB == swc_PSSB && PSSA == swc_PSSA && KSSB == swc_KSSB && KSSA == swc_KSSA && TSSB == swc_TSSB && TSSA == swc_TSSA && TK == swc_TK && positionsBuy != null)
                {
                    ClosePosition(position);
                }
            }*/




if (PT = swe_PT)
            {
                if (PK = swe_PK)
                {
                    if (PSSB = swe_PSSB)
                    {
                        if (PSSA = swe_PSSA)
                        {
                            if (KSSB = swe_KSSB)
                            {
                                if (KSSA = swe_KSSA)
                                {
                                    if (TSSB = swe_TSSB)
                                    {
                                        if (TSSA = swe_TSSA)
                                        {
                                            if (TK = swe_TK)
                                            {
                                                if (positionsBuy != null)
                                                {
                                                    ExecuteMarketOrder(TradeType.Buy, Symbol, Volume, "Buy", StopLossPips, TakeProfitPips);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }



        }
    }
}



//Fonction de close si trade open et que le prix touche le cloud et ou Tenkan / Kijun ? tester ça   penser à utiliser les ichimoku des UT H1 et H4 pour affiner ? 
