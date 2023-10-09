using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class MACDONEMK1 : Robot
    {
        private ExponentialMovingAverage _ema;
        private SimpleMovingAverage _sma;





        string Label = "Principes";

        public MacdCrossOver i_macd5;




        [Parameter(DefaultValue = 10000)]
        public int Volume { get; set; }

        [Parameter()]
        public DataSeries Source { get; set; }

        [Parameter("Stop Loss (pips)", DefaultValue = 20, MinValue = 1)]
        public int StopLoss { get; set; }
// declaration of loss amount
        [Parameter("Take Profit (pips)", DefaultValue = 20, MinValue = 1)]
        public int TakeProfit { get; set; }





        /*protected override void OnTick()
        {
        var lastIndex = MarketSeries.Close.Count - 1;  // last index
       var ema = .Result[lastIndex - 1];      // previous to last MA1 value
       var valueClose = MarketSeries.Close[lastIndex - 1];   // previous to last Close value

// compare the two values
if(valueClose >= valueMa)
{
  //... do something
   ClosePosition(_position);
}
            /*var lastIndex = Bars.ClosePrices.Count - 1;
            double ema = _ema.Result[lastIndex];
            double sma = _sma.Result[lastIndex];
            if (sma == ema)

                ClosePosition(_position);*/
        /* }*/

        protected override void OnStart()
        {
            _ema = Indicators.ExponentialMovingAverage(Source, 200);
            _sma = Indicators.SimpleMovingAverage(Source, 2);

            i_macd5 = Indicators.MacdCrossOver(26, 12, 9);



        }

        protected override void OnTick()
        {


            var lastIndex = Bars.ClosePrices.Count - 1;
            //test positions ******************************
            var longPositions = Positions.FindAll(Label, Symbol, TradeType.Buy);
            var shortPositions = Positions.FindAll(Label, Symbol, TradeType.Sell);




            double close = Bars.ClosePrices[lastIndex];
            double lastClose = Bars.ClosePrices[lastIndex];
            double ema = _ema.Result[lastIndex];
            double lastEma = _ema.Result[lastIndex];
            var longPositions = Positions.FindAll(Label, Symbol, TradeType.Buy);
            var shortPositions = Positions.FindAll(Label, Symbol, TradeType.Sell);
            foreach (var position in Positions)
            {
                if (longPositions != null && Functions.HasCrossedAbove(MarketSeries.Close, _ema.Result, 0))
                {
                    ClosePosition(position);
                }
                else if (shortPositions != null && Functions.HasCrossedBelow(MarketSeries.Close, _ema.Result, 0))
                {
                    ClosePosition(position);
                }
            }




            if (ema < close && i_macd5.MACD.LastValue < 0 && i_macd5.Signal.HasCrossedAbove(i_macd5.MACD, 0))
            {

                Buy();



            }

            if (ema > close && i_macd5.MACD.LastValue > 0 && i_macd5.Signal.HasCrossedAbove(i_macd5.MACD, 0))
            {

                Sell();



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
            /*Trade.CreateSellMarketOrder(Symbol, Volume);*/
            ExecuteMarketOrder(TradeType.Sell, Symbol, Volume, Label, StopLoss, TakeProfit);
        }

        /// <summary>
        /// Close Position
        /// </summary>
        /// <param name="pos"></param>

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
