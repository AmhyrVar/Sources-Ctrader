using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class MACDbot : Robot
    {


        private Position _position;

        private ZeroLagMACD _macd;
        private ATRStops _atr;
        private VWAP _vwap;
        string Label = "MACDbot";


        public int MaxLongTrades = 1;
        public int MaxShortTrades = 1;






        [Parameter(DefaultValue = 10000)]
        public int Volume { get; set; }



        [Parameter("Stop Loss (pips)", DefaultValue = 20, MinValue = 1)]
        public int StopLoss { get; set; }

        [Parameter("Take Profit (pips)", DefaultValue = 20, MinValue = 1)]
        public int TakeProfit { get; set; }







        protected override void OnStart()
        {

            _macd = Indicators.GetIndicator<ZeroLagMACD>(26, 12, 9);
            _atr = Indicators.GetIndicator<ATRStops>(MovingAverageType.Simple, 15, 3.0, true);
            _vwap = Indicators.GetIndicator<VWAP>(0, false);

        }

        protected override void OnTick()
        {



            var lastIndex = Bars.ClosePrices.Count - 1;
            double close = Bars.ClosePrices[lastIndex - 1];

            var longPosition = Positions.FindAll(Label, Symbol, TradeType.Buy);

            var shortPosition = Positions.FindAll(Label, Symbol, TradeType.Sell);
            var OpenPos = Positions.Count;

                        /*foreach (var position in Account.Positions)
                if (Bars.ClosePrices.HasCrossedAbove(_atr.Result.Last(0), 0))
                {
                    ClosePosition(position);
                }

            foreach (var position in Account.Positions)
                if (Bars.ClosePrices.HasCrossedBelow(_atr.Result.Last(0), 0))
                {
                    ClosePosition(position);
                }*/
var longPositions = Positions.FindAll(Label, Symbol, TradeType.Buy);
            var shortPositions = Positions.FindAll(Label, Symbol, TradeType.Sell);
            foreach (var position in Positions)
            {
                if (longPositions != null && close < _atr.Result.Last(0))
                {
                    ClosePosition(position);
                }
                else if (shortPositions != null && close > _atr.Result.Last(0))
                {
                    ClosePosition(position);
                }
            }









            if (Positions.Count < MaxLongTrades && _macd.Signal.HasCrossedBelow(_macd.MACD, 0) && close > _atr.Result.Last(0) && close > _vwap.Result.Last(0))
            {

                Buy();

            }
            if (Positions.Count < MaxShortTrades && _macd.Signal.HasCrossedAbove(_macd.MACD, 0) && close < _atr.Result.Last(0) && close < _vwap.Result.Last(0))
            {
                /*i_macd5.Signal.HasCrossedAbove(i_macd5.MACD, 0)*/
                Sell();




            }
        }





        private void Buy()
        {
            ExecuteMarketOrder(TradeType.Buy, Symbol, Volume, "RavenMKII", StopLoss, TakeProfit);
        }

        private void Sell()
        {
            ExecuteMarketOrder(TradeType.Sell, Symbol, Volume, "RavenMKII", StopLoss, TakeProfit);
        }



    }
}
