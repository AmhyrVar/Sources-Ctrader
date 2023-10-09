using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class Ichiwithnoclose : Robot
    {

        private Position _position;
        private ExponentialMovingAverage _ema5;
        private ExponentialMovingAverage _ema1;
        private ExponentialMovingAverage _ema2;

        [Parameter(DefaultValue = 10000, MinValue = 0)]
        public int Volume { get; set; }
        IchimokuKinkoHyo ichimoku15;
        IchimokuKinkoHyo ichimoku1;
        IchimokuKinkoHyo ichimoku4;
        [Parameter()]
        public DataSeries Source { get; set; }



        [Parameter("Take Profit", DefaultValue = 25)]
        public int TakeProfitPips { get; set; }



        protected override void OnStart()
        {
            ichimoku15 = Indicators.IchimokuKinkoHyo(9, 26, 52);
            ichimoku1 = Indicators.IchimokuKinkoHyo(36, 104, 208);
            ichimoku4 = Indicators.IchimokuKinkoHyo(144, 416, 832);
            _ema5 = Indicators.ExponentialMovingAverage(Source, 1);
            _ema2 = Indicators.ExponentialMovingAverage(Source, 26);
            _ema1 = Indicators.ExponentialMovingAverage(Source, 12);
        }

        protected override void OnBar()
        {

            // ArtificialTP ?  var positionsBuy = Positions.FindAll("Buy");
            //                   var positionsSell = Positions.FindAll("Sell");
            // if (positionsBuy.Length == 0 && positionsSell.Length == 0) closeposition


            var positionsBuy = Positions.FindAll("Buy");
            var positionsSell = Positions.FindAll("Sell");



            var distanceFromUpKumo = (Symbol.Bid - ichimoku15.SenkouSpanA.Last(26)) / Symbol.PipSize;
            var distanceFromDownKumo = (ichimoku15.SenkouSpanA.Last(26) - Symbol.Ask) / Symbol.PipSize;

            if (Trade.IsExecuting)
                return;

            bool isLongPositionOpen = _position != null && _position.TradeType == TradeType.Buy;
            bool isShortPositionOpen = _position != null && _position.TradeType == TradeType.Sell;
            // Close position at signal close





            // Conditions to open a trade


            if (isLongPositionOpen != null && Bars.OpenPrices.Last(1) <= ichimoku15.SenkouSpanA.Last(27) && Bars.OpenPrices.Last(1) > ichimoku15.SenkouSpanB.Last(27))
            {
                if (Bars.ClosePrices.Last(1) > ichimoku15.SenkouSpanA.Last(27) && Bars.ClosePrices.Last(1) > ichimoku15.KijunSen.Last(1) && Bars.ClosePrices.Last(1) > ichimoku15.TenkanSen.Last(1) && ichimoku15.SenkouSpanA.Last(1) > ichimoku15.SenkouSpanB.Last(1) && ichimoku1.SenkouSpanA.Last(1) > ichimoku1.SenkouSpanB.Last(1) && ichimoku4.SenkouSpanA.Last(1) > ichimoku4.SenkouSpanB.Last(1))
                {
                    if (distanceFromUpKumo <= 30)
                    {
                        Buy();
                    }
                }
            }


            if (isShortPositionOpen != null && Bars.OpenPrices.Last(1) >= ichimoku15.SenkouSpanA.Last(27) && Bars.OpenPrices.Last(1) < ichimoku15.SenkouSpanB.Last(27))
            {
                if (Bars.ClosePrices.Last(1) < ichimoku15.SenkouSpanA.Last(27) && Bars.ClosePrices.Last(1) < ichimoku15.KijunSen.Last(1) && Bars.ClosePrices.Last(1) < ichimoku15.TenkanSen.Last(1) && ichimoku15.SenkouSpanA.Last(1) < ichimoku15.SenkouSpanB.Last(1) && ichimoku1.SenkouSpanA.Last(1) < ichimoku1.SenkouSpanB.Last(1) && ichimoku4.SenkouSpanA.Last(1) < ichimoku4.SenkouSpanB.Last(1))
                {
                    if (distanceFromDownKumo <= 30)
                    {
                        Sell();
                    }
                }
            }
        }


        private void Buy()
        {
            Trade.CreateBuyMarketOrder(Symbol, Volume);
        }

        private void Sell()
        {
            Trade.CreateSellMarketOrder(Symbol, Volume);
        }

        protected override void OnPositionOpened(Position openedPosition)
        {
            _position = openedPosition;
        }

    }
}
