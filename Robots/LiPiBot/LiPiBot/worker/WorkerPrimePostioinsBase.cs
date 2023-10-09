using cAlgo;
using cAlgo.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public abstract class WorkerPrimePostioinsBase : WorkerClosePostionsBase {

    /*
     * Hledani Prime Position v History je casove mirne narocnejsi a muze zpomalovat bota predevsim pri backtestingu.
     * Proto mame list s jiz uzavrenymi Prime Positions, abychom se pro ne nemuseli stale dotazovat do History.
     * Z listu je Prime Position odstranena, kdyz uz neni otevrena zadna Additional Position patrici k danemu Prime Position.
     * */
    private List<PrimePosition> closedPrimePositions = new List<PrimePosition>();

    protected void ProcessOpenPrimePosition() {
        if (robot.BotPositionAction.Equals(LiPiBotBase.Bot_Position_Action.Open_And_Close_Positions) == false
            && robot.BotPositionAction.Equals(LiPiBotBase.Bot_Position_Action.Open_Positions) == false) {
            // Pokud není dovoleno otevírání pozic, nebudeme hledat ani sygnál.
            // I když není dovoleno, otevérání nových pozic, bot stále může otevírat pozice přikupováním - pokud ani to nechceme, musíme počet přikupovaných pozic nastavit = 0.
            return;
        }

        HashSet<TradeType> allowedTradeTypes = AllowedTradeTypeForPrimePosition();
        if (allowedTradeTypes == null) {
            // Jiz je otevren maximalni pocet Prime Positions pro Buy i Sell.
            return;
        }
        // TODO: jen pro testovani - DELETE::
        /*
        foreach(TradeType tt in allowedTradeTypes) {
            TradeResult tr = Open(tt, 1, null);
        }
        */
        //return;
        // ---- TODO ----


        SIGNAL signal = robot.SignalWorker.GetSignal();
        if (signal != SIGNAL.NONE) {
            TradeType tradeType = GetTradeTypeBySignal(signal);

            if (tradeType.Equals(TradeType.Buy) && robot.BotTradeType.Equals(LiPiBotBase.Bot_Trade_Type.Sell)) {
                // Není dovoleno otevírat Buy pozice.
                return;
            }
            if (tradeType.Equals(TradeType.Sell) && robot.BotTradeType.Equals(LiPiBotBase.Bot_Trade_Type.Buy)) {
                // Není dovoleno otevírat Sell pozice.
                return;
            }
            if (allowedTradeTypes.Contains(tradeType) == false) {
                // Jiz mame otevreno maximalni pocet Prime Pozic pro dany TraderType.
                return;
            }

            //List<Position> positions = robot.GetPositions().FindAll(item => item.TradeType == tradeType);

            //if (positions.Count == 0) {
            //if (IsAllowedOpenNewPrimePosition(tradeType)) {
            // pri main signalu = okamzite orvirame prvni pozici
            TradeResult tr = Open(tradeType, 1, null, LiPiBotCoreBase.Open_Position_Cause_Type.PRIME_POSITION);
            if (tr != null) {
                robot.SignalWorker.OnPositionOpen(tr);

                // TODO: toto je jen pro testovaeni  = DELETE
                //Open(tradeType, 1, tr.Position);
            }
            //}
        }
    }

    private HashSet<TradeType> AllowedTradeTypeForPrimePosition() {
        List<List<Position>> positions = robot.GetPositionsGrouped();

        List<List<Position>> positionsBuy = positions.FindAll(item => item[0].TradeType == TradeType.Buy);
        List<List<Position>> positionsSell = positions.FindAll(item => item[0].TradeType == TradeType.Sell);
        
        HashSet<TradeType> result = new HashSet<TradeType>();
        //if (positionsBuy.Count < robot.PrimePositionsMax) {
        if (robot.OpenOppositePrimePositionIfOppositeSignalExists == LiPiBotBase.Yes_No.Yes
            || (robot.OpenOppositePrimePositionIfOppositeSignalExists == LiPiBotBase.Yes_No.No && (positionsSell.Count == 0 || positions.Count == 0))) {

            if (IsPossibleToOpenNewPrimePosition(positionsBuy)) {
                result.Add(TradeType.Buy);
            }
        }
        //}
        //if (positionsSell.Count < robot.PrimePositionsMax) {
        if (robot.OpenOppositePrimePositionIfOppositeSignalExists == LiPiBotBase.Yes_No.Yes
            || (robot.OpenOppositePrimePositionIfOppositeSignalExists == LiPiBotBase.Yes_No.No && (positionsBuy.Count == 0 || positions.Count == 0))) {

            if (IsPossibleToOpenNewPrimePosition(positionsSell)) {
                result.Add(TradeType.Sell);
            }
        }
        //}

        if (result.Count == 0) return null;
        return result;
    }


    private bool IsPossibleToOpenNewPrimePosition(List<List<Position>> positionsGrouped) {
        if (positionsGrouped.Count == 0) return true;
        if (positionsGrouped.Count >= robot.PrimePositionsMax) {
            return false;
        }

        // Najdeme vsechnz Prime Position (u pozic, ktere jiz byly zavreny, bereme pozici z historie):
        List<PrimePosition> primePositions = GetPrimePositions(positionsGrouped);
        if (primePositions.Count == 0) {
            throw new Exception("Pocet ziskanych Prime Positions je nulovy.");
        }

        DateTime primePositionLastEntry = primePositions.Max(r => r.entryTime);// .Find(item => item.entryTime == primePositions.Max(r => r.entryTime));
        if (robot.PrimePositionMinAge != 0 && GetMultiplikatorValue(robot.PrimePositionMinAge, primePositions.Count, robot.PrimePositionMinAgeMultiplicator) > robot.Server.TimeInUtc.ToUniversalTime().Subtract(primePositionLastEntry.ToUniversalTime()).TotalMinutes) {
            // Neni splnena minimalni doba od posledni otevrene Prime Position.
            return false;
        }
        if (robot.PrimePositionMinLossPips == 0 && robot.PrimePositionMinProfitPips == 0) {
            return true;
        }

        TradeType tradeType = positionsGrouped[0][0].TradeType;
        double priceAsk = robot.Symbol.Ask;
        double priceBid = robot.Symbol.Bid;
        double priceCurrent = tradeType == TradeType.Buy ? priceBid : priceAsk;

        if (robot.PrimePositionMinLossPips != 0) {
            // Najdeme Prime Posistion s Entry Price, ktera je nejblize aktualni Price pro Loss.
            PrimePosition nearestPrimePositionLoss =
                tradeType == TradeType.Buy ?
                    primePositions.Find(item => item.entryPrice - priceCurrent > 0 && item.entryPrice - priceCurrent == primePositions.Min(r => r.entryPrice - priceCurrent))
                    :
                    primePositions.Find(item => priceCurrent - item.entryPrice > 0 && priceCurrent - item.entryPrice == primePositions.Min(r => priceCurrent - r.entryPrice));

            if (nearestPrimePositionLoss != null
                && ((tradeType == TradeType.Buy && -1 * GetMultiplikatorValue(robot.PrimePositionMinLossPips, primePositions.Count, robot.PrimePositionMinLossPipsMultiplicator) > GetDifferenceInPips(priceCurrent, nearestPrimePositionLoss.entryPrice))
                    || (tradeType == TradeType.Sell && -1 * GetMultiplikatorValue(robot.PrimePositionMinLossPips, primePositions.Count, robot.PrimePositionMinLossPipsMultiplicator) > GetDifferenceInPips(nearestPrimePositionLoss.entryPrice, priceCurrent)))) {
                return true;
            }
        }
        if (robot.PrimePositionMinProfitPips != 0) {
            // Najdeme Prime Posistion s Entry Price, ktera je nejblize aktualni Price pro Profit.
            PrimePosition nearestPrimePositionProfit =
                tradeType == TradeType.Buy ?
                    primePositions.Find(item => priceCurrent - item.entryPrice > 0 && priceCurrent - item.entryPrice == primePositions.Min(r => priceCurrent - r.entryPrice))
                    :
                    primePositions.Find(item => item.entryPrice - priceCurrent > 0 && item.entryPrice - priceCurrent == primePositions.Min(r => r.entryPrice - priceCurrent));

            if (nearestPrimePositionProfit != null
                && ((tradeType == TradeType.Buy && GetMultiplikatorValue(robot.PrimePositionMinProfitPips, primePositions.Count, robot.PrimePositionMinProfitPipsMultiplicator) < GetDifferenceInPips(priceCurrent, nearestPrimePositionProfit.entryPrice))
                    || (tradeType == TradeType.Sell && GetMultiplikatorValue(robot.PrimePositionMinProfitPips, primePositions.Count, robot.PrimePositionMinProfitPipsMultiplicator) < GetDifferenceInPips(nearestPrimePositionProfit.entryPrice, priceCurrent)))) {
                return true;
            }
        }


        return false;
    }

    private List<PrimePosition> GetPrimePositions(List<List<Position>> positionsGrouped) {
        List<PrimePosition> result = new List<PrimePosition>();

        foreach (List<Position> positions in positionsGrouped) {
            PrimePosition primePosition = GetPrimePosition(positions);
            if (primePosition != null) {
                result.Add(primePosition);
            }
        }

        return result;
    }

    protected override void OnPositionClosed(PositionClosedEventArgs args) {
        base.OnPositionClosed(args);

        Position positionClosed = args.Position;
        List<Position> positions = robot.GetPositionsAll();

        if (positionClosed.Comment.StartsWith(LiPiBotBase.commentPrime)) {
            if (positions.Find(item => item.Comment.StartsWith(LiPiBotBase.commentAdditional + ":" + positionClosed.Id)) != null) {
                // Pokud byla uzavrena Prime Position, pak si ulozime informace o teto pozici do listu uzavrenych Prime Positions.
                closedPrimePositions.Add(new PrimePosition(positionClosed.Id, positionClosed.TradeType, positionClosed.EntryPrice, positionClosed.EntryTime, true));
            }
        } else {
            if (positions.Find(item => item.Comment.StartsWith(positionClosed.Comment)) == null) {
                // Pokud pozice, ktera byla uzavrena je Additional Position a byla to posledni Additional Position od urcite Prime Position, pak ji z listu uzavrenych Prime Positions odstranime, protoze ji jiz nebudeme potrebovat.
                closedPrimePositions.RemoveAll(item => item.id.ToString() == positionClosed.Comment.Substring((LiPiBotBase.commentAdditional + ":").Length));
            }
        }
    }

    protected PrimePosition GetPrimePosition(List<Position> positions) {
        if (positions.Count == 0) return null;

        Position position = positions.Find(item => item.Comment.StartsWith(LiPiBotBase.commentPrime));
        if (position != null) {
            return new PrimePosition(position.Id, position.TradeType, position.EntryPrice, position.EntryTime, false);
        }

        PrimePosition primePositionClosed = closedPrimePositions.Find(item => item.id.ToString() == positions[0].Comment.Substring((LiPiBotBase.commentAdditional + ":").Length));
        if (primePositionClosed != null) {
            return primePositionClosed;
        }

        // Protoze si ukladame informace o uzavrenych Prime Positions, ktere pak nacitame, tato cast kodu by se nemela temer nikdy provest - vzdy by Prime Position mela byt dohledatelna z ulozech Prime Positions.
        // Krome pripadu, kdy spoustime bota a Prime Position je jiz uzavrena, ale jeho Additional Positions jsou stale jeste otevrene.
        HistoricalTrade positionFromHistory = robot.History.ToList().Find(item => positions[0].Comment.StartsWith(LiPiBotBase.commentAdditional) && item.PositionId.ToString() == positions[0].Comment.Substring((LiPiBotBase.commentAdditional + ":").Length));
        PrimePosition primePositionFromHistory = new PrimePosition(positionFromHistory.PositionId, positionFromHistory.TradeType, positionFromHistory.EntryPrice, positionFromHistory.EntryTime, true);
        closedPrimePositions.Add(primePositionFromHistory);
        return primePositionFromHistory;
    }

    protected class PrimePosition {
        public int id;
        public TradeType tradeType;
        public double entryPrice;
        public DateTime entryTime;
        public bool closed;

        public PrimePosition(int id, TradeType tradeType, double entryPrice, DateTime entryTime, bool closed) {
            this.id = id;
            this.tradeType = tradeType;
            this.entryPrice = entryPrice;
            this.entryTime = entryTime;
            this.closed = closed;
        }
    }

}
