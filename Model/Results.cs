using System.Collections.ObjectModel;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nanopath.Service;

namespace Nanopath.Model
{
    #region PositionResult
    /// <summary>
    /// PositionResult
    /// Simple class representing a single Pos/Neg result
    /// Contains all relevant data
    /// </summary>
    public class PositionResult
    {
        public PositionResult(double samplePeak, double backgroundPeak, PosNeg result,
                              List<double> bg1RawData, List<double> sampleRawData, List<double> bg2RawData,
                              List<double> bg1CalcData, List<double> sampleCalcData, List<double> bg2CalcData)
        {
            SamplePeak = samplePeak;
            BackgroundPeak = backgroundPeak;
            Result = result;
            Bg1RawData = new List<double>(bg1RawData);
            SampleRawData = new List<double>(sampleRawData);
            Bg2RawData = new List<double>(bg2RawData);
            Bg1CalcData = new List<double>(bg1CalcData);
            SampleCalcData = new List<double>(sampleCalcData);
            Bg2CalcData = new List<double>(bg2CalcData);
        }
        public double SamplePeak { get; }
        public double BackgroundPeak { get; }
        public PosNeg Result { get; }
        public List<double> Bg1CalcData { get; }
        public List<double> Bg2CalcData { get; }
        public List<double> SampleCalcData { get; }
        public List<double> Bg1RawData{ get; }
        public List<double> Bg2RawData { get; }
        public List<double> SampleRawData { get; }
    } 
    #endregion

    /// <summary>
    /// Results Class
    /// Class to manage results for a test
    /// </summary>
    public class Results
    {
        #region Fields
        private readonly AppConfig _appCfg;                         // Application configuration
        private readonly string _sampleId;                          // Sample ID entered by user
        private readonly List<double> _calcWavelengthsNm;           // List of the waelengths used for calculations (based on the start and stop wavelength settings)
        private readonly List<double> _measWavelengthsNm;           // List of raw measurement wavelengths from the spectrometer
        private readonly int _calcWavelengthStartIndex = -1;        // Index of the firs caluclation wavelength in the measurement wavelength list
        private IDiagnosticsService _diag;                          // Diagnostics service for logging and reporting errors
        private List<double> _lightSourceRawData;          // Raw measurement data for the light source
        private Settings _testSettings;
        #endregion

        #region Constructor
        public Results(IDiagnosticsService diagService, AppConfig appCfg, Settings testSettings, string sampleId, List<double> measWavelengths, double wavelengthStartNm, double wavelengthEndNm)
        {
            _diag = diagService;
            _appCfg = appCfg;
            _testSettings = testSettings;
            _sampleId = sampleId;
            _measWavelengthsNm = new List<double>(measWavelengths);

            int calcEndWavelengthIndex = -1;

            //Find the calculation start and stop wavelength frequencies
            for (int i = 0; _measWavelengthsNm.Count >= 2 && i < _measWavelengthsNm.Count - 1; i++)
            {
                if (_measWavelengthsNm[i] == wavelengthStartNm || (_measWavelengthsNm[i] < wavelengthStartNm && _measWavelengthsNm[i + 1] > wavelengthStartNm)) _calcWavelengthStartIndex = i;
                if (_measWavelengthsNm[i] == wavelengthEndNm || (_measWavelengthsNm[i] < wavelengthEndNm && _measWavelengthsNm[i + 1] > wavelengthEndNm)) calcEndWavelengthIndex = i;
                if (_calcWavelengthStartIndex >= 0 && calcEndWavelengthIndex >= 0) break;
            }

            // Get the calculcation wavelengths we're concerned with
            if (_calcWavelengthStartIndex < 0 || calcEndWavelengthIndex < 0)
            {
                _diag?.AddTrace(TraceLevel.Error, $"Results constructor - wavelength range issue. Check device specifications.", true);
            }
            else
            {
                _calcWavelengthsNm = new List<double>(_measWavelengthsNm.GetRange(_calcWavelengthStartIndex, calcEndWavelengthIndex - _calcWavelengthStartIndex + 1));
            }
        }
        #endregion


        #region PositionResults Property
        /// <summary>
        /// PositionResults - A collection of the position results
        /// </summary>
        public ObservableCollection<PositionResult> PositionResults { get; private set; } = new ObservableCollection<PositionResult>();
        public void ClearResults()
        {
            PositionResults = new ObservableCollection<PositionResult>();
        }
        public bool CreateResult(List<double> bg1RawData, List<double> sampleRawData, List<double> bg2RawData)
        {
            // Create calculation data
            List<double> bg1Calc = bg1RawData.GetRange(_calcWavelengthStartIndex, _calcWavelengthsNm.Count);
            List<double> sampleCalc = sampleRawData.GetRange(_calcWavelengthStartIndex, _calcWavelengthsNm.Count);
            List<double> bg2Calc = bg2RawData.GetRange(_calcWavelengthStartIndex, _calcWavelengthsNm.Count);

            // Calculate the results
            CalculateResult(_calcWavelengthsNm, LightSourceCalcData, bg1Calc, sampleCalc, bg2Calc, out double samplePeak, out double bgPeak);

            PosNeg result = PosNeg.Positive;
            if (samplePeak <= bgPeak + _appCfg.PeakShiftMinimum) result = PosNeg.Negative; // NOTE: POS is only id sample is > than BG+1

            // Create calculation data
            PositionResults.Add(new PositionResult(samplePeak, bgPeak, result, bg1RawData, sampleRawData, bg2RawData, bg1Calc, sampleCalc, bg2Calc));

            return result == PosNeg.Positive;
        }
        #endregion

        #region LightSource Property
        /// <summary>
        /// LightSourceRawData Property
        /// Sets the raw light source measured data, and sets the light source calculation data based on the desired start and end wavelengths
        /// </summary>
        public List<double> LightSourceRawData
        {
            get => _lightSourceRawData;
            set
            {
                _lightSourceRawData = new List<double>(value);
                LightSourceCalcData = value.GetRange(_calcWavelengthStartIndex, _calcWavelengthsNm.Count);
            }
        }
        /// <summary>
        /// LightSourceCalcData Property
        /// The calculation data for the light source measurement
        /// </summary>
        public List<double> LightSourceCalcData { get; private set; }  
        #endregion

        #region SaveFile Method
        /// <summary>
        /// SaveFile Method
        /// Saves the results to a CSV file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public FileStatus SaveFile(string path, Settings settings)
        {
            // Check the path for validity
            if (string.IsNullOrEmpty(path) || path.IndexOfAny(Path.GetInvalidPathChars()) >= 0) return FileStatus.InvalidPath;

            // Create the CSV string to write
            string toWrite = ToCsvString(settings);

            // Attempt to write the CSV file to the path provided
            try
            {
                File.WriteAllText(path, toWrite);
                //while(!File.Exists(path)) Task.Delay(100);
                //while(Utilities.IsFileLocked(path)) Task.Delay(100);
            }
            catch (Exception)
            {
                return FileStatus.Exception;
            }

            // If we've made it here, it's a success
            return FileStatus.Success;
        }
        #endregion

        #region ToCsvString Method
        /// <summary>
        /// ToCsvString Method - returns a CSV-formatted string of the test results for saving to a file
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public string ToCsvString(Settings settings)
        {
            StringBuilder csvBuilder = new StringBuilder();

            // Format results into a string for CSV file write
            csvBuilder.Append($"Machine ID,{_appCfg.MachineId}" + Environment.NewLine +
                              $"Sample ID,{_sampleId}" + Environment.NewLine +
                              $"Peak Shift Minimum,{_appCfg.PeakShiftMinimum}" + Environment.NewLine + Environment.NewLine +
                              $"Results" + Environment.NewLine +
                              $"#,Result,Sample Peak,BG Avg Peak" + Environment.NewLine);

            foreach (PositionResult position in PositionResults)
            {
                string posNeg = position.Result == PosNeg.Positive ? "POS" : "NEG";
                csvBuilder.Append($"{PositionResults.IndexOf(position) + 1},{posNeg},{position.SamplePeak},{position.BackgroundPeak}" + Environment.NewLine);
            }

            // Add all the raw data

            // Add the wavelengths
            csvBuilder.Append(Environment.NewLine + "Raw Data / Wavelength");
            foreach (double wavelength in _measWavelengthsNm)
            {
                csvBuilder.Append($",{wavelength:F4}");
            }

            // Add the Light Source Background Data
            csvBuilder.Append(Environment.NewLine + "Light Source");
            foreach (double datapoint in LightSourceRawData)
            {
                csvBuilder.Append($",{datapoint:F3}");
            }

            // Add each scan data
            foreach (PositionResult position in PositionResults)
            {
                csvBuilder.Append(Environment.NewLine + $"{PositionResults.IndexOf(position) + 1} BG1");
                foreach (double datapoint in position.Bg1RawData)
                {
                    csvBuilder.Append($",{datapoint:F3}");
                }
                csvBuilder.Append(Environment.NewLine + $"{PositionResults.IndexOf(position) + 1} Sample");
                foreach (double datapoint in position.SampleRawData)
                {
                    csvBuilder.Append($",{datapoint:F3}");
                }
                csvBuilder.Append(Environment.NewLine + $"{PositionResults.IndexOf(position) + 1} BG2");
                foreach (double datapoint in position.Bg2RawData)
                {
                    csvBuilder.Append($",{datapoint:F3}");
                }
            }

            // Tack on the settings that were used to perform the test
            csvBuilder.Append(Environment.NewLine + Environment.NewLine + settings.ToCsvString());

            return csvBuilder.ToString();
        }
        #endregion

        #region CalculateResult Method
        /// <summary>
        /// CalculateResult Method
        /// Calculates the result for a given location measurement data
        /// Ported from Matlab prototype code provided by Nanopath
        /// </summary>
        /// <param name="wavelengths"></param>
        /// <param name="lightSourceData"></param>
        /// <param name="bg1Data"></param> 
        /// <param name="sampleData"></param>
        /// <param name="bg2Data"></param>
        /// <param name="samplePeak"></param>
        /// <param name="bgPeak"></param>
        private void CalculateResult(List<double> wavelengths, List<double> lightSourceData, List<double> bg1Data, List<double> sampleData,
                                     List<double> bg2Data, out double samplePeak, out double bgPeak)
        {
            samplePeak = 0;
            bgPeak = 0;

            double mass = 0;    // mass = 0; % initialize variables to 0
            double wMass = 0;   // wmass = 0;
            //center_of_mass = 0;

            double bgMass = 0;  //bmass = 0; % initialize variables to 0
            double bgWmass = 0; //bwmass = 0;
            //b_center_of_mass = 0;

            // smoothing_percent = 0.05 % set by user in settings file(screen 8e) or direct input(screen 8c)

            // background = (B1A + B1B) / 2; % calculate average background
            List<double> bgAvg = bg1Data.Zip(bg2Data, (a, b) => (a + b) / 2).ToList();
            // % glass background is collected, termed GB1
            // bspectrum = background / GB1; % correct the background spectrum
            List<double> bgSpectrum = bgAvg.Zip(lightSourceData, (a, b) => a / b).ToList();
            // spectrum = M1 / GB1; % correct the signal spectrum
            List<double> spectrum = sampleData.Zip(lightSourceData, (a, b) => a / b).ToList();

            // Cleanup any anomalies in the data
            bgSpectrum = bgSpectrum.ToArray().Select(x => double.IsPositiveInfinity(x) ? double.MaxValue : x).ToList();
            bgSpectrum = bgSpectrum.ToArray().Select(x => double.IsNegativeInfinity(x) ? double.MinValue : x).ToList();
            //bgSpectrum = bgSpectrum.ToArray().Select(x => double.IsNaN(x)?0.0:x).ToList();

            spectrum = spectrum.ToArray().Select(x => double.IsPositiveInfinity(x) ? double.MaxValue : x).ToList();
            spectrum = spectrum.ToArray().Select(x => double.IsNegativeInfinity(x) ? double.MinValue : x).ToList();
            //spectrum = spectrum.ToArray().Select(x => double.IsNaN(x) ? 0.0 : x).ToList();

            // bspectra1 = smooth(bspectrum, smoothing_percent, 'loess'); % smooth background spectrum slightly to remove noise, ”bspectrum" is the background corrected data
            // spectra1 = smooth(spectrum, smoothing_percent, 'loess'); % smooth signal spectrum slightly to remove noise, "spectrum" is the background corrected data
            LoessInterpolator loess = new LoessInterpolator(_testSettings.SmoothingPercent / 100.0, 0);
            List<double> spectra1 = new List<double>();
            List<double> bgSpectra1 = new List<double>();
            try
            {
                bgSpectra1 = loess.smooth(wavelengths.ToArray(), bgSpectrum.ToArray()).ToList();
                spectra1 = loess.smooth(wavelengths.ToArray(), spectrum.ToArray()).ToList();
            }
            catch (Exception e)
            {
                _diag.AddTrace(TraceLevel.Error, $"Algorithm - Failed to perform loess smoothing. {e.Message}", true);
                return;
            }

            //for x = lb:ub
            //    bmass = bmass + bspectra1(x); % calculate numerical integration
            //    bwmass = wmass + wv(x) * bspectra1(x); % calculate numerical weighted integration
            //end
            //for x = lb:ub
            //    mass = mass + spectra1(x); % calculate numerical integration
            //    wmass = wmass + wv(x) * spectra1(x); % calculate numerical weighted integration
            //end
            for (int i = 0; i < wavelengths.Count; i++)
            {
                bgMass += bgSpectra1[i];                    // calculate numerical integration
                bgWmass += wavelengths[i] * bgSpectra1[i];  // calculate numerical weighted integration

                mass += spectra1[i];                        // Repeat for sample
                wMass += wavelengths[i] * spectra1[i];
            }

            //    b_center_of_mass = bwmass / bmass; % calculate background centroid PEAK VALUE
            //    center_of_mass = wmass / mass; % calculate SIGNAL centroid PEAK VALUE
            bgPeak = bgWmass / bgMass;  // calculate centroid PEAK VALUE 
            samplePeak = wMass / mass;

            // Simulation
            //avgBgPeak = new Random().Next(7000, 8000) / 10.0;
            //samplePeak = avgBgPeak + (new Random().Next(-30, 30) / 10.0);
        }
        #endregion
    }
}
