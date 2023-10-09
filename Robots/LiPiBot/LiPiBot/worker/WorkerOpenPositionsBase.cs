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

public abstract class WorkerOpenPositionsBase : WorkerBase {


    public TradeResult Open(TradeType tradeType, int multiplyVolume, int? primePositionId, LiPiBotCoreBase.Open_Position_Cause_Type postionType) {
        double volumeInUnits = robot.Symbol.QuantityToVolumeInUnits(robot.PositionVolumeInLots * multiplyVolume);
        if (robot.BotAction.Equals(LiPiBotBase.Bot_Action.Signal) || robot.BotAction.Equals(LiPiBotBase.Bot_Action.Trade_And_Signal)) {
            WorkerSignal.OpenSignal(robot, tradeType, volumeInUnits, postionType);
        }

        if (robot.BotAction.Equals(LiPiBotBase.Bot_Action.Trade) || robot.BotAction.Equals(LiPiBotBase.Bot_Action.Trade_And_Signal)) {
            //double volumeInUnits = robot.Symbol.QuantityToVolumeInUnits(robot.PositionVolumeInLots * multiplyVolume);

            double? stoploss = null;
            if (robot.PositionStopLossInPips != 0) stoploss = robot.PositionStopLossInPips;
            double? profit = null;
            if (robot.PositionTakeProfitInPips != 0) profit = robot.PositionTakeProfitInPips;

            TradeResult tradeResult = robot.ExecuteMarketOrder(tradeType, robot.SymbolName, volumeInUnits, robot.GetLabel(), stoploss, profit, primePositionId == null ? LiPiBotBase.commentPrime : LiPiBotBase.commentAdditional + ":" + primePositionId);
            if (robot.PositionActiveTrailingStopLoss == LiPiBotBase.Yes_No.Yes) {
                tradeResult.Position.ModifyTrailingStop(true);
            }
            return tradeResult;
        }
        return null;
    }

}
