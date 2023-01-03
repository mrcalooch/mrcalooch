using System.ComponentModel;


namespace Nanopath
{
    public enum Screen
    {
        [Description("Back")]
        Back = -1,
        [Description("Splash")]
        Splash = 1,
        [Description("Menu")]
        Menu = 2,
        [Description("Setup")]
        Setup = 3,
        [Description("Current Settings")]
        CurrentSettings = 4,
        [Description("Load Sample")]
        LoadSample = 5,
        [Description("Test")]
        Test = 6,
        [Description("Results")]
        Results = 7,
        [Description("Details")]
        ResultDetails = 7,
    }

    public enum FileStatus : int
    {
        Success = 0,

        InvalidPath = -1,
        Exception = -2,
        InvalidFormat = -3,

        DataRangeWarning = 1,
    }

    public enum PosNeg
    {
        Positive = 0,
        Negative = 1
    }
}
