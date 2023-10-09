using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;


namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Minitronv4 : Robot
    {


        [Parameter(DefaultValue = 1000)]
        public int Volume { get; set; }










        public double close;
        public Position _position;
        IchimokuKinkoHyo ichimoku;
        MacdCrossOver macd;





        protected override void OnStart()
        {
            ichimoku = Indicators.IchimokuKinkoHyo(9, 26, 52);
            macd = Indicators.MacdCrossOver(26, 12, 9);

            double close = Bars.ClosePrices.LastValue;

        }

        //int totalPositions = Positions.Count;



        bool IsPosOpen()
        {
            if (Positions.Count == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        protected override void OnBar()
        {

            int perO = Openceptron();

            bool IPO = IsPosOpen();











            if (!IPO)
            {
                if (perO == 1 && macd.Histogram.LastValue > 0)
                {
                    ExecuteMarketOrder(TradeType.Buy, Symbol, Volume, "Buy");
                }
            }

                        /*else if (IPO && per == PerceptronBuyClose)
            {
                ClosePosition(_position);
            }*/
foreach (var position in Positions)
            {
                if (IPO)
                {
                    if (macd.Histogram.LastValue < 0)
                    {
                        ClosePosition(position);
                    }
                }
            }





        }

        private int Openceptron()
        {



            if (ichimoku.TenkanSen.LastValue > ichimoku.KijunSen.LastValue && ichimoku.TenkanSen.LastValue > ichimoku.SenkouSpanA.Last(26) && ichimoku.KijunSen.LastValue > ichimoku.SenkouSpanA.Last(26) && macd.Histogram.LastValue > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }





    }
}




