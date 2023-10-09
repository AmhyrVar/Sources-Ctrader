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






        [Parameter(DefaultValue = 0)]
        public int PerceptronBuyOpenTrigger { get; set; }
        [Parameter(DefaultValue = 0)]
        public int PerceptronBuyCloseTrigger { get; set; }

        public double close;
        public Position _position;












        IchimokuKinkoHyo ichimoku;




        protected override void OnStart()
        {
            ichimoku = Indicators.IchimokuKinkoHyo(9, 26, 52);
            double close = Bars.ClosePrices.LastValue;

        }

        //int totalPositions = Positions.Count;

        int PerceptronBuyOpen()
        {
            if (PerceptronBuyOpenTrigger == 1)
            {
                return 0;
            }
            else if (PerceptronBuyOpenTrigger == 2)
            {
                return 16;
            }
            else if (PerceptronBuyOpenTrigger == 3)
            {
                return 32;
            }
            else if (PerceptronBuyOpenTrigger == 4)
            {
                return 48;
            }
            else if (PerceptronBuyOpenTrigger == 5)
            {
                return 80;
            }
            else if (PerceptronBuyOpenTrigger == 6)
            {
                return 112;
            }
            else if (PerceptronBuyOpenTrigger == 7)
            {
                return 160;
            }
            else if (PerceptronBuyOpenTrigger == 8)
            {
                return 240;
            }
            else if (PerceptronBuyOpenTrigger == 9)
            {
                return 256;
            }
            else if (PerceptronBuyOpenTrigger == 10)
            {
                return 336;
            }
            else if (PerceptronBuyOpenTrigger == 11)
            {
                return 384;
            }
            else if (PerceptronBuyOpenTrigger == 12)
            {
                return 416;
            }
            else if (PerceptronBuyOpenTrigger == 13)
            {
                return 448;
            }
            else if (PerceptronBuyOpenTrigger == 14)
            {
                return 480;
            }
            else if (PerceptronBuyOpenTrigger == 15)
            {
                return 496;
            }

            else
            {
                return 9999;
            }

        }

        int PerceptronBuyClose()
        {
            if (PerceptronBuyCloseTrigger == 1)
            {
                return 0;
            }
            else if (PerceptronBuyOpenTrigger == 2)
            {
                return 16;
            }
            else if (PerceptronBuyOpenTrigger == 3)
            {
                return 32;
            }
            else if (PerceptronBuyOpenTrigger == 4)
            {
                return 48;
            }
            else if (PerceptronBuyOpenTrigger == 5)
            {
                return 80;
            }
            else if (PerceptronBuyOpenTrigger == 6)
            {
                return 112;
            }
            else if (PerceptronBuyOpenTrigger == 7)
            {
                return 160;
            }
            else if (PerceptronBuyOpenTrigger == 8)
            {
                return 240;
            }
            else if (PerceptronBuyOpenTrigger == 9)
            {
                return 256;
            }
            else if (PerceptronBuyOpenTrigger == 10)
            {
                return 336;
            }
            else if (PerceptronBuyOpenTrigger == 11)
            {
                return 384;
            }
            else if (PerceptronBuyOpenTrigger == 12)
            {
                return 416;
            }
            else if (PerceptronBuyOpenTrigger == 13)
            {
                return 448;
            }
            else if (PerceptronBuyOpenTrigger == 14)
            {
                return 480;
            }
            else if (PerceptronBuyOpenTrigger == 15)
            {
                return 496;
            }

            else
            {
                return 9999;
            }

        }

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

        int PSSA()
        {

            if (close > ichimoku.SenkouSpanA.Last(26))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }


        int PSSB()
        {
            if (close > ichimoku.SenkouSpanB.Last(26))
            {
                return 2;
            }
            else
            {
                return 0;
            }
        }

        int PK()
        {
            if (close > ichimoku.KijunSen.Last(0))
            {
                return 4;
            }
            else
            {
                return 0;
            }
        }

        int PT()
        {
            if (close > ichimoku.TenkanSen.Last(0))
            {
                return 8;
            }
            else
            {
                return 0;
            }
        }

        int KSSB()
        {
            if (ichimoku.KijunSen.Last(0) > ichimoku.SenkouSpanB.Last(26))
            {
                return 16;
            }
            else
            {
                return 0;
            }
        }

        int KSSA()
        {
            if (ichimoku.KijunSen.Last(0) > ichimoku.SenkouSpanA.Last(26))
            {
                return 32;
            }
            else
            {
                return 0;
            }
        }

        int TSSB()
        {
            if (ichimoku.TenkanSen.Last(0) > ichimoku.SenkouSpanB.Last(26))
            {
                return 64;
            }
            else
            {
                return 0;
            }
        }

        int TSSA()
        {
            if (ichimoku.TenkanSen.Last(0) > ichimoku.SenkouSpanA.Last(26))
            {
                return 128;
            }
            else
            {
                return 0;
            }
        }

        int TK()
        {
            if (ichimoku.TenkanSen.Last(0) > ichimoku.KijunSen.Last(0))
            {
                return 256;
            }
            else
            {
                return 0;
            }
        }




        protected override void OnBar()
        {

            int per = perceptron();
            bool IPO = IsPosOpen();
            int PerBuyOpen = PerceptronBuyOpen();
            int PerBuyClose = PerceptronBuyClose();





            if (!IPO)
            {
                if (per == PerBuyOpen)
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
                if (IPO && per == PerBuyClose)
                {
                    ClosePosition(position);
                }
            }





        }

        private int perceptron()
        {
            int i1 = PSSA();
            int i2 = PSSB();
            int i3 = PK();
            int i4 = PT();
            int i5 = KSSB();
            int i6 = KSSA();
            int i7 = TSSB();
            int i8 = TSSA();
            int i9 = TK();




            return i1 + i2 + i3 + i4 + i5 + i6 + i7 + i8 + i9;

        }
    }
}








