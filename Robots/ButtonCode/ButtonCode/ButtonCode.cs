using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class ClosingButton : Robot
    {
        [Parameter("Lines Thickness", DefaultValue = 3)]
        public int LineThickness { get; set; }
        protected override void OnStart()
        {

            //tests
           /* ExecuteMarketOrder(TradeType.Sell, SymbolName, 1000);
            var z= Chart.DrawHorizontalLine("First", 1.062, Color.Green, LineThickness);
            z.IsInteractive = true;
            var y = Chart.DrawHorizontalLine("Second", 1.035, Color.Green, LineThickness);
            y.IsInteractive = true;*/
            //Draw a line at every position
            foreach (var po in Positions)
            {
                if (po.SymbolName == SymbolName )
                {
                    var a = Chart.DrawHorizontalLine(po.ToString(), po.EntryPrice,GetColor(po),LineThickness) ;
                    a.IsInteractive = true;
                }
            }
            var tradingPanel = new TradingPanel(this, Symbol);
 
            var border = new Border 
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Right,
                Style = Styles.CreatePanelBackgroundStyle(),
                Margin = "8 8 8 8",
                Width = 150,
                Child = tradingPanel
            };
 
            Chart.AddControl(border);
        }

        Color GetColor(Position po)
        {
            if (po.TradeType == TradeType.Buy)
            {
                return Color.Green;
            }
            else { return Color.Red; }
        }
      

    }
 
    public class TradingPanel : CustomControl
    {

        

        private readonly Robot _robot;
        private readonly Symbol _symbol;
 
        public TradingPanel(Robot robot, Symbol symbol)
        {
            _robot = robot;
            _symbol = symbol;
            AddChild(CreateTradingPanel());
        }
 
        private ControlBase CreateTradingPanel()
        {
            var mainPanel = new StackPanel();
 
            var contentPanel = CreateContentPanel();
            mainPanel.AddChild(contentPanel);
 
            return mainPanel;
        }
 
        private StackPanel CreateContentPanel()
        {
            var contentPanel = new StackPanel 
            {
                Margin = 5
            };
            var grid = new Grid(1, 3);
            grid.Columns[1].SetWidthInPixels(5);
 
            var closeButton = CreateCloseButton("Aggregate", Styles.CreateBuyButtonStyle());
            grid.AddChild(closeButton, 0, 0);
 
            var closeAllButton = CreateCloseAllButton("Create Line", Styles.CreateBuyButtonStyle());
            grid.AddChild(closeAllButton, 0, 2);
 
            contentPanel.AddChild(grid);
 
            return contentPanel;
        }


       
        public Button CreateCloseButton(string text, Style style)
        {
           
           
            var closeButton = new Button 
            {
                Text = text,
                Style = style,
                Height = 25
            };

            
            closeButton.Click += args => Close(); 
 
            return closeButton;
        }
 
        private ControlBase CreateCloseAllButton(string text, Style style)
        {
            var closeAllButton = new Button 
            {
                Text = text,
                Style = style,
                Height = 25
            };
 
            closeAllButton.Click += args => CloseAll();
 
            return closeAllButton;
        }


        public void Close()
        {
           
            var price = 0.0;

            
            var trendLines = _robot.Chart.FindAllObjects<ChartHorizontalLine>();
            

            _robot.Print("number of lines " +trendLines.Length);
            foreach (var obj in trendLines)
            {
                
                price += obj.Y;

            }

            var aaa = _robot.Chart.DrawHorizontalLine("midline", (price / trendLines.Length), Color.Blue, 3);
            aaa.IsInteractive = true;

            
            
        }


        private void CloseAll()
        {
            
            ChartHorizontalLine aaa = (ChartHorizontalLine)_robot.Chart.FindObject("midline");
            
            var pos = _robot.Positions;
            foreach (var po in pos)
            {
                if (po.EntryPrice > aaa.Y && po.TradeType == TradeType.Sell)
                {
                   
                    po.ModifyTakeProfitPrice(aaa.Y);
                }
                if (po.EntryPrice < aaa.Y && po.TradeType == TradeType.Buy)
                {
                    po.ModifyTakeProfitPrice(aaa.Y);
                }
            }
        }
    }
 
    public static class Styles
    {
        public static Style CreatePanelBackgroundStyle()
        {
            var style = new Style();
            style.Set(ControlProperty.CornerRadius, 3);
            style.Set(ControlProperty.BackgroundColor, GetColorWithOpacity(Color.FromHex("#292929"), 0.85m), ControlState.DarkTheme);
            style.Set(ControlProperty.BackgroundColor, GetColorWithOpacity(Color.FromHex("#FFFFFF"), 0.85m), ControlState.LightTheme);
            style.Set(ControlProperty.BorderColor, Color.FromHex("#3C3C3C"), ControlState.DarkTheme);
            style.Set(ControlProperty.BorderColor, Color.FromHex("#C3C3C3"), ControlState.LightTheme);
            style.Set(ControlProperty.BorderThickness, new Thickness(1));
 
            return style;
        }
 
        public static Style CreateCommonBorderStyle()
        {
            var style = new Style();
            style.Set(ControlProperty.BorderColor, GetColorWithOpacity(Color.FromHex("#FFFFFF"), 0.12m), ControlState.DarkTheme);
            style.Set(ControlProperty.BorderColor, GetColorWithOpacity(Color.FromHex("#000000"), 0.12m), ControlState.LightTheme);
            return style;
        }
 
        public static Style CreateBuyButtonStyle()
        {
            return CreateButtonStyle(Color.FromHex("#009345"), Color.FromHex("#10A651"));
        }
 
        public static Style CreateSellButtonStyle()
        {
            return CreateButtonStyle(Color.FromHex("#F05824"), Color.FromHex("#FF6C36"));
        }
 
        public static Style CreateCloseButtonStyle()
        {
            return CreateButtonStyle(Color.FromHex("#F05824"), Color.FromHex("#FF6C36"));
        }
 
        private static Style CreateButtonStyle(Color color, Color hoverColor)
        {
            var style = new Style(DefaultStyles.ButtonStyle);
            style.Set(ControlProperty.BackgroundColor, color, ControlState.DarkTheme);
            style.Set(ControlProperty.BackgroundColor, color, ControlState.LightTheme);
            style.Set(ControlProperty.BackgroundColor, hoverColor, ControlState.DarkTheme | ControlState.Hover);
            style.Set(ControlProperty.BackgroundColor, hoverColor, ControlState.LightTheme | ControlState.Hover);
            style.Set(ControlProperty.ForegroundColor, Color.FromHex("#FFFFFF"), ControlState.DarkTheme);
            style.Set(ControlProperty.ForegroundColor, Color.FromHex("#FFFFFF"), ControlState.LightTheme);
            return style;
        }
 
        private static Color GetColorWithOpacity(Color baseColor, decimal opacity)
        {
            var alpha = (int)Math.Round(byte.MaxValue * opacity, MidpointRounding.AwayFromZero);
            return Color.FromArgb(alpha, baseColor);
        }
    }
}