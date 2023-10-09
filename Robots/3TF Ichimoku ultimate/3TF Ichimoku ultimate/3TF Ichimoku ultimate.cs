using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class MACDbot : Robot
    {


        private Position _position;

        string Label = "Bot";

        IchimokuKinkoHyo ichimoku1;
        IchimokuKinkoHyo ichimoku15;
        IchimokuKinkoHyo ichimoku60;


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

            ichimoku1 = Indicators.IchimokuKinkoHyo(9, 26, 52);
            ichimoku15 = Indicators.IchimokuKinkoHyo(135, 390, 780);
            ichimoku60 = Indicators.IchimokuKinkoHyo(540, 1560, 3120);

        }

        protected override void OnTick()
        {



            var lastIndex = Bars.ClosePrices.Count - 1;
            double close = Bars.ClosePrices[lastIndex - 1];

            var longPosition = Positions.FindAll(Label, Symbol, TradeType.Buy);

            var shortPosition = Positions.FindAll(Label, Symbol, TradeType.Sell);
            var OpenPos = Positions.Count;


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
