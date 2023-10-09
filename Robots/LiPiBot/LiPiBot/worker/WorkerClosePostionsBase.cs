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

public abstract class WorkerClosePostionsBase : WorkerStopLossPositionsBase {

    private List<Position> positionsToClose = new List<Position>();

    public void CloseOnEndTradingHour(List<Position> positionsToClose, LiPiBotCoreBase.Close_Position_Cause closeCuase) {
        this.positionsToClose.AddRange(positionsToClose);
        Close(positionsToClose, closeCuase);
    }

    public void Close(List<Position> positionsToClose, LiPiBotCoreBase.Close_Position_Cause closeCuase) {
        if (robot.BotPositionAction.Equals(LiPiBotCoreBase.Bot_Position_Action.Open_And_Close_Positions) == false
            && robot.BotPositionAction.Equals(LiPiBotCoreBase.Bot_Position_Action.Close_Positions) == false) {
            // Není dovoleno zavírání pozic.
            return;
        }

        if (robot.BotAction.Equals(LiPiBotBase.Bot_Action.Signal) || robot.BotAction.Equals(LiPiBotBase.Bot_Action.Trade_And_Signal)) {
            WorkerSignal.CloseSignal(robot, positionsToClose, closeCuase);
        }

        if (robot.BotAction.Equals(LiPiBotBase.Bot_Action.Trade) || robot.BotAction.Equals(LiPiBotBase.Bot_Action.Trade_And_Signal)) {
            foreach (Position p in positionsToClose.ToList()) {
                //P("CLOSE Position ID=" + p.Id);
                p.Close();
            }
        }
    }


    private int backtestingClosedLosingTrades = 0;
    private void CloseInBacktesting(Position p) {
        if (robot.RunningMode == RunningMode.RealTime) {
            return;
        }
        if (robot.BotBacktestingStopOnMaximumLosingTrades == 0) {
            return;
        }

        if (robot.GetPositionProfit(p) < 0) {
            backtestingClosedLosingTrades++;
        }
        if (backtestingClosedLosingTrades >= robot.BotBacktestingStopOnMaximumLosingTrades) {
            backtestingClosedLosingTrades = 0;
            robot.Stop();
        }
    }

    //private bool LockPositionsOnClosed = false;
    //private List<long> pid = new List<long>();
    protected virtual void OnPositionClosed(PositionClosedEventArgs args) {
        // Tato metoda je zavolana, i kdyz je zavrena pozice jeneho symbolu i jineho robot - je tedy nutne kontrolovat, jaka pozice byla zavrena
        Position position = args.Position;
        if (position.Label != robot.GetLabel()
                || position.SymbolName != robot.SymbolName) {
            return;
        }
        CloseInBacktesting(position);
        if (this.robot.AdditionalPositionsType != LiPiBotBase.Additional_Positions_Type.Aggregated) {
            //positionsToClose.Clear();
            positionsToClose.Remove(position);
            return;
        }

        //if (pid.Remove(position.Id)) {
        if (positionsToClose.Remove(position)) {
            // Zavirame pozici, jejich zavreni bylo vynuceno zavolanim teto metody jiz drive a proto nebudeme dale metodu zpracovavat.
            //P("111 PositionsOnClosed_CloseAggregatedPositions position.Id=" + position.Id + " ::: pid.Count=" + positionsToClose.Count);
            return;
        }
        //P("222 PositionsOnClosed_CloseAggregatedPositions position.Id=" + position.Id + " ::: pid.Count=" + positionsToClose.Count);

        //if (LockPositionsOnClosed == false) {
        //  LockPositionsOnClosed = true;
        //P("ON CLOSE POSITION: ID=" + args.Position.Id + " SymbolName=" + args.Position.SymbolName + " Label=" + args.Position.Label);
        if (robot.BotAction.Equals(LiPiBotBase.Bot_Action.Signal) || robot.BotAction.Equals(LiPiBotBase.Bot_Action.Trade_And_Signal)) {
            WorkerSignal.CloseSignal(robot, new List<Position>() { position }, null, args.Reason);
        }

        //List<Position> positionsToClose = new List<Position>();
        foreach (List<Position> pp in robot.GetPositionsGrouped()) {
            if (position.Comment.StartsWith(LiPiBotBase.commentPrime)) {
                // Pokud je uzavren Prime Position - vezmeme jen pozice, ktere jsou jeho Additional.
                if (pp[0].Comment.StartsWith(LiPiBotBase.commentAdditional + ":" + position.Id) == false) continue;
            } else {
                // Pokud je uzavrena Additional Position - vezmeme vsechny, kde je stejne Addtional ID pro Prime Position.
                if (pp[0].Comment.StartsWith(position.Comment) == false) continue;
            }

            foreach (Position p in pp) {
                if (p.TradeType == position.TradeType) {
                    if ((robot.ClosingAggregatedAdditionalPositions == LiPiBotBase.Closing_Aggregated_Positions.All)
                        || (robot.ClosingAggregatedAdditionalPositions == LiPiBotBase.Closing_Aggregated_Positions.Only_In_Profit && robot.GetPositionProfit(p) > 0)
                            || (robot.ClosingAggregatedAdditionalPositions == LiPiBotBase.Closing_Aggregated_Positions.Only_In_Loss && robot.GetPositionProfit(p) < 0)) {
                        positionsToClose.Add(p);
                        //pid.Add(p.Id);
                    }
                }
            }
        }
        if (positionsToClose.Count > 0) {
            P("CLOSE PositionsOnClosed positionsToClose.Count=" + positionsToClose.Count + " >>>>> "
                + " ID=" + positionsToClose[0].Id + "  SymbolNaame=" + positionsToClose[0].SymbolName + "  Label=" + positionsToClose[0].Label
                + " ON CLOSE POSITION: ID=" + args.Position.Id + " SymbolName=" + args.Position.SymbolName + " Label=" + args.Position.Label);
            Close(positionsToClose, LiPiBotCoreBase.Close_Position_Cause.CLOSE_AGGREGATED_POSITIONS);
        }
        //LockPositionsOnClosed = false;
        //}
        //}
    }


    protected void ClosePositionsWhenOpositeSignalExists() {
        SIGNAL signal = robot.SignalWorker.GetSignal();
        if (signal != SIGNAL.NONE) {
            //TradeType tradeType = GetTradeTypeBySignal(signal);

            //List<Position> positionsToClose = new List<Position>();
            foreach (List<Position> pp in robot.GetPositionsGrouped()) {
                foreach (Position position in pp) {
                    if ((position.TradeType == TradeType.Buy && signal == SIGNAL.SELL)
                        || position.TradeType == TradeType.Sell && signal == SIGNAL.BUY) {

                        if ((robot.ClosePositionsIfOppositeSignalExists == LiPiBotCoreBase.Close_Positions_If_Opposite_Signal_Exists.All)
                            || (robot.ClosePositionsIfOppositeSignalExists == LiPiBotCoreBase.Close_Positions_If_Opposite_Signal_Exists.Only_In_Loss && robot.GetPositionProfit(position) < 0)
                            || (robot.ClosePositionsIfOppositeSignalExists == LiPiBotCoreBase.Close_Positions_If_Opposite_Signal_Exists.Only_In_Profit && robot.GetPositionProfit(position) > 0)) {
                            positionsToClose.Add(position);
                        }
                    }
                }
            }
            if (positionsToClose.Count > 0) {
                P("CLOSE ClosePositionsWhenOpositeSignalExists positionsToClose.Count=" + positionsToClose.Count);
                Close(positionsToClose, LiPiBotCoreBase.Close_Position_Cause.OPPOSITE_SIGNAL_EXISTS);
            }
        }
    }

    protected void ClosePositionsByAge() {
        foreach (Position p in robot.GetPositionsAll()) {

            double positionAge = robot.Server.TimeInUtc.ToUniversalTime().Subtract(p.EntryTime).TotalMinutes;
            if (positionAge > robot.ClosePositionByAgeValue 
                && ((int) positionAge) % robot.ClosePositionByAgeOnTimeInterval == 0) {

                if (robot.ClosePositionsByAge.Equals(LiPiBotBase.Close_Positions_By_Age.All)
                    || (robot.ClosePositionsByAge.Equals(LiPiBotBase.Close_Positions_By_Age.Only_In_Loss) && robot.GetPositionProfit(p) < 0)
                    || (robot.ClosePositionsByAge.Equals(LiPiBotBase.Close_Positions_By_Age.Only_In_Profit) && robot.GetPositionProfit(p) > 0)) {
                    positionsToClose.Add(p);
                }
            }
        }

        if (positionsToClose.Count > 0) {
            P("CLOSE ClosePositionsByAge positionsToClose.Count=" + positionsToClose.Count);
            Close(positionsToClose, LiPiBotCoreBase.Close_Position_Cause.AGE);
        }
    }
}
