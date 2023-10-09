using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class SuperProfitBot : Robot
    {

        [Parameter(DefaultValue = 1000)]
        public int Volume { get; set; }

        [Parameter("Source")]
        public DataSeries SourceSeries { get; set; }

        [Parameter(DefaultValue = 35)]
        public int DllPeriod { get; set; }

        [Parameter(DefaultValue = 1.7)]
        public double Period { get; set; }

        [Parameter(DefaultValue = MovingAverageType.Weighted)]
        public MovingAverageType MaType { get; set; }

        [Parameter()]
        public DataSeries Price { get; set; }

        [Output("Up", PlotType = PlotType.Points, Thickness = 4)]
        public IndicatorDataSeries UpSeries { get; set; }

        [Output("Down", PlotType = PlotType.Points, Color = Colors.Red, Thickness = 4)]
        public IndicatorDataSeries DownSeries { get; set; }

        [Parameter("TP (pips)", DefaultValue = 500)]
        public int TP { get; set; }
        [Parameter("SL (pips)", DefaultValue = 500)]
        public int SL { get; set; }
        
        
          [Parameter("SMAs Source")]
        public DataSeries Sma_Source { get; set; }


        private SuperProfit _SuperProfit;
        private SimpleMovingAverage SMA50;
        private SimpleMovingAverage SMA200;



        protected override void OnStart()
        {
            _SuperProfit = Indicators.GetIndicator<SuperProfit>(DllPeriod, Period, MaType, Price, 100);
            SMA50 = Indicators.SimpleMovingAverage(Sma_Source,50);
             SMA200 = Indicators.SimpleMovingAverage(Sma_Source,200);

        }


        double GetSPValue()
        {
            if (double.IsNaN(_SuperProfit.UpSeries.LastValue))
            {
                Print("Up is Nan");
                return _SuperProfit.DownSeries.LastValue; 
            }
            
            else {
                 Print("Down is Nan");
                return _SuperProfit.UpSeries.LastValue; 
            }
            
        }

        protected override void OnBar()
        {
        
        GetSPValue();
        
        Print("Up last value "+_SuperProfit.UpSeries.LastValue);
                Print("Down last value "+_SuperProfit.DownSeries.LastValue);
            
            /*if (_SuperProfit.TimeToBuy == true)
            {
                Print("Buy");
                //            foreach (var position in Positions)
                //           {
                //               if (position.TradeType == TradeType.Sell)
                //               {
                //                   ClosePosition(position);
                //                }
                //           }
                ExecuteMarketOrder(TradeType.Buy, Symbol, Volume, "Label", SL, TP);
            }

            if (_SuperProfit.TimeToBuy == false)
            {
                Print("Sell");

                //            foreach (var position in Positions)
                //            {
                //                if (position.TradeType == TradeType.Buy)
                //                {
                //                   ClosePosition(position);
                //                }
                //            }
                ExecuteMarketOrder(TradeType.Sell, Symbol, Volume, "Label", SL, TP);
            }*/
        }
    }
}