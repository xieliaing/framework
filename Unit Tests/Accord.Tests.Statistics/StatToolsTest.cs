// Accord Unit Tests
// The Accord.NET Framework
// http://accord-framework.net
//
// Copyright Liang Xie 2017
// xie1978 at hotmail dot com
// Copyright © César Souza, 2009-2017
// cesarsouza at gmail.com
//
//    This library is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 2.1 of the License, or (at your option) any later version.
//
//    This library is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public
//    License along with this library; if not, write to the Free Software
//    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
namespace Accord.Tests.Statistics
{
    using Accord.Statistics.TimeSeries;
    using NUnit.Framework;
    using Accord.Statistics;
    using Accord.Math;
    using System;

    [TestFixture]
    public class StatToolsTest
    {
        [Test]
        public void acfTest()
        {
            // expectedACF is ACF results from Python StatsModels ACF function applied to Sin(X) where X=0, 1, .., 99
            /*
             Python 3.5.2 |Anaconda 4.2.0 (64-bit)| (default, Jul  5 2016, 11:41:13) [MSC v.1900 64 bit (AMD64)] on win32
             Type "help", "copyright", "credits" or "license" for more information.
             >>> import statsmodels.api as sm
             c:\Anaconda3\lib\site-packages\statsmodels\compat\pandas.py:56: FutureWarning: The pandas.core.datetools module is deprecated and will be removed in a future version. Please use the pandas.tseries module instead.
               from pandas.core import datetools
             >>> from statsmodels.tsa.stattools import acf
             >>> import numpy as np             
             >>> x = np.sin(np.linspace(0, 99, 100))
             >>> y = acf(x, nlags=9)
             >>> print(y)
             [ 1.          0.53515447 -0.4075514  -0.96025719 -0.62773328  0.2691908
               0.90248604  0.70133678 -0.13356576 -0.82902385 -0.75534937]
             >>>
             */
            double[] expectedACF = new double[] { 1.0, 0.53515447, -0.4075514, -0.96025719, -0.62773328, 0.2691908, 0.90248604, 0.70133678, -0.13356576, -0.82902385 };
            int windowSize = expectedACF.Length;
            int SeriesSize = 100;
            double[] values = new double[SeriesSize];
            for (int i = 0; i < SeriesSize; i++)
            {
                values[i] = Math.Sin(i);
            }

            double[] computedACF = Accord.Statistics.TimeSeries.TimeSeriesTools.AutoCorrelationFunction(values, windowSize);

            Assert.AreEqual(expectedACF, computedACF);
        }

        [Test]
        public void q_statTest()
        {
            // q_stat and pvalues from a given acf series from sin(X), where X=1, ..., 100
            /*
             >>>import numpy as np
             >>> from statsmodels.tsa.stattools import acf, q_stat
             >>> x0 = np.linspace(0, 99, 100)
             >>> s = np.sin(x0)
             >>> c = np.cos(x0)
             >>> y = acf( (s+c), nlags=9)
             >>> print(y)
             [ 1.          0.53515447 -0.4075514  -0.96025719 -0.62773328  0.2691908
               0.90248604  0.70133678 -0.13356576 -0.82902385 -0.75534937]
             >>> q, p = q_stat(y, nobs=12)
             >>> print(q)
             [  15.27272727   20.08408434   23.184583     42.5485544    52.00573205
                54.03471526   81.40127849  102.05995627  103.0589857   160.79055181
                256.64340022]
             >>> print(p)
             [  9.30503358e-05   4.35307850e-05   3.69589037e-05   1.28381494e-08
                5.38019559e-10   7.25918604e-10   7.13163734e-15   1.61829964e-18
                3.78001774e-18   2.22515629e-29   1.13361455e-48]
             >>>
             */
            double[] expectedQStatistic = new double[] { 15.27272727, 20.08408434, 23.184583, 42.5485544 , 52.00573205, 54.03471526, 81.40127849, 102.05995627, 103.0589857,   160.79055181, 256.64340022 };
            double[] expectedPValue = new double[] {9.30503358e-05, 4.35307850e-05, 3.69589037e-05, 1.28381494e-08, 5.38019559e-10, 7.25918604e-10, 7.13163734e-15, 1.61829964e-18, 3.78001774e-18, 2.22515629e-29, 1.13361455e-48 };

            int SeriesSize = 100, nlags = 9, nobs = 12;

            double[] values = new double[SeriesSize];
            for (int i = 0; i < SeriesSize; i++)
            {
                values[i] = Math.Sin(i) + Math.Cos(i);
            }
            double[] computedACF = Accord.Statistics.TimeSeries.TimeSeriesTools.AutoCorrelationFunction(values, nlags);
            TimeSeriesTools.QStatResult qstat = Accord.Statistics.TimeSeries.TimeSeriesTools.q_stat(computedACF, nobs);

            double[] computedQStatistic = qstat.qstat;
            double[] computedPValue = qstat.pvalue;

            Assert.AreEqual(expectedQStatistic, computedQStatistic);
            Assert.AreEqual(expectedPValue, computedPValue);
        }
    }
}