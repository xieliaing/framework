// Accord Statistics Library
// The Accord.NET Framework
// http://accord-framework.net
//
// Copyright © César Souza, 2009-2017
// cesarsouza at gmail.com
//
// Copyright © Liang Xie 2017
// xie1978 at hotmail dot com
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

namespace Accord.Statistics.TimeSeries
{
    using Accord.Math;
    using System;
    using System.Numerics;
    using Accord.Math.Transforms;
    using Accord.Compat;
    using Accord.Statistics;

    /// <summary>
    ///   Static tools for time series analysis (e.g. <see cref="AutoCorrelationFunction(double[], int)">ACF</see>,
    ///   <see cref="Periodogram(double[])">periodograms</see>, and others).
    /// </summary>
    /// 
    public static class TimeSeriesTools
    {
        /// <summary>
        ///   Calculates the auto correlation function using Wiener–Khinchin theorem.
        /// </summary>
        /// 
        /// <param name="vector">
        ///   A vector of observations whose AutoCorrelation Function will be calculated. 
        ///   It is assumed that the vector is a 1-D array of type double.
        /// </param>
        /// <param name="nlag">
        ///   Number of lags to return autocorrelation for. Integer.
        /// </param>
        /// 
        /// <returns>
        ///   Returns a vector of type double giving the autocorrelation function upto given lags.
        /// </returns>
        /// 
        public static double[] AutoCorrelationFunction(double[] vector, int nlag)
        {
            int nTime = vector.Length;

            if (nlag < 1)
                nlag = nTime;

            if (nTime <= 1)
                return new double[] { };

            // padding the length to be the power of 2 to facilitate FFT speed.
            int newLength = Convert.ToInt32(Math.Pow(2, Math.Ceiling(Math.Log(nTime, 2))));

            // define and calculate FFT
            double VecMean = vector.Mean();
            Complex[] Frf = new Complex[newLength];
            for (int k = 0; k < newLength; k++)
            {
                if (k < nTime)
                    Frf[k] = new Complex(vector[k] - VecMean, 0);
            }

            FourierTransform2.FFT(Frf, Accord.Math.FourierTransform.Direction.Forward);

            // calculate inverse(backward) FFT of ( Frf*Conjugate(Frf) )
            Complex[] iFTFTj = new Complex[Frf.Length];
            for (int k = 0; k < Frf.Length; k++)
            {
                Complex FrfConj = Complex.Conjugate(Frf[k]);
                double RealPart = Frf[k].Real * FrfConj.Real - Frf[k].Imaginary * FrfConj.Imaginary;
                double ImaginaryPart = Frf[k].Real * FrfConj.Imaginary + Frf[k].Imaginary * FrfConj.Real;
                iFTFTj[k] = new Complex(RealPart, ImaginaryPart);
            }

            FourierTransform2.FFT(iFTFTj, Accord.Math.FourierTransform.Direction.Backward);

            // calculate ACF, normalized against the first item

            double normalizer = 1.0;
            int newlag = nTime < nlag ? nTime : nlag;

            double[] acf = new double[newlag];

            for (int k = 0; k < acf.Length; k++)
            {
                acf[k] = iFTFTj[k].Real / (nTime * normalizer);

                if (k == 0)
                {
                    normalizer = acf[0];
                    acf[0] = 1.0;
                }
            }

            return acf;
        }


        /// <summary>
        ///   Calculates Ljung-Box Q Statistic. Based on Python StatsModels q_stat function.
        /// </summary>
        /// 
        /// <param name="x">
        ///   Array of autocorrelation coefficients. It is assumed that
        ///   the vector is a 1-D array of type double
        /// </param>
        /// <param name="nobs">
        ///    Number of observations in the entire sample (ie., not just the length
        ///    of the autocorrelation function results.
        /// </param>
        /// <param name="type">
        ///     
        /// </param>
        /// <returns>
        ///   Returns a tuple of q-stat and p-value:
        ///   q-stat: array of double, Ljung-Box Q-statistic for autocorrelation parameters
        ///   p-value : array of double of the Q-statitic
        /// </returns>

        public struct QStatResult
        {
            public double[] qstat;
            public double[] pvalue;
        }
        public static QStatResult q_stat(double[] x, int nobs, string type = "ljungbox")
        {
            if (nobs <= 0)
                throw new ArgumentOutOfRangeException("nobs", "Num Obs must be positive.");

            int xlength = x.Length;
            int xlength2 = xlength + 1;
            double[] ret = new double[xlength];

            if (type == "ljungbox")
            {

                double[] cumsum = new double[xlength];
                for (int i = 1; i <= (xlength2); i++)
                {
                    cumsum[i - 1] = (1 / (nobs - i)) * (x[i - 1] * x[i - 1]);
                }
                double[] csum = Accord.Math.Matrix.CumulativeSum(cumsum);

                for (int i = 0; i < xlength; i++)
                {
                    ret[i] = nobs * (nobs + 2) * cumsum[i];
                }
            }

            double[] chi2sf = new double[xlength];
            var ChiSq = new Accord.Statistics.Distributions.Univariate.ChiSquareDistribution(degreesOfFreedom: xlength2);
            for (int i = 0; i < xlength; i++)
            {
                chi2sf[i] = ChiSq.ComplementaryDistributionFunction(x: ret[i]);
            }

            var result = new QStatResult
            {
                qstat = ret,
                pvalue = chi2sf
            };
            return result;
        }
    


    /// <summary>
    ///   Calculates Periodogram of a given vector of type double based on Welch method.
    /// </summary>
    /// 
    /// <param name="vector">
    ///   A vector of observations whose Periodogram will be calculated. It is assumed that
    ///   the vector is a 1-D array of type double
    /// </param> 
    /// 
    /// <returns>
    ///   Returns a vector of type double giving the periodogram of the vector.
    /// </returns>

    public static double[] Periodogram(double[] vector)
        {
            int nTime = vector.Length;
            double[] pwr = new double[vector.Length];

            if (nTime == 1)
            {
                pwr[0] = 1.0;
                return pwr;
            }

            Complex[] vectorComplex = new Complex[vector.Length];
            for (int k = 0; k < vector.Length; k++)
                vectorComplex[k] = new Complex(vector[k], 0);

            FourierTransform2.FFT(vectorComplex, FourierTransform.Direction.Forward);
            pwr = FourierTransform2.GetPowerSpectrum(vectorComplex);
            return pwr;
        }

    }
}