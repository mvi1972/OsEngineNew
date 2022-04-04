using System.Collections.Generic;
using System.Drawing;
using OsEngine.Entity;
using OsEngine.Indicators;

namespace CustomIndicators.Scripts
{
    internal class DSR : Aindicator
    {
        private IndicatorParameterInt _lengthLongterm;
        private IndicatorParameterInt _lengthDSR1;
        private IndicatorParameterInt _lengthDSR2;

        private Aindicator _longTermEMA;
        private Aindicator _dsr1EMA;
        private Aindicator _dsr2EMA;

        private IndicatorDataSeries _seriesTrend;

        public override void OnStateChange(IndicatorState state)
        {
            if (state == IndicatorState.Configure)
            {
                _lengthLongterm = CreateParameterInt("Longterm length", 9);
                _lengthDSR1 = CreateParameterInt("DSR1 length", 7);
                _lengthDSR2 = CreateParameterInt("DSR2 length", 1);

                _seriesTrend = CreateSeries("TrendDirection", Color.Yellow, IndicatorChartPaintType.Point, true);

                _longTermEMA = IndicatorsFactory.CreateIndicatorByName("Ema", Name + "EmaLongterm", false);
                ((IndicatorParameterInt)_longTermEMA.Parameters[0]).Bind(_lengthLongterm);
                ProcessIndicator("Longterm EMA", _longTermEMA);

                _dsr1EMA = IndicatorsFactory.CreateIndicatorByName("Ema", Name + "EmaDSR1", false);
                ((IndicatorParameterInt)_dsr1EMA.Parameters[0]).Bind(_lengthDSR1);
                ProcessIndicator("DSR1 EMA", _dsr1EMA);

                _dsr2EMA = IndicatorsFactory.CreateIndicatorByName("Ema", Name + "EmaDSR2", false);
                ((IndicatorParameterInt)_dsr2EMA.Parameters[0]).Bind(_lengthDSR2);
                ProcessIndicator("DSR2 EMA", _dsr2EMA);
            }
        }

        public override void OnProcess(List<Candle> candles, int index)
        {
            decimal Longterm = _longTermEMA.DataSeries[0].Values[index];
            decimal DSR1 = _dsr1EMA.DataSeries[0].Values[index];
            decimal DSR2 = _dsr2EMA.DataSeries[0].Values[index];

            if (Longterm > DSR1 && DSR1 > DSR2)
            {
                //bearTrend = ema200 > ema50 and ema50 > ema20
                // DownTrend = _longTermEMA > _dsr1EMA and _dsr1EMA > _dsr2EMA  // Bearish trend

                _seriesTrend.Values[index] = 0;
            }
            if (Longterm < DSR1 && DSR1 < DSR2)
            {
                //bullTrend = ema200 < ema50 and ema50<ema20
                // UpTrend = _longTermEMA < _dsr1EMA and _dsr1EMA < _dsr2EMA // Bullish trend

                _seriesTrend.Values[index] = 1;
            }
        }
    }
}