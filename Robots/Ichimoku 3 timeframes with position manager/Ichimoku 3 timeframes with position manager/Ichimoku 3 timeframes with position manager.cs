using System;
using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Ichimoku3timeframeswithpositionmanager : Robot
    {


        [Parameter(DefaultValue = 1000)]
        public int Volume { get; set; }

        [Parameter("Stop Loss", DefaultValue = 20)]
        public int StopLossPips { get; set; }

        [Parameter("Take Profit", DefaultValue = 25)]
        public int TakeProfitPips { get; set; }

        IchimokuKinkoHyo ichimoku15;
        IchimokuKinkoHyo ichimoku1;
        IchimokuKinkoHyo ichimoku4;

        private Position _position;
        private bool _isShortPositionOpen;
        private bool _isLongPositionOpen;




        protected override void OnStart()
        {
            ichimoku15 = Indicators.IchimokuKinkoHyo(9, 26, 52);
            ichimoku1 = Indicators.IchimokuKinkoHyo(36, 104, 208);
            ichimoku4 = Indicators.IchimokuKinkoHyo(144, 416, 832);
        }


        protected override void OnBar()
        {
            var positionsBuy = Positions.FindAll("Buy");
            var positionsSell = Positions.FindAll("Sell");

            var distanceFromUpKumo = (Symbol.Bid - ichimoku15.SenkouSpanA.Last(26)) / Symbol.PipSize;
            var distanceFromDownKumo = (ichimoku15.SenkouSpanA.Last(26) - Symbol.Ask) / Symbol.PipSize;


            if (positionsBuy.Length != 0)
            {
                if (MarketSeries.Open.Last(1) <= ichimoku15.TenkanSen.Last(1))
                {
                    ClosePosition(positionsBuy);
                }
            }


            if (positionsBuy.Length == 0 && positionsSell.Length == 0)
            {
                if (MarketSeries.Open.Last(1) <= ichimoku15.SenkouSpanA.Last(27) && MarketSeries.Open.Last(1) > ichimoku15.SenkouSpanB.Last(27))
                {
                    if (MarketSeries.Close.Last(1) > ichimoku15.SenkouSpanA.Last(27) && MarketSeries.Close.Last(1) > ichimoku15.KijunSen.Last(1) && MarketSeries.Close.Last(1) > ichimoku15.TenkanSen.Last(1) && ichimoku15.SenkouSpanA.Last(1) > ichimoku15.SenkouSpanB.Last(1) && ichimoku1.SenkouSpanA.Last(1) > ichimoku1.SenkouSpanB.Last(1) && ichimoku4.SenkouSpanA.Last(1) > ichimoku4.SenkouSpanB.Last(1))
                    {
                        if (distanceFromUpKumo <= 30)
                        {
                            Buy();
                        }
                    }
                }

                if (MarketSeries.Open.Last(1) >= ichimoku15.SenkouSpanA.Last(27) && MarketSeries.Open.Last(1) < ichimoku15.SenkouSpanB.Last(27))
                {
                    if (MarketSeries.Close.Last(1) < ichimoku15.SenkouSpanA.Last(27) && MarketSeries.Close.Last(1) < ichimoku15.KijunSen.Last(1) && MarketSeries.Close.Last(1) < ichimoku15.TenkanSen.Last(1) && ichimoku15.SenkouSpanA.Last(1) < ichimoku15.SenkouSpanB.Last(1) && ichimoku1.SenkouSpanA.Last(1) < ichimoku1.SenkouSpanB.Last(1) && ichimoku4.SenkouSpanA.Last(1) < ichimoku4.SenkouSpanB.Last(1))
                    {
                        if (distanceFromDownKumo <= 30)
                        {
                            positionsSell();
                        }
                    }
                }
            }
        }
    }
}

          private void Buy()
        {
            ExecuteMarketOrder(TradeType.Sell, Symbol, Volume, "IchiSniper", StopLossPips, TakeProfitPips);
        }

        /// <summary>
        /// Create Sell Order
        /// </summary>
        private void Sell()
        {
            ExecuteMarketOrder(TradeType.Sell, Symbol, Volume, "IchiSniper", StopLossPips, TakeProfitPips ); // Label Sl TP
        }

//Fonction de close si trade open et que le prix touche le cloud et ou Tenkan / Kijun ? tester ça   penser à utiliser les ichimoku des UT H1 et H4 pour affiner ? 
// conditions d'ouverture : ne pas ouvrir dans le cloud forcément, genre dès que c'est sup à Kijun tenkan et cloud on long
