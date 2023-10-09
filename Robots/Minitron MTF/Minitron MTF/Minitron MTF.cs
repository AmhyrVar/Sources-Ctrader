using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;


namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MinitronMTF : Robot
    {


        [Parameter(DefaultValue = 1000)]
        public int Volume { get; set; }






        [Parameter(DefaultValue = 0)]
        public int PerceptronBuyOpenTrigger { get; set; }
        [Parameter(DefaultValue = 0)]
        public int PerceptronBuyOpenTrigger1 { get; set; }
        [Parameter(DefaultValue = 0)]
        public int PerceptronBuyOpenTrigger4 { get; set; }
        [Parameter(DefaultValue = 0)]
        public int PerceptronBuyCloseTrigger { get; set; }
        [Parameter(DefaultValue = 0)]
        public int PerceptronBuyCloseTrigger1 { get; set; }
        [Parameter(DefaultValue = 0)]
        public int PerceptronBuyCloseTrigger4 { get; set; }
        [Parameter(DefaultValue = 0)]
        public int SSAtrigger { get; set; }
        [Parameter(DefaultValue = 0)]
        public int SSAtrigger1 { get; set; }
        [Parameter(DefaultValue = 0)]
        public int SSAtrigger4 { get; set; }
        [Parameter(DefaultValue = 0)]
        public int SSAtriggerclose { get; set; }
        [Parameter(DefaultValue = 0)]
        public int SSAtriggerclose1 { get; set; }
        [Parameter(DefaultValue = 0)]
        public int SSAtriggerclose4 { get; set; }

        public double close;
        public Position _position;
        IchimokuKinkoHyo ichimoku;
        IchimokuKinkoHyo ichimoku1;
        IchimokuKinkoHyo ichimoku4;




        protected override void OnStart()
        {
            ichimoku = Indicators.IchimokuKinkoHyo(9, 26, 52);
            ichimoku1 = Indicators.IchimokuKinkoHyo(36, 104, 208);
            ichimoku4 = Indicators.IchimokuKinkoHyo(144, 416, 832);
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
        int PerceptronBuyOpen1()
        {
            if (PerceptronBuyOpenTrigger1 == 1)
            {
                return 0;
            }
            else if (PerceptronBuyOpenTrigger1 == 2)
            {
                return 16;
            }
            else if (PerceptronBuyOpenTrigger1 == 3)
            {
                return 32;
            }
            else if (PerceptronBuyOpenTrigger1 == 4)
            {
                return 48;
            }
            else if (PerceptronBuyOpenTrigger1 == 5)
            {
                return 80;
            }
            else if (PerceptronBuyOpenTrigger1 == 6)
            {
                return 112;
            }
            else if (PerceptronBuyOpenTrigger1 == 7)
            {
                return 160;
            }
            else if (PerceptronBuyOpenTrigger1 == 8)
            {
                return 240;
            }
            else if (PerceptronBuyOpenTrigger1 == 9)
            {
                return 256;
            }
            else if (PerceptronBuyOpenTrigger1 == 10)
            {
                return 336;
            }
            else if (PerceptronBuyOpenTrigger1 == 11)
            {
                return 384;
            }
            else if (PerceptronBuyOpenTrigger1 == 12)
            {
                return 416;
            }
            else if (PerceptronBuyOpenTrigger1 == 13)
            {
                return 448;
            }
            else if (PerceptronBuyOpenTrigger1 == 14)
            {
                return 480;
            }
            else if (PerceptronBuyOpenTrigger1 == 15)
            {
                return 496;
            }

            else
            {
                return 9999;
            }

        }

        int PerceptronBuyOpen4()
        {
            if (PerceptronBuyOpenTrigger4 == 1)
            {
                return 0;
            }
            else if (PerceptronBuyOpenTrigger4 == 2)
            {
                return 16;
            }
            else if (PerceptronBuyOpenTrigger4 == 3)
            {
                return 32;
            }
            else if (PerceptronBuyOpenTrigger4 == 4)
            {
                return 48;
            }
            else if (PerceptronBuyOpenTrigger4 == 5)
            {
                return 80;
            }
            else if (PerceptronBuyOpenTrigger4 == 6)
            {
                return 112;
            }
            else if (PerceptronBuyOpenTrigger4 == 7)
            {
                return 160;
            }
            else if (PerceptronBuyOpenTrigger4 == 8)
            {
                return 240;
            }
            else if (PerceptronBuyOpenTrigger4 == 9)
            {
                return 256;
            }
            else if (PerceptronBuyOpenTrigger4 == 10)
            {
                return 336;
            }
            else if (PerceptronBuyOpenTrigger4 == 11)
            {
                return 384;
            }
            else if (PerceptronBuyOpenTrigger4 == 12)
            {
                return 416;
            }
            else if (PerceptronBuyOpenTrigger4 == 13)
            {
                return 448;
            }
            else if (PerceptronBuyOpenTrigger4 == 14)
            {
                return 480;
            }
            else if (PerceptronBuyOpenTrigger4 == 15)
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
            else if (PerceptronBuyCloseTrigger == 2)
            {
                return 16;
            }
            else if (PerceptronBuyCloseTrigger == 3)
            {
                return 32;
            }
            else if (PerceptronBuyCloseTrigger == 4)
            {
                return 48;
            }
            else if (PerceptronBuyCloseTrigger == 5)
            {
                return 80;
            }
            else if (PerceptronBuyCloseTrigger == 6)
            {
                return 112;
            }
            else if (PerceptronBuyCloseTrigger == 7)
            {
                return 160;
            }
            else if (PerceptronBuyCloseTrigger == 8)
            {
                return 240;
            }
            else if (PerceptronBuyCloseTrigger == 9)
            {
                return 256;
            }
            else if (PerceptronBuyCloseTrigger == 10)
            {
                return 336;
            }
            else if (PerceptronBuyCloseTrigger == 11)
            {
                return 384;
            }
            else if (PerceptronBuyCloseTrigger == 12)
            {
                return 416;
            }
            else if (PerceptronBuyCloseTrigger == 13)
            {
                return 448;
            }
            else if (PerceptronBuyCloseTrigger == 14)
            {
                return 480;
            }
            else if (PerceptronBuyCloseTrigger == 15)
            {
                return 496;
            }

            else
            {
                return 9999;
            }

        }

        int PerceptronBuyClose1()
        {
            if (PerceptronBuyCloseTrigger1 == 1)
            {
                return 0;
            }
            else if (PerceptronBuyCloseTrigger1 == 2)
            {
                return 16;
            }
            else if (PerceptronBuyCloseTrigger1 == 3)
            {
                return 32;
            }
            else if (PerceptronBuyCloseTrigger1 == 4)
            {
                return 48;
            }
            else if (PerceptronBuyCloseTrigger1 == 5)
            {
                return 80;
            }
            else if (PerceptronBuyCloseTrigger == 6)
            {
                return 112;
            }
            else if (PerceptronBuyCloseTrigger1 == 7)
            {
                return 160;
            }
            else if (PerceptronBuyCloseTrigger1 == 8)
            {
                return 240;
            }
            else if (PerceptronBuyCloseTrigger1 == 9)
            {
                return 256;
            }
            else if (PerceptronBuyCloseTrigger1 == 10)
            {
                return 336;
            }
            else if (PerceptronBuyCloseTrigger1 == 11)
            {
                return 384;
            }
            else if (PerceptronBuyCloseTrigger1 == 12)
            {
                return 416;
            }
            else if (PerceptronBuyCloseTrigger1 == 13)
            {
                return 448;
            }
            else if (PerceptronBuyCloseTrigger1 == 14)
            {
                return 480;
            }
            else if (PerceptronBuyCloseTrigger1 == 15)
            {
                return 496;
            }

            else
            {
                return 9999;
            }

        }

        int PerceptronBuyClose4()
        {
            if (PerceptronBuyCloseTrigger4 == 1)
            {
                return 0;
            }
            else if (PerceptronBuyCloseTrigger4 == 2)
            {
                return 16;
            }
            else if (PerceptronBuyCloseTrigger4 == 3)
            {
                return 32;
            }
            else if (PerceptronBuyCloseTrigger4 == 4)
            {
                return 48;
            }
            else if (PerceptronBuyCloseTrigger4 == 5)
            {
                return 80;
            }
            else if (PerceptronBuyCloseTrigger4 == 6)
            {
                return 112;
            }
            else if (PerceptronBuyCloseTrigger4 == 7)
            {
                return 160;
            }
            else if (PerceptronBuyCloseTrigger4 == 8)
            {
                return 240;
            }
            else if (PerceptronBuyCloseTrigger4 == 9)
            {
                return 256;
            }
            else if (PerceptronBuyCloseTrigger4 == 10)
            {
                return 336;
            }
            else if (PerceptronBuyCloseTrigger4 == 11)
            {
                return 384;
            }
            else if (PerceptronBuyCloseTrigger4 == 12)
            {
                return 416;
            }
            else if (PerceptronBuyCloseTrigger4 == 13)
            {
                return 448;
            }
            else if (PerceptronBuyCloseTrigger4 == 14)
            {
                return 480;
            }
            else if (PerceptronBuyCloseTrigger4 == 15)
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

        int SSASSB1()
        {
            if (ichimoku1.SenkouSpanA.Last(26) > ichimoku1.SenkouSpanB.Last(26))
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }

        int PSSA1()
        {

            if (close > ichimoku1.SenkouSpanA.Last(26))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }


        int PSSB1()
        {
            if (close > ichimoku1.SenkouSpanB.Last(26))
            {
                return 2;
            }
            else
            {
                return 0;
            }
        }

        int PK1()
        {
            if (close > ichimoku1.KijunSen.Last(0))
            {
                return 4;
            }
            else
            {
                return 0;
            }
        }

        int PT1()
        {
            if (close > ichimoku1.TenkanSen.Last(0))
            {
                return 8;
            }
            else
            {
                return 0;
            }
        }

        int KSSB1()
        {
            if (ichimoku1.KijunSen.Last(0) > ichimoku1.SenkouSpanB.Last(26))
            {
                return 16;
            }
            else
            {
                return 0;
            }
        }

        int KSSA1()
        {
            if (ichimoku1.KijunSen.Last(0) > ichimoku1.SenkouSpanA.Last(26))
            {
                return 32;
            }
            else
            {
                return 0;
            }
        }

        int TSSB1()
        {
            if (ichimoku1.TenkanSen.Last(0) > ichimoku1.SenkouSpanB.Last(26))
            {
                return 64;
            }
            else
            {
                return 0;
            }
        }

        int TSSA1()
        {
            if (ichimoku1.TenkanSen.Last(0) > ichimoku1.SenkouSpanA.Last(26))
            {
                return 128;
            }
            else
            {
                return 0;
            }
        }

        int TK1()
        {
            if (ichimoku1.TenkanSen.Last(0) > ichimoku1.KijunSen.Last(0))
            {
                return 256;
            }
            else
            {
                return 0;
            }
        }


        int SSASSB4()
        {
            if (ichimoku4.SenkouSpanA.Last(26) > ichimoku4.SenkouSpanB.Last(26))
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }

        int PSSA4()
        {

            if (close > ichimoku4.SenkouSpanA.Last(26))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }


        int PSSB4()
        {
            if (close > ichimoku4.SenkouSpanB.Last(26))
            {
                return 2;
            }
            else
            {
                return 0;
            }
        }

        int PK4()
        {
            if (close > ichimoku4.KijunSen.Last(0))
            {
                return 4;
            }
            else
            {
                return 0;
            }
        }

        int PT4()
        {
            if (close > ichimoku4.TenkanSen.Last(0))
            {
                return 8;
            }
            else
            {
                return 0;
            }
        }

        int KSSB4()
        {
            if (ichimoku4.KijunSen.Last(0) > ichimoku4.SenkouSpanB.Last(26))
            {
                return 16;
            }
            else
            {
                return 0;
            }
        }

        int KSSA4()
        {
            if (ichimoku4.KijunSen.Last(0) > ichimoku4.SenkouSpanA.Last(26))
            {
                return 32;
            }
            else
            {
                return 0;
            }
        }

        int TSSB4()
        {
            if (ichimoku4.TenkanSen.Last(0) > ichimoku4.SenkouSpanB.Last(26))
            {
                return 64;
            }
            else
            {
                return 0;
            }
        }

        int TSSA4()
        {
            if (ichimoku4.TenkanSen.Last(0) > ichimoku4.SenkouSpanA.Last(26))
            {
                return 128;
            }
            else
            {
                return 0;
            }
        }

        int TK4()
        {
            if (ichimoku4.TenkanSen.Last(0) > ichimoku4.KijunSen.Last(0))
            {
                return 256;
            }
            else
            {
                return 0;
            }
        }

        int SSASSB()
        {
            if (ichimoku.SenkouSpanA.Last(26) > ichimoku.SenkouSpanB.Last(26))
            {
                return 1;
            }
            else
            {
                return 0;
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
            int per1 = perceptron1();
            int per4 = perceptron4();
            bool IPO = IsPosOpen();
            int PerBuyOpen = PerceptronBuyOpen();
            int PerBuyClose = PerceptronBuyClose();
            int PerBuyOpen1 = PerceptronBuyOpen1();
            int PerBuyClose1 = PerceptronBuyClose1();
            int PerBuyOpen4 = PerceptronBuyOpen4();
            int PerBuyClose4 = PerceptronBuyClose4();
            int SSA = SSASSB();
            int SSA1 = SSASSB1();
            int SSA4 = SSASSB4();







            if (!IPO)
            {
                if (per == PerBuyOpen && per1 == PerBuyOpen1 && per4 == PerBuyOpen4 && SSA == SSAtrigger && SSA1 == SSAtrigger1 && SSA4 == SSAtrigger4)
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
                if (IPO)
                {
                    if (per == PerBuyClose && per1 == PerBuyClose1 && per4 == PerBuyClose4 && SSA == SSAtriggerclose && SSA1 == SSAtriggerclose1 && SSA4 == SSAtriggerclose4)
                    {
                        ClosePosition(position);
                    }
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

        private int perceptron1()
        {
            int i1 = PSSA1();
            int i2 = PSSB1();
            int i3 = PK1();
            int i4 = PT1();
            int i5 = KSSB1();
            int i6 = KSSA1();
            int i7 = TSSB1();
            int i8 = TSSA1();
            int i9 = TK1();




            return i1 + i2 + i3 + i4 + i5 + i6 + i7 + i8 + i9;

        }
        private int perceptron4()
        {
            int i1 = PSSA4();
            int i2 = PSSB4();
            int i3 = PK4();
            int i4 = PT4();
            int i5 = KSSB4();
            int i6 = KSSA4();
            int i7 = TSSB4();
            int i8 = TSSA4();
            int i9 = TK4();




            return i1 + i2 + i3 + i4 + i5 + i6 + i7 + i8 + i9;


        }
    }
}




