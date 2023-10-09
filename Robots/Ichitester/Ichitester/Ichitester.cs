using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using CsvHelper;

using System.Collections.Generic;
using System.IO;


namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class Ichitester : Robot
    {
        [Parameter(DefaultValue = "F:\\\\Ichimoku.csv")]
        public string LogFile { get; set; }

        protected override void OnStart()
        {
            using (var reader = new StreamReader("F:\\\\Ichimoku.csv"))
                //, CultureInfo.InvariantCulture (after reader)
                using (var csv = new CsvReader(reader))
                {
                    var records = csv.GetRecords<Foo>();
                    var s = records.ToList();



                    var i = 0;
                    foreach (var e in s)
                    {
                        i++;
                        //Print(e.TF + " " + e.TimeEvent + " " + e.Direction);


                        Print(e.TimeEvent);
                        string[] stuff = e.TimeEvent.Split(' ');


                        var two = stuff[0];
                        var three = stuff[1];






                        string[] myDate = two.Split('/');
                        //day month year



                        var Day = Convert.ToInt32(myDate[0]);
                        var Month = Convert.ToInt32(myDate[1]);

                        var Year = Convert.ToInt32(myDate[2]);


                        string[] myTime = three.Split(':');

                        var Hour = Convert.ToInt32(myTime[0]);
                        var Minute = Convert.ToInt32(myTime[1]);
                        var Second = Convert.ToInt32(myTime[2]);




                        var date = new DateTime(Year, Month, Day, Hour, Minute, Second);

                        if (e.TF == "1H")
                        {
                            if (e.Direction == "Long")
                            {
                                var line = Chart.DrawVerticalLine("Line" + i, date, Color.Blue, 5);
                                line.IsInteractive = true;
                            }
                            if (e.Direction == "short")
                            {
                                var line = Chart.DrawVerticalLine("Line" + i, date, Color.Purple, 5);
                                line.IsInteractive = true;
                            }

                        }



                        if (e.TF == "30min")
                        {

                            if (e.Direction == "Long")
                            {
                                var line = Chart.DrawVerticalLine("Line" + i, date, Color.Red, 5);

                                line.IsInteractive = true;
                            }

                            if (e.Direction == "short")
                            {
                                var line = Chart.DrawVerticalLine("Line" + i, date, Color.Pink, 5);

                                line.IsInteractive = true;
                            }

                        }


                    }



                }
        }



        public class Foo
        {
            public string Direction { get; set; }
            public string TimeEvent { get; set; }

            public string TF { get; set; }
        }

        protected override void OnTick()
        {
            // Put your core logic here
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
