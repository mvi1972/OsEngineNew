using System;
using System.Collections.Generic;
using System.IO;
using OsEngine.Charts.CandleChart.Indicators;
using OsEngine.Entity;
using OsEngine.Market;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;


namespace OsEngine.Robots
{
    public class Scalper : BotPanel
    {
        public Scalper(string name, StartProgram startProgram) : base(name, startProgram)
        {

        }
        public override string GetNameStrategyType()
        {
            return "Scalper";
        }

        public override void ShowIndividualSettingsDialog()
        {
           
        }
    }
}
