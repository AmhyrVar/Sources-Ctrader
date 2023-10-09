using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class EURJPY : Robot
    {
        [Parameter("Period za proveru ulaska u trgovinu", Group = "Lookback za ulaz", DefaultValue = 3, MinValue = 0)]
        public int LookbackCandles_EURJPY { get; set; }

        [Parameter("Ukupan Fond", Group = "Parametri Trgovine", DefaultValue = 48000)]
        public double TotalBal_EURJPY { get; set; }

        [Parameter("Veličina Pozicije (Lotovi)", Group = "Parametri Trgovine", DefaultValue = 1, MinValue = 0.001, MaxValue = 100, Step = 0.001)]
        public double FixedPositionSize_EURJPY { get; set; }

        [Parameter("Period", Group = "ADX", DefaultValue = 14, MinValue = 1)]
        public int Len { get; set; }

        [Parameter("Nivo", Group = "ADX", DefaultValue = 10)]
        public double AdxLevel { get; set; }

        [Parameter("Nivo", Group = "Volume", DefaultValue = 100)]
        public double VolLevel { get; set; }

        private ParabolicSAR sar;
        private AverageDirectionalMovementIndexRating adx;
        private double totalProfit = 0;
        private double totalLoss = 0;
        private double totalSpreadCost = 0;
        private double currentVolume = 0;
        private TradeType currentTradeType;
        private DateTime lastTradeTime = DateTime.MinValue;

        private bool isEURJPYPositionOpen = false;
        private bool isExecutingTrade = false;

        protected override void OnStart()
        {
            sar = Indicators.ParabolicSAR(LookbackCandles_EURJPY, 3.0);
            adx = Indicators.AverageDirectionalMovementIndexRating(Len);

            Positions.Closed += OnPositionClosed;
            Positions.Opened += OnPositionOpened;

            lastTradeTime = Server.Time;
        }

        public bool AdxAndVolumeCondition(int index)
        {
            return adx.ADX[index] > AdxLevel && Bars.TickVolumes[index] > VolLevel;
        }

        public bool AdxDICross(string direction, int lookback)
        {
            return direction == "Buy" ? adx.DIPlus.HasCrossedAbove(adx.DIMinus, lookback) : adx.DIPlus.HasCrossedBelow(adx.DIMinus, lookback);
        }

        public bool SarCondition(string direction, int lookback)
        {
            return direction == "Buy" ? sar.Result.HasCrossedAbove(Bars.LowPrices, lookback) : sar.Result.HasCrossedBelow(Bars.HighPrices, lookback);
        }

        public bool ShouldEnterLong(int index)
        {
            return ((AdxDICross("Buy", LookbackCandles_EURJPY) && SarCondition("Buy", 1)) || (AdxDICross("Buy", 1) && SarCondition("Buy", LookbackCandles_EURJPY))) && AdxAndVolumeCondition(index);
        }

        public bool ShouldEnterShort(int index)
        {
            return ((AdxDICross("Sell", LookbackCandles_EURJPY) && SarCondition("Sell", 1)) || (AdxDICross("Sell", 1) && SarCondition("Sell", LookbackCandles_EURJPY))) && AdxAndVolumeCondition(index);
        }

        public void ExecuteTrade(int index, TradeType tradeType)
        {
            double volume = Symbol.QuantityToVolumeInUnits(FixedPositionSize_EURJPY);

            if (volume < Symbol.VolumeInUnitsMin)
            {
                Print("Zapremina trgovine ({0}) je manja od dozvoljene ({1})", volume, Symbol.VolumeInUnitsMin);
                return;
            }

            currentVolume = volume;
            currentTradeType = tradeType;

            if (tradeType == TradeType.Buy)
            {
                var result = ExecuteMarketOrder(tradeType, "EURJPY", volume, "Dugačko");
                if (result.IsSuccessful)
                {
                    Print("Dugački ulaz izvršen po ceni {0}", result.Position.EntryPrice);
                }
                else
                {
                    Print("Greška pri izvršavanju dugog ulaza: {0}", result.Error);
                }
            }
            else if (tradeType == TradeType.Sell)
            {
                var result = ExecuteMarketOrder(tradeType, "EURJPY", volume, "Kratko");
                if (result.IsSuccessful)
                {
                    Print("Kratki ulaz izvršen po ceni {0}", result.Position.EntryPrice);
                }
                else
                {
                    Print("Greška pri izvršavanju kratkog ulaza: {0}", result.Error);
                }
            }
        }

        private void OnPositionOpened(PositionOpenedEventArgs args)
        {
            Print("Pozicija Otvorena - Simbol: {0}, Tip: {1}, Cena Ulaza: {2}, Zapremina: {3}, Nett Prof: {4}, Provizija: {5}",
                args.Position.SymbolName,
                args.Position.TradeType,
                args.Position.EntryPrice,
                args.Position.VolumeInUnits,
                args.Position.NetProfit,
                args.Position.Commissions);

            // Postaviti isEURJPYPositionOpen na true kada se otvori nova pozicija za EURJPY
            if (args.Position.SymbolName == "EURJPY")
            {
                isEURJPYPositionOpen = true;
            }
        }

        private void OnPositionClosed(PositionClosedEventArgs args)
        {
            // Prikaz informacija o zatvorenoj poziciji u konzoli (nastavak)
            double cenaIzlaza;
            if (args.Position.TradeType == TradeType.Buy)
                cenaIzlaza = Symbol.Bid;
            else
                cenaIzlaza = Symbol.Ask;

            Print("Pozicija Zatvorena - Simbol: {0}, Tip: {1}, Cena Ulaza: {2}, Cena Izlaza: {3}, Zapremina: {4}, Bruto Profit: {5}, Nett Profit: {6}, Provizija: {7}",
                args.Position.SymbolName,
                args.Position.TradeType,
                args.Position.EntryPrice,
                cenaIzlaza,
                args.Position.VolumeInUnits,
                args.Position.GrossProfit,
                args.Position.NetProfit,
                args.Position.Commissions);

            // Postaviti isEURJPYPositionOpen na false kada se zatvori pozicija za EURJPY
            if (args.Position.SymbolName == "EURJPY")
            {
                isEURJPYPositionOpen = false;
            }
        }

        protected override void OnBar()
        {
            int index = Bars.Count - 2;

            // Dodati ovaj uslov kako biste proverili da li robot već izvršava trgovinu
            if (isExecutingTrade)
                return;

            // Dodati ovaj uslov kako biste proverili proteklo vreme od poslednjeg otvaranja pozicije
            if ((Server.Time - lastTradeTime).TotalSeconds < 3) // Tolerancija od 3 sekunde
                return;

            if (!IsTradingTime())
                return;

            // Na kraju izvršavanja trgovine ažurirajte vreme poslednjeg otvaranja pozicije
            lastTradeTime = Server.Time;

            // Koristite Thread.Sleep(3000) kako biste zaustavili izvršavanje koda na 3 sekunde pre nego što robot razmotri sledeću poziciju
            System.Threading.Thread.Sleep(3000);

            // Provera da li je simbol trenutnog okvira podataka jednak simbolu koji robot prati
            if (Symbol.Name != "EURJPY")
                return;

            // Provera da li je već otvorena pozicija za ovaj instrument
            if (isEURJPYPositionOpen)
                return;

            if (Positions.Count > 0)
            {
                if (currentTradeType == TradeType.Buy && ShouldEnterShort(index))
                {
                    CloseAllPositions("EURJPY");
                    currentTradeType = TradeType.Sell;
                    ExecuteTrade(index, currentTradeType);
                    // Postavite isExecutingTrade na true jer robot trenutno izvršava trgovinu
                    isExecutingTrade = true;
                }
                else if (currentTradeType == TradeType.Sell && ShouldEnterLong(index))
                {
                    CloseAllPositions("EURJPY");
                    currentTradeType = TradeType.Buy;
                    ExecuteTrade(index, currentTradeType);
                    // Postavite isExecutingTrade na true jer robot trenutno izvršava trgovinu
                    isExecutingTrade = true;
                }
            }
            else
            {
                if (ShouldEnterLong(index))
                {
                    currentTradeType = TradeType.Buy;
                    ExecuteTrade(index, currentTradeType);
                    // Postavite isExecutingTrade na true jer robot trenutno izvršava trgovinu
                    isExecutingTrade = true;
                }
                else if (ShouldEnterShort(index))
                {
                    currentTradeType = TradeType.Sell;
                    ExecuteTrade(index, currentTradeType);
                    // Postavite isExecutingTrade na true jer robot trenutno izvršava trgovinu
                    isExecutingTrade = true;
                }
            }

            // Izračunavanje profita i gubitka za otvorene pozicije
            double tradeResult = 0;
            foreach (var position in Positions)
            {
                tradeResult += (position.TradeType == TradeType.Buy) ?
                                (Symbol.Bid - position.EntryPrice) * position.VolumeInUnits :
                                (position.EntryPrice - Symbol.Ask) * position.VolumeInUnits;
            }
            // Ažuriranje ukupnih troškova provizije
            totalSpreadCost += tradeResult;
        }

        protected override void OnStop()
        {
            // Ispis ukupnog profita, gubitka i ukupnih troškova provizije na kraju testiranja ili kada se robot zaustavi
            Print("Ukupan Profit: {0}, Ukupan Gubitak: {1}, Ukupni Troškovi Provizije: {2}", totalProfit, totalLoss, totalSpreadCost);
        }

        private bool IsTradingTime()
        {
            // Dodajte svoju logiku za definisanje trading vremena
            return true;
        }

        private void CloseAllPositions(string symbolName)
        {
            foreach (var position in Positions)
            {
                ClosePosition(position);
            }
        }
    }
}
