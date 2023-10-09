using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class BBKeltv2fixed : Robot
    {

        [Parameter(DefaultValue = 0.1)]
        public double volume { get; set; }

        [Parameter("Keltner MA Period", DefaultValue = 20)]
        public int MA_Period { get; set; }
        [Parameter("Keltner MA Type", DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType MA_Type { get; set; }

        [Parameter("Keltner ATR Period", DefaultValue = 10)]
        public int ATR_Period { get; set; }

        [Parameter("Keltner ATR MA Type", DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType ATR_MA_Type { get; set; }

        [Parameter("Keltner Band Distance", DefaultValue = 2)]
        public int ATR_BD { get; set; }

        [Parameter("Bollinger Bands Source ")]
        public DataSeries BB_Source { get; set; }
        [Parameter("Bollinger Bands Periods", DefaultValue = 20)]
        public int BB_Period { get; set; }

        [Parameter("Bollinger Standard Dev", DefaultValue = 2)]
        public int BB_StD { get; set; }
        [Parameter("Bollinger MA Type", DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType BB_MA_Type { get; set; }





        BollingerBands BB;
        KeltnerChannels KC;

        private Des_Squeeze_Play_Indicator Cindy;

        protected override void OnStart()
        {

            BB = Indicators.BollingerBands(BB_Source, BB_Period, BB_StD, BB_MA_Type);
            KC = Indicators.KeltnerChannels(MA_Period, MA_Type, ATR_Period, ATR_MA_Type, ATR_BD);



            Cindy = Indicators.GetIndicator<Des_Squeeze_Play_Indicator>(20, 2.0, 20, 1.5, 12, 1000, true, true);
        }

        protected bool LPos()
        {
            var Lpos = Positions.FindAll("Buy", SymbolName);
            if (Lpos.Length == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        protected bool SPos()
        {
            var Spos = Positions.FindAll("Sell", SymbolName);
            if (Spos.Length == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        protected override void OnTick()
        {
            // Put your core logic here

            //(1) price hitting upper BB band check***
            //(2) blue is above 0(means look for long) check***
            //(3) green apears means trade(red means no trade)***
            //(4) use bottom of BB as exit (stop loss or take profit, like the donchain channel you made me a few months ago)

            var Longpos = Positions.FindAll("Buy", SymbolName);
            var Shortpos = Positions.FindAll("Sell", SymbolName);

            if (Symbol.Bid < KC.Bottom.LastValue)
            {
                Print("Node close buy");
                foreach (var pos in Longpos)
                {



                    ClosePosition(pos);
                }
            }

            if (Symbol.Ask > KC.Top.LastValue)
            {
                Print("Node close sell");
                foreach (var pos in Shortpos)
                {



                    ClosePosition(pos);
                }
            }


            if (!SPos() && Symbol.Bid < BB.Bottom.Last(1) && Cindy.Momentum_AlgoOutputDataSeries.LastValue < 0 && Cindy.Pos_Diff_AlgoOutputDataSeries.LastValue > 0)
            {
                ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Sell");
            }

            if (!LPos() && Symbol.Ask > BB.Top.Last(1) && Cindy.Momentum_AlgoOutputDataSeries.LastValue > 0 && Cindy.Pos_Diff_AlgoOutputDataSeries.LastValue > 0)
            {
                ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "Buy");
            }
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
