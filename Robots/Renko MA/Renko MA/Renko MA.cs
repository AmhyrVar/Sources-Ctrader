using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class RenkoMA : Robot
    {
        //, Group = "Normal mode",

        [Parameter("Take Profit", Group = "Position variables", DefaultValue = 10)]
        public double TP { get; set; }
        [Parameter("Stop Loss", Group = "Position variables", DefaultValue = 50)]
        public double SL { get; set; }

        [Parameter("Lots", Group = "Position variables", DefaultValue = 0.01)]
        public double Lots { get; set; }

        [Parameter("Max spread", Group = "Position variables", DefaultValue = 1)]
        public double maxSpread { get; set; }

        [Parameter("Moving average Type", Group = "Moving average variables", DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType mA_Type { get; set; }

        [Parameter("Moving average Period", Group = "Moving average variables", DefaultValue = 20)]
        public int mA_Period { get; set; }

        [Parameter("Moving average source", Group = "Moving average variables")]
        public DataSeries SourceSeries { get; set; }
        [Parameter("Use Moving average ?", Group = "Moving average variables", DefaultValue = true)]
        public bool useMA { get; set; }

        MovingAverage MA;

        public double Volume;

        protected override void OnStart()
        {
            MA = Indicators.MovingAverage(SourceSeries, mA_Period,mA_Type);
            Volume = (Symbol.QuantityToVolumeInUnits(Lots));
            Print("spread is "+Symbol.Spread/ Symbol.PipSize);
            
        }

        private bool CheckUse (TradeType trade_type)
        {
            if (useMA)
            {
                if (Bars.OpenPrices.Last(1) > MA.Result.Last(1) && Bars.ClosePrices.Last(1) > MA.Result.Last(1) && trade_type == TradeType.Buy)
                {
                    return true;
                }

                if (Bars.OpenPrices.Last(1) < MA.Result.Last(1) && Bars.ClosePrices.Last(1) < MA.Result.Last(1) && trade_type == TradeType.Sell)
                {
                    return true;
                }

            }
            if (!useMA)
            {
                return true;
            }
            else { return false; }
        }
        private bool CheckSpread()
        {
            if (Symbol.Spread / Symbol.PipSize < maxSpread)
            {
                return true;
            }

            else { return false; }
           
        }

        protected override void OnBar()
        {
         

              

            foreach (var o in PendingOrders)
            {
                if (o.Label == "Renko MA" && o.SymbolName == SymbolName)
                {
                   
                    CancelPendingOrder(o);
                }
            }
            //Sell logic
            if (Bars.OpenPrices.Last(1) > Bars.ClosePrices.Last(1)&& CheckUse(TradeType.Sell) && CheckSpread() )
            {
               
                PlaceLimitOrder(TradeType.Sell, SymbolName, Volume, Bars.OpenPrices.Last(1),"Renko MA",SL,TP);
            }



            //Buy Logic
            if (Bars.OpenPrices.Last(1) < Bars.ClosePrices.Last(1) && CheckUse(TradeType.Buy)&& CheckSpread()  ) 
            {
                
                PlaceLimitOrder(TradeType.Buy, SymbolName, Volume, Bars.OpenPrices.Last(1), "Renko MA", SL, TP);
            }
        }

       
    }
}
