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
    public class TestViewModel : ViewModelBase
    {
        private readonly ITestService _testService;         // Instance of the TestService object

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public TestViewModel(ITestService testService)
        {
            //if (IsInDesignMode)
            //{
            //    // Code runs in Blend --> create design time data.
            //}
            //else
            //{
            //    // Code runs "for real"
            //}
            _testService = testService;
            Messenger.Default.Register<double>(this, "Progress", UpdateProgressMsgHandler);
            _abortTestCommand = new RelayCommand(AbortTest);
        }
        #endregion

        #region HeaderText Property
        /// <summary>
        /// Header Text Property
        /// The currently displayed text in the header region of the UI. Compiler shows it as unused because it is dynamically bound in XAML.
        /// </summary>
        public string HeaderText { get; private set; } = "Test in Progress...";
        #endregion

        #region Progress Property
        /// <summary>
        /// Progress Property
        /// The progress on the currently running test
        /// </summary>
        private int _progress = 0;
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Abort Test Command
        /// Command and method to stop the currently executing test and return
        /// to the Load Sample screen
        /// </summary>
        private RelayCommand _abortTestCommand;
        public ICommand AbortTestCommand => _abortTestCommand;

        private void AbortTest()
        {
            _testService.AbortTest();
            Messenger.Default.Send(Screen.LoadSample);
        }
        #endregion Commands

        #region Message Handlers
        /// <summary>
        /// Progress Update Message handler
        /// Will update the local Progress property from any messages sent from the Test Service
        /// </summary>
        /// <param name="progress"></param>
        private void UpdateProgressMsgHandler(double progress)
        {
            Progress = (int)progress;
            if (Progress >= 100)
            {
                Messenger.Default.Send(Screen.Results);
            }
        } 
        #endregion
    }
}