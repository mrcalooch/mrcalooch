using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using Nanopath.Model;

namespace Nanopath.Service
{
    class TestService :ITestService
    {
        #region Fields
        private CancellationTokenSource _cancelTokSource;
        private Task _runTask;
        private readonly IDiagnosticsService _diag;
        private AppConfig _appCfg = new AppConfig();
        private string _sampleId = string.Empty;
        private ZaberStage _stage;
        private ThorlabsSpectrometer _spectrometer;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// Uses IOC to set the diagnostics service
        /// </summary>
        /// <param name="diagService"></param>
        public TestService(IDiagnosticsService diagService)
        {
            _diag = diagService;
        }
        #endregion

        #region Initialize Method
        /// <summary>
        /// Initialize Method
        /// Initializes the test system
        ///  - stores the application configuration
        ///  - connects to and initializes hardware resources
        /// </summary>
        /// <returns></returns>
        public bool Initialize(AppConfig appCfg)
        {
            _appCfg = appCfg;
            if (File.Exists(_appCfg.LastSettingsFile)) LoadSettings(_appCfg.LastSettingsFile);      // Load the last used settings

            // Initialize the stage
            _stage = new ZaberStage(_diag);
            bool success = _stage.Initialize(null, _appCfg.StageSpeedMmSec);

            // Initialize the spectrometer
            _spectrometer = new ThorlabsSpectrometer(_diag);
            success |= _spectrometer.Initialize(_appCfg.SpectrometerSerialNo);

            return success;
        }
        #endregion

        #region LoadSample Method
        /// <summary>
        /// LoadSample Method
        /// Commands the stage to the load position
        /// </summary>
        /// <returns></returns>
        public bool LoadSample()
        {
            // Command stage to loading position
            if (_stage == null) return false;
            return _stage.SetPosition(_appCfg.StageLoadPositionMm);
        }
        #endregion

        #region RetractStage Method
        /// <summary>
        /// RetractStage Method
        /// Commands the stage to retract to position 0
        /// </summary>
        /// <returns></returns>
        public bool RetractStage()
        {
            // Command stage to loading position
            if (_stage == null) return false;
            return _stage.SetPosition(0.0);
        }
        #endregion

        #region PerformAlignment Method (FUTURE GROWTH)
        /// <summary>
        /// PerformAlignment Method
        /// Starts an asynchronous task that performs stage alignment
        /// </summary>
        /// <returns></returns>
        public bool PerformAlignment()
        {
            // ToDo: This is an optional feature at this time
            return true;
        }
        #endregion

        #region PerformTest Method
        /// <summary>
        /// PerformTest Method
        /// Starts an asynchronous task that performs the test procedure
        /// </summary>
        /// <returns></returns>
        public bool PerformTest(string sampleId)
        {
            if (_stage == null) return false;

            if (_runTask == null || (_runTask.Status != TaskStatus.Running && _runTask.Status != TaskStatus.WaitingToRun))
            {
                _sampleId = sampleId;
                _cancelTokSource = new CancellationTokenSource();
                _runTask = Test(_cancelTokSource.Token);
                return true;
            }
            _diag.AddTrace(TraceLevel.Warning, "Failed to start test - test already in progress.", true);
            return false;
        }
        #endregion

        #region AbortTest Method
        /// <summary>
        /// Abort Method
        /// Cancels the currently executing asynchronous test task
        /// </summary>
        /// <returns></returns>
        public bool AbortTest()
        {
            // Of the cancellation token is valid an not already canceled...
            if (_cancelTokSource != null && !_cancelTokSource.IsCancellationRequested) _cancelTokSource.Cancel();
            return true;
        } 
        #endregion

        #region Shutdown Method
        /// <summary>
        /// Shutdown Method
        /// Puts hardware into a known states and disconnects from all hardware resources
        /// </summary>
        /// <returns></returns>
        public bool Shutdown()
        {
            // Cleanup any allocated/connected HW resources
            _stage?.Shutdown();
            _spectrometer.Shutdown();
            return true;
        }
        #endregion

        #region Progress Property
        /// <summary>
        /// Progress Property
        /// The progress of an operation from 0 to 100
        /// </summary>
        private double _progress = 0;
        public double Progress
        {
            get => _progress;
            private set
            {
                _progress = value;
                Messenger.Default.Send(Progress, "Progress");
            }
        }
        #endregion

        #region Results Property
        /// <summary>
        /// Results Property
        /// The results of an test
        /// </summary>
        public Results TestResults { get; private set; }
        #endregion

        #region TestSettings Property
        /// <summary>
        /// TestSettings Property
        /// The configuration for a test
        /// </summary>
        public Settings TestSettings { get; set; } = new Settings();
        #endregion

        #region CheckTestAbort Method
        /// <summary>
        /// CheckTestAbort Method
        /// Internal method to check the cancellation flag
        /// </summary>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        private bool CheckTestAbort(CancellationToken cancelToken)
        {
            if (cancelToken.IsCancellationRequested)
            {
                Progress = 0;
                return true;
            }
            return false;
        }
        #endregion

        #region Test Task
        /// <summary>
        /// Test Task
        /// An Asynchronous task implementing the test procedure
        /// </summary>
        /// <returns></returns>
        public async Task Test(CancellationToken cancelToken)
        {
            Progress = 0;
            await Task.Run(() =>
            {
                #region Initialize
                int numLocs = TestSettings.TestLocations.Count(f => f.SampleMm.HasValue && f.BackgroundMm.HasValue);
                List<Location> locs = TestSettings.TestLocations.Where(f => f.SampleMm.HasValue && f.BackgroundMm.HasValue).ToList();
                Progress = 100;

                // Set the Spectrometer
                if (!_spectrometer.SetAcquisitionParameters(TestSettings.IntegrationTimeMs)) return;
                // Find the progress step size for updating the UI
                double progressStep = 99.0 / ((numLocs * 7.0) + 1);

                // Initialize the test results model with the measurement wavelengths from the spectrometer
                TestResults = new Results(_diag, _appCfg, TestSettings, _sampleId, _spectrometer.MeasWavelengthsNm, TestSettings.WavelengthStartNm, TestSettings.WavelengthEndNm);
                #endregion

                #region Get the light source measurement
                // Move the stage to the light source position
                _stage.SetPosition(TestSettings.LightSourceMm);
                Progress += progressStep;
                Task.Delay(100).Wait();
                if (CheckTestAbort(cancelToken)) return;

                // Take the light source measurement
                if (_spectrometer.AcquireSpectrum(out var scanData)) TestResults.LightSourceRawData = scanData;      // Will automatically set the calc data
                Progress += progressStep;
                if (CheckTestAbort(cancelToken)) return;
                #endregion

                #region Iterate through the valid locations....
                foreach (Location loc in locs)
                {
                    List<double> bg1Data = null;
                    List<double> sampleData = null;
                    List<double> bg2Data = null;

                    #region Get BG1
                    // 1. Move the stage to the BG position
                    _stage.SetPosition(loc.BackgroundMm.Value);
                    Progress += progressStep;
                    Task.Delay(100).Wait();
                    if (CheckTestAbort(cancelToken)) return;

                    // 2. Take BG1 measurement
                    if (_spectrometer.AcquireSpectrum(out scanData)) bg1Data = scanData;
                    Progress += progressStep;
                    if (CheckTestAbort(cancelToken)) return;
                    #endregion

                    #region Get Sample
                    // 3. Move the stage to the sample location
                    _stage.SetPosition(loc.SampleMm.Value);
                    Progress += progressStep;
                    Task.Delay(100).Wait();
                    if (CheckTestAbort(cancelToken)) return;

                    // 4. Take sample measurement
                    if (_spectrometer.AcquireSpectrum(out scanData)) sampleData = scanData;
                    Progress += progressStep;
                    if (CheckTestAbort(cancelToken)) return;
                    #endregion

                    #region Get BG2
                    // 5. Move the stage to the BG location again
                    _stage.SetPosition(loc.BackgroundMm.Value);
                    Progress += progressStep;
                    Task.Delay(100).Wait();
                    if (CheckTestAbort(cancelToken)) return;

                    // 6. Take BG2 measurement
                    if (_spectrometer.AcquireSpectrum(out scanData)) bg2Data = scanData;
                    Progress += progressStep;
                    if (CheckTestAbort(cancelToken)) return;
                    #endregion

                    #region Perform Calculations
                    // 7. Calculate the result
                    if (sampleData != null && bg1Data != null && bg2Data != null)
                    {
                        TestResults.CreateResult(bg1Data, sampleData, bg2Data);
                    }
                    else
                    {
                        _diag.AddTrace(TraceLevel.Error, "Algorithm - Unable to perform calculations due to null data.", true);
                    }

                    Progress += progressStep;
                    if (CheckTestAbort(cancelToken)) return;
                    #endregion
                }
                #endregion

                #region Test Complete
                if (!cancelToken.IsCancellationRequested)
                {
                    Progress = 100;
                    SaveResults();
                }
                else Progress = 0;
                _stage.SetPosition(_appCfg.StageLoadPositionMm);
                #endregion
            });
        }
        #endregion

        #region SaveResults Method
        /// <summary>
        /// SaveResults Method
        /// Saves a results file after test has completed
        /// </summary>
        /// <returns></returns>
        private FileStatus SaveResults()
        {
            // SampleID_M1_DATE_TIME.CSV
            string date = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");     // Make a new log for each day, keep it simple
            string resultPath = _appCfg.ResultPath + $"\\{_sampleId}_{date}.csv";
            return TestResults.SaveFile(resultPath, TestSettings);
        }
        #endregion

        #region SaveSettings Method
        /// <summary>
        /// SaveSettings Method
        /// Saves the settings to a file and, if successful, saves the last settings file path to the app.config file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public FileStatus SaveSettings(string path)
        {
            string diagMsg = string.Empty;
            FileStatus status = TestSettings.SaveFile(path);

            switch (status)
            {
                case FileStatus.Success:
                    _appCfg.LastSettingsFile = path;
                    _appCfg.SaveToFile();
                    break;
                case FileStatus.InvalidPath:
                    diagMsg = $"Failed to save settings CSV file: {path}. Invalid path.";
                    break;
                case FileStatus.Exception:
                    diagMsg = $"Failed to save settings CSV file: {path}. Check access permissions.";
                    break;
            }
            if (diagMsg.Length != 0) _diag.AddTrace(TraceLevel.Error, diagMsg);

            return status;
        }
        #endregion

        #region LoadSettings Method
        /// <summary>
        /// LoadSettings Method
        /// Loads the settings from a file and, if successful, saves the last settings file path to the app.config file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public FileStatus LoadSettings(string path)
        {
            string diagMsg = string.Empty;
            TraceLevel diagLevel = TraceLevel.Error;
            FileStatus status = TestSettings.LoadFile(path);
            switch (status)
            {
                case FileStatus.DataRangeWarning:
                    diagLevel = TraceLevel.Warning;
                    diagMsg = $"Loading settings CSV: {path}. Load Complete. Some values were coerced into a valid range.";
                    _appCfg.LastSettingsFile = path;
                    _appCfg.SaveToFile();
                    break;
                case FileStatus.Success:
                    _appCfg.LastSettingsFile = path;
                    _appCfg.SaveToFile();
                    break;
                case FileStatus.InvalidFormat:
                    diagMsg = $"Loading settings CSV: {path}. Invalid File Format. Check number of rows and columns.";
                    break;
                case FileStatus.InvalidPath:
                    diagMsg = $"Loading settings CSV: {path}. Invalid Path";
                    break;
                case FileStatus.Exception:
                    diagMsg = $"Loading settings CSV: {path}. Failed to read file. Check file format and permissions.";
                    break;
            }
            if (diagMsg.Length != 0) _diag.AddTrace(diagLevel, diagMsg);

            return status;
        }
        #endregion
    }
}
