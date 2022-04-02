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
        public decimal priceGrowthPhase;

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
        /// рабочий объём сделки
        /// </summary>
        private StrategyParameterInt volume; // для тестов

        /// <summary>
        /// количество свечей зоны роста
        /// </summary>
        private StrategyParameterInt candleBack;

        /// <summary>
        /// процентная величина зоны роста
        /// </summary>
        private StrategyParameterDecimal growthPercent;

        /// <summary>
        /// расстояние до трейдинг стопа в процентах
        /// </summary>
        private StrategyParameterDecimal TrailStopLength;

        #endregion настройки на параметрах

        /// <summary>
        /// вкладка через которую робот совершает торговлю
        /// </summary>
        private BotTabSimple _tab;

        #region конструктор

        public Scalper(string name, StartProgram startProgram) : base(name, startProgram)
        {
            // для теста
            Tail = true;

            TabCreate(BotTabType.Simple);
            _tab = TabsSimple[0];
            _tab.CandleFinishedEvent += _tab_CandleFinishedEvent;

            phaseGrowth = false; // значение при создании

            IsOn = CreateParameter("Включить", false);
            volume = CreateParameter("рабочий объём", 1, 1, 1, 1); // тестовые значения
            candleBack = CreateParameter("Зона роста сколько свечей", 10, 5, 20, 1);
            growthPercent = CreateParameter("Процент роста бумаги", 3m, 2, 10, 1);
            TrailStopLength = CreateParameter("Процент Трейлинг стопа", 1.5m, 2, 10, 1);
        }

        #endregion конструктор

        /// <summary>
        /// событие завершения новой свечи главный вход в логику
        /// </summary>
        private void _tab_CandleFinishedEvent(List<Candle> candles)
        {
            if (IsOn.ValueBool == false) // если робот выключен
            {
                return;
            }
            List<Position> positions = _tab.PositionsOpenAll;
            if (positions.Count == 0) // логика входа
            {
                СalculationPhaseGrowth(candles);
                if (phaseGrowth == false)
                {
                    return;
                }
                // проверка индикатора

                decimal lastPrice = _tab.PriceBestAsk;
                _tab.BuyAtMarket(volume.ValueInt); // тестовый вход
            }
            else // логика выхода
            {
                TrelingStop();
            }
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
                    if (candles[i].IsUp)
                    {
                        if (candles[i].Close > maxBodyClose)
                        {
                            maxBodyClose = candles[i].Close;
                        }
                        if (candles[i].Open < minBodyOpen)
                        {
                            minBodyOpen = candles[i].Open;
                        }
                        decimal move = maxBodyClose - minBodyOpen;
                        if (maxBodyClose > minBodyOpen)
                        {
                            decimal moveInPepcent = move / minBodyOpen * 100; // изменение значение в процентах
                            if (moveInPepcent >= growthPercent.ValueDecimal)
                            {
                                /*  ставим в phaseGrowth значение тру
                                записываем значение цены в PriceGrowthPhase */
                                phaseGrowth = true;
                            }
                        }
                    }
                    else phaseGrowth = false;
                }
            }
            if (Tail == true) // расчет по свечам с хвостами
            {
                maxCandlesHigh = candles[candles.Count - 1 - canBack].High;
                minCandelesLow = candles[candles.Count - 1].Low;

                if (maxCandlesHigh < minCandelesLow)
                {
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
        }

        private void TrelingStop() // трейлинг стоп для тестов
        {
            List<Position> position = _tab.PositionsOpenAll;
            if (position.Count == 0)
            {
                return;
            }
            else
            {
                decimal lastPrice = _tab.PriceBestAsk;
                decimal trailActiv = lastPrice - lastPrice * TrailStopLength.ValueDecimal / 100;
                decimal trailOrder = trailActiv - 5 * _tab.Securiti.PriceStep;
                _tab.CloseAtTrailingStop(position[0], trailActiv, trailOrder);
            }
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