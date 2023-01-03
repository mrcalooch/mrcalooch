namespace Nanopath.Service
{
    /// <summary>
    /// IDiagnosticsService
    /// Interface for a simple trace diagnostics service
    /// </summary>
    public interface IDiagnosticsService
    {
        int Initialize(string publicDir);
        void Shutdown();
        void AddTrace(TraceLevel level, string content, bool showDialog = false);
    }
}
