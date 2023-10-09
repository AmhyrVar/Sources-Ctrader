using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class MACDbot : Robot
    {
        private SimpleMovingAverage _sma;

        private Position _position;
        private bool _isShortPositionOpen;
        private bool _isLongPositionOpen;
        private ZeroLagMACD _macd;
        private ATRStops _atr;
        private VWAP _vwap;
        string Label = "MACDbot";




        [Parameter(DefaultValue = 200)]
        public int Period { get; set; }

        [Parameter(DefaultValue = 10000)]
        public int Volume { get; set; }



        [Parameter("Stop Loss (pips)", DefaultValue = 20, MinValue = 1)]
        public int StopLoss { get; set; }

        [Parameter("Take Profit (pips)", DefaultValue = 20, MinValue = 1)]
        public int TakeProfit { get; set; }







        protected override void OnStart()
        {

            _macd = Indicators.GetIndicator<ZeroLagMACD>(26, 12, 9);
            _atr = Indicators.GetIndicator<ATRStops>();
            _vwap = Indicators.GetIndicator<VWAP>();

        }

        protected override void OnBar()
        {
            if (Trade.IsExecuting)
                return;

            var lastIndex = MarketSeries.Close.Count - 1;

            _isLongPositionOpen = _position != null && _position.TradeType == TradeType.Buy;
            _isShortPositionOpen = _position != null && _position.TradeType == TradeType.Sell;

            double close = MarketSeries.Close[lastIndex - 1];
            double lastClose = MarketSeries.Close[lastIndex - 2];



            if (_isLongPositionOpen != null && _macd.MACD.Last(0) > _macd.Signal.Last(0) && close > _atr.Result.Last(0) && close > _vwap.Result.Last(0))
            {
                ClosePosition(_position);
                Buy();

            }
            else if (_isShortPositionOpen != null && _macd.MACD.Last(0) < _macd.Signal.Last(0) && close < _atr.Result.Last(0) && close < _vwap.Result.Last(0))
            {
                ClosePosition(_position);
                Sell();



            }
        }


        private void ClosePosition()
        {
            if (_position != null)
            {
                Trade.Close(_position);
                _position = null;
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
