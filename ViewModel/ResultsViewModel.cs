using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
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
    public class ResultsViewModel : ViewModelBase
    {
        private readonly ITestService _testService;         // Instance of the TestService object

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public ResultsViewModel(ITestService testService)
        {
            _testService = testService;
            _showDetailsCommand = new RelayCommand(ShowDetails);
            _testCompleteCommand = new RelayCommand(TestComplete);

            //if (IsInDesignMode)
            //{
            //    // Code runs in Blend --> create design time data.
            //}
            //else
            //{
            //    // Code runs "for real"
            //}
        }
        #endregion

        #region TestLocations Property
        /// <summary>
        /// TestResults Property
        /// Pass-through property to Test Service Results Member
        /// </summary>
        public ObservableCollection<PositionResult> TestResults => _testService.TestResults.PositionResults;
        #endregion

        #region HeaderText Property
        /// <summary>
        /// Header Text Property
        /// The currently displayed text in the header region of the UI. Compiler shows it as unused because it is dynamically bound in XAML.
        /// </summary>
        private string _headerText = "Results";
        public string HeaderText
        {
            get { return _headerText; }
            set
            {
                _headerText = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ShowDetails Property
        /// <summary>
        /// ShowDetails Visible Property
        /// Whether of not to show the Details Button on the Results Screen UI
        /// </summary>
        private bool _detailsVisible = false;
        public bool DetailsVisible
        {
            get { return _detailsVisible; }
            set
            {
                _detailsVisible = value;
                RaisePropertyChanged();
            }
        } 
        #endregion

        #region Commands
        /// <summary>
        /// Show Details Command
        /// Command and method to display the details of the test results
        /// </summary>
        private readonly RelayCommand _showDetailsCommand;
        public ICommand ShowDetailsCommand => _showDetailsCommand;
        private void ShowDetails()
        {
            DetailsVisible = true;
        }

        /// <summary>
        /// Test Complete Command
        /// Command and method to retract the stage and go back to the main menu
        /// Sends a message to the Main ViewModel
        /// </summary>
        private readonly RelayCommand _testCompleteCommand;
        public ICommand TestCompleteCommand => _testCompleteCommand;
        private void TestComplete()
        {
            _testService.RetractStage();
            Messenger.Default.Send(Screen.Menu);
        }
        #endregion Commands
    }
}