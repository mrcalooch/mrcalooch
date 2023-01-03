using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Nanopath.Service
{
    /// <summary>
    /// TraceLevel Enum
    /// Enum for the various trace levels supported by the Diagnostics Service
    /// </summary>
    public enum TraceLevel
    {
        Error = 0,
        Warning,
        Info
    }

    /// <summary>
    /// DiagnosticsService
    /// Implementation of the IDiagnosticsService interface
    /// </summary>
    public class DiagnosticsService : IDiagnosticsService
    {
        #region Fields
        private bool _initialized = false;                          // Have we initialized the trace components?
        private TextWriterTraceListener _textListener = null;       // The text listener that will handle all of the trace logging 
        #endregion

        #region Initialize Method
        /// <summary>
        /// Initialize Method
        /// Initialized the diagnostics service by creating and configuring a TextWriterTraceListener
        /// </summary>
        /// <param name="publicDir"></param>
        /// <returns></returns>
        public int Initialize(string publicDir)
        {
            try
            {
                string logPath = publicDir + @"\Logs";
                if (!Directory.Exists(logPath)) Directory.CreateDirectory(logPath);
                string date = DateTime.Now.ToString("yyyy-MM-dd");     // Make a new log for each day, keep it simple
                _textListener = new TextWriterTraceListener(logPath + $"\\{date}.log");
                _textListener.TraceOutputOptions = TraceOptions.DateTime;
                Trace.Listeners.Add(_textListener);
                Trace.AutoFlush = true;
            }
            catch (Exception)
            {
                _initialized = false;
                return -1;
            }

            _initialized = true;
            return 0;
        }
        #endregion

        #region Shutdown Method
        /// <summary>
        /// Shutdown Method
        /// Cleans up the service by disposing the TextWriterTraceListener
        /// </summary>
        public void Shutdown()
        {
            if (_initialized)
            {
                Trace.Listeners.Remove(_textListener);
                _textListener.Dispose();
                _initialized = false;
            }
        }
        #endregion

        #region AddTrace Method
        /// <summary>
        /// AddTrace Method
        /// Adds a line to the trace log, optionally displays a simple message box to the user
        /// </summary>
        /// <param name="level"></param>
        /// <param name="content"></param>
        /// <param name="showDialog"></param>
        public void AddTrace(TraceLevel level, string content, bool showDialog = false)
        {
            if (_initialized)
            {
                switch (level)
                {
                    case TraceLevel.Error:
                        Trace.TraceError(content);
                        break;
                    case TraceLevel.Warning:
                        Trace.TraceWarning(content);
                        break;
                    case TraceLevel.Info:
                        Trace.TraceInformation(content);
                        break;
                }
            }

            // Show the dialog even if we didn't initialize, no reason not to
            if (showDialog)
            {
                MessageBox.Show(content, "Attention", MessageBoxButton.OK);
            }
        } 
        #endregion
    }
}
