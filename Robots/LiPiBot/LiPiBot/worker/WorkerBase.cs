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

public abstract class WorkerBase {

    protected LiPiBotCoreBase robot;

    public double GetMultiplikatorValue(double value, int count, double multiplikator) {
        if (count == 0 || value == 0 || multiplikator == 0) {
            return value;
        }

        double result = value + value * (count - 1) * multiplikator;
        if (multiplikator < 0) {
            if (value > 0) {
                double min = Math.Min(value * Math.Abs(multiplikator), value - value * Math.Abs(multiplikator));
                return result < min ? min : result;//A
            }
            double max = Math.Max(value * Math.Abs(multiplikator), value - value * Math.Abs(multiplikator));
            return result > max ? max : result;//B
        }
        // multiplikator > 0
        return result;//C
    }

    protected int GetVolumeMultiplyValue(int positions) {
        int multiply = 1;
        if (robot.AdditionalPositionsVolumeMultiply > 0) {
            if (robot.AdditionalPositionsVolumeMultiplyType == LiPiBotBase.Multiply_Volume_Type.Quadratic) {
                multiply = (int)Math.Pow(robot.AdditionalPositionsVolumeMultiply, positions);
            } else if (robot.AdditionalPositionsVolumeMultiplyType == LiPiBotBase.Multiply_Volume_Type.Linear) {
                multiply = robot.AdditionalPositionsVolumeMultiply * positions;
            }
        }
        return multiply;
    }

    protected Position FindPositionWithMaximumProfitPips(List<Position> positions) {
        // najdeme pozici s nejvetsim ziskem v pipech
        Position result = null;
        foreach (Position position in positions) {
            if (result == null || result.Pips < position.Pips) {
                result = position;
            }
        }
        return result;
    }

    protected Position FindPositionWithMinimumProfitPips(List<Position> positions) {
        // najdeme pozici s nejvetsim ziskem v pipech
        Position result = null;
        foreach (Position position in positions) {
            if (result == null || result.Pips > position.Pips) {
                result = position;
            }
        }
        return result;
    }


    protected double GetDifferenceInPips(double valueA, double valueB) {
        // Metoda vrací hodnotu mezi zadanými cenami v pipech (tj. valueA a valueB jsou nějaké ceny (Price) a chceme znát jejich rozdíl v pipech).

        // Pro SELL pozici:
        // GetDifferenceInPips(position.EntryPrice, (double) position.StopLoss); == vysledek metody je  "-4.8" a SL je umisten v minusu = tj. uzavreni ve ztrate
        // GetDifferenceInPips(position.EntryPrice, (double) position.StopLoss); == vysledek metody je  "+1.8" a SL je umisten v plusu = tj. uzavreni v profitu

        // Pro BUY pozici:
        // GetDifferenceInPips(position.EntryPrice, (double) position.StopLoss); == vysledek metody je  "+8.7" a SL je umisten v minusu = tj. uzavreni ve ztrate
        // GetDifferenceInPips(position.EntryPrice, (double) position.StopLoss); == vysledek metody je  "-1.3" a SL je umisten v plusu = tj. uzavreni v profitu
        double values = (valueA - valueB) / robot.Symbol.PipSize;
        return Math.Round(values, 3);
    }

    protected TradeType GetTradeTypeBySignal(SIGNAL signal) {
        TradeType tradeType;
        if (signal == SIGNAL.BUY) {
            tradeType = TradeType.Buy;
        } else {
            tradeType = TradeType.Sell;
        }
        return tradeType;
    }

    /// <summary>
    /// Tuto metodu používat pouze pro testování.
    /// </summary>
    /// <param name="message"></param>
    public void P(string message) {
        //robot.Print(robot.GetLabel() + " " + robot.SymbolName + "  :::  " + message);
    }
}
