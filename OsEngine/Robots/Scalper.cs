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
        #region публичные переменные

        /// <summary>
        /// вкл/выкл для бумаги
        /// </summary>
        public bool IsOnPapir;

        /// <summary>
        /// вкл/выкл по хвостам
        /// </summary>
        public bool Tail;

        /// <summary>
        /// цена начала фазы роста
        /// </summary>
        public decimal PriceGrowthPhase;

        /// <summary>
        /// значение фазы роста
        /// </summary>
        public bool phaseGrowth;

        #endregion публичные переменные

        #region настройки на параметрах

        /// <summary>
        /// вкл/выкл для робота
        /// </summary>
        private StrategyParameterBool IsOn;

        /// <summary>
        /// количество свечей зоны роста
        /// </summary>
        private StrategyParameterInt candleBack;

        /// <summary>
        /// процентная величина зоны роста
        /// </summary>
        private StrategyParameterDecimal growthPercent;

        #endregion настройки на параметрах

        /// <summary>
        /// вкладка через которую робот совершает торговлю
        /// </summary>
        private BotTabSimple _tab;

        #region конструктор

        public Scalper(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            _tab = TabsSimple[0];
            _tab.CandleFinishedEvent += _tab_CandleFinishedEvent;

            candleBack = CreateParameter("Зона роста сколько свечей", 10, 5, 20, 1);
            growthPercent = CreateParameter("Процент роста бумаги", 3m, 2, 10, 1);
        }

        #endregion конструктор

        /// <summary>
        /// событие завершения новой свечи
        /// </summary>
        private void _tab_CandleFinishedEvent(List<Candle> candles)
        {
        }

        #region Логика

        /// <summary>
        /// расчет фазы роста
        /// </summary>
        private void СalculationPhaseGrowth(List<Candle> candles)
        {
            int canBack = candleBack.ValueInt;

            decimal maxBodyClose = 0; //максимальное значение закрытия последней свечи
            decimal minBodyOpen = decimal.MaxValue; //минимальное значение закрытия последней свечи
            decimal maxCandlesHigh = 0;  //  максимальное значение хая свечи
            decimal minCandelesLow = decimal.MaxValue; //минимальный лой

            if (Tail == false) //если галочка по хвостам ложь считаем по телам
            {
                for (int i = candles.Count - 1; i > 0 && i > candles.Count - 1 - canBack; i--)
                { // вычисляем значения открытия и закрытия свечей
                    if (candles[i].Close > maxBodyClose)
                    {
                        maxBodyClose = candles[i].Close;
                    }
                    if (candles[i].Open < minBodyOpen)
                    {
                        minBodyOpen = candles[i].Open;
                    }
                    decimal move = maxBodyClose - minBodyOpen;
                    if (move > 0)
                    {
                        decimal moveInPepcent = move / minBodyOpen * 100; // изменение значение в процентах
                        if (moveInPepcent >= growthPercent.ValueDecimal)
                        {
                            /*  ставим в phaseGrowth значение тру
                            записываем значение цены в PriceGrowthPhase */
                            phaseGrowth = true;
                        }
                    }
                    else phaseGrowth = false;
                }
            }
            if (Tail == true) // расчет по свечам с хвостами
            {
                for (int i = candles.Count - 1; i > 0 && i > candles.Count - 1 - canBack; i--)
                {   // вычисляем значения High и Low
                    if (candles[i].High > maxCandlesHigh)
                    {
                        maxCandlesHigh = candles[i].High;
                    }
                    if (candles[i].Low < minCandelesLow)
                    {
                        minCandelesLow = candles[i].Low;
                    }
                    decimal move = maxCandlesHigh - minCandelesLow;
                    if (move > 0)
                    {
                        decimal moveInPepcent = move / minCandelesLow * 100; // изменение значение в процентах
                        if (moveInPepcent >= growthPercent.ValueDecimal)
                        {
                            /*  ставим в phaseGrowth значение тру
                            записываем значение цены в PriceGrowthPhase */
                            phaseGrowth = true;
                        }
                    }
                    else phaseGrowth = false;
                }
            }
        }

        /// <summary>
        /// проверить условия на вход в позицию
        /// </summary>
        private void TryOpenPosition(List<Candle> candles)
        {
            СalculationPhaseGrowth(candles);
            if (phaseGrowth == false)
            {
                return;
            }
            decimal lastPrice = candles[candles.Count - 1].Close;
            // проверка индикатора
        }

        #endregion Логика

        #region сервис

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

        #endregion сервис
    }
}