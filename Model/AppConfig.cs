using System;
using System.IO;
using System.Configuration;
using System.Text.RegularExpressions;

namespace Nanopath.Model
{
    /// <summary>
    /// AppConfig Class
    /// A simple class to represent the application configuration
    /// The values are populated from the application's configuration file.
    /// </summary>
    public class AppConfig
    {
        #region Fields
        private const string _defaultMachineId = "Machine01";
        private readonly string _defaultResultsPath;
        private const double _defaultPeakShift = 1.0;
        //private const string _defaultStageCom = "COM1";
        private const double _defaultStageLoadMm = 100.0;     // ToDo: Find a good value for this (ask Customer after initial testing)
        private const double _defaultStageSpeedMmSec = 60.0;
        private const string _defaultSpectrometerSn = "00642109";
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// Sets the private read-only fields programmatically.
        /// </summary>
        public AppConfig()
        {
            _defaultResultsPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Nanopath\Results";
        }
        #endregion

        public string MachineId { get; private set; } = _defaultMachineId;
        
        #region ResultPath Property
        private string _resultPath;
        public string ResultPath
        {
            get => _resultPath;
            private set
            {
                if (value.IndexOfAny(Path.GetInvalidPathChars()) < 0)
                {
                    if (!Directory.Exists(value)) Directory.CreateDirectory(value); // Create the path if it does not already exist
                    _resultPath = value;
                }
            }
        }
        #endregion

        public double PeakShiftMinimum { get; private set; } = _defaultPeakShift;

        #region LastSettingsFile Property
        private string _lastSettingsFile = null;
        public string LastSettingsFile
        {
            get => _lastSettingsFile;
            set
            {
                if (value.IndexOfAny(Path.GetInvalidPathChars()) < 0  && File.Exists(value))
                {
                    _lastSettingsFile = value;
                }
            }
        }
        #endregion

        #region Stage Settings

        #region StageComPort Property
        private string _stageComPort = string.Empty;
        public string StageComPort
        {
            get => _stageComPort;
            private set
            {
                // Validate that string contains a valid com port
                string match = Regex.Match(value.Trim().ToUpper(), @"^COM[1-9]{1}[0-9]*").Value;
                if (match.Length > 0)
                {
                    _stageComPort = match;
                }
            }
        }
        #endregion
        public double StageLoadPositionMm { get; private set; } = _defaultStageLoadMm;
        public double StageSpeedMmSec { get; private set; } = _defaultStageSpeedMmSec;

        #endregion

        #region Spectrometer Settings

        public string SpectrometerSerialNo { get; private set; } = string.Empty; 

        #endregion

        #region LoadFromFile
        /// <summary>
        /// LoadFromFile
        /// Loads the application settings from the app.config file
        /// </summary>
        public FileStatus LoadFromFile()
        {
            bool rangeBreach = false;
            try
            {
                // Get the Machine ID
                MachineId = ConfigurationManager.AppSettings.Get("MachineId").Trim();
                if (MachineId.Length == 0) // Validate the Machine ID
                {
                    MachineId = _defaultMachineId;
                    rangeBreach = true;
                }

                // Get the Result Path
                string strResultPath = ConfigurationManager.AppSettings.Get("ResultsPath").Trim();
                // Validate the path
                if (strResultPath.Length == 0 || (strResultPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)) // Check for an invalid path
                {
                    ResultPath = _defaultResultsPath;
                    rangeBreach = true;
                }
                else ResultPath = strResultPath;

                // Get the Peak Shift Minimum limit (do not allow negative numbers)
                PeakShiftMinimum = double.Parse(ConfigurationManager.AppSettings.Get("PeakShiftMinimum")).CoerceToLimits(0, double.MaxValue, out var coerced);
                rangeBreach |= coerced;

                // Get the last saved settings file (Property - it's settable by the app)
                // LastSettingsFile = Properties.Settings.Default.LastSettingsFile;
                LastSettingsFile = ConfigurationManager.AppSettings.Get("LastSettingsFile").Trim();
                rangeBreach |= coerced;

                // Get the Stage COM port (COM port will be "discovered")
                //StageComPort = ConfigurationManager.AppSettings.Get("StageComPort").Trim();
                //if (string.IsNullOrEmpty(StageComPort)) // Validate the COM port
                //{
                //    StageComPort = _defaultStageCom;
                //    rangeBreach = true;
                //}

                // Get the Stage Load position in mm (do not allow negative numbers)
                StageLoadPositionMm = double.Parse(ConfigurationManager.AppSettings.Get("StageLoadPositionMm")).CoerceToLimits(0, double.MaxValue, out coerced);
                rangeBreach |= coerced;

                // Get the Stage Load position in mm (do not allow negative numbers)
                StageSpeedMmSec = double.Parse(ConfigurationManager.AppSettings.Get("StageSpeedMmSec")).CoerceToLimits(1, 104.0, out coerced);
                rangeBreach |= coerced;

                // Get the Spectrometer serial number (for connection)
                SpectrometerSerialNo = ConfigurationManager.AppSettings.Get("SpectrometerSerialNo").Trim();
                if (SpectrometerSerialNo.Length == 0) // Validate the Machine ID
                {
                    SpectrometerSerialNo = _defaultSpectrometerSn;
                    rangeBreach = true;
                }
            }
            catch (Exception)
            {
                MachineId = _defaultMachineId;
                ResultPath = _defaultResultsPath;
                PeakShiftMinimum = _defaultPeakShift;
                LastSettingsFile = string.Empty;
                //StageComPort = _defaultStageCom;
                StageLoadPositionMm = _defaultStageLoadMm;
                SpectrometerSerialNo = _defaultSpectrometerSn;

                return FileStatus.Exception;
            }

            // Check for overall range breach, and if so return a warning
            if (rangeBreach) return FileStatus.DataRangeWarning;

            // If we've made it here, it's a success
            return FileStatus.Success;
        }
        #endregion

        #region SaveToFile Method
        /// <summary>
        /// SaveToFile Method
        /// Saves the latest info to the app.config file, currently the only modified value is the "Last Settings File"
        /// </summary>
        /// <returns></returns>
        public FileStatus SaveToFile()
        {
            try
            {
                // Properties methodology (stores in user local data, so it's a bit disconnected...we'll try writing directly to the app config instead
                //Properties.Settings.Default.LastSettingsFile = LastSettingsFile;
                //Properties.Settings.Default.Save();

                Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                configuration.AppSettings.Settings["LastSettingsFile"].Value = LastSettingsFile;
                configuration.Save(ConfigurationSaveMode.Full, true);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception)
            {
                return FileStatus.Exception;
            }

            return FileStatus.Success;
        } 
        #endregion
    }
}
