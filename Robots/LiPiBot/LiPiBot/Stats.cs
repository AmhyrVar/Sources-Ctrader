using cAlgo.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cAlgo {
    public class Stats {

        private LiPiBotBase robot;

        private int MaxPocetOtevrenychPozic = 0;

        private double MaxZtrataAktualneOtevrenychPozic = 0;
        private double MaxZtrataAktualneOtevrenychBuyPozic = 0;
        private double MaxZtrataAktualneOtevrenychSellPozic = 0;

        private double MaxZtrataAktualneOtevrenychPozicNetProfit = 0;
        private double MaxZtrataAktualneOtevrenychBuyPozicNetProfit = 0;
        private double MaxZtrataAktualneOtevrenychSellPozicNetProfit = 0;


        public Stats(Robot robot) {
            this.robot = (LiPiBotBase) robot;
        }

        public void OnTick() {
            List<Position> positions = robot.GetPositionsAll();

            MaxPocetOtevrenychPozic = Math.Max(MaxPocetOtevrenychPozic, positions.Count);

            MaxZtrataAktualneOtevrenychPozic = Math.Min(MaxZtrataAktualneOtevrenychPozic, positions.Sum(item => item.Pips));
            MaxZtrataAktualneOtevrenychBuyPozic = Math.Min(MaxZtrataAktualneOtevrenychBuyPozic, positions.Sum(item => item.TradeType == TradeType.Buy ? item.Pips : 0));
            MaxZtrataAktualneOtevrenychSellPozic = Math.Min(MaxZtrataAktualneOtevrenychSellPozic, positions.Sum(item => item.TradeType == TradeType.Sell ? item.Pips : 0));


            MaxZtrataAktualneOtevrenychPozicNetProfit = Math.Min(MaxZtrataAktualneOtevrenychPozicNetProfit, positions.Sum(item => item.NetProfit));
            MaxZtrataAktualneOtevrenychBuyPozicNetProfit = Math.Min(MaxZtrataAktualneOtevrenychBuyPozicNetProfit, positions.Sum(item => item.TradeType == TradeType.Buy ? item.NetProfit : 0));
            MaxZtrataAktualneOtevrenychSellPozicNetProfit = Math.Min(MaxZtrataAktualneOtevrenychSellPozicNetProfit, positions.Sum(item => item.TradeType == TradeType.Sell ? item.NetProfit : 0));

            if (MaxZtrataAktualneOtevrenychPozic < -1000) {
                //robot.Stop();
            }
        }


        public void Print() {
            /*
            robot.Print("LPB Symbol : " + robot.SymbolName);

            robot.Print("MaxPocetOtevrenychPozic = " + MaxPocetOtevrenychPozic);

            robot.Print("MaxZtrataAktualneOtevrenychPozic = " + MaxZtrataAktualneOtevrenychPozic);
            robot.Print("MaxZtrataAktualneOtevrenychBuyPozic = " + MaxZtrataAktualneOtevrenychBuyPozic);
            robot.Print("MaxZtrataAktualneOtevrenychSellPozic = " + MaxZtrataAktualneOtevrenychSellPozic);

            robot.Print("MaxZtrataAktualneOtevrenychPozicNetProfit = " + MaxZtrataAktualneOtevrenychPozicNetProfit);
            robot.Print("MaxZtrataAktualneOtevrenychBuyPozicNetProfit = " + MaxZtrataAktualneOtevrenychBuyPozicNetProfit);
            robot.Print("MaxZtrataAktualneOtevrenychSellPozicNetProfit = " + MaxZtrataAktualneOtevrenychSellPozicNetProfit);
            */
        }

    }
}
