using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Nanopath.Model;
using Nanopath.Service;

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
    public class SetupViewModel: ViewModelBase
    {
        private readonly ITestService _testService;         // Instance of the TestService object
        private string _lastFile;                           // Last accessed file

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public SetupViewModel(ITestService testService)
        {
            _testService = testService;
            _lastFile = string.Empty;
            FileMessageBrush = new SolidColorBrush(Colors.Transparent);

            _saveSettingsCommand = new RelayCommand(SaveSettings);
            _loadSettingsCommand = new RelayCommand(LoadSettings);
            _loadControlCommand = new RelayCommand(LoadControl);
            _performAlignmentCommand = new RelayCommand(PerformAlignment);
            _settingsFilesControlLoadedCommand = new RelayCommand(SettingsFilesControlLoaded);
        }
        #endregion

        #region RefreshSettingFromModel Method
        /// <summary>
        /// RefreshSettingsFromModel Method
        /// Raises property changed notifications for all settings
        /// </summary>
        private void RefreshSettingsFromModel()
        {
            RaisePropertyChanged(nameof(TestLocations));
            RaisePropertyChanged(nameof(IntegrationTimeMs));
            RaisePropertyChanged(nameof(WaveLengthStartNm));
            RaisePropertyChanged(nameof(WaveLengthEndNm));
            RaisePropertyChanged(nameof(SmoothingPercent));
            RaisePropertyChanged(nameof(LightSourceMm));
        }
        #endregion

        #region HeaderText Property
        /// <summary>
        /// Header Text Property
        /// The currently displayed text in the header region of the UI. Compiler shows it as unused because it is dynamically bound in XAML.
        /// </summary>
        public string HeaderText { get; } = "Setup";
        #endregion

        #region WaveLengthStartNm Property
        /// <summary>
        /// WaveLengthStartNm Property
        /// Pass-through property to Test Service Settings
        /// </summary>
        public int WaveLengthStartNm
        {
            get => _testService.TestSettings.WavelengthStartNm;
            set
            {
                _testService.TestSettings.WavelengthStartNm = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region WaveLengthEndNm Property
        /// <summary>
        /// WaveLengthEndNm Property
        /// Pass-through property to Test Service Settings
        /// </summary>
        public int WaveLengthEndNm
        {
            get => _testService.TestSettings.WavelengthEndNm;
            set
            {
                _testService.TestSettings.WavelengthEndNm = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region SmoothingPercent Property
        /// <summary>
        /// SmoothingPercent Property
        /// Pass-through property to Test Service Settings
        /// </summary>
        public int SmoothingPercent
        {
            get => _testService.TestSettings.SmoothingPercent;
            set
            {
                _testService.TestSettings.SmoothingPercent = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region IntegrationTimeMs Property
        /// <summary>
        /// IntegrationTimeMs Property
        /// Pass-through property to Test Service Settings
        /// </summary>
        public double IntegrationTimeMs
        {
            get => _testService.TestSettings.IntegrationTimeMs;
            set
            {
                _testService.TestSettings.IntegrationTimeMs = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region TestLocations Property
        /// <summary>
        /// TestLocations Property
        /// Pass-through property to Test Service Settings
        /// </summary>
        public ObservableCollection<Location> TestLocations => _testService.TestSettings.TestLocations;
        #endregion

        #region ShowFileMessage Property
        /// <summary>
        /// ShowFileMessage Property - Simple property to know when to show the file message
        /// </summary>
        private bool _showFileMessage = false;
        public bool ShowFileMessage
        {
            get => _showFileMessage;
            set
            {
                _showFileMessage = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region FileMessageBrush Property
        /// <summary>
        /// ShowFileMessage Property - Simple property to know when to show the file message
        /// </summary>
        private Brush _fileMessageBrush;
        public Brush FileMessageBrush
        {
            get => _fileMessageBrush;
            set
            {
                _fileMessageBrush = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region FileMessage Property
        /// <summary>
        /// FileMessage Property - simple property to contain a UI message regarding file operations
        /// </summary>
        private string _fileMessage = string.Empty;
        public string FileMessage
        {
            get => _fileMessage;
            set
            {
                _fileMessage = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region LightSourceMm Property
        /// <summary>
        /// LightSourceMm Property
        /// Pass-through property to Test Service Settings
        /// </summary>
        public double LightSourceMm
        {
            get => _testService.TestSettings.LightSourceMm;
            set
            {
                _testService.TestSettings.LightSourceMm = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Limit Properties
        /// <summary>
        /// Simple Properties to bind UI control limits
        /// </summary>
        public double MinIntegrationMs => _testService.TestSettings.MinIntegrationMs;
        public double MaxIntegrationMs => _testService.TestSettings.MaxIntegrationMs;
        public int MinWavelengthNm => _testService.TestSettings.MinWavelengthNm;
        public int MaxWavelengthNm => _testService.TestSettings.MaxWavelengthNm;
        public int MinSmoothingPercent => _testService.TestSettings.MinSmoothingPercent;
        public int MaxSmoothingPercent => _testService.TestSettings.MaxSmoothingPercent;
        public double MinLightSourceMm => _testService.TestSettings.MinLightSourceMm;
        public double MaxLightSourceMm => _testService.TestSettings.MaxLightSourceMm;
        #endregion

        #region Commands

        #region SaveSettings Command
        /// <summary>
        /// Save Settings Command
        /// Command and method to save all settings to a CSV file
        /// </summary>
        private readonly RelayCommand _saveSettingsCommand;
        public ICommand SaveSettingsCommand => _saveSettingsCommand;
        private void SaveSettings()
        {
            // Save a CSV file
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = _lastFile,                   // Default file name
                DefaultExt = ".csv",                    // Default file extension
                Filter = "CSV documents (.csv)|*.csv"   // Filter files by extension
            };

            // Show open file dialog box
            bool? result = dlg.ShowDialog();

            // Process file dialog box results
            if (result == true)
            {
                FileStatus status = _testService.SaveSettings(dlg.FileName);
                switch (status)
                {
                    case FileStatus.Success:
                        FileMessage = "Save Complete";
                        FileMessageBrush = new SolidColorBrush(Colors.Green);
                        break;
                    case FileStatus.InvalidPath:
                        FileMessage = "Invalid Path";
                        FileMessageBrush = new SolidColorBrush(Colors.Red);
                        break;
                    case FileStatus.Exception:
                        FileMessage = "Failed to write file. Check access permissions.";
                        FileMessageBrush = new SolidColorBrush(Colors.Red);
                        break;
                }
                ShowFileMessage = true;
            }
        }
        #endregion

        #region LoadSettings Command
        /// <summary>
        /// Save Settings Command
        /// Command and method to save all settings to a CSV file
        /// </summary>
        private readonly RelayCommand _loadSettingsCommand;
        public ICommand LoadSettingsCommand => _loadSettingsCommand;
        private void LoadSettings()
        {
            // Read a CSV file
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                FileName = _lastFile,                   // Default file name
                DefaultExt = ".csv",                    // Default file extension
                Filter = "CSV documents (.csv)|*.csv"   // Filter files by extension
            };

            // Show file dialog box
            bool? result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                FileStatus status = _testService.LoadSettings(dlg.FileName);
                switch (status)
                {
                    case FileStatus.DataRangeWarning:
                        _lastFile = dlg.FileName;
                        FileMessage = "Load Complete. Some values were coerced into a valid range.";
                        FileMessageBrush = new SolidColorBrush(Colors.DarkOrange);
                        break;
                    case FileStatus.Success:
                        _lastFile = dlg.FileName;
                        FileMessage = "Load Complete";
                        FileMessageBrush = new SolidColorBrush(Colors.Green);
                        break;
                    case FileStatus.InvalidFormat:
                        FileMessage = "Invalid File Format.";
                        FileMessageBrush = new SolidColorBrush(Colors.Red);
                        break;
                    case FileStatus.InvalidPath:
                        FileMessage = "Invalid Path";
                        FileMessageBrush = new SolidColorBrush(Colors.Red);
                        break;
                    case FileStatus.Exception:
                        FileMessage = "Failed to read file. Check file format and permissions.";
                        FileMessageBrush = new SolidColorBrush(Colors.Red);
                        break;
                }
                ShowFileMessage = true;
                RefreshSettingsFromModel();
            }
        }
        #endregion

        #region Alignment Commands (FUTURE GROWTH)
        /// <summary>
        /// Load Control Command
        /// Command and method for Stage Alignment Load Control operation
        /// This method triggers the Stage to move into the loading position.
        /// </summary>
        private readonly RelayCommand _loadControlCommand;
        public ICommand LoadControlCommand => _loadControlCommand;
        private void LoadControl()
        {
            // ToDo: Move stage to loading position for loading of control
        }

        /// <summary>
        /// Perform Alignment Command
        /// Command and method for stage alignment 
        /// This method triggers the stage alignment procedure
        /// </summary>
        private readonly RelayCommand _performAlignmentCommand;
        public ICommand PerformAlignmentCommand => _performAlignmentCommand;
        private void PerformAlignment()
        {
            // ToDo: Start stage alignment
        }
        #endregion

        #region SettingsFilesControlLoaded Command
        /// <summary>
        /// SettingsFilesControlLoadedCommand
        /// Command and method for handling when the file settings control is loaded (each time the tab selected)
        /// Clears out any messages on screen.
        /// </summary>
        private readonly RelayCommand _settingsFilesControlLoadedCommand;
        public ICommand SettingsFilesControlLoadedCommand => _settingsFilesControlLoadedCommand;
        private void SettingsFilesControlLoaded()
        {
            ShowFileMessage = false;
            FileMessage = string.Empty;
            FileMessageBrush = new SolidColorBrush(Colors.Transparent);
        } 
        #endregion

        #endregion
    }
}