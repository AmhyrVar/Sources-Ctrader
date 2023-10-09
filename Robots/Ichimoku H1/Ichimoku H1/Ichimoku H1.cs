using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo
{

//changer les tp et sl pour que ce soit dynamique en fonction du last fractal down.
//Logic change : price above all tenkan above kijun and cloud, kijun above cloud 

    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class IchimokuH1 : Robot
    {

        //use lot size
        // option lotsize or risk %



        [Parameter("Stop Loss", DefaultValue = 50)]
        public int SL { get; set; }

        [Parameter("Take Profit", DefaultValue = 100)]
        public int TP { get; set; }


        [Parameter(DefaultValue = 9)]
        public int periodFast { get; set; }

        [Parameter(DefaultValue = 26)]
        public int periodMedium { get; set; }

        [Parameter(DefaultValue = 52)]
        public int periodSlow { get; set; }


        [Parameter("Lot size", DefaultValue = 0.01)]
        public double Volume { get; set; }

        [Parameter("Use Risk %", DefaultValue = true)]
        public bool RPU { get; set; }

        [Parameter("Risk %", DefaultValue = 5)]
        public double RiskP { get; set; }





        IchimokuKinkoHyo ichimoku;
        IchimokuKinkoHyo ichimokuD;

        protected override void OnStart()
        {
            ichimoku = Indicators.IchimokuKinkoHyo(periodFast, periodMedium, periodSlow);
            ichimokuD = Indicators.IchimokuKinkoHyo(periodFast * 24, periodMedium * 24, periodSlow * 24);


        }

        protected double GetVolume(double SL)
        {


            if (RPU)
            {


                var x = 1.0;



                var RawRisk = Account.Balance * RiskP / 100;
                x = Math.Round((RawRisk) / (SL * Symbol.PipValue * Symbol.VolumeInUnitsMin));


                if (Symbol.VolumeInUnitsMin > 1)
                {
                    return Convert.ToInt32(x * Symbol.VolumeInUnitsMin);
                }
                else
                {
                    return (x * Symbol.VolumeInUnitsMin);
                }
            }

            else
            {
                return (Symbol.QuantityToVolumeInUnits(Volume));
            }



        }


        protected override void OnBar()
        {


            var positionsBuy = Positions.FindAll("Buy", SymbolName);
            var positionsSell = Positions.FindAll("Sell", SymbolName);




            if (positionsBuy.Length == 0 && positionsSell.Length == 0)
            {



                if (ichimoku.SenkouSpanA.LastValue > ichimoku.SenkouSpanB.LastValue && ichimoku.ChikouSpan.Last(1) > ichimoku.SenkouSpanA.Last(52) && ichimoku.ChikouSpan.Last(1) > ichimoku.SenkouSpanB.Last(52) && ichimoku.TenkanSen.LastValue > ichimoku.KijunSen.LastValue && Bars.ClosePrices.Last(1) > ichimoku.SenkouSpanA.Last(26) && Bars.ClosePrices.Last(1) > ichimoku.SenkouSpanB.Last(26) && ichimokuD.SenkouSpanA.LastValue > ichimokuD.SenkouSpanB.LastValue)
                {


                    //BUY LOGIC
                    ExecuteMarketOrder(TradeType.Buy, Symbol.Name, GetVolume(SL), "Buy", SL, TP);


                }


                if (ichimokuD.SenkouSpanA.LastValue < ichimokuD.SenkouSpanB.LastValue && ichimoku.SenkouSpanA.LastValue < ichimoku.SenkouSpanB.LastValue && ichimoku.ChikouSpan.Last(1) < ichimoku.SenkouSpanA.Last(52) && ichimoku.ChikouSpan.Last(1) < ichimoku.SenkouSpanB.Last(52) && ichimoku.TenkanSen.LastValue < ichimoku.KijunSen.LastValue && Bars.ClosePrices.Last(1) < ichimoku.SenkouSpanA.Last(26) && Bars.ClosePrices.Last(1) < ichimoku.SenkouSpanB.Last(26))
                {
                    //SELL LOGIC
                    ExecuteMarketOrder(TradeType.Sell, Symbol.Name, GetVolume(SL), "Sell", SL, TP);


                }
            }
        }
    }
}

//Fonction de close si trade open et que le prix touche le cloud et ou Tenkan / Kijun ? tester ça   penser à utiliser les ichimoku des UT H1 et H4 pour affiner ? 
