using System;
using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Ichimokuclosekijuntenkancloud : Robot
    {
        [Parameter(DefaultValue = 9)]
        public int periodFast { get; set; }

        [Parameter(DefaultValue = 26)]
        public int periodMedium { get; set; }

        [Parameter(DefaultValue = 52)]
        public int periodSlow { get; set; }

        [Parameter(DefaultValue = 1000)]
        public int Volume { get; set; }

        [Parameter("Stop Loss", DefaultValue = 20)]
        public int StopLossPips { get; set; }

        [Parameter("Take Profit", DefaultValue = 25)]
        public int TakeProfitPips { get; set; }

        IchimokuKinkoHyo ichimoku;

        protected override void OnStart()
        {
            ichimoku = Indicators.IchimokuKinkoHyo(periodFast, periodMedium, periodSlow);
        }

        protected override void OnBar()
        {
            var positionsBuy = Positions.FindAll("Buy");
            var positionsSell = Positions.FindAll("Sell");

            var distanceFromUpKumo = (Symbol.Bid - ichimoku.SenkouSpanA.Last(26)) / Symbol.PipSize;
            var distanceFromDownKumo = (ichimoku.SenkouSpanA.Last(26) - Symbol.Ask) / Symbol.PipSize;


            if (positionsBuy.Length == 0 && positionsSell.Length == 0)
            {
                if (MarketSeries.Open.Last(1) <= ichimoku.SenkouSpanA.Last(27) && MarketSeries.Open.Last(1) > ichimoku.SenkouSpanB.Last(27))
                {
                    if (MarketSeries.Close.Last(1) > ichimoku.SenkouSpanA.Last(27) && MarketSeries.Close.Last(1) > ichimoku.KijunSen.Last(1) && MarketSeries.Close.Last(1) > ichimoku.TenkanSen.Last(1))
                    {
                        if (distanceFromUpKumo <= 30)
                        {
                            ExecuteMarketOrder(TradeType.Buy, Symbol, Volume, "Buy", StopLossPips, TakeProfitPips);
                        }
                    }
                }

                if (MarketSeries.Open.Last(1) >= ichimoku.SenkouSpanA.Last(27) && MarketSeries.Open.Last(1) < ichimoku.SenkouSpanB.Last(27))
                {
                    if (MarketSeries.Close.Last(1) < ichimoku.SenkouSpanA.Last(27) && MarketSeries.Close.Last(1) < ichimoku.KijunSen.Last(1) && MarketSeries.Close.Last(1) < ichimoku.TenkanSen.Last(1))
                    {
                        if (distanceFromDownKumo <= 30)
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
