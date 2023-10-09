using cAlgo.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace cAlgo {
    class WorkerSignal {

        public static void OpenSignal(LiPiBotCoreBase robot, TradeType tradeType, double volume, LiPiBotCoreBase.Open_Position_Cause_Type postionType) {
            if (robot.BotTradingTypeSignalConsole == LiPiBotCoreBase.Log_On_Position_Event.All || robot.BotTradingTypeSignalConsole == LiPiBotCoreBase.Log_On_Position_Event.Open_Signal || robot.BotTradingTypeSignalConsole == LiPiBotCoreBase.Log_On_Position_Event.Open_And_Close_Signal) {
                robot.Print("Signal to Open Position was found: " + robot.GetLabel() + " : " + robot.SymbolName + " : " + " : " + tradeType.ToString() + " : volume = " + volume + " : postionType=" + postionType);
            }
            if ((robot.BotTradingTypeSignalMessageBox == LiPiBotCoreBase.Log_On_Position_Event.All || robot.BotTradingTypeSignalMessageBox == LiPiBotCoreBase.Log_On_Position_Event.Open_Signal || robot.BotTradingTypeSignalMessageBox == LiPiBotCoreBase.Log_On_Position_Event.Open_And_Close_Signal)
                    && robot.IsMessageBoxShowEnabled()) {
                
                string postionTypeText = "unknown";
                switch (postionType) {
                    case LiPiBotCoreBase.Open_Position_Cause_Type.PRIME_POSITION:
                        postionTypeText = "Prime Position";
                        break;
                    case LiPiBotCoreBase.Open_Position_Cause_Type.ADDITIONAL_POSITION:
                        postionTypeText = "Additional Position";
                        break;
                }

                new Thread(new ThreadStart(delegate {
                    // Aby MessageBox nebyl zobrazen jako modal, zobrazíme ho v Threadu.
                    MessageBox.Show(@"Signal to Open Position was found: "
                        + Environment.NewLine + "Time: " + robot.Server.TimeInUtc.AddHours(TimeZoneInfo.Local.GetUtcOffset(robot.Server.TimeInUtc).Hours)
                        + Environment.NewLine + "[" + postionTypeText + "]"
                        + Environment.NewLine + robot.GetLabel() + " : " + robot.SymbolName + " : " + tradeType.ToString() + " : " + volume.ToString(), 
                        robot.GetBotName() + " : Signal to Open Position", MessageBoxButtons.OK, MessageBoxIcon.Information);
                })).Start();
            }

            PlaySound(robot);
            /*
            // Pokud existuje WAV soubor, který by mohl být přehrán, bude přehrán.
            //if (robot.BotTradingTypeSignalSound == true) {
            // c:\Users\<username>\Documents\cAlgo\Sources\Robots\
                string folderUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string folderBotSounds = folderUser + "\\" + "cAlgo\\Sources\\Robots" + "\\" + "LiPiBot_Sounds";
                if (Directory.Exists(folderBotSounds)) {
                    if (File.Exists(@"" + folderBotSounds + "\\" + robot.GetLabel() + robot.GetBotVersion() + "-Open.wav")) robot.Notifications.PlaySound(@"" + folderBotSounds + "\\" + robot.GetLabel() + robot.GetBotVersion() + "-Open.wav");
                    else if (File.Exists(@"" + folderBotSounds + "\\" + robot.GetLabel() + "-Open.wav")) robot.Notifications.PlaySound(@"" + folderBotSounds + "\\" + robot.GetLabel() + "-Open.wav");
                    else if (File.Exists(@"" + folderBotSounds + "\\" + robot.GetLabel() + robot.GetBotVersion() + ".wav")) robot.Notifications.PlaySound(@"" + folderBotSounds + "\\" + robot.GetLabel() + robot.GetBotVersion() + ".wav");
                    else if (File.Exists(@"" + folderBotSounds + "\\" + robot.GetLabel() + ".wav")) robot.Notifications.PlaySound(@"" + folderBotSounds + "\\" + robot.GetLabel() + ".wav");
                    else if (File.Exists(@"" + folderBotSounds + "\\" + "LiPiBot-Open.wav")) robot.Notifications.PlaySound(@"" + folderBotSounds + "\\" + "LiPiBot-Open.wav");
                    else if (File.Exists(@"" + folderBotSounds + "\\" + "LiPiBot.wav")) robot.Notifications.PlaySound(@"" + folderBotSounds + "\\" + "LiPiBot.wav");
                    //else robot.Print("No Open Position Sound Exists in :" + folderBotSounds);
                } else {
                    //robot.Print("No Open Position Sound Exists in :" + folderBotSounds);
                }
            //}
            */
        }

        public static void CloseSignal(LiPiBotCoreBase robot, List<Position> positions, LiPiBotCoreBase.Close_Position_Cause? closeCause, PositionCloseReason? reason = null) {
            if (positions.Count == 0) return;
            TradeType tt = positions[0].TradeType;
            //robot.Print("############################ CloseSignal positions=" + positions.Count + " closeCause=" + closeCause + " reason=" + reason);
            string closeCauseText = "unknown";
            if (closeCause != null) {
                switch (closeCause) {
                    case LiPiBotCoreBase.Close_Position_Cause.START_TRADING_HOUR:
                        closeCauseText = "Start Trading Hour";
                        break;
                    case LiPiBotCoreBase.Close_Position_Cause.END_TRADING_HOUR:
                        closeCauseText = "End Trading Hour";
                        break;
                    case LiPiBotCoreBase.Close_Position_Cause.CLOSE_AGGREGATED_POSITIONS:
                        closeCauseText = "Close Aggregated Positions";
                        break;
                    case LiPiBotCoreBase.Close_Position_Cause.OPPOSITE_SIGNAL_EXISTS:
                        closeCauseText = "Opposite Signal Exists";
                        break;
                    case LiPiBotCoreBase.Close_Position_Cause.AGE:
                        closeCauseText = "Age";
                        break;
                    case LiPiBotCoreBase.Close_Position_Cause.LAST_TRADING_DAY_IN_WEEK:
                        closeCauseText = "Last Trading Day in Week";
                        break;
                }
            } else {
                closeCauseText = reason.ToString();
            }

            if (robot.BotTradingTypeSignalConsole == LiPiBotCoreBase.Log_On_Position_Event.All 
                    || (closeCause != null && (robot.BotTradingTypeSignalConsole == LiPiBotCoreBase.Log_On_Position_Event.Close_Signal || robot.BotTradingTypeSignalConsole == LiPiBotCoreBase.Log_On_Position_Event.Open_And_Close_Signal))
                    || (reason != null && robot.BotTradingTypeSignalConsole == LiPiBotCoreBase.Log_On_Position_Event.Close_Action)) {
                string positionsText = string.Join(",", positions.Select(item => "[ " + item.Comment + ", ID: " + item.Id + ", Entry Time: " + item.EntryTime + ", Volume: " + item.VolumeInUnits + ", Pips:" + item.Pips + " ]").ToArray());
                robot.Print("Signal to Close Position(s) was found: " + " [ " + closeCauseText + " ]" + robot.GetLabel() + " : " + robot.SymbolName + " : " + tt.ToString() + " : Positions: " + "[" + positions.Count + "]" + " : " + positionsText);
            }
            //if (robot.BotTradingTypeSignalMessageBox == LPBBase.Log_Signals.Yes && robot.IsMessageBoxShowEnabled()) {
            if ((robot.BotTradingTypeSignalMessageBox == LiPiBotCoreBase.Log_On_Position_Event.All
                    || (closeCause != null && (robot.BotTradingTypeSignalMessageBox == LiPiBotCoreBase.Log_On_Position_Event.Close_Signal || robot.BotTradingTypeSignalMessageBox == LiPiBotCoreBase.Log_On_Position_Event.Open_And_Close_Signal))
                    || (reason != null && robot.BotTradingTypeSignalMessageBox == LiPiBotCoreBase.Log_On_Position_Event.Close_Action))
                        && robot.IsMessageBoxShowEnabled()) {
                DateTime dt = robot.Server.TimeInUtc.AddHours(TimeZoneInfo.Local.GetUtcOffset(robot.Server.TimeInUtc).Hours);
                string positionsText = string.Join(@"" + Environment.NewLine, positions.Select(item =>  "[ " + item.Comment + " ]" + Environment.NewLine + "ID:" + item.Id + ", " + item.EntryTime + ", Vol.:" + item.VolumeInUnits + ", Pips:" + item.Pips + "").ToArray());
                new Thread(new ThreadStart(delegate {
                    // Aby MessageBox nebyl zobrazen jako modal, zobrazíme ho v Threadu.                    
                    MessageBox.Show(@"Signal to Close Position(s) was found: "
                            + Environment.NewLine + "Time: " + dt
                            + Environment.NewLine 
                            + "[ " + closeCauseText + " ]" + Environment.NewLine 
                            + robot.GetLabel() + " : " + robot.SymbolName + " : " + tt.ToString() + " : " + Environment.NewLine 
                            + "Positions :" + "[" + positions.Count + "]" + " : " + Environment.NewLine
                            + Environment.NewLine
                            + "" + positionsText,
                        robot.GetBotName() + " : Signal to Close Position(s)", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Information);
                })).Start();
            }
            
            PlaySound(robot);
            /*
            // Pokud existuje WAV soubor, který by mohl být přehrán, bude přehrán.
            //if (robot.BotTradingTypeSignalSound == true) {
                // c:\Users\<username>\Documents\cAlgo\Sources\Robots\
                string folderUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string folderBotSounds = folderUser + "\\" + "cAlgo\\Sources\\Robots" + "\\" + "LiPiBot_Sounds";
                if (Directory.Exists(folderBotSounds)) {
                    if (File.Exists(@"" + folderBotSounds + "\\" + robot.GetLabel() + robot.GetBotVersion() + "-Close.wav")) robot.Notifications.PlaySound(@"" + folderBotSounds + "\\" + robot.GetLabel() + robot.GetBotVersion() + "-Close.wav");
                    else if (File.Exists(@"" + folderBotSounds + "\\" + robot.GetLabel() + "-Close.wav")) robot.Notifications.PlaySound(@"" + folderBotSounds + "\\" + robot.GetLabel() + "-Close.wav");
                    else if (File.Exists(@"" + folderBotSounds + "\\" + robot.GetLabel() + robot.GetBotVersion() + ".wav")) robot.Notifications.PlaySound(@"" + folderBotSounds + "\\" + robot.GetLabel() + robot.GetBotVersion() + ".wav");
                    else if (File.Exists(@"" + folderBotSounds + "\\" + robot.GetLabel() + ".wav")) robot.Notifications.PlaySound(@"" + folderBotSounds + "\\" + robot.GetLabel() + ".wav");
                    else if (File.Exists(@"" + folderBotSounds + "\\" + "LiPiBot-Close.wav")) robot.Notifications.PlaySound(@"" + folderBotSounds + "\\" + "LiPiBot-Close.wav");
                    else if (File.Exists(@"" + folderBotSounds + "\\" + "LiPiBot.wav")) robot.Notifications.PlaySound(@"" + folderBotSounds + "\\" + "LiPiBot.wav");
                    //else robot.Print("No Close Position Sound Exists in :" + folderBotSounds);
                } else {
                    //robot.Print("No Close Position Sound Exists in :" + folderBotSounds);
                }
            //}
            */
        }

        private static void PlaySound(LiPiBotCoreBase robot) {
            /*
            // Pokud existuje WAV soubor, který by mohl být přehrán, bude přehrán.
            //if (robot.BotTradingTypeSignalSound == true) {
            // c:\Users\<username>\Documents\cAlgo\Sources\Robots\
            string folderUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string folderBotSounds = folderUser + "\\" + "cAlgo\\Sources\\Robots" + "\\" + "LiPiBot_Sounds";
            if (Directory.Exists(folderBotSounds)) {
                if (File.Exists(@"" + folderBotSounds + "\\" + robot.GetLabel() + robot.GetBotVersion() + "-Close.wav")) robot.Notifications.PlaySound(@"" + folderBotSounds + "\\" + robot.GetLabel() + robot.GetBotVersion() + "-Close.wav");
                else if (File.Exists(@"" + folderBotSounds + "\\" + robot.GetLabel() + "-Close.wav")) robot.Notifications.PlaySound(@"" + folderBotSounds + "\\" + robot.GetLabel() + "-Close.wav");
                else if (File.Exists(@"" + folderBotSounds + "\\" + robot.GetLabel() + robot.GetBotVersion() + ".wav")) robot.Notifications.PlaySound(@"" + folderBotSounds + "\\" + robot.GetLabel() + robot.GetBotVersion() + ".wav");
                else if (File.Exists(@"" + folderBotSounds + "\\" + robot.GetLabel() + ".wav")) robot.Notifications.PlaySound(@"" + folderBotSounds + "\\" + robot.GetLabel() + ".wav");
                else if (File.Exists(@"" + folderBotSounds + "\\" + "LiPiBot-Close.wav")) robot.Notifications.PlaySound(@"" + folderBotSounds + "\\" + "LiPiBot-Close.wav");
                else if (File.Exists(@"" + folderBotSounds + "\\" + "LiPiBot.wav")) robot.Notifications.PlaySound(@"" + folderBotSounds + "\\" + "LiPiBot.wav");
                //else robot.Print("No Close Position Sound Exists in :" + folderBotSounds);
            } else {
                //robot.Print("No Close Position Sound Exists in :" + folderBotSounds);
            }
            //}
            */
        }
    }
}
