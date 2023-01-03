using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Nanopath.Model;
using Nanopath.Service;
using TraceLevel = Nanopath.Service.TraceLevel;


namespace Nanopath.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Fields
        private ITestService _testService;
        private IDiagnosticsService _diag;
        private AppConfig _appCfg;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(ITestService testService, IDiagnosticsService diagService)
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {
                ChangeScreenMsgHandler(Screen.Splash);

                _testService = testService;

                #region Public Directory
                // Make sure the application's public directory exists
                string publicDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Nanopath";
                if (!Directory.Exists(publicDir)) Directory.CreateDirectory(publicDir);
                #endregion

                #region Diagnostics Service
                _diag = diagService;
                if (diagService.Initialize(publicDir) != 0)
                {
                    MessageBoxResult result = MessageBox.Show("Failed to initilize diagnostics service.", "Error", MessageBoxButton.OK);
                }
                #endregion

                #region Application Config File
                //string cfgPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Nanopath\config.csv";
                _appCfg = new AppConfig();
                FileStatus status = _appCfg.LoadFromFile();
                string msg = string.Empty;
                TraceLevel msgLevel = TraceLevel.Error;
                switch (status)
                {
                    case FileStatus.InvalidPath:
                        msg = $"Configuration file not found." + Environment.NewLine + "Default values will be used."; ;
                        break;
                    case FileStatus.Exception:
                        msg = $"failure to read configuration file." + Environment.NewLine + "Verify file format and access permissions." + Environment.NewLine + "Default values will be used.";
                        break;
                    case FileStatus.InvalidFormat:
                        msg = $"failure to read configuration file." + Environment.NewLine + "Verify file format." + Environment.NewLine + "Default values will be used.";
                        break;
                    case FileStatus.DataRangeWarning:
                        msgLevel = TraceLevel.Warning;
                        msg = $"Some values from the configuration file were coerced into a valid range. Verify setting values.";
                        break;
                }

                if (msg.Length != 0)
                {
                    _diag.AddTrace(msgLevel, msg, true);
                }
                #endregion

                Task.Delay(10).ContinueWith(t => CloseSplash()); 

                _goBackCommand = new RelayCommand(GoToPrevScreen);
                _goToCurrentSettingsCommand = new RelayCommand(GoToCurrentSettingsScreen);
                _goToMenuCommand = new RelayCommand(GoToMenuScreen);
                _appClosingCommand = new RelayCommand(AppClosing);

                Messenger.Default.Register<Screen>(this, ChangeScreenMsgHandler);
            }
        }
        #endregion

        #region CurrentView Property
        /// <summary>
        /// Current View Property
        /// The currently displayed view (screen).
        /// When this property is set, the screen in the main window will change to the view corresponding to the view model assigned.
        /// The ScreenDataTemplate.xaml file contains the markup for associating Views and Viewmodels for this purpose.
        /// </summary>
        private ViewModelBase _previousView;
        private ViewModelBase _currentView;
        public ViewModelBase CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region CloseSplash Method
        /// <summary>
        /// CloseSplash Method
        /// This method replaces the splash screen with the main menu screen
        /// </summary>
        private void CloseSplash()
        {
            _testService.Initialize(_appCfg);    // Initialize the Test Service
            ChangeScreenMsgHandler(Screen.Menu);
        } 
        #endregion

        #region ChangeScreenMsgHandler
        /// <summary>
        /// Change Screen Message Handler
        /// Reponds to an uncoming message to change the UI screen
        /// </summary>
        /// <param name="screen"></param>
        private void ChangeScreenMsgHandler(Screen screen)
        {
            ViewModelBase lastView = _currentView;
            switch (screen)
            {
                case Screen.Back:
                    CurrentView = _previousView;
                    break;
                case Screen.Splash:
                    CurrentView = SimpleIoc.Default.GetInstance<SplashViewModel>();
                    break;
                case Screen.Menu:
                    CurrentView = SimpleIoc.Default.GetInstance<MenuViewModel>();
                    break;
                case Screen.Setup:
                    CurrentView = SimpleIoc.Default.GetInstance<SetupViewModel>();
                    break;
                case Screen.CurrentSettings:
                    CurrentView = SimpleIoc.Default.GetInstance<CurrentSettingsViewModel>();
                    break;
                case Screen.LoadSample:
                    CurrentView = SimpleIoc.Default.GetInstance<LoadSampleViewModel>();
                    break;
                case Screen.Test:
                    CurrentView = SimpleIoc.Default.GetInstance<TestViewModel>();
                    break;
                case Screen.Results:
                    CurrentView = SimpleIoc.Default.GetInstance<ResultsViewModel>();
                    break;
                default:
                    break;
            }
            _previousView = lastView;
        }
        #endregion

        #region Commands
        /// <summary>
        /// Go Back Command
        /// Command and method to navigate to the previous screen
        /// </summary>
        private RelayCommand _goBackCommand;
        public ICommand GoBackCommand => _goBackCommand;
        private void GoToPrevScreen() { ChangeScreenMsgHandler(Screen.Back); }

        /// <summary>
        /// Go To Current Settings Command
        /// Command and method to navigate to the Current Settings screen
        /// </summary>
        private RelayCommand _goToCurrentSettingsCommand;
        public ICommand GoToCurrentSettingsCommand => _goToCurrentSettingsCommand;
        private void GoToCurrentSettingsScreen() { ChangeScreenMsgHandler(Screen.CurrentSettings); }

        /// <summary>
        /// Go To Menu Command
        /// Command and method to navigate to the Current Settings screen
        /// </summary>
        private RelayCommand _goToMenuCommand;
        public ICommand GoToMenuCommand => _goToMenuCommand;
        private void GoToMenuScreen() { ChangeScreenMsgHandler(Screen.Menu); }

        /// <summary>
        /// AppClosing Command
        /// Command and method to handle shutting down anything important when the application closes
        /// </summary>
        private RelayCommand _appClosingCommand;
        public ICommand AppClosingCommand => _appClosingCommand;
        private void AppClosing()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            _testService.Shutdown();
            _diag.Shutdown();
            Mouse.OverrideCursor = Cursors.Arrow;
        } 
        #endregion
    }
}