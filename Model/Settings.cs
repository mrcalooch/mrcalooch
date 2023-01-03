using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Nanopath.Model
{
    #region Location Class
    /// <summary>
    /// Location Class
    /// Very simple class to represent a single sample/BG location pair
    /// </summary>
    public class Location
    {
        public Location(double? sampleMm, double? backgroundMm)
        {
            SampleMm = sampleMm;
            BackgroundMm = backgroundMm;
        }
        public double? SampleMm { get; set; }
        public double? BackgroundMm { get; set; }
    }
    #endregion

    public class Settings /*: GalaSoft.MvvmLight.ViewModelBase*/
    {
        private const int _numCsvLines = 22;
        private const int _numCsvCols = 3;

        public ObservableCollection<Location> TestLocations { get; set; } = new ObservableCollection<Location> {new Location(6.0,6.1), new Location(null, null), new Location(null, null), new Location(null, null), new Location(null, null), new Location(null, null), new Location(null, null), new Location(null, null), new Location(null, null), new Location(null, null) };
        
        #region Properties
        public double LightSourceMm { get; set; } = 0.0;
        public double IntegrationTimeMs { get; set; } = 0.05;   // Default Values
        public int WavelengthStartNm { get; set; } = 500;
        public int WavelengthEndNm { get; set; } = 1000;
        public int SmoothingPercent { get; set; } = 10;
        public double MinIntegrationMs { get; } = 0.05;         // Default limits
        public double MaxIntegrationMs { get; } = 500.0;
        public int MinSmoothingPercent { get; } = 0;
        public int MaxSmoothingPercent { get; } = 50;
        public int MinWavelengthNm { get; } = 500;
        public int MaxWavelengthNm { get; } = 1000;
        public double MinLightSourceMm { get; } = 0;
        public double MaxLightSourceMm { get; } = 99.99;
        #endregion

        #region LoadFile Method
        /// <summary>
        /// LoadFile Method
        /// Reads the CSV data from the file path specified and populates the settings
        /// NOTE: Always assumes the file contains exactly 10 locations
        /// This is fixed implementation to read 10 locations, future growth would require modifying the algorithm to determine the number of locations
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public FileStatus LoadFile(string path)
        {
            // Check the path for validity
            if (string.IsNullOrEmpty(path) || (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0) || !File.Exists(path)) return FileStatus.InvalidPath;

            // Attempt to read the file
            string[,] csvData;
            try
            {
                // Validate the number of lines in the file
                string[] lines = File.ReadAllLines(path);
                if (lines.Length != _numCsvLines) return FileStatus.InvalidFormat;

                // Validate the number of columns in the file
                string[] columns = lines[0].Split(',');
                if (columns.Length != _numCsvCols) return FileStatus.InvalidFormat;

                csvData = new string[lines.Length, columns.Length];
                int row = 0;
                foreach (string line in lines)
                {
                    int col = 0;
                    columns = line.Split(',');
                    foreach (string value in columns)
                    {
                        csvData[row, col++] = value;
                    }
                    row++;
                }
            }
            catch (Exception)
            {
                return FileStatus.Exception;
            }

            bool rangeBreach = false;
            try
            {
                bool coerced;

                // Grab the Light Source 
                double? lightSourceMm = Utilities.StringToLocation(csvData[0, 1], out coerced);
                rangeBreach |= coerced;
                if (lightSourceMm.HasValue) LightSourceMm = lightSourceMm.Value;
                else LightSourceMm = 0.0;

                // Grab the positions from the file
                TestLocations = new ObservableCollection<Location>(); // In order for any binding to update, we need to make a new collection
                for (int i = 0; i < 10; i++)
                {
                    double? samp = Utilities.StringToLocation(csvData[i + 4, 1], out coerced);
                    rangeBreach |= coerced;
                    double? bg = Utilities.StringToLocation(csvData[i + 4, 2], out coerced);
                    rangeBreach |= coerced;
                    // Index the csvData where the locations exists (starting at line 3 or i + 2)
                    TestLocations.Add(new Location(samp, bg));
                }

                // Get the rest of the settings
                IntegrationTimeMs = double.Parse(csvData[16, 1]).CoerceToLimits(MinIntegrationMs, MaxIntegrationMs, out coerced);
                rangeBreach |= coerced;
                SmoothingPercent = int.Parse(csvData[19, 1]).CoerceToLimits(MinSmoothingPercent, MaxSmoothingPercent, out coerced);
                rangeBreach |= coerced;
                WavelengthStartNm = int.Parse(csvData[20, 1]).CoerceToLimits(MinWavelengthNm, MaxWavelengthNm, out coerced);
                rangeBreach |= coerced;
                WavelengthEndNm = int.Parse(csvData[21, 1]).CoerceToLimits(MinWavelengthNm, MaxWavelengthNm, out coerced);
                rangeBreach |= coerced;

                // Fix any start/end issues with the wavelength range
                if (WavelengthStartNm == WavelengthEndNm)
                {
                    if (WavelengthStartNm > WavelengthEndNm) WavelengthStartNm--;
                    else WavelengthEndNm++;
                    rangeBreach = true;
                }
                else if (WavelengthStartNm > WavelengthEndNm)
                {
                    int temp = WavelengthStartNm;
                    WavelengthStartNm = WavelengthEndNm;
                    WavelengthEndNm = temp;
                    rangeBreach = true;
                }
            }
            catch (Exception)
            {
                return FileStatus.InvalidFormat;
            }
            
            // Check for overall range breach, and if so return a warning
            if (rangeBreach) return FileStatus.DataRangeWarning;

            // If we've made it here, it's a success
            return FileStatus.Success;
        } 
        #endregion

        #region SaveFile Method
        /// <summary>
        /// SaveFile Method
        /// Writes a CSV formatted file at the provided path
        /// Performs basic error checking and handling, returning a simple status for UI reporting
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public FileStatus SaveFile(string path)
        {
            // Check the path for validity
            if ((!string.IsNullOrEmpty(path) && path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)) return FileStatus.InvalidPath;

            // Create the CSV string to write
            string toWrite = ToCsvString();

            // Attempt to write the CSV file to the path provided
            try
            {
                File.WriteAllText(path, toWrite);
            }
            catch (Exception)
            {
                return FileStatus.Exception;
            }

            // If we've made it here, it's a success
            return FileStatus.Success;
        } 
        #endregion

        #region ToString Override
        /// <summary>
        /// ToString Override
        /// Creates a formatted multi-line string fromm the setting values
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            int numLocs = TestLocations.Count(f => f.SampleMm.HasValue && f.BackgroundMm.HasValue);
            string summary = $"Locations: {numLocs}" + Environment.NewLine +
                             $"Integration Time: {IntegrationTimeMs}ms" + Environment.NewLine +
                             $"Range: {WavelengthStartNm}nm - {WavelengthEndNm}nm" + Environment.NewLine +
                             $"Smoothing: {SmoothingPercent}%";
            return summary;
        } 
        #endregion

        #region ToCsvString Method
        /// <summary>
        /// ToCsvString Method
        /// Creates a CSV style string containing all of the settings data
        /// </summary>
        /// <returns></returns>
        public string ToCsvString()
        {
            // Format settings into a string for settings and results CSV file write
            string csv = $"Light Source Location (mm),{LightSourceMm}," + Environment.NewLine + Environment.NewLine + 
                         $"Measurement Locations,," + Environment.NewLine + $"#,Sample (mm),Background (mm)" + Environment.NewLine;

            foreach (Location loc in TestLocations)
            {
                string sampString = loc.SampleMm.HasValue ? loc.SampleMm.ToString() : "X";
                string bgString = loc.SampleMm.HasValue ? loc.BackgroundMm.ToString() : "X";
                csv += $"{TestLocations.IndexOf(loc) + 1},{sampString},{bgString}" + Environment.NewLine;
            }

            csv += Environment.NewLine + "Spectrometer Settings,," + Environment.NewLine;
            csv += $"Integration Time (ms),{IntegrationTimeMs},{MinIntegrationMs}-{MaxIntegrationMs}" + Environment.NewLine;

            csv += Environment.NewLine + "Algorithm Settings,," + Environment.NewLine;
            csv += $"Smoothing (%),{SmoothingPercent},{MinSmoothingPercent}-{MaxSmoothingPercent}" + Environment.NewLine;
            csv += $"Wavelength Minimum (nm),{WavelengthStartNm},{MinWavelengthNm}-{MaxWavelengthNm}" + Environment.NewLine;
            csv += $"Wavelength Maximum (nm),{WavelengthEndNm},{MinWavelengthNm}-{MaxWavelengthNm}";

            return csv;
        } 
        #endregion
    }
}
