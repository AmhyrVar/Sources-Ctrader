using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Hooks : Robot
    {
        [Parameter(DefaultValue = 0.2)]
        public double gamma { get; set; }



        [Parameter(DefaultValue = 0.1)]
        public double volume { get; set; }
        [Parameter(DefaultValue = 50)]
        public double TP { get; set; }
        [Parameter(DefaultValue = 25)]
        public double SL { get; set; }


        [Parameter("Allow Buy", DefaultValue = true)]
        public bool AllowBuy { get; set; }
        [Parameter("Allow Sell", DefaultValue = true)]
        public bool AllowSell { get; set; }


        Laguerre_RSI LRSI;


        protected override void OnStart()
        {

            LRSI = Indicators.GetIndicator<Laguerre_RSI>(gamma);

        }

        protected override void OnBar()
        {



            var LP = Positions.FindAll("LaGuerre", SymbolName, TradeType.Buy);
            var SP = Positions.FindAll("LaGuerre", SymbolName, TradeType.Sell);

            if (LRSI.laguerrersi.HasCrossedAbove(LRSI.oversold, 1))
            {
                if (SP.Length == 0 && LP.Length == 0 && AllowBuy)
                {


                    ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "LaGuerre", SL, TP);

                }









            }
            if (LRSI.laguerrersi.HasCrossedBelow(LRSI.overbought, 1))
            {


                if (SP.Length == 0 && LP.Length == 0 && AllowSell)
                {


                    ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "LaGuerre", SL, TP);

                }



            }



        }


    }
}
