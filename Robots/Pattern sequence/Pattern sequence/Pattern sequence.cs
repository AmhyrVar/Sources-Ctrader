using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;


namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Patternsequence : Robot
    {
        [Parameter(DefaultValue = 2)]
        public double percentageEquity { get; set; }



        [Parameter(DefaultValue = 1)]
        public int Seq1_1 { get; set; }
        [Parameter(DefaultValue = 2)]
        public int Seq1_2 { get; set; }
        [Parameter(DefaultValue = 1)]
        public int Seq1_3 { get; set; }
        [Parameter(DefaultValue = 0)]
        public int Seq1_4 { get; set; }
        [Parameter(DefaultValue = 0)]
        public int Seq1_5 { get; set; }
        [Parameter(DefaultValue = 0)]
        public int Seq1_6 { get; set; }
        [Parameter(DefaultValue = 0)]
        public int Seq1_7 { get; set; }
        [Parameter(DefaultValue = 0)]
        public int Seq1_8 { get; set; }
        [Parameter(DefaultValue = 0)]
        public int Seq1_9 { get; set; }


        [Parameter(DefaultValue = 1)]
        public int Seq2_1 { get; set; }
        [Parameter(DefaultValue = 2)]
        public int Seq2_2 { get; set; }
        [Parameter(DefaultValue = 1)]
        public int Seq2_3 { get; set; }
        [Parameter(DefaultValue = 0)]
        public int Seq2_4 { get; set; }
        [Parameter(DefaultValue = 0)]
        public int Seq2_5 { get; set; }
        [Parameter(DefaultValue = 0)]
        public int Seq2_6 { get; set; }
        [Parameter(DefaultValue = 0)]
        public int Seq2_7 { get; set; }
        [Parameter(DefaultValue = 0)]
        public int Seq2_8 { get; set; }
        [Parameter(DefaultValue = 0)]
        public int Seq2_9 { get; set; }




        int NbrBougies_1;
        int NbrBougies_2;

        double Volume = 10000;
        double Eq;





        //extend the pattern to 9



        List<int> Sequence1 = new List<int>();
        List<double> Sequence1High = new List<double>();
        List<double> Sequence1Low = new List<double>();
        List<int> Sequence2 = new List<int>();
        List<double> Sequence2High = new List<double>();
        List<double> Sequence2Low = new List<double>();


        //tradepos handle


        public double last_trade_price;

        //hammer upper limit and lower limit
        List<double> HUL = new List<double>();
        List<double> HLL = new List<double>();

        //TP levels UP and Down
        List<double> TPHL = new List<double>();
        List<double> TPLL = new List<double>();



        //Position direction index
        //Buy position index and sell position index
        List<int> BPI = new List<int>();
        List<int> SPI = new List<int>();

        protected int bougie(int candleindex)
        {
            if (Bars.ClosePrices.Last(candleindex) > Bars.OpenPrices.Last(candleindex))
            {
                return 1;
            }
            //1 for haussier       
            else
            {
                return 2;
            }
            //2 pour baissiez
        }


        protected bool pattern1()
        {
            var test = false;
            var header = 0;
            while (header < NbrBougies_1)
            {
                if (bougie(NbrBougies_1 - header) == Sequence1[header])
                {
                    test = true;
                    header = header + 1;

                }

                else
                {
                    test = false;
                    break;
                }
            }




            if (test == true)
            {
                return true;




            }
            else
            {
                return false;
            }

        }


        protected bool pattern2()
        {
            var test = false;
            var header = 0;
            while (header < NbrBougies_2)
            {
                if (bougie(NbrBougies_2 - header) == Sequence2[header])
                {
                    test = true;
                    header = header + 1;

                }

                else
                {
                    test = false;
                    break;
                }
            }




            if (test == true)
            {
                return true;




            }
            else
            {
                return false;
            }

        }

        protected void Seq_1()
        {
            var i = 0;
            while (i < Seq1_1)
            {
                Sequence1.Add(1);

                i = i + 1;
                ;
            }
            i = 0;
            while (i < Seq1_2)
            {
                Sequence1.Add(2);
                i = i + 1;
            }
            i = 0;
            while (i < Seq1_3)
            {
                Sequence1.Add(1);
                i = i + 1;
            }
            i = 0;
            while (i < Seq1_4)
            {
                Sequence1.Add(2);
                i = i + 1;
            }
            i = 0;
            while (i < Seq1_5)
            {
                Sequence1.Add(1);
                i = i + 1;
            }
            while (i < Seq1_6)
            {
                Sequence1.Add(2);
                i = i + 1;
            }
            i = 0;
            while (i < Seq1_7)
            {
                Sequence1.Add(1);
                i = i + 1;
            }
            i = 0;
            while (i < Seq1_8)
            {
                Sequence1.Add(2);
                i = i + 1;
            }
            i = 0;
            while (i < Seq1_9)
            {
                Sequence1.Add(1);
                i = i + 1;
            }

        }

        protected void Seq_2()
        {
            var i = 0;
            while (i < Seq2_1)
            {
                Sequence2.Add(1);

                i = i + 1;
                ;
            }
            i = 0;
            while (i < Seq2_2)
            {
                Sequence2.Add(2);
                i = i + 1;
            }
            i = 0;
            while (i < Seq2_3)
            {
                Sequence2.Add(1);
                i = i + 1;
            }
            i = 0;
            while (i < Seq2_4)
            {
                Sequence2.Add(2);
                i = i + 1;
            }
            i = 0;
            while (i < Seq2_5)
            {
                Sequence2.Add(1);
                i = i + 1;
            }
            while (i < Seq2_6)
            {
                Sequence2.Add(2);
                i = i + 1;
            }
            i = 0;
            while (i < Seq2_7)
            {
                Sequence2.Add(1);
                i = i + 1;
            }
            i = 0;
            while (i < Seq2_8)
            {
                Sequence2.Add(2);
                i = i + 1;
            }
            i = 0;
            while (i < Seq2_9)
            {
                Sequence2.Add(1);
                i = i + 1;
            }

        }

        protected override void OnStart()
        {
            Eq = Account.Equity;

            HUL.Add(0);
            HLL.Add(0);
            BPI.Add(0);
            SPI.Add(0);
            TPHL.Add(0);
            TPLL.Add(0);


            Seq_1();
            Seq_2();

            NbrBougies_1 = Seq1_1 + Seq1_2 + Seq1_3 + Seq1_4 + Seq1_5 + Seq1_6 + Seq1_7 + Seq1_8 + Seq1_9;



            NbrBougies_2 = Seq2_1 + Seq2_2 + Seq2_3 + Seq2_4 + Seq2_5 + Seq2_6 + Seq2_7 + Seq2_8 + Seq2_9;



        }
        protected void Seq_1_HighLow()
        {
            Sequence1High.Clear();
            Sequence1Low.Clear();
            var i = 1;
            while (i <= NbrBougies_1)
            {
                Sequence1High.Add(Bars.HighPrices.Last(i));
                Sequence1Low.Add(Bars.LowPrices.Last(i));
                i = i + 1;
            }




        }

        protected void Seq_2_HighLow()
        {
            Sequence2High.Clear();
            Sequence2Low.Clear();
            var i = 1;
            while (i <= NbrBougies_2)
            {
                Sequence2High.Add(Bars.HighPrices.Last(i));
                Sequence2Low.Add(Bars.LowPrices.Last(i));
                i = i + 1;
            }

        }



        protected override void OnBar()
        {


            if (pattern1() == true && Positions.Count < 2)
            {
                Seq_1_HighLow();
                last_trade_price = Bars.OpenPrices.LastValue;
                //écriture de l'index de position
                BPI.Add(BPI[BPI.Count - 1] + 1);
                //écriture du High à dépasser une première fois HUL hammer up limit
                HUL.Add(Sequence1High.Max());
                //initialisation de valeurs temporaires des TPHL et TPLL
                TPHL.Add(99999999);
                ExecuteMarketOrder(TradeType.Buy, SymbolName, Volume, "buy", 999999, 99999999, "" + BPI[BPI.Count - 1]);



                SPI.Add(SPI[SPI.Count - 1] + 1);
                HLL.Add(Sequence1Low.Min());
                TPLL.Add(0);
                ExecuteMarketOrder(TradeType.Sell, SymbolName, Volume, "sell", 999999, 99999999, "" + BPI[BPI.Count - 1]);
            }
            if (pattern2() == true && Positions.Count < 2)
            {
                Seq_2_HighLow();

                last_trade_price = Bars.OpenPrices.LastValue;
                //écriture de l'index de position
                BPI.Add(BPI[BPI.Count - 1] + 1);
                //écriture du High à dépasser une première fois HUL hammer up limit
                HUL.Add(Math.Max(Bars.HighPrices.Last(1), Bars.HighPrices.Last(2)));
                HUL.Add(Sequence2High.Max());
                //initialisation de valeurs temporaires des TPHL et TPLL
                TPHL.Add(99999999);
                ExecuteMarketOrder(TradeType.Buy, SymbolName, Volume, "buy", 999999, 99999999, "" + BPI[BPI.Count - 1]);

                SPI.Add(SPI[SPI.Count - 1] + 1);
                HLL.Add(Sequence2Low.Min());
                TPLL.Add(0);
                ExecuteMarketOrder(TradeType.Sell, SymbolName, Volume, "sell", 999999, 99999999, "" + BPI[BPI.Count - 1]);
            }


            foreach (var position in Positions)
            {


                var pos_label = Convert.ToInt32(position.Comment);
                if (position.TradeType == TradeType.Buy && Bars.ClosePrices.LastValue > HUL[pos_label] && TPHL[pos_label] == 99999999)
                {
                    TPHL[pos_label] = Bars.ClosePrices.LastValue;

                }

                if (position.TradeType == TradeType.Sell && Bars.ClosePrices.LastValue < HLL[pos_label] && TPLL[pos_label] == 0)
                {
                    TPLL[pos_label] = Bars.ClosePrices.LastValue;
                }
                if (position.TradeType == TradeType.Buy && Bars.ClosePrices.LastValue > TPHL[pos_label])
                {
                    ClosePosition(position);
                }
                if (position.TradeType == TradeType.Sell && Bars.ClosePrices.LastValue < TPLL[pos_label])
                {
                    ClosePosition(position);
                }

                //Close at % Eq
                if (position.NetProfit <= -(percentageEquity * Eq / 100))
                {
                    ClosePosition(position);
                    Print(position.NetProfit);
                }



            }
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
