using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class MACDbot : Robot
    {


        private Position _position;


        private ElliotOscillator _EWO;

        string Label = "MACDbot";


        public int MaxLongTrades = 1;
        public int MaxShortTrades = 1;
        public bool Direction;

        [Parameter("Source", Group = "RSI")]
        public DataSeries Source { get; set; }






        [Parameter(DefaultValue = 10000)]
        public int Volume { get; set; }



        [Parameter("Stop Loss (pips)", DefaultValue = 20, MinValue = 1)]
        public int StopLoss { get; set; }

        [Parameter("Take Profit (pips)", DefaultValue = 20, MinValue = 1)]
        public int TakeProfit { get; set; }







        protected override void OnStart()
        {

            _EWO = Indicators.GetIndicator<ElliotOscillator>(Source, 5, 35);


        }
        protected override void OnBar()
        {
            if (_EWO.Line.LastValue < 0)
            {
                Direction = true;
            }
            if (_EWO.Line.LastValue > 0)
            {
                Direction = false;
            }
        }

        protected override void OnTick()
        {



            var lastIndex = Bars.ClosePrices.Count - 1;
            double close = Bars.ClosePrices[lastIndex - 1];

            var longPosition = Positions.FindAll(Label, Symbol, TradeType.Buy);

            var shortPosition = Positions.FindAll(Label, Symbol, TradeType.Sell);
            var OpenPos = Positions.Count;


                        /*if (Bars.ClosePrices.HasCrossedAbove(_atr.Result.Last(0), 0))
                {
                    ClosePosition(position);
                }

            foreach (var position in Account.Positions)
                if (Bars.ClosePrices.HasCrossedBelow(_atr.Result.Last(0), 0))
                {
                    ClosePosition(position);
                }*/

            /*foreach (var position in Positions)
            {
                if (longPositions != null && _EWO.Line.LastValue > 0)
                {
                    ClosePosition(position);
                }
                else if (shortPositions != null && close > _atr.Result.Last(0))
                {
                    ClosePosition(position);
                }
            }*/


            /*foreach (var position in Positions)
            {
                if (longPosition != null && _EWO.Line.LastValue > 0)
                {
                    ClosePosition(position);
                }
                else if (shortPosition != null && _EWO.Line.LastValue < 0)
                {
                    ClosePosition(position);
                }
            }*/








if (_EWO.Line.IsRising())
            {
                if (Positions.Count < MaxLongTrades && _EWO.Line.LastValue > 0 && _EWO.Line.LastValue < 0.0005)
                {


                    Buy();

                }
            }


            if (_EWO.Line.IsFalling())
            {
                if (Positions.Count < MaxShortTrades && _EWO.Line.LastValue < 0 && _EWO.Line.LastValue > -0.0005)
                {
                    /*i_macd5.Signal.HasCrossedAbove(i_macd5.MACD, 0)*/

                    Sell();




                }
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
