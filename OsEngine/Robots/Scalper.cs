using System;
using System.Collections.Generic;
using System.IO;
using OsEngine.Charts.CandleChart.Indicators;
using OsEngine.Entity;
using OsEngine.Market;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Indicators;
using System.Diagnostics;

namespace OsEngine.Robots
{
    public class Scalper : BotPanel
    {
        #region переменные

        /// <summary>
        /// вкл/выкл для бумаги
        /// </summary>
        public bool IsOnPapir;

        /// <summary>
        /// рыночная цена бумаги
        /// </summary>
        public decimal marketPrice;

        /// <summary>
        /// вкл/выкл по хвостам
        /// </summary>
        public bool _tail;

        /// <summary>
        /// расчет фазы роста вкл/выкл
        /// </summary>
        public bool _calculationGP;

        /// <summary>
        /// минимальная цена начала фазы роста
        /// </summary>
        public decimal minPriceGrowthPhase;

        /// <summary>
        /// значение фазы роста
        /// </summary>
        public bool phaseGrowth;

        #endregion переменные

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
        /// процентная величина от минимума зоны роста
        /// </summary>
        private StrategyParameterDecimal riseFromLow;

        /// <summary>
        /// расстояние до трейдинг стопа в процентах
        /// </summary>
        private StrategyParameterDecimal TrailStopLength;

        //индикатор
        private Aindicator _dsr;

        private StrategyParameterInt Longterm; // длина длинной ema
        private StrategyParameterInt DSR1; //  длина средней ema
        private StrategyParameterInt DSR2; //  длина короткой ema

        #endregion настройки на параметрах

        /// <summary>
        /// вкладка через которую робот совершает торговлю
        /// </summary>
        private BotTabSimple _tab;

        #region конструктор

        public Scalper(string name, StartProgram startProgram) : base(name, startProgram)
        {
            // для теста хвосты и расчет фазы
            _tail = true;
            _calculationGP = true;

            // базовые инициализации
            phaseGrowth = false;
            minPriceGrowthPhase = 0;

            TabCreate(BotTabType.Simple);
            _tab = TabsSimple[0];
            // события
            _tab.CandleFinishedEvent += MainInterLogic; // привязываем логику
            ParametrsChangeByUser += _ParametrsChangeUserы; // событие пользователь изменил параметры
            _tab.NewTickEvent += NewTickEvent; // новые тики
            _tab.PositionClosingSuccesEvent += PositionClosingEvent; // обнуление переменных при закрытии поз

            // настройки
            IsOn = CreateParameter("Включить", false);
            volume = CreateParameter("рабочий объём", 1, 1, 1, 1); // тестовые значения
            candleBack = CreateParameter("Зона роста сколько свечей", 10, 5, 20, 1);
            growthPercent = CreateParameter("Процент роста бумаги", 3m, 2, 10, 1);
            riseFromLow = CreateParameter("Процент до минимума", 1m, 1, 10, 1);
            TrailStopLength = CreateParameter("Процент Трейлинг стопа", 3m, 2, 10, 1);
            // настройки индюка
            Longterm = CreateParameter("Longterm Length", 9, 4, 100, 2);
            DSR1 = CreateParameter("DSR1 Length", 7, 1, 4, 1);
            DSR2 = CreateParameter("DSR2 Length", 1, 1, 4, 1);

            _dsr = IndicatorsFactory.CreateIndicatorByName("DSR", name + "DSR", false);
            _dsr.ParametersDigit[0].Value = Longterm.ValueInt;
            _dsr.ParametersDigit[1].Value = DSR1.ValueInt;
            _dsr.ParametersDigit[2].Value = DSR2.ValueInt;
            _dsr = (Aindicator)_tab.CreateCandleIndicator(_dsr, "Prime");

            _dsr.Save();
        }

        /// <summary>
        /// закрылась позиция обнуляю переменные тут
        /// </summary>
        private void PositionClosingEvent(Position position)
        {
            minPriceGrowthPhase = 0;
            _calculationGP = true; //  разрешаем считать заново
        }

        #endregion конструктор

        #region Логика

        /// <summary>
        /// событие завершения новой свечи главный вход в логику
        /// </summary>
        private void MainInterLogic(List<Candle> candles)
        {
            List<Position> positions = _tab.PositionsOpenAll;
            if (positions.Count != 0)
            {
                TrelingProfit();
            }
            if (IsOn.ValueBool == false) // если робот выключен
            {
                return;
            }
            IndicatorDSR(); // проверка состояния индикатора DSR

            if (positions.Count == 0) // логика входа
            {
                СalculationPhaseGrowthExtremeCandels(candles); // расчет фазы роста

                if (phaseGrowth == false)  // если фаза роста
                {
                    return;
                }
                decimal indent = minPriceGrowthPhase * riseFromLow.ValueDecimal / 100;
                if (marketPrice > minPriceGrowthPhase + indent && minPriceGrowthPhase != 0)
                {
                    _tab.BuyAtMarket(volume.ValueInt); // тестовый вход
                }
            }
        }

        /// <summary>
        /// событие новый тик(трейды)
        /// </summary>
        private void NewTickEvent(Trade trade)
        {
            marketPrice = trade.Price;
            if (phaseGrowth == true)
            {
                if (marketPrice < minPriceGrowthPhase)
                {
                    GetMinPriceGP();
                }
            }
        }

        private void GetMinPriceGP()
        {
            minPriceGrowthPhase = marketPrice;
        }

        /// <summary>
        /// расчет фазы роста по цене крайних свечей
        /// </summary>
        private void СalculationPhaseGrowthExtremeCandels(List<Candle> candles)
        {
            int canBack = candleBack.ValueInt;
            if (candles.Count < canBack + 1)
            {
                return;
            }
            if (_calculationGP == false)
            {
                return;
            }
            decimal maxBodyClose = candles[candles.Count - 1].Close; //максимальное значение закрытия последней свечи периода
            decimal minBodyOpen = candles[candles.Count - 1 - canBack].Open; //минимальное значение открытия первой свечи периода
            decimal highPriceOutPeriod = candles[candles.Count - 1].High; // цена хая последней свечи периода
            decimal lowPriceInPerod = candles[candles.Count - 1 - canBack].Low; // цена  лоя начальной свечи периода

            if (_tail == false) //если галочка по хвостам ложь считаем по телам
            {
                decimal rost = maxBodyClose - minBodyOpen;
                decimal rostPers = rost / minBodyOpen * 100;
                if (rostPers >= growthPercent.ValueDecimal)
                {
                    /*  ставим в phaseGrowth значение тру
                      записываем значение цены в minPriceGrowthPhase */
                    phaseGrowth = true;
                    string str = "Значение фазы роста = " + phaseGrowth.ToString() + "\n";
                    Debug.WriteLine(str);
                    minPriceGrowthPhase = marketPrice;
                    _calculationGP = false;
                }
                else phaseGrowth = false;
            }
            if (_tail == true) // расчет по свечам с хвостами
            {
                decimal rost = highPriceOutPeriod - lowPriceInPerod;
                decimal rostPers = rost / lowPriceInPerod * 100;
                if (rostPers >= growthPercent.ValueDecimal)
                {
                    /*  ставим в phaseGrowth значение тру
                      записываем значение цены в PriceGrowthPhase */
                    phaseGrowth = true;
                    string str = "Значение фазы роста = " + phaseGrowth.ToString() + "\n";
                    Debug.WriteLine(str);
                    minPriceGrowthPhase = marketPrice;
                    _calculationGP = false;
                }
                else phaseGrowth = false;
            }
        }

        /// <summary>
        /// расчет фазы роста по изменению  цен  внутри свечей периода
        /// </summary>
        private void СalculationPhaseGrowthInsideCandels(List<Candle> candles)
        {
            int canBack = candleBack.ValueInt;

            decimal maxBodyClose = 0; //максимальное значение закрытия последней свечи
            decimal minBodyOpen = decimal.MaxValue; //минимальное значение закрытия последней свечи
            decimal maxCandlesHigh = 0;  //  максимальное значение хая свечи
            decimal minCandelesLow = decimal.MaxValue; //минимальный лой

            if (_tail == false) //если галочка по хвостам ложь считаем по телам
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
            if (_tail == true) // расчет по свечам с хвостами
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
        ///отключение фазы роста согласно значению DSR
        /// </summary>
        private void IndicatorDSR()
        {
            decimal _trendDSR = _dsr.DataSeries[0].Last; //  значение индикатора DSR
            if (_trendDSR == 0) // если 0 фаза роста закончилась
            {
                phaseGrowth = false; //выключаем
                _calculationGP = true; //  разрешаем считать заново
            }
        }

        /// <summary>
        ///  трейлинг профит
        /// </summary>
        private void TrelingProfit()
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
        /// пользователь изменил настройки параметров
        /// </summary>
        private void _ParametrsChangeUserы()
        {
            if (_dsr.ParametersDigit[0].Value != Longterm.ValueInt)
            {
                _dsr.ParametersDigit[0].Value = Longterm.ValueInt;
                _dsr.Reload();
            }
            if (_dsr.ParametersDigit[1].Value != DSR1.ValueInt)
            {
                _dsr.ParametersDigit[1].Value = DSR1.ValueInt;
                _dsr.Reload();
            }
            if (_dsr.ParametersDigit[2].Value != DSR2.ValueInt)
            {
                _dsr.ParametersDigit[2].Value = DSR2.ValueInt;
                _dsr.Reload();
            }
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

        #endregion сервис
    }
}