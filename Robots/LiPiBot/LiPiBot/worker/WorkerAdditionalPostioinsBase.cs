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

public abstract class WorkerAdditionalPostioinsBase : WorkerPrimePostioinsBase {

    //private int LastCheckProcessOpenMorePositionsForBuy = -1;
    //private int LastCheckProcessOpenMorePositionsForSell = -1;
    protected void ProcessOpenAdditionalPosition() {
        //return;
        List<List<Position>> positionsByPrimes = robot.GetPositionsGrouped();//.ToList();
                                                                             //TODO: otestovat posledni postz : LastCheckProcessOpenMorePositionsForBuy

        /*
         * Musime kontrolovat pocet otevrenych pozic pro Buy a Sell, ktery musi byt stejny jako pri poslednim Ticku.
         * Duvod je ten, ze pri Aggregovanem otevirani dalsich pozic, muze byt pozice uzavrena pres Stop Loss, otevrena pozice nova a až potom budou uzavreny vsechny pozice vcetne te, ktera byla prave otevrena.
         * Ta nova pozice vubec byt otevrena nemala, protoze ostatni pozice mely byt uzavreny drive nez se otevrela.
         * 
         * Backtesting-AUDCAD-H1-001
         * 19/05/2021 - 21/05/2021
         * 
         * 19/05/2021 15:00 - Otevrena BUY
         * 19/05/2021 15:06 - Otevrena BUY
         * 19/05/2021 16:31 - Otevrena BUY a ihned zavrena
         * 
         * Duvod je ten, ze akce jsou vykonany v tomto poradi:
         * 1) Jedna BUY pozice byla zavrena pres nastaveny StopLoss (bez interakce s LiPiBotem)
         * 2) Potom OnTick LiPiBota a doslo ke kontrole, zda se nema otevrit nova pozice a bylo zjisteno, ze minimum pipu pro otevreni nove pozice je splneno a pocet otevrenych pozic jednoho typu (buy/sell) neni na maximu a tak byla otevrena nova BUY pozice (jedna BUY pozice totiž byla právě zavřena).
         * 3) Potom ale byla zavolana metoda, ktera je volana pri zavrenim nejake pozice (byla zavrena ta prvni BUY pozice (1)), aby byly zavreny i ostatni BUY pozice - tedy i ta nova, ktera byla prave otevrena (2).
         * */

        foreach (List<Position> positions in positionsByPrimes) {
            PrimePosition primePosition = GetPrimePosition(positions);
            if (primePosition != null) {
                OpenAdditionalPositionIfPossible(positions, GetPrimePosition(positions));
            }


            /*

            List<Position> positionsBuy = positions.FindAll(item => item.TradeType == TradeType.Buy);
            //if (positionsBuy.Count == LastCheckProcessOpenMorePositionsForBuy) {

            //}
            //LastCheckProcessOpenMorePositionsForBuy = positionsBuy.Count;

            List<Position> positionsSell = positions.FindAll(item => item.TradeType == TradeType.Sell);
            //if (positionsSell.Count == LastCheckProcessOpenMorePositionsForSell) {
            //      OpenMorePositionIfPossible(positionsSell, GetPrimePosition(positions));!!!
            //}
            //LastCheckProcessOpenMorePositionsForSell = positionsSell.Count;
            */
        }

    }


    private void OpenAdditionalPositionIfPossible(List<Position> positions, PrimePosition prime) {
        if (positions.Count > 0 && positions.Count <= robot.AdditionalPositionsMax) {
            Position positionWithMaximuPips = FindPositionWithMaximumProfitPips(positions);
            Position positionWithMinimumPips = FindPositionWithMinimumProfitPips(positions);

            if ((robot.AdditionalPositionsMinLossPips != 0 || robot.AdditionalPositionsMinProfitPips != 0)
                 && ((robot.AdditionalPositionsMinLossPips != 0 && positionWithMaximuPips.Pips < 0 && positionWithMaximuPips.Pips < -1 * GetMultiplikatorValue(robot.AdditionalPositionsMinLossPips, positions.Count, robot.AdditionalPositionsMinLossPipsMultiplicator))
                    || (robot.AdditionalPositionsMinProfitPips != 0 && positionWithMinimumPips.Pips > 0 && positionWithMinimumPips.Pips > GetMultiplikatorValue(robot.AdditionalPositionsMinProfitPips, positions.Count, robot.AdditionalPositionsMinProfitPipsMultiplicator)))) {
                //if (robot.OpenMoreSamePositionsMinLossPips == 0 || (robot.OpenMoreSamePositionsMinLossPips != 0 && position.Pips < -1 * robot.OpenMoreSamePositionsMinLossPips)) {

                Position position = positions[0];
                if (robot.AdditionalPositionsSignalRequired == LiPiBotBase.Yes_No.Yes) {
                    SIGNAL signal = robot.SignalWorker.GetSignal();
                    if (signal != SIGNAL.NONE) {
                        TradeType tradeType = GetTradeTypeBySignal(signal);
                        //P("OpenMorePositionIfPossible A robot.OpenMoreSamePositionsSignalRequired=" + robot.AdditionalPositionsSignalRequired);
                        //P("positionWithMaximuPips=" + positionWithMaximuPips + " | positionWithMinimumPips=" + positionWithMinimumPips);
                        //P("positions.Count=" + positions.Count);
                        if (tradeType == position.TradeType) {
                            Open(position.TradeType, GetVolumeMultiplyValue(positions.Count), prime.id, LiPiBotCoreBase.Open_Position_Cause_Type.ADDITIONAL_POSITION);
                        }
                    }
                } else {
                    //P("OpenMorePositionIfPossible B robot.OpenMoreSamePositionsSignalRequired=" + robot.AdditionalPositionsSignalRequired);
                    //P("positionWithMaximuPips=" + positionWithMaximuPips + " | positionWithMinimumPips=" + positionWithMinimumPips);
                    //P("positions.Count=" + positions.Count);
                    Open(position.TradeType, GetVolumeMultiplyValue(positions.Count), prime.id, LiPiBotCoreBase.Open_Position_Cause_Type.ADDITIONAL_POSITION);
                }
            }
        }
    }

}
