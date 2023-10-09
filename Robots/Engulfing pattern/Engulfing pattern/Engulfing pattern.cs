using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Engulfingpattern : Robot
    {

        [Parameter("Use wicks", DefaultValue = false)]
        public bool Wicks { get; set; }

        [Parameter("Moving average periods", DefaultValue = 20)]
        public int MovingAveragePeriods { get; set; }

        [Parameter("RSI periods", DefaultValue = 14)]
        public int RSI_Periods { get; set; }


        [Parameter("RSI Bull Level", DefaultValue = 50)]
        public int RSI_BullLvL { get; set; }


        [Parameter("RSI Bear Level", DefaultValue = 50)]
        public int RSI_BearLvL { get; set; }


        [Parameter("Pips to add to SL", DefaultValue = 1.1)]
        public double SL_pips_add { get; set; }

        [Parameter("Use Take profit ratio", DefaultValue = true)]
        public bool TP_Ratio { get; set; }



        [Parameter("TP Ratio", DefaultValue = 2)]
        public int Ratio { get; set; }


        [Parameter("TP in Pips", DefaultValue = 10)]
        public double TP_Pips { get; set; }


        [Parameter("Use balance %", DefaultValue = true)]
        public bool Balance_use { get; set; }


        [Parameter("Balance % position", DefaultValue = 2)]
        public double Balance_per { get; set; }


        [Parameter("Raw volume", DefaultValue = 10000)]
        public int Vol { get; set; }











        private MovingAverage Ma;
        private RelativeStrengthIndex RSI;

        protected override void OnStart()
        {
            Ma = Indicators.ExponentialMovingAverage(Bars.ClosePrices, MovingAveragePeriods);
            RSI = Indicators.RelativeStrengthIndex(Bars.ClosePrices, RSI_Periods);
        }

        protected override void OnBar()
        {

            var BuyPos = Positions.FindAll("ES", SymbolName, TradeType.Buy);
            var SellPos = Positions.FindAll("ES", SymbolName, TradeType.Sell);


            //doivent aller en trade
            //var volume = 1000;
            //var SL = 5.1;
            //var TP = SL * 2;
            //bullish
            if (!Wicks)
            {
                if (BuyPos.Length == 0 && Bars.OpenPrices.Last(2) > Bars.ClosePrices.Last(2) && Bars.ClosePrices.Last(1) > Bars.OpenPrices.Last(2) && Bars.OpenPrices.Last(1) < Bars.ClosePrices.Last(2) && Bars.ClosePrices.Last(1) > Ma.Result.Last(1) && RSI.Result.LastValue > RSI_BullLvL)
                {
                    var SL = (Bars.ClosePrices.Last(1) - Bars.OpenPrices.Last(1)) * 10000 + SL_pips_add;


                    ExecuteMarketOrder(TradeType.Buy, SymbolName, GetVolume(SL), "ES", SL, GetTP(SL));

                }

                if (SellPos.Length == 0 && Bars.OpenPrices.Last(2) < Bars.ClosePrices.Last(2) && Bars.ClosePrices.Last(1) < Bars.OpenPrices.Last(2) && Bars.OpenPrices.Last(1) > Bars.ClosePrices.Last(2) && Bars.ClosePrices.Last(1) < Ma.Result.Last(1) && RSI.Result.Last(1) < RSI_BearLvL)
                {
                    var SL = (Bars.OpenPrices.Last(1) - Bars.ClosePrices.Last(1)) * 10000 + SL_pips_add;


                    ExecuteMarketOrder(TradeType.Sell, SymbolName, GetVolume(SL), "ES", SL, GetTP(SL));

                }
            }


            if (Wicks)
            {
                if (BuyPos.Length == 0 && Bars.OpenPrices.Last(2) > Bars.ClosePrices.Last(2) && Bars.ClosePrices.Last(1) > Bars.OpenPrices.Last(2) && Bars.ClosePrices.Last(1) > Bars.HighPrices.Last(2) && Bars.OpenPrices.Last(1) < Bars.ClosePrices.Last(2) && Bars.OpenPrices.Last(1) < Bars.LowPrices.Last(2) && Bars.ClosePrices.Last(1) > Ma.Result.Last(1) && RSI.Result.LastValue > RSI_BullLvL)
                {
                    var SL = (Bars.ClosePrices.Last(1) - Bars.OpenPrices.Last(1)) * 10000 + SL_pips_add;


                    ExecuteMarketOrder(TradeType.Buy, SymbolName, GetVolume(SL), "ES", SL, GetTP(SL));

                }

                if (SellPos.Length == 0 && Bars.OpenPrices.Last(2) < Bars.ClosePrices.Last(2) && Bars.ClosePrices.Last(1) < Bars.OpenPrices.Last(2) && Bars.OpenPrices.Last(1) > Bars.ClosePrices.Last(2) && Bars.ClosePrices.Last(1) < Bars.LowPrices.Last(2) && Bars.OpenPrices.Last(1) > Bars.HighPrices.Last(2) && Bars.ClosePrices.Last(1) < Ma.Result.Last(1) && RSI.Result.Last(1) < RSI_BearLvL)
                {
                    var SL = (Bars.OpenPrices.Last(1) - Bars.ClosePrices.Last(1)) * 10000 + SL_pips_add;


                    ExecuteMarketOrder(TradeType.Sell, SymbolName, GetVolume(SL), "ES", SL, GetTP(SL));

                }
            }


        }

        protected double GetTP(double SL)
        {
            if (TP_Ratio)
            {
                return SL * Ratio;
            }

            else
            {
                return TP_Pips;
            }
        }

        protected int GetVolume(double SL)
        {
            //rount to 1000
            if (Balance_use)
            {
                // x which is 1 = (balance * risk%)/(SL*pipvalue*1000) ROUND TO INT
                var x = Math.Round((Account.Balance * Balance_per) / (100 * SL * Symbol.PipValue * 1000));

                //Convert.ToInt32(double)
                Print(x);
                return Convert.ToInt32(x * 1000);
            }

            else
            {
                return Vol;
            }
        }


    }
}
