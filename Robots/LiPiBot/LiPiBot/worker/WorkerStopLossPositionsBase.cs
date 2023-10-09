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

public abstract class WorkerStopLossPositionsBase : WorkerOpenPositionsBase {

    protected void MoveStopLossToProfit() {
        if (robot.IsMoveStopLossToProfitActive() == false) return;

        //P("MoveStopLossToProfit 1");

        double priceAsk = robot.Symbol.Ask;
        double priceBid = robot.Symbol.Bid;

        foreach (Position position in robot.GetPositionsAll()) {
            double priceCurrent = position.TradeType == TradeType.Buy ? priceBid : priceAsk;
            double priceEntry = position.EntryPrice;

            /*
             * Spocitame si jak daleko je aktualni cena od Stop Loss.
             * Pokud Stop Loss neni u pozice nastaven, budeme si predstavovat, ze je na pocici Entry Price.
             **/
            double stoploss = position.EntryPrice;
            if (position.StopLoss != null && robot.MoveStopLossToProfitType == LiPiBotBase.Move_Stop_Loss_Type.Repetitive_Move) {
                //P("MoveStopLossToProfit 2");
                // Nastavime stoploss na hodnotu Stop Loss pouze pokud je nastaveny Stop Loss nastaven jiz v profitu.
                if (position.TradeType == TradeType.Buy && position.StopLoss > priceEntry && GetDifferenceInPips((double)position.StopLoss, priceEntry) > robot.MoveStopLossToProfitAtPips * robot.MoveStopLossToProfitSetValue / 100f) stoploss = (double)position.StopLoss;
                else if (position.TradeType == TradeType.Sell && position.StopLoss < priceEntry && GetDifferenceInPips(priceEntry, (double)position.StopLoss) > robot.MoveStopLossToProfitAtPips * robot.MoveStopLossToProfitSetValue / 100f) stoploss = (double)position.StopLoss;
            }
            //if (position.StopLoss != null && robot.MoveStopLossToProfitType == LPBBase.Move_Stop_Loss_Type.Single_Move) {
            // Pokud je nastaven Single Move, pak je povolen pouze 1 Move Stop Loss do profitu
            //if (position.TradeType == TradeType.Buy && position.StopLoss > priceEntry && GetDifferenceInPips(priceCurrent, priceEntry) >= robot.MoveStopLossToProfitAtPips - 1) continue;
            //else if (position.TradeType == TradeType.Sell && position.StopLoss < priceEntry && GetDifferenceInPips(priceEntry, priceCurrent) >= robot.MoveStopLossToProfitAtPips - 1) continue;
            //}

            //if (Math.Abs(GetDifferenceInPips(stoploss, priceCurrent)) > robot.MoveStopLossToProfitAtPips) {
            // Aktualni cena je vzdalena od Entry Price nebo od Stop Loss (plati jen pro Stop Loss v profitu) je ve vetsi vzdalenosti nez je nastaveni Bota (MoveStopLossToProfitAtPips) ==> muzeme se pokusit provest zmenu Stop Loss.
            //P("MoveStopLossToProfit 3");
            // Spocitame si hodnotu, na kterou by mel Stop Loss byt umisten:
            double stoplossUpdateValue;
            if (position.TradeType == TradeType.Buy) {
                double stoplossForBid = (stoploss - priceBid) * robot.MoveStopLossToProfitSetValue / 100f;
                stoplossUpdateValue = Math.Round(stoploss + Math.Abs(stoplossForBid), robot.Symbol.Digits);
            } else {
                double stoplossForAsk = (stoploss - priceAsk) * robot.MoveStopLossToProfitSetValue / 100f;
                stoplossUpdateValue = Math.Round(stoploss - Math.Abs(stoplossForAsk), robot.Symbol.Digits);
            }

            if (IsUpdateMoveSLAllowed(position.TradeType, priceEntry, position.StopLoss, stoplossUpdateValue, priceCurrent)) {
                //P("MoveStopLossToProfit 4");
                /*
                 * Aktualizaci provest pouze pokud je rozdil mezi starym a novym SL vetsi ney hodnota robot.Symbol.PipSize nebo robot.Symbol.PipValue ???? jen pro pozice s TSL ??? 
                 * */
                double diffs = 0;
                if (position.StopLoss != null) {
                    diffs = Math.Abs(Math.Round((double)(position.StopLoss - stoplossUpdateValue), robot.Symbol.Digits));
                } else {
                    diffs = Math.Abs(Math.Round((double)(stoplossUpdateValue), robot.Symbol.Digits));
                }

                if (diffs > robot.Symbol.PipSize) {
                    //P("MoveStopLossToProfit 5 A");
                    // Posun SL musi by vetsi nez 1 robot.Symbol.TickSize = 1 tick je mensi nez pipSize - jeste mozno zkusit robot.Symbol.PipSize = velikost 1 pipu
                    position.ModifyStopLossPrice(stoplossUpdateValue);
                }


                if (position.StopLoss != null) {
                    //P("MoveStopLossToProfit 5 B1");
                    if (robot.MoveStopLossToProfitType == LiPiBotBase.Move_Stop_Loss_Type.Trailing_Stop_Loss && position.HasTrailingStop == false) {
                        // P("MoveStopLossToProfit 5 B2");
                        position.ModifyTrailingStop(true);
                    }
                    if (robot.MoveStopLossToProfitType != LiPiBotBase.Move_Stop_Loss_Type.Trailing_Stop_Loss && position.HasTrailingStop == true) {
                        // P("MoveStopLossToProfit 5 B3");
                        position.ModifyTrailingStop(false);
                    }
                }

                if (robot.RemoveTakeProfitOnMoveStopLossToProfit == LiPiBotBase.Yes_No.Yes && position.TakeProfit != null) {
                    // P("MoveStopLossToProfit 5 C");
                    position.ModifyTakeProfitPips(null);
                }

            }
            //}
        }
    }

    private bool IsUpdateMoveSLAllowed(TradeType tradeType, double priceEntry, double? valueSLCurrent, double valueSLNew, double priceCurrent) {
        if (tradeType.Equals(TradeType.Buy)) {
            // Backtesting: AUDCAD H1 25/03/2021 : v OnStart metode otevrit pozici: worker.Open(TradeType.Buy, 1); [Move SL nastaven na 50 pipu s 50% pro nastaveni hodnoty SL. Pozice otevrena s pocatecnim TSL nastavenym na 40 pipu - aby pri dosazeni 50 pipu byl TSL v plusovych hodnotach mensich nez 50 * 50% = 25 pipech.]
            if (valueSLCurrent == null && GetDifferenceInPips(priceCurrent, priceEntry) >= robot.MoveStopLossToProfitAtPips) {
                return true;
            }
            if (valueSLCurrent != null && valueSLNew > valueSLCurrent && GetDifferenceInPips(priceCurrent, priceEntry) >= robot.MoveStopLossToProfitAtPips && GetDifferenceInPips((double)valueSLCurrent, priceEntry) <= robot.MoveStopLossToProfitAtPips * robot.MoveStopLossToProfitSetValue / 100f) {
                // SL je v pozici jeste pred tim nez mame nastaveno v Move Stop Loss
                // napr. Jsme v pozici v profitu 50 pipů. Otevirali jsme pozici s TSL a SL se presunul ze zaporne hodnoty na hodnotu v profitu +10 pipů. Nastaveny Move Stop Loss je ale pri dosazeni zisku 50 pipu presunout SL na 50% teto hodnoty - tj. SL by mel byt na hodnote 25 pipů.
                //return true;
                return true;
            }
            if (robot.MoveStopLossToProfitType != LiPiBotBase.Move_Stop_Loss_Type.Repetitive_Move) {
                // Pro Single Move a Trailing Stop Loss dalsi podminky nekontrolujeme. Uprava SL muze byt totiz provedena pouze jednou a to jen pokud je SL na mensi hodnote, nez ktera je nastavena pro Move Stop Loss.
                // U Single Move se tedy jiz vice SL neposouva a u Traling Stop Loss se SL posouva jiz automaticky.
                return false;
            }
            if (GetDifferenceInPips(priceCurrent, valueSLCurrent == null || valueSLCurrent < priceEntry ? priceEntry : (double)valueSLCurrent) <= robot.MoveStopLossToProfitAtPips) {
                // Aktualni cena neni v minimalni vydalenosti od Entry Price nebo od Stop Loss (podle toho co je bliz), ktery byl jiz nastaven do profitu.
                return false;
            }
            if (priceEntry < valueSLNew && (valueSLCurrent < priceEntry || valueSLCurrent < valueSLNew)) {
                return true;
            }
        } else {
            // Backtesting: AUDCAD H1 23/03/2021 : v OnStart metode otevrit pozici: worker.Open(TradeType.Sell, 1); [Move SL nastaven na 50 pipu s 50% pro nastaveni hodnoty SL. Pozice otevrena s pocatecnim TSL nastavenym na 40 pipu - aby pri dosazeni 50 pipu byl TSL v plusovych hodnotach mensich nez 50 * 50% = 25 pipech.]
            if (valueSLCurrent == null && GetDifferenceInPips(priceEntry, priceCurrent) >= robot.MoveStopLossToProfitAtPips) {
                return true;
            }
            if (valueSLCurrent != null && valueSLNew < valueSLCurrent && GetDifferenceInPips(priceEntry, priceCurrent) >= robot.MoveStopLossToProfitAtPips && GetDifferenceInPips(priceEntry, (double)valueSLCurrent) <= robot.MoveStopLossToProfitAtPips * robot.MoveStopLossToProfitSetValue / 100f) {
                // SL je v pozici jeste pred tim nez mame nastaveno v Move Stop Loss
                // napr. Jsme v pozici v profitu 50 pipů. Otevirali jsme pozici s TSL a SL se presunul ze zaporne hodnoty na hodnotu v profitu +10 pipů. Nastaveny Move Stop Loss je ale pri dosazeni zisku 50 pipu presunout SL na 50% teto hodnoty - tj. SL by mel byt na hodnote 25 pipů.
                //return true;
                return true;
            }
            if (robot.MoveStopLossToProfitType != LiPiBotBase.Move_Stop_Loss_Type.Repetitive_Move) {
                // Pro Single Move a Trailing Stop Loss dalsi podminky nekontrolujeme. Uprava SL muze byt totiz provedena pouze jednou a to jen pokud je SL na mensi hodnote, nez ktera je nastavena pro Move Stop Loss.
                // U Single Move se tedy jiz vice SL neposouva a u Traling Stop Loss se SL posouva jiz automaticky.
                return false;
            }
            if (GetDifferenceInPips(valueSLCurrent == null || valueSLCurrent > priceEntry ? priceEntry : (double)valueSLCurrent, priceCurrent) <= robot.MoveStopLossToProfitAtPips) {
                // Aktualni cena neni v minimalni vydalenosti od Entry Price nebo od Stop Loss, ktery byl jiz nastaven do profitu.
                return false;
            }
            if (priceEntry > valueSLNew && (valueSLCurrent > priceEntry || valueSLCurrent > valueSLNew)) {
                return true;
            }
        }
        return false;
    }


}
