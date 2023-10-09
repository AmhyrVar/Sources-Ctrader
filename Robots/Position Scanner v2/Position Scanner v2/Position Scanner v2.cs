using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class PositionScannerv2 : Robot
    {


        [Parameter(DefaultValue = 2)]
        public double Risk_Percentage { get; set; }

        [Parameter(DefaultValue = 2)]
        public double Ratio { get; set; }

        public double delta;

        public bool TriggerStop = true;

        public double Entry;
        public double StopLossLevel;

        public int DecimalPrecision;


        public double SL;

        public bool Protection = false;



        protected override void OnStart()
        {


            Positions.Closed += PositionsClosed;

            DecimalPrecision = Symbol.PipSize.ToString().Split('.').Count() > 1 ? Symbol.PipSize.ToString().Split('.').ToList().ElementAt(1).Length : 0;

            Print("Decimal Precision " + DecimalPrecision);
            Print("Symbol pipsize " + Symbol.PipSize);

            var EntryLine = Chart.DrawHorizontalLine("Entry", (Symbol.Ask + 10 * Symbol.PipSize), Color.Blue, 3, LineStyle.Solid);
            EntryLine.IsInteractive = true;
            EntryLine.Comment = "Entry";

            var StopLine = Chart.DrawHorizontalLine("Stop", (Symbol.Ask + 10 * Symbol.PipSize), Color.Red, 3, LineStyle.Solid);
            StopLine.IsInteractive = true;
            StopLine.Comment = "Stop";

            var stackPanel = new StackPanel 
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                BackgroundColor = Color.Gold,
                Opacity = 0.7
            };
            for (int i = 0; i < 1; i++)
            {
                var button = new Button 
                {
                    Text = "Start",
                    Margin = 10
                };
                button.Click += Button_Click;
                stackPanel.AddChild(button);
            }
            Chart.AddControl(stackPanel);

        }




        private void Button_Click(ButtonClickEventArgs obj)
        {
            var textSplit = obj.Button.Text.Split(' ').TakeWhile(text => !text.Equals("Clicked", StringComparison.OrdinalIgnoreCase)).ToArray();




            TriggerStop = false;

            if (TriggerStop == true)
            {
                obj.Button.Text = "Waiting";

            }
            if (TriggerStop == false)
            {
                obj.Button.Text = "Working";
            }

        }

        private void PositionsClosed(PositionClosedEventArgs args)
        {



            if (TriggerStop == true && args.Position.Label == "First Pos" && args.Reason == PositionCloseReason.StopLoss)
            {
                Stop();
            }

            if (TriggerStop == false && args.Position.Label == "Second Position" && (args.Reason == PositionCloseReason.StopLoss || args.Reason == PositionCloseReason.TakeProfit))
            {
                Stop();
            }

        }



        protected override void OnTick()
        {


            //Trigger stop order region

            if (TriggerStop == false)
            {

                var lines = Chart.FindAllObjects<ChartHorizontalLine>();

                foreach (var line in lines)
                {
                    if (line.Comment == "Entry")
                    {
                        Entry = line.Y;


                    }
                    if (line.Comment == "Stop")
                    {
                        StopLossLevel = line.Y;


                    }
                }

                if (Entry > StopLossLevel && Protection == false)
                {
                    Entry = Math.Round(Entry, DecimalPrecision);
                    StopLossLevel = Math.Round(StopLossLevel, DecimalPrecision);
                    var SL_in_Pips = ((Entry - StopLossLevel) / Symbol.PipSize);
                    SL_in_Pips = Convert.ToInt32(SL_in_Pips);

                    SL = SL_in_Pips;

                    PlaceLimitOrder(TradeType.Buy, SymbolName, GetVol(SL_in_Pips), Entry, "First Pos", SL_in_Pips, SL_in_Pips * Ratio);

                    TriggerStop = true;
                    Protection = true;
                }

                if (Entry < StopLossLevel && Protection == false)
                {

                    Entry = Math.Round(Entry, DecimalPrecision);
                    StopLossLevel = Math.Round(StopLossLevel, DecimalPrecision);
                    var SL_in_Pips = ((StopLossLevel - Entry) / Symbol.PipSize);

                    SL_in_Pips = Convert.ToInt32(SL_in_Pips);

                    SL = SL_in_Pips;

                    PlaceLimitOrder(TradeType.Sell, SymbolName, GetVol(SL_in_Pips), Entry, "First Pos", SL_in_Pips, SL_in_Pips * Ratio);


                    TriggerStop = true;
                    Protection = true;
                }




            }


            //Second Pos Region
            var InitialPosition = Positions.Find("First Pos", SymbolName);

            if (InitialPosition != null)
            {

                var SecondPos = Positions.Find("Second Position", SymbolName);

                delta = Convert.ToDouble(InitialPosition.EntryPrice - InitialPosition.StopLoss);

                var SL_Pips = Math.Abs(delta) / Symbol.PipSize;



                if (InitialPosition.Pips >= SL_Pips)
                {
                    if (InitialPosition.StopLoss != InitialPosition.EntryPrice)
                    {
                        InitialPosition.ModifyStopLossPrice(InitialPosition.EntryPrice);
                    }

                    if (TriggerStop == true && SecondPos == null && InitialPosition.TradeType == TradeType.Buy)
                    {




                        ExecuteMarketOrder(TradeType.Buy, SymbolName, InitialPosition.VolumeInUnits, "Second Position", SL, (SL * Ratio) - SL);
                        TriggerStop = false;
                    }
                    if (TriggerStop == true && SecondPos == null && InitialPosition.TradeType == TradeType.Sell)
                    {

                        ExecuteMarketOrder(TradeType.Sell, SymbolName, InitialPosition.VolumeInUnits, "Second Position", SL, (SL * Ratio) - SL);
                        TriggerStop = false;
                    }
                }
            }

            //Second Pos End Region
        }

        //sl in pips
        protected double GetVol(double SL)
        {

            var x = Math.Round((Account.Balance * Risk_Percentage / 100) / (SL * Symbol.PipValue * Symbol.VolumeInUnitsMin));


            if (Symbol.VolumeInUnitsMin > 1)
            {
                return Convert.ToInt32(x * Symbol.VolumeInUnitsMin);
            }
            else
            {
                return (x * Symbol.VolumeInUnitsMin);
            }



        }
    }
}
