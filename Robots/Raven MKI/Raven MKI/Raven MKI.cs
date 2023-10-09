using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class RavenMKI : Robot
    {
        private SimpleMovingAverage _sma;


        private Position _position;
        private bool _isShortPositionOpen;
        private bool _isLongPositionOpen;
        private ZeroLagMACD _macd5;
        private ZeroLagMACD _macd15;

        string Label = "Raven";

        [Parameter("Stop Loss (pips)", DefaultValue = 20, MinValue = 1)]
        public int StopLoss { get; set; }
// declaration of loss amount
        [Parameter("Take Profit (pips)", DefaultValue = 20, MinValue = 1)]
        public int TakeProfit { get; set; }



        [Parameter(DefaultValue = 10000)]
        public int Volume { get; set; }
        [Parameter()]
        public DataSeries Source { get; set; }





        protected override void OnStart()
        {
            _sma = Indicators.SimpleMovingAverage(Source, 200);

            _macd5 = Indicators.GetIndicator<ZeroLagMACD>(26, 12, 9);
            _macd15 = Indicators.GetIndicator<ZeroLagMACD>(78, 36, 27);

        }


        protected override void OnBar()
        {
            if (Trade.IsExecuting)
                return;

            var lastIndex = Bars.ClosePrices.Count - 1;

            _isLongPositionOpen = _position != null && _position.TradeType == TradeType.Buy;
            _isShortPositionOpen = _position != null && _position.TradeType == TradeType.Sell;

            double close = Bars.ClosePrices[lastIndex - 1];
            double lastClose = Bars.ClosePrices[lastIndex - 2];
            double sma = _sma.Result[lastIndex - 1];
            double lastSma = _sma.Result[lastIndex - 2];

            if (!_isLongPositionOpen && sma > close && _macd5.Histogram.LastValue > 0.0 && _macd5.Signal.IsRising() && _macd15.Histogram.LastValue > 0.0 && _macd15.Signal.IsRising())
            {

                Buy();
            }
            else if (!_isShortPositionOpen && sma < close && _macd5.Histogram.LastValue < 0.0 && _macd5.Signal.IsFalling() && _macd15.Histogram.LastValue < 0.0 && _macd15.Signal.IsFalling())
            {

                Sell();
            }



        }

        protected override void OnTick()
        {
            var lastIndex = Bars.ClosePrices.Count;
            double close = Bars.ClosePrices[lastIndex];
            double lastClose = Bars.ClosePrices[lastIndex];
            double sma = _sma.Result[lastIndex];
            double lastSma = _sma.Result[lastIndex];

            if (sma == close)
            {
                ClosePosition(_position);
            }


        }

        /// <summary>
        /// _position holds the latest opened position
        /// </summary>
        /// <param name="openedPosition"></param>


        /// <summary>
        /// Create Buy Order
        /// </summary>
        private void Buy()
        {
            ExecuteMarketOrder(TradeType.Buy, Symbol, Volume, Label, StopLoss, TakeProfit);
        }

        /// <summary>
        /// Create Sell Order
        /// </summary>
        private void Sell()
        {
            ExecuteMarketOrder(TradeType.Sell, Symbol, Volume, Label, StopLoss, TakeProfit);
        }

        /// <summary>
        /// Close Position
        /// </summary>
        /// <param name="pos"></param>
        private void ClosePosition(Position pos)
        {
            if (pos == null)
                return;
            Trade.Close(pos);

        }
        /// <summary>
        /// Print Error
        /// </summary>
        /// <param name="error"></param>
        protected override void OnError(Error error)
        {
            Print(error.Code.ToString());
        }
    }
}
