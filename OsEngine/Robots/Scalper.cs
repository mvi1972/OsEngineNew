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
        /// <summary>
        /// конструктор 
        /// </summary>
        public Scalper(string name, StartProgram startProgram) : base(name, startProgram)
        {

        }

        /// <summary>
        /// вернуть название робота
        /// </summary>
        public override string GetNameStrategyType()
        {
            return "Scalper";
        }

        /// <summary>
        /// показать окно индивидуальных настроек 
        /// </summary>
        public override void ShowIndividualSettingsDialog()
        {
           
        }

        /// <summary>
        /// проверить условия на вход в позицию
        /// </summary>
        private void TryOpenPosition(List<Candle> candles)
        {
            decimal lastPrice = candles[candles.Count - 1].Close;
            // проверка индикатора 
        }
    }
}// темт 
