using cAlgo.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cAlgo {
    public enum SIGNAL {
        NONE,
        BUY,
        SELL
    }

    public interface ISignal {
        SIGNAL GetSignal();

        // potom, co je otevrena pozice (pouze pro Prime Position) je zavolana tato metoda
        void OnPositionOpen(TradeResult position);
    }
}
