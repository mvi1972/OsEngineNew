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
        // публичные настройки
        /// <summary>
        /// вкл/выкл для робота
        /// </summary>
        public bool IsOn;

        /// <summary>
        /// вкл/выкл для бумаги
        /// </summary>
        public bool IsOnPapir;

        // публичные переменные 
        /// <summary>
        /// цена начала фазы роста
        /// </summary>
        public decimal PriceGrowthPhase;

        /// <summary>
        /// вкладка через которую робот совершает торговлю
        /// </summary>
        private BotTabSimple _tab;


        /// <summary>
        /// конструктор 
        /// </summary>
        public Scalper(string name, StartProgram startProgram) : base(name, startProgram)
        {

            TabCreate(BotTabType.Simple);
            _tab = TabsSimple[0];
            _tab.CandleFinishedEvent += _tab_CandleFinishedEvent;
        }

        /// <summary>
        /// событие завершения новой свечи
        /// </summary>
        private void _tab_CandleFinishedEvent(List<Candle> candles)
        {
            
        }

        // логика 
        /// <summary>
        /// расчет точки роста
        /// </summary>
        private void СalculationPointGrowth(List<Candle> candles)
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

        // сервис

               /// <summary>
        /// проверить условия на вход в позицию
        /// </summary>

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

    }
   
}
