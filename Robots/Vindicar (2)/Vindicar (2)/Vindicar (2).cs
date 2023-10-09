using System;
using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Vindicar : Robot
    {


        [Parameter(DefaultValue = 1000)]
        public int Volume { get; set; }

        [Parameter("Stop Loss", DefaultValue = 200)]
        public int StopLossPips { get; set; }

        [Parameter("Take Profit", DefaultValue = 25)]
        public int TakeProfitPips { get; set; }



        IchimokuKinkoHyo ichimoku1;
        IchimokuKinkoHyo ichimoku15;
        IchimokuKinkoHyo ichimoku60;

        protected override void OnStart()
        {
            ichimoku1 = Indicators.IchimokuKinkoHyo(9, 26, 52);
            ichimoku15 = Indicators.IchimokuKinkoHyo(135, 390, 780);
            ichimoku60 = Indicators.IchimokuKinkoHyo(540, 1560, 3120);
        }

        protected override void OnBar()
        {
            var positionsBuy = Positions.FindAll("Buy");
            var positionsSell = Positions.FindAll("Sell");
            var lastIndex = Bars.ClosePrices.Count;
            double close = Bars.ClosePrices.LastValue;
            //close conditions
                        /*var longPositions = Positions.FindAll(Label, Symbol, TradeType.Buy);
            var shortPositions = Positions.FindAll(Label, Symbol, TradeType.Sell);
            foreach (var position in Positions)
            {
                if (longPositions != null && Bars.ClosePrices < ichimoku60.KijunSen.LastValue)
                {
                    ClosePosition(position);
                }
                else if (shortPositions != null && Bars.ClosePrices > ichimoku60.KijunSen.LastValue)
                {
                    ClosePosition(position);
                }
            }*/







if (ichimoku60.TenkanSen.Last(0) > ichimoku60.KijunSen.Last(0) && close > ichimoku60.SenkouSpanA.Last(26))
            {
                if (ichimoku15.TenkanSen.Last(0) > ichimoku1.KijunSen.Last(0) && close > ichimoku15.SenkouSpanA.Last(26))
                {

                    if (ichimoku1.TenkanSen.HasCrossedAbove(ichimoku1.KijunSen, 1))
                    {



                        ExecuteMarketOrder(TradeType.Buy, Symbol, Volume, "Buy", StopLossPips, TakeProfitPips);

                    }
                }

                if (ichimoku60.TenkanSen.Last(0) < ichimoku60.KijunSen.Last(0) && close < ichimoku60.SenkouSpanA.Last(26))
                {
                    if (ichimoku15.TenkanSen.Last(0) < ichimoku1.KijunSen.Last(0) && close < ichimoku15.SenkouSpanA.Last(26))
                    {

                        if (ichimoku1.TenkanSen.HasCrossedBelow(ichimoku1.KijunSen, 1))
                        {

                            ExecuteMarketOrder(TradeType.Sell, Symbol, Volume, "Sell", StopLossPips, TakeProfitPips);

                        }
                    }
                }
            }
        }
    }
}


//Fonction de close si trade open et que le prix touche le cloud et ou Tenkan / Kijun ? tester ça   penser à utiliser les ichimoku des UT H1 et H4 pour affiner ? 
