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
using System.Windows;
using OsEngine.Robots.Screeners;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OsEngine.Robots
{
    public class Scalper : BotPanel, INotifyPropertyChanged
    {
        #region переменные

        /// <summary>
        /// поле для тестов 1
        /// </summary>
        private decimal _test; // поле

        public decimal Test
        {
            get => _test;
            set => Set(ref _test, value);
        }

        /// <summary>
        /// вкл/выкл для бумаги
        /// </summary>
        public bool IsOnPapir;

        /// <summary>
        /// рыночная цена бумаги
        /// </summary>
        public decimal marketPrice;

        /// <summary>
        /// время последнего трейда
        /// </summary>
        public DateTime real_time;

        /// <summary>
        /// вкл/выкл трейлинг стоп лосс
        /// </summary>
        public bool isOnTrelingStopLoss;

        /// <summary>
        /// временное значение стоп лосс 2
        /// </summary>
        public decimal stopLoss2Temp;

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

        private StrategyParameterBool TestIsOnStopLoss; // для тестов включение
        private StrategyParameterBool TestIsOnProfit; // для тестов включение
        private StrategyParameterBool TestIsOnTrelingProfit; // для тестов включение

        /// <summary>
        /// расстояние до стоп лосса 1
        /// </summary>
        private StrategyParameterDecimal stopLoss1;

        /// <summary>
        /// расстояние до стоп лосса 2
        /// </summary>
        private StrategyParameterDecimal stopLoss2;

        /// <summary>
        /// количество минут на стоп 2
        /// </summary>
        private StrategyParameterInt minutBackStopLoss2;

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
        /// расстояние до стоп лосса в процентах
        /// </summary>
        private StrategyParameterDecimal TrailStopLossLength;

        /// <summary>
        /// расстояние до трейлинг профита в процентах
        /// </summary>
        private StrategyParameterDecimal TrailProfitLength;

        /// <summary>
        /// расстояние до профита в процентах
        /// </summary>
        private StrategyParameterDecimal ProfitLength;

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

        /// <summary>
        /// вкладка скринера
        /// </summary>
        private BotTabScreener _tabScreen;

        /// <summary>
        /// список роботов в скринере
        /// </summary>
        public List<BotTabSimple> robots = new List<BotTabSimple>();

        #region конструктор

        public Scalper(string name, StartProgram startProgram) : base(name, startProgram)
        {
            // для теста хвосты, расчет фазы, трейлинг стоп лосс
            _tail = true;
            _calculationGP = true;
            isOnTrelingStopLoss = true;

            // базовые инициализации
            phaseGrowth = false;
            minPriceGrowthPhase = 0;

            TabCreate(BotTabType.Screener);
            _tabScreen = TabsScreener[0];
            _tabScreen.NewTabCreateEvent += _tabScreen_NewTabCreateEvent;

            // события
            ParametrsChangeByUser += _ParametrsChangeUser; // событие пользователь изменил параметры

            // настройки
            IsOn = CreateParameter("Включить", false, "Вход");
            volume = CreateParameter("рабочий объём", 1, 1, 1, 1, "Вход"); // тестовые значения
            candleBack = CreateParameter("Зона роста сколько свечей", 10, 5, 20, 1, "Вход");
            growthPercent = CreateParameter("Процент зоны роста бумаги", 3m, 2, 10, 1, "Вход");
            riseFromLow = CreateParameter("От зоны роста входим через", 1m, 1, 10, 1, "Вход");
            ProfitLength = CreateParameter("Процент до профита ", 3m, 1, 10, 1, " Выход");
            TrailStopLossLength = CreateParameter("Процент Трейлинг стопа", 3m, 2, 10, 1, " Выход");
            TrailProfitLength = CreateParameter("Процент Трейлинг профита ", 1m, 1, 10, 1, " Выход");
            stopLoss1 = CreateParameter("Процент Cтоп лосс ", 3m, 2, 10, 1, " Выход");
            stopLoss2 = CreateParameter("Процент Cтоп лосс 2 ", 5m, 2, 10, 1, " Выход");
            minutBackStopLoss2 = CreateParameter("Минут назад для стоп 2", 40, 10, 10, 600, " Выход");
            // для теста
            TestIsOnStopLoss = CreateParameter("Включить трейлинг стоп лосс", false, " Галочки");
            TestIsOnTrelingProfit = CreateParameter("Включить Трейлинг Профит", true, " Галочки");
            TestIsOnProfit = CreateParameter("Включить Тейк Профит", false, " Галочки");

            // настройки индюка
            _tabScreen.CreateCandleIndicator(1, "DSR", new List<string>() { "9", "7", "1" });

            Longterm = CreateParameter("Longterm Length", 9, 4, 100, 2, " индикатор");
            DSR1 = CreateParameter("DSR1 Length", 7, 1, 4, 1, " индикатор");
            DSR2 = CreateParameter("DSR2 Length", 1, 1, 4, 1, " индикатор");

            // см  метод изменения параметров
        }

        private void _tabScreen_NewTabCreateEvent(BotTabSimple newTab)
        {
            robots.Add(newTab);
            newTab.PositionClosingSuccesEvent += (Position position) =>
            {
                PositionClosingEvent(position);
            };

            newTab.PositionOpeningSuccesEvent += (Position position) => PositionOpeningEvent(position, newTab);

            newTab.NewTickEvent += (Trade trade) => NewTickEvent(trade);

            newTab.CandleFinishedEvent += (List<Candle> candles) =>
            {
                MainInterLogic(candles, newTab);
                IndicatorDSR(candles, newTab);
            };
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
        private void MainInterLogic(List<Candle> candles, BotTabSimple _tab)
        {
            List<Position> positions = _tab.PositionsOpenAll;
            if (positions.Count != 0)
            {
                TrelingStopLossAndProfit(_tab);
                Profit(_tab);
            }
            if (IsOn.ValueBool == false) // если робот выключен
            {
                return;
            }
            IndicatorDSR(candles, _tab); // проверка состояния индикатора DSR

            if (positions.Count == 0) // логика входа
            {
                СalculationPhaseGrowthExtremeCandels(candles); // расчет фазы роста

                if (phaseGrowth == false)  // если не фаза роста
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
        /// входящее событие о том что открылась некая сделка ставим стоп
        /// </summary>
        private void PositionOpeningEvent(Position position, BotTabSimple _tab)
        {
            StopLoss(position, _tab);
        }

        /// <summary>
        /// событие новый тик(трейды) берем цену
        /// </summary>
        private void NewTickEvent(Trade trade)
        {
            marketPrice = trade.Price;
            real_time = trade.Time;
            Test = marketPrice;

            GetMinPriceGP();
        }

        /// <summary>
        /// выставляет стоп лосс
        /// </summary>
        private void StopLoss(Position position, BotTabSimple _tab)  // выставляем стоп по отступу в обход вызова из метода окончания свечи
        {
            decimal indent = stopLoss1.ValueDecimal * marketPrice / 100;  // отступ для стопа
            decimal priceOpenPos = _tab.PositionsLast.EntryPrice;  // цена открытия позиции
            decimal priceStopLoss1 = priceOpenPos - indent;

            List<Position> positionClose = _tab.PositionsCloseAll;
            if (positionClose.Count > 2)
            {
                // 2. Если  в течении 40 минут [2 Профита] было закрыто 2 прибыльных сделки,
                // то в следующих сделках "Стоп-лосс" устанавливается на 5% [Стоп-лосс2].
                // 3. После выключения "Фазы роста", когда будет следующая "Фаза роста",
                // "Стоп-лосс" снова будет ставиться на 3% [Стоп-лосс1].

                decimal profitLast = positionClose[positionClose.Count - 1].ProfitPortfolioPunkt;// профит последней следки
                decimal profitLast2 = positionClose[positionClose.Count - 2].ProfitOperationPunkt;// профит предпоследней сделки
                DateTime timeBackTrade = positionClose[positionClose.Count - 2].TimeClose; // время закрытия предп позы
                DateTime timePeriodBack = timeBackTrade.AddMinutes(minutBackStopLoss2.ValueInt); // время учтенного периода

                if (profitLast > 0 && profitLast2 > 0 && real_time < timePeriodBack && phaseGrowth == true)
                {
                    indent = stopLoss2.ValueDecimal * marketPrice / 100;  // отступ для стопа
                    priceOpenPos = _tab.PositionsLast.EntryPrice;  // цена открытия позиции
                    decimal priceStopLoss2 = priceOpenPos - indent;

                    if (position.StopOrderPrice == 0 ||
                        position.StopOrderPrice < priceStopLoss2)
                    {
                        //_tab.CloseAtMarket(position, position.OpenVolume);
                        _tab.CloseAtStop(position, priceStopLoss2, priceStopLoss2 - _tab.Securiti.PriceStep);
                        //_tab.CloseAtMarket()
                    }

                    if (position.StopOrderIsActiv == false)
                    {
                        if (position.StopOrderRedLine - _tab.Securiti.PriceStep * 10 > _tab.PriceBestAsk)
                        {
                            _tab.CloseAtMarket(position, position.OpenVolume);
                            //_tab.CloseAtLimit(position, _tab.PriceBestAsk, position.OpenVolume);
                            return;
                        }
                        position.StopOrderIsActiv = true;
                    }
                }
            }
            //_tab.BuyAtStopCancel();
            //_tab.SellAtStopCancel();

            if (position.StopOrderPrice == 0 ||
                position.StopOrderPrice < priceStopLoss1)
            {
                //_tab.CloseAtMarket(position, position.OpenVolume);
                _tab.CloseAtStop(position, priceStopLoss1, priceStopLoss1 - _tab.Securiti.PriceStep);
            }

            if (position.StopOrderIsActiv == false)
            {
                if (position.StopOrderRedLine - _tab.Securiti.PriceStep * 10 > _tab.PriceBestAsk)
                {
                    _tab.CloseAtMarket(position, position.OpenVolume);
                    //_tab.CloseAtLimit(position, _tab.PriceBestAsk, position.OpenVolume);
                    return;
                }
                position.StopOrderIsActiv = true;
            }
        }

        /// <summary>
        /// перерасчет минимальной цены зоны
        /// </summary>
        private void GetMinPriceGP()
        {
            if (phaseGrowth == true)
            {
                if (marketPrice < minPriceGrowthPhase)// цена ниже начала зоны роста
                {
                    minPriceGrowthPhase = marketPrice;
                    //
                    string str = "Минимум фазы роста сменился и = " + minPriceGrowthPhase.ToString() + "\n";
                    Debug.WriteLine(str);
                }
            }
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
            if (_calculationGP == false)// отключает метод что бы зафиксировать цену начала фазы роста
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
                    phaseGrowth = true; //  ставим в phaseGrowth значение тру
                                        //
                    string str = "Значение фазы роста = " + phaseGrowth.ToString() + "\n";
                    Debug.WriteLine(str);
                    minPriceGrowthPhase = marketPrice; // записываем значение цены в minPriceGrowthPhase
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
                    phaseGrowth = true; //  ставим в phaseGrowth значение тру
                                        //
                    string str = "Значение фазы роста = " + phaseGrowth.ToString() + "\n";
                    Debug.WriteLine(str);
                    minPriceGrowthPhase = marketPrice; // записываем значение цены в minPriceGrowthPhase
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
        private void IndicatorDSR(List<Candle> candles, BotTabSimple _tab)
        {
            if (candles.Count > candleBack.ValueInt + 1)
            {
                Aindicator _dsr = (Aindicator)_tab.Indicators[0];
                decimal _trendDSR = _dsr.DataSeries[0].Last; // последние значение индикатора DSR
                if (_trendDSR == 0) // если 0 тренд вниз - фаза роста закончилась
                {
                    phaseGrowth = false; //выключаем
                    _calculationGP = true; //  разрешаем считать заново
                }
            }
        }

        /// <summary>
        ///  трейлинг стоп лосс и профит
        /// </summary>
        private void TrelingStopLossAndProfit(BotTabSimple _tab)
        {
            List<Position> position = _tab.PositionsOpenAll;
            if (position.Count == 0)
            {
                return;
            }
            if (TestIsOnStopLoss.ValueBool == true)
            {
                decimal lastPrice = _tab.PriceBestAsk;
                decimal trailActiv = lastPrice - lastPrice * TrailStopLossLength.ValueDecimal / 100;
                decimal trailOrder = trailActiv - 5 * _tab.Securiti.PriceStep;
                _tab.CloseAtTrailingStop(position[0], trailActiv, trailOrder);
                //return; для того если включены одновременно и трейлигн стоп и трейлинг профит работает только первый
            }
            if (TestIsOnTrelingProfit.ValueBool == true)
            {
                // Трейлинг профит
                // При росте цены минимум на 3 % [Тейк профит] от точки входа,
                // а затем снижении на 1 % [Трейлинг профит] от "образовавшегося максимума",
                // позиция закрывается рыночным ордером.

                decimal OpenPrice = _tab.PositionsLast.EntryPrice;
                decimal priceProfit = OpenPrice + OpenPrice * ProfitLength.ValueDecimal / 100;

                if (OpenPrice != 0 && marketPrice > priceProfit)
                {
                    decimal trailActiv = marketPrice - OpenPrice * TrailProfitLength.ValueDecimal / 100;
                    decimal trailOrder = trailActiv - 5 * _tab.Securiti.PriceStep;
                    _tab.CloseAtTrailingStop(position[0], trailActiv, trailOrder);
                }
            }
        }

        /// <summary>
        ///  тэйк профит
        /// </summary>
        private void Profit(BotTabSimple _tab)
        {
            List<Position> position = _tab.PositionsOpenAll;

            if (TestIsOnProfit.ValueBool == true)
            {
                if (position.Count == 0)
                {
                    return;
                }
                else
                {
                    decimal lastPrice = _tab.PriceBestAsk;
                    decimal openPos = _tab.PositionsLast.EntryPrice;  // цена открытия позиции
                    decimal profit = openPos + lastPrice * ProfitLength.ValueDecimal / 100;
                    if (openPos != 0)
                    {
                        _tab.CloseAtProfit(position[0], profit, profit);
                    }
                }
            }
        }

        #endregion Логика

        #region сервис

        /// <summary>
        /// пользователь изменил настройки параметров
        /// </summary>
        private void _ParametrsChangeUser()
        {
            /*_dsr = IndicatorsFactory.CreateIndicatorByName("DSR", "DSR", false);
            _dsr.ParametersDigit[0].Value = Longterm.ValueInt;
            _dsr.ParametersDigit[1].Value = DSR1.ValueInt;
            _dsr.ParametersDigit[2].Value = DSR2.ValueInt;
            _dsr = (Aindicator)_tab.CreateCandleIndicator(_dsr, "Prime");
            _dsr.Save();
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
            }*/
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
            Window Uiss = new ScalperSettings(this);
            Uiss.Show();
        }

        // дальше реализация INotifyPropertyChanged
        /// <summary>
        /// обработчик события изменения свойств
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void СallUpdate(string name)  // сигнализирует об изменении свойств
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        ///  сверяет значения данных и выдает сигнал об изменении
        /// </summary>
        protected void Set<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (!field.Equals(value))
            {
                field = value;
                СallUpdate(name);
            }
        }

        #endregion сервис
    }
}