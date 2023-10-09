using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class MACDbot : Robot
    {





        private string Label = "MACDbot";
        private BelkhayateCog _COG;


        public int MaxLongTrades = 1;
        public int MaxShortTrades = 1;

        public double TickValue { get; set; }






        [Parameter(DefaultValue = 10000)]
        public int Volume { get; set; }



        [Parameter("Stop Loss (pips)", DefaultValue = 20, MinValue = 1)]
        public int StopLoss { get; set; }

        [Parameter("Take Profit (pips)", DefaultValue = 20, MinValue = 1)]
        public int TakeProfit { get; set; }







        protected override void OnStart()
        {


            _COG = Indicators.GetIndicator<BelkhayateCog>(3, 125, 1.4, 2.4, 3.4);



        }

        protected override void OnTick()
        {





            var OpenPos = Positions.Count;

            var longPositions = Positions.FindAll(Label, Symbol, TradeType.Buy);
            var shortPositions = Positions.FindAll(Label, Symbol, TradeType.Sell);
            foreach (var position in Positions)
            {
                if (longPositions != null && MarketSeries.Close.Last(0) > _COG.Sqh.LastValue)
                {
                    ClosePosition(position);
                }
                else if (shortPositions != null && MarketSeries.Close.Last(0) < _COG.Sql.LastValue)
                {
                    ClosePosition(position);
                }
            }









            if (Positions.Count < MaxLongTrades && MarketSeries.Close.Last(0) < _COG.Sql2.LastValue)
            {

                Buy();

            }
            else if (Positions.Count < MaxShortTrades && MarketSeries.Close.Last(0) > _COG.Sqh2.LastValue)
            {

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
