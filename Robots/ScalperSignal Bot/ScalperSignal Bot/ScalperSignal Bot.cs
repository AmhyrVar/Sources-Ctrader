using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class ScalperSignalBot : Robot
    {
        [Parameter("Sensitivity", DefaultValue = 2, MinValue = 1, MaxValue = 3, Step = 1)]
        public int Sensitivity { get; set; }

        [Parameter("Signal Bar Color", DefaultValue = "Gold")]
        public string SignalBarColor { get; set; }


        [Parameter("Position size", DefaultValue = "0.01")]
        public double PSizer { get; set; }

        [Parameter("Stop Loss", DefaultValue = "10")]
        public double SL { get; set; }
        [Parameter("Take Profit", DefaultValue = "20")]
        public double TP { get; set; }

        private ScalperSignal ScalpSig;

        private int BuyCount;
        private int SellCount;

        protected override void OnStart()
        {
            ScalpSig = Indicators.GetIndicator<ScalperSignal>(Sensitivity, SignalBarColor);



            SellCount = ScalpSig.SellIndicator.Count;
            BuyCount = ScalpSig.BuyIndicator.Count;


        }

        protected override void OnBar()
        {

            var InitialPosition = Positions.Find("ScalpSig", SymbolName);

            if (InitialPosition == null && ScalpSig.SellIndicator.Count != SellCount)
            {
                ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(PSizer), "ScalpSig", SL, TP);

            }

            if (InitialPosition == null && ScalpSig.BuyIndicator.Count != BuyCount)
            {
                ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(PSizer), "ScalpSig", SL, TP);

            }

            //quand deux greens se suivent ne pas reverse une green
            if (InitialPosition != null && InitialPosition.TradeType == TradeType.Buy && ScalpSig.SellIndicator.Count != SellCount)
            {
                ReversePosition(InitialPosition);
                Print("Reversed");

            }

            if (InitialPosition != null && InitialPosition.TradeType == TradeType.Sell && ScalpSig.BuyIndicator.Count != BuyCount)
            {
                ReversePosition(InitialPosition);
                Print("Reversed");


            }

            var NPos = Positions.Find("ScalpSig", SymbolName);

            if (NPos != null && NPos.StopLoss == null)
            {
                NPos.ModifyStopLossPips(SL);
                Print("SL PUT");


            }

            //add a check, if position has no tp and sl give it one


            SellCount = ScalpSig.SellIndicator.Count;
            BuyCount = ScalpSig.BuyIndicator.Count;

        }

        protected override void OnStop()
        {
        }
    }

}
