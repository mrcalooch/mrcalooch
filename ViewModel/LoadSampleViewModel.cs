using System.IO;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
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
    public class LoadSampleViewModel : ViewModelBase
    {
        private readonly ITestService _testService;         // Instance of the TestService object

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public LoadSampleViewModel(ITestService testService)
        {
            _testService = testService;
            _runTestCommand = new RelayCommand(RunTest);
            _userControlLoadedCommand = new RelayCommand(UserControlLoaded);
        }
        #endregion

        #region HeaderText Property
        /// <summary>
        /// Header Text Property
        /// The currently displayed text in the header region of the UI. Compiler shows it as unused because it is dynamically bound in XAML.
        /// </summary>
        public string HeaderText { get; } = "Load Sample";
        #endregion

        #region SampleId Property
        /// <summary>
        /// SampleId Property
        /// The Sample ID, will get passed on to the TestService and eventually become part of a filename
        /// </summary>
        private string _sampleId = string.Empty;
        public string SampleId
        {
            get => _sampleId;
            set
            {
                _sampleId = value;
                // Check for a valid value (no bad filename chars!)
                if (_sampleId.Length == 0 || _sampleId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) SampleIdValid = false;
                else SampleIdValid = true;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region SampleIdValid Property
        /// <summary>
        /// SampleIdValid Property
        /// Whether or not the Sample ID value is valid. Used to control the border color of the input box as well
        /// as the enable state of the Next button
        /// </summary>
        private bool _sampleIdValid = false;
        public bool SampleIdValid
        {
            get => _sampleIdValid;
            set
            {
                _sampleIdValid = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region RunTest Command
        /// <summary>
        /// Run Test Command
        /// Command and method to start running a test and to navigate to the Test screen
        /// Sends a message to the Main ViewModel
        /// </summary>
        private readonly RelayCommand _runTestCommand;
        public ICommand RunTestCommand => _runTestCommand;
        private void RunTest()
        {
            if(_testService.PerformTest(SampleId))         // Tell the Test Service to start the test
                Messenger.Default.Send(Screen.Test);       // If the test started, go to the test screen
        }
        #endregion

        #region UserControlLoaded Command
        /// <summary>
        /// UserControlLoaded Command
        /// Command and method to track when the user control is loaded
        /// </summary>
        private readonly RelayCommand _userControlLoadedCommand;
        public ICommand UserControlLoadedCommand => _userControlLoadedCommand;
        private void UserControlLoaded()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            _testService.LoadSample();  // Move the stage in parallel to showing the user control
            Mouse.OverrideCursor = Cursors.Arrow;
        }
        #endregion
    }
}