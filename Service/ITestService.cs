using Nanopath.Model;

namespace Nanopath.Service
{
    public interface ITestService
    {
        bool Initialize(AppConfig appCfg);
        bool LoadSample();
        bool RetractStage();
        //int PerformAlignment(); // Future Growth
        bool PerformTest(string sampleId);
        bool AbortTest();
        bool Shutdown();
        FileStatus SaveSettings(string path);
        FileStatus LoadSettings(string path);

        Settings TestSettings { get; set; }
        Results TestResults { get; }
    }
}
