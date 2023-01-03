using System;
using System.Collections.Generic;
using System.Threading;
using Nanopath.Service;
using Thorlabs.CCS_Series;

namespace Nanopath.Model
{
    public class ThorlabsSpectrometer
    {
        #region Fields
        private const int _numFreqs = 3648;
        private readonly IDiagnosticsService _diag;
        private bool _initialized = false;
        private Mutex _mutex = new Mutex();     // mutex to control HW access
        private const int _mutexTimeout = 500; // 0.5 seconds
        private TLCCS _device;
        #endregion

        #region Properties
        /// <summary>
        /// MeasWavelengths - Read-only property to get all of the actual measurement wavelengths
        /// </summary>
        public List<double> MeasWavelengthsNm { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// Stores the instance of the diagnostics service
        /// </summary>
        /// <param name="diag">Use null to bypass all diagnostics</param>
        public ThorlabsSpectrometer(IDiagnosticsService diag)
        {
            _diag = diag;
        }
        #endregion

        #region Mutex Methods (private)
        /// <summary>
        /// AcquireMutex - Waits for the device mutex (up to 10 seconds)
        /// </summary>
        /// <returns></returns>
        private bool AcquireMutex()
        {
            if (!_mutex.WaitOne(_mutexTimeout))
            {
                _diag?.AddTrace(TraceLevel.Error, $"Thorlabs communications - failure to acquire mutex within {_mutexTimeout}ms.", true);
                return false;
            }

            return true;
        }

        /// <summary>
        /// ReleaseMutex - releases the device mutex
        /// </summary>
        /// <returns></returns>
        private void ReleaseMutex()
        {
            _mutex.ReleaseMutex();
        }
        #endregion

        #region Initialize Method
        /// <summary>
        /// Initialize Method
        /// Initializes the Throlabs Spectrometer
        /// </summary>
        /// <param name="serialNo"></param>
        /// <returns>success</returns>
        public bool Initialize(string serialNo)
        {
            if (_initialized) return false;

            if (!AcquireMutex()) return false;

            try
            {
                // connect the ccs device and start the sample c application. Read out the instrument resource name from the sample application
                string instrumentNumber = "0x8087"; // 0x8087: CCS175
                string resourceName = "USB0::0x1313::" + instrumentNumber + "::M" + serialNo + "::RAW";

                // Initialize the device and reset it
                _device = new TLCCS(resourceName, false, true);
                if (_device.Handle == IntPtr.Zero)
                {
                    _diag?.AddTrace(TraceLevel.Error, $"Thorlabs communications - failure to initialize spectrometer SN: {serialNo}.", true);
                    ReleaseMutex();
                    return false;
                }

                int res = _device.getDeviceStatus(out var status);
                if (res != 0)
                {
                    _diag?.AddTrace(TraceLevel.Error, $"Thorlabs communications - device status failure on initialization. Result: {res}. Status: {status}", true);
                    ReleaseMutex();
                    return false;
                }

                // Get the actual wavelengths the spectrometer will measure at
                double[] actualWavelengths = new double[_numFreqs];
                res = _device.getWavelengthData(0, actualWavelengths, out var min, out var max);       // 1 is the "user" settings. There will be 3647 wavelengths in the array.
                if (res != 0)
                {
                    _diag?.AddTrace(TraceLevel.Error, $"Thorlabs communications - device status failure retrieving measurement wavelengths. Result: {res}. Status: {status}", true);
                    ReleaseMutex();
                    return false;
                }
                else MeasWavelengthsNm = new List<double>(actualWavelengths);

                _initialized = true;
            }
            catch (Exception e)
            {
                _diag?.AddTrace(TraceLevel.Error, $"Thorlabs communications - failure to initialize spectrometer. {e.Message}", true);
                return false;
            }
            finally
            {
                ReleaseMutex();
            }

            return true;
        }
        #endregion

        #region Shutdown Method
        /// <summary>
        /// Shutdown Method
        /// Clean up the resources we allocated
        /// </summary>
        public void Shutdown()
        {
            if (!_initialized || !AcquireMutex()) return;
            _device.Dispose();
            ReleaseMutex();
        }
        #endregion

        #region AcquireSpectrum Method
        /// <summary>
        /// AcquireSpectrum Method
        /// Triggers the spectrum analyzer to acquire a single spectrum.
        /// Waits for the results, optional.
        /// </summary>
        /// <param name="scanData"></param>
        /// <param name="timeoutMs"></param>
        /// <returns></returns>
        public bool AcquireSpectrum(out List<double> scanData, int timeoutMs = 5000)
        {
            scanData = new List<double>();

            if (!_initialized || !AcquireMutex()) return false;

            bool success = true;
            try
            {
                // trigger acquisition
                int result = _device.startScan();
                if (result != 0)
                {
                    _device.getDeviceStatus(out var status);
                    _diag?.AddTrace(TraceLevel.Error, $"Thorlabs communications - failed to start acquisition. Result: {result}. Status: {status}", true);
                    success = false;
                }
                else
                {
                    // Wait for scan to complete
                    DateTime start = DateTime.Now;
                    result = _device.getDeviceStatus(out var status);
                    while ((DateTime.Now - start).TotalMilliseconds < timeoutMs && 
                           result == 0 && 
                           (status & 0x0010) == 0)
                    {
                        result = _device.getDeviceStatus(out status);
                    }

                    // See what happened....
                    if (result != 0)
                    {
                        // Failure to query device status
                        _diag?.AddTrace(TraceLevel.Error, $"Thorlabs communications - faiure to query device status. Result: {result}.", true);
                        success = false;
                    }
                    else if ((status & 0x0010) > 0)
                    {
                        // Camera has data available for transfer
                        double[] data = new double[_numFreqs];
                        result = _device.getScanData(data);
                        if (result != 0)
                        {
                            _diag?.AddTrace(TraceLevel.Error, $"Thorlabs communications - faiure to retrieve scan datas. Result: {result}. Status: {status}", true);
                            success = false;
                        }
                        scanData = new List<double>(data);
                    }
                    else
                    {
                        // Timeout
                        _diag?.AddTrace(TraceLevel.Error, $"Thorlabs communications - timeout waiting for scan. Result: {result}. Status: {status}", true);
                        success = false;
                    }
                }
            }
            catch (Exception e)
            {
                _diag?.AddTrace(TraceLevel.Error, $"Thorlabs communications - failed to configure device. {e.Message}", true);
                success = false;
            }

            ReleaseMutex();
            return success;
        }
        #endregion

        #region SetAcquisitionParameters Method
        /// <summary>
        /// SetAcquisitionParameters Method
        /// Sets the acquisition parameters for the spectrometer. Outputs the actual spectrometer measured wavelengths, and the start and end indices for scan data manipulation.
        /// </summary>
        /// <param name="integrationMs"></param>
        /// <param name="wavelengthStartNm"></param>
        /// <param name="wavelengthEndNm"></param>
        /// <param name="wavelengthsNm"></param>
        /// <param name="dataStartIndex"></param>
        /// <returns></returns>
        public bool SetAcquisitionParameters(double integrationMs/*, double wavelengthStartNm, double wavelengthEndNm, 
                                             out List<double> wavelengthsNm, out int dataStartIndex*/)
        {
            if (!_initialized || !AcquireMutex()/* || wavelengthEndNm < wavelengthStartNm*/) return false;

            bool success = true;

            try
            {
                int result = _device.setIntegrationTime(integrationMs / 1000.0);        // Set the integration time
                if (result != 0)
                {
                    _device.getDeviceStatus(out var status);
                    _diag?.AddTrace(TraceLevel.Error, $"Thorlabs communications - failed to set integration time to {integrationMs}ms. Status: {status}", true);
                    success = false;
                }
            }
            catch (Exception e)
            {
                _diag?.AddTrace(TraceLevel.Error, $"Thorlabs communications - failed to configure device. {e.Message}", true);
                success = false;
            }
            ReleaseMutex();
            return success;
        }
        #endregion
    }
}
