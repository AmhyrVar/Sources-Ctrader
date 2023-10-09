using cAlgo.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace cAlgo {
    public class Worker : WorkerAdditionalPostioinsBase {

        public Worker(Robot robot) {
            this.robot = (LiPiBotCoreBase) robot;

            robot.Positions.Closed += PositionsOnClosedPrint;
            //if (this.robot.OpenMoreSamePositions == LPBBase.Open_More_Positions.Yes_Aggregated) {
                // Jakmile je uzavrena jedna pozice pri nastavenem agregovani oteviranych dalsich pozic, uzavreme vsechny pozice stejneho typu.
                robot.Positions.Closed += OnPositionClosed;
            //}
        }

        private void PositionsOnClosedPrint(PositionClosedEventArgs args) {
            Position p = args.Position;
            string txt = "POSITION CLOSED: " + args.Position.SymbolName + " | TRADE TYPE: " + args.Position.TradeType.ToString() + " | VOLUME : " + args.Position.VolumeInUnits + " | LABEL: " + args.Position.Label
                + " | ID=" + args.Position.Id
                + " | StopLoss=" + args.Position.StopLoss
                + " | TakeProfit=" + args.Position.TakeProfit
                + " | REASON: " + args.Reason.ToString();

            P(txt);

            double priceAsk = robot.Symbol.Ask;
            double priceBid = robot.Symbol.Bid;
            double priceCurrent = p.TradeType == TradeType.Buy ? priceBid : priceAsk;
            DateTime primePositionLastEntry = p.EntryTime;
            double delkaOtevreniPozice = robot.Server.TimeInUtc.ToUniversalTime().Subtract(primePositionLastEntry.ToUniversalTime()).TotalMinutes;
            if (p.Comment.StartsWith(LiPiBotBase.commentAdditional) && 5 > delkaOtevreniPozice && Math.Abs(GetDifferenceInPips(p.EntryPrice, priceCurrent)) < 2) {
                // Neni splnena minimalni doba od posledni otevrene Prime Position.
                //P("pozice byla otevrena mene nez 5 minut!!! delkaOtevreniPozice=" + delkaOtevreniPozice + ", pips=" + Math.Abs(GetDifferenceInPips(p.EntryPrice, priceCurrent)));
                P("Position Age is less than 5 minut! Age = " + delkaOtevreniPozice + ", Pips = " + Math.Abs(GetDifferenceInPips(p.EntryPrice, priceCurrent)));
            }
        }

        public void OnTick() {
            if (robot.ClosePositionsByAge != LiPiBotBase.Close_Positions_By_Age.None) {
                ClosePositionsByAge();
            }

            MoveStopLossToProfit();
            //if (robot.AdditionalPositionsType != LPBBase.Additional_Positions_Type.No && robot.AdditionalPositionsSignalRequired == LPBBase.Yes_No.No) {
            if (robot.AdditionalPositionsMax > 0 && robot.AdditionalPositionsSignalRequired == LiPiBotBase.Yes_No.No) {
                //P("OnTick A");
                ProcessOpenAdditionalPosition();
            }
        }

        public void OnBar() {
                int positionsCount = robot.GetPositionsAll().Count;
                if (positionsCount > 0 && robot.ClosePositionsIfOppositeSignalExists != LiPiBotBase.Close_Positions_If_Opposite_Signal_Exists.None) {
                    //P("OnBar A positionsCount=" + positionsCount);
                    ClosePositionsWhenOpositeSignalExists();
                }

                positionsCount = robot.GetPositionsGrouped().Count; // Opětovně musíme počet pozic načíst, protože některé mohly být zavřeny.
                                                                    //if (positionsCount < robot.PrimePositionsMax || (positionsCount > 0 && robot.OpenOppositePositionIfOppositeSignalExists == LPBBase.Yes_No.Yes)) {
                                                                    //    P("OnBar B positionsCount=" + positionsCount);
                ProcessOpenPrimePosition();
                //}
                //if (robot.AdditionalPositionsType != LPBBase.Additional_Positions_Type.No && robot.AdditionalPositionsSignalRequired == LPBBase.Yes_No.Yes) {
                if (robot.AdditionalPositionsMax > 0 && robot.AdditionalPositionsSignalRequired == LiPiBotBase.Yes_No.Yes) {
                    //P("OnBar C positionsCount=" + positionsCount);
                    ProcessOpenAdditionalPosition();
                }
        }

        public void OnStop() {

        }
    }
}
