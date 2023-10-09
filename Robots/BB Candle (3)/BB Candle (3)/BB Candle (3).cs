using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class BBCandle : Robot
    {

        [Parameter(DefaultValue = 0.1)]
        public double volume { get; set; }

        [Parameter("Drawdown to stop", DefaultValue = 40)]
        public double DrawdownStop { get; set; }

        [Parameter("Pips to trail", DefaultValue = 10)]
        public double Trail { get; set; }

        [Parameter("Number of trades above/below", DefaultValue = 5)]
        public int nbrtrades { get; set; }

        [Parameter("wait for the pair of trades to resolve", DefaultValue = false)]
        public bool Wait { get; set; }

        [Parameter("Double the volume", DefaultValue = false)]
        public bool Double_Volume { get; set; }

        public int DV;



        [Parameter("Bollinger Bands Source ")]
        public DataSeries BB_Source { get; set; }
        [Parameter("Bollinger Bands Periods", DefaultValue = 20)]
        public int BB_Period { get; set; }

        [Parameter("Bollinger Standard Dev", DefaultValue = 2)]
        public int BB_StD { get; set; }
        [Parameter("Bollinger MA Type", DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType BB_MA_Type { get; set; }


        public bool waitrigger;


        BollingerBands BB;

        public int SnbrTrades;
        public int LnbrTrades;

        public double StartBalance;



        protected override void OnStart()
        {

            StartBalance = Account.Balance;

            if (Double_Volume)
            {
                DV = 2;
            }
            if (!Double_Volume)
            {
                DV = 1;
            }


            SnbrTrades = nbrtrades;
            LnbrTrades = nbrtrades;
            BB = Indicators.BollingerBands(BB_Source, BB_Period, BB_StD, BB_MA_Type);

            if (Wait == false)
            {
                waitrigger = true;
            }

            if (Wait == true)
            {
                waitrigger = true;
            }



        }
        private double GetSLPips(double PoPips, double PoSLPrice, double PoEntry)
        {
            var SLpips = Math.Abs(Convert.ToDouble(PoSLPrice - PoEntry));

            var x = Math.Floor(PoPips / Trail);

            return (x - 1) * Trail;
        }

        protected override void OnBar()
        {
            var Longpos = Positions.FindAll("LongBB", SymbolName);
            var Shortpos = Positions.FindAll("ShortBB", SymbolName);

            var Longposs = Positions.FindAll("LongBBs", SymbolName);
            var Shortposs = Positions.FindAll("ShortBBs", SymbolName);

            //BreakEven Trigger

            if (Longpos.Length > 0)
            {
                foreach (var po in Longpos)
                {
                    if (po.StopLoss == null && po.Pips >= Trail)
                    {
                        po.ModifyStopLossPrice(po.EntryPrice);
                    }

                }
            }
            if (Longposs.Length > 0)
            {
                foreach (var po in Longposs)
                {

                    if (po.StopLoss == null && po.Pips >= Trail)
                    {
                        po.ModifyStopLossPrice(po.EntryPrice);
                    }
                }
            }

            if (Shortpos.Length > 0)
            {
                foreach (var po in Shortpos)
                {
                    if (po.StopLoss == null && po.Pips >= Trail)
                    {
                        po.ModifyStopLossPrice(po.EntryPrice);
                    }

                }
            }
            if (Shortposs.Length > 0)
            {
                foreach (var po in Shortposs)
                {
                    if (po.StopLoss == null && po.Pips >= Trail)
                    {
                        po.ModifyStopLossPrice(po.EntryPrice);


                    }
                }
            }
            //End of Breakeven region

            //Start of Trail Region
            if (Longpos.Length > 0)
            {
                foreach (var po in Longpos)
                {
                    if (po.StopLoss != null && po.Pips >= Math.Abs(Convert.ToDouble(po.EntryPrice - po.StopLoss)) + 2 * Trail)
                    {
                        po.ModifyStopLossPips(-GetSLPips(po.Pips, Convert.ToDouble(po.StopLoss), po.EntryPrice));
                    }

                }
            }

            if (Longposs.Length > 0)
            {
                foreach (var po in Longposs)
                {

                    if (po.StopLoss != null && po.Pips >= Math.Abs(Convert.ToDouble(po.EntryPrice - po.StopLoss)) + 2 * Trail)
                    {
                        po.ModifyStopLossPips(-GetSLPips(po.Pips, Convert.ToDouble(po.StopLoss), po.EntryPrice));
                    }
                }
            }

            if (Shortpos.Length > 0)
            {
                foreach (var po in Shortpos)
                {
                    if (po.StopLoss != null && po.Pips >= Math.Abs(Convert.ToDouble(po.EntryPrice - po.StopLoss)) + 2 * Trail)
                    {
                        po.ModifyStopLossPips(-GetSLPips(po.Pips, Convert.ToDouble(po.StopLoss), po.EntryPrice));
                    }

                }
            }
            if (Shortposs.Length > 0)
            {
                foreach (var po in Shortposs)
                {
                    if (po.StopLoss != null && po.Pips >= Math.Abs(Convert.ToDouble(po.EntryPrice - po.StopLoss)) + 2 * Trail)
                    {
                        po.ModifyStopLossPips(-GetSLPips(po.Pips, Convert.ToDouble(po.StopLoss), po.EntryPrice));
                    }
                }
            }



            //End of trail region 
            if ((Account.Equity / StartBalance) <= (1 - (DrawdownStop / 100)))
            {




                if (Longpos.Length > 0)
                {
                    foreach (var po in Longpos)
                    {
                        ClosePosition(po);


                    }
                }
                if (Longposs.Length > 0)
                {
                    foreach (var po in Longposs)
                    {
                        ClosePosition(po);


                    }
                }

                if (Shortpos.Length > 0)
                {
                    foreach (var po in Shortpos)
                    {
                        ClosePosition(po);


                    }
                }
                if (Shortposs.Length > 0)
                {
                    foreach (var po in Shortposs)
                    {
                        ClosePosition(po);

                    }
                }

                LnbrTrades = nbrtrades;
                SnbrTrades = nbrtrades;
                StartBalance = Account.Balance;
            }

            //Custom logic


            if (Longposs.Length == 0)
            {
                LnbrTrades = nbrtrades;

            }
            if (Shortposs.Length == 0)
            {
                SnbrTrades = nbrtrades;

            }


            if (Wait && (Longpos.Length > 0 || Shortpos.Length > 0))
            {
                waitrigger = false;
            }

            if (Wait && (Longpos.Length == 0 && Shortpos.Length == 0))
            {
                waitrigger = true;
            }


            // top scenario
            if (Bars.ClosePrices.Last(1) > BB.Top.Last(1) && (Bars.ClosePrices.Last(1) - BB.Top.Last(1)) >= (Bars.ClosePrices.Last(1) - Bars.OpenPrices.Last(1)) * 0.7)
            {
                if (Longpos.Length > 0)
                {
                    foreach (var po in Longpos)
                    {
                        ClosePosition(po);

                    }
                }
                if (Longposs.Length > 0)
                {
                    foreach (var po in Longposs)
                    {
                        ClosePosition(po);

                    }
                }


                if (waitrigger)
                {
                    ExecuteMarketOrder(TradeType.Buy, SymbolName, DV * Symbol.QuantityToVolumeInUnits(volume), "LongBB");

                    if (SnbrTrades > 0)
                    {
                        ExecuteMarketOrder(TradeType.Sell, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "ShortBBs");
                        SnbrTrades--;

                    }

                }
            }
            //bot scenario
            if (Bars.ClosePrices.Last(1) < BB.Bottom.Last(1) && (BB.Top.Last(1) - Bars.ClosePrices.Last(1)) >= (Bars.OpenPrices.Last(1) - Bars.ClosePrices.Last(1)) * 0.7)
            {


                if (Shortpos.Length > 0)
                {
                    foreach (var po in Shortpos)
                    {
                        ClosePosition(po);

                    }
                }
                if (Shortposs.Length > 0)
                {
                    foreach (var po in Shortposs)
                    {
                        ClosePosition(po);

                    }
                }

                if (waitrigger)
                {
                    if (LnbrTrades > 0)
                    {
                        ExecuteMarketOrder(TradeType.Buy, SymbolName, Symbol.QuantityToVolumeInUnits(volume), "LongBBs");
                        LnbrTrades--;

                    }
                    ExecuteMarketOrder(TradeType.Sell, SymbolName, DV * Symbol.QuantityToVolumeInUnits(volume), "ShortBB");

                }
            }


            // var Longpos = Positions.FindAll("Buy", SymbolName);
            // var Shortpos = Positions.FindAll("Sell", SymbolName);





        }



        protected override void OnStop()
        {
            Print("it stopped at" + DateTime.Now);
        }
    }
}
