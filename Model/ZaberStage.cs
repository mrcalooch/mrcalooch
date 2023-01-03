using System;
using System.Linq;
using System.Threading;
using Nanopath.Service;
using Zaber.Motion;
using Zaber.Motion.Ascii;

namespace Nanopath.Model
{
    /// <summary>
    /// ZaberStage Class
    /// Class representing the Zaber stage as it exists in the Nanopath configuration (single axis)
    /// </summary>
    public class ZaberStage
    {
        #region Fields
        private IDiagnosticsService _diag;
        private Connection _connection; // ToDo: it may not be necessary to hold onto this object after initialization
        private Device _device;
        private Mutex _mutex = new Mutex();     // mutex to control HW access
        private bool _initialized = false;
        private const int _mutexTimeout = 5000; // 5 seconds
        private const int _maxComPort = 9;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// Stores the instance of the diagnostics service
        /// </summary>
        /// <param name="diag">Use null to bypass all diagnostics</param>
        public ZaberStage(IDiagnosticsService diag)
        {
            _diag = diag;
        }
        #endregion

        #region InHomePosition Property
        /// <summary>
        /// InHomePosition Property - Whether or not the stage is in the home position
        /// </summary>
        public bool InHomePosition { get; private set; } = false;
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
                _diag?.AddTrace(TraceLevel.Error, $"Zaber communications - failure to acquire mutex within {_mutexTimeout}ms.", true);
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
        /// Opens the Zaber library, creates a connection at the specific COM port, and
        /// creates a device for the first enumerated device.
        /// Will display error messages and log any error or warning events.
        /// </summary>
        /// <param name="com"></param>
        /// <return>boolean success flag</return>
        public bool Initialize(string com, double speedMmSec)
        {
            if (_initialized || !AcquireMutex()) return false;


            #region Initialize the Zaber Device Library
            try
            {
                Library.EnableDeviceDbStore(); // Initialize the Zaber library (requires internet)
            }
            catch (Exception e)
            {
                _diag?.AddTrace(TraceLevel.Error, $"Zaber communications - failure to initialize the device library. {e.Message}", true);
                ReleaseMutex();
                return false;
            }
            #endregion

            // Connect to COM port...
            #region Pre-specified COM port
            // If the COM port is specified...
            if (!string.IsNullOrEmpty(com))
            {
                try
                {
                    _connection = Connection.OpenSerialPort(com); // Open the com port connection
                    if (_connection == null)
                    {
                        _diag?.AddTrace(TraceLevel.Error, $"Zaber communications - failure to open com port {com}.", true);
                        ReleaseMutex();
                        return false;
                    }

                }
                catch (Exception e)
                {
                    _diag?.AddTrace(TraceLevel.Error, $"Zaber communications - failure to initialize stage at port {com}. {e.Message}", true);
                    ReleaseMutex();
                    return false;
                }
            } 
            #endregion
            // Discover the COM port...
            else
            {
                int comNum = 1;
                while (_connection == null && comNum <= _maxComPort)
                {
                    try
                    {
                        _connection = Connection.OpenSerialPort($"com{comNum}"); // Open the com port connection
                        if (_connection == null) comNum++;
                    }
                    catch (Exception) 
                    {
                        comNum++;
                    }
                }
                if (comNum > _maxComPort || _connection == null)
                {
                    _diag?.AddTrace(TraceLevel.Error, $"Zaber communications - failure to discover connected com port.", true);
                    ReleaseMutex();
                    return false;
                }
                com = $"com{comNum}";
            }

            // We've made it this far...detect devices
            try
            {
                var deviceList = _connection.DetectDevices(); // Find connected devices
                if (!deviceList.Any())
                {
                    _diag?.AddTrace(TraceLevel.Error, $"Zaber communications - no devices detect on connected com port {com}.", true);
                    ReleaseMutex();
                    return false;
                }

                _device = deviceList[0]; // Always grab the first device, there should be only 1
                var axis = _device.GetAxis(1);
                axis.Settings.Set(SettingConstants.Maxspeed, speedMmSec, Units.Velocity_MillimetresPerSecond);
                axis.Home();
                axis.WaitUntilIdle();
                _initialized = true;
                InHomePosition = true;
            }
            catch (Exception e)
            {
                _diag?.AddTrace(TraceLevel.Error, $"Zaber communications - failure to initialize stage at port {com}. {e.Message}", true);
                ReleaseMutex();
                return false;
            }

            ReleaseMutex();
            return true;
        }
        #endregion

        #region Shutdown Method
        /// <summary>
        /// Shutdown Method
        /// Homes the device. Other objects are not disposable so there is nothing else we can do.
        /// </summary>
        public void Shutdown()
        {
            if(_initialized) Home();
        }
        #endregion

        #region Home Method
        /// <summary>
        /// Home Method
        /// Homes the device's axis
        /// </summary>
        /// <returns></returns>
        public bool Home()
        {
            if (!_initialized || _device == null) return false;

            bool status = false;
            try
            {
                if (!AcquireMutex()) return false;
                var axis = _device.GetAxis(1);
                axis.Home();
                axis.WaitUntilIdle();
                status = true;
                InHomePosition = true;
            }
            catch (Exception e)
            {
                _diag?.AddTrace(TraceLevel.Error, $"Zaber communications - failed to home. {e.Message}", true);
            }
            finally
            {
                ReleaseMutex();
            }

            return status;
        }
        #endregion

        #region SetPosition Method
        /// <summary>
        /// SetPosition Method
        /// Moves the stage to the absolute position specified in mm
        /// </summary>
        /// <param name="mm"></param>
        /// <returns></returns>
        public bool SetPosition(double mm)
        {
            if (!_initialized || _device == null) return false;

            bool status = false;
            try
            {
                if (!AcquireMutex()) return false;
                InHomePosition = false;
                var axis = _device.GetAxis(1);
                axis.MoveAbsolute(mm, Units.Length_Millimetres);
                axis.WaitUntilIdle();
                status = true;
            }
            catch (Exception e)
            {
                // Look for OK exceptions
                if (e is MovementInterruptedException cfe && cfe.Details.Reason.ToLower().Contains("new command")) status = true;
                else _diag?.AddTrace(TraceLevel.Error, $"Zaber communications - failed to move to {mm}mm. {e.Message}", true);
            }
            finally
            {
                ReleaseMutex();
            }

            return status;
        }
        #endregion
    }
}
