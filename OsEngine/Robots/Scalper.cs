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
        // публичные переменные 
        /// <summary>
        /// вкл/выкл для бумаги
        /// </summary>
        public bool IsOnPapir;
        /// <summary>
        /// цена начала фазы роста
        /// </summary>
        public decimal PriceGrowthPhase;
        /// <summary>
        /// значение фазы роста
        /// </summary>
        public bool PhaseGrowth;

        // настройки на параметрах 
        private StrategyParameterInt CandleBack;
        /// <summary>
        /// вкл/выкл для робота
        /// </summary>
        private StrategyParameterBool IsOn;

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
        /// расчет фазы роста
        /// </summary>
        private void СalculationPhaseGrowth(List<Candle> candles)
        {
            /*
             *  если тру выходим 
            взять значение открытия свечи Н баров назад 
            взять значение закрытия последней свечи 
            рассчитать заданное значение в заданных процентах 
            если совпадает 
            ставим в PhaseGrowth значение тру
            записываем значение цены в PriceGrowthPhase

             */

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
