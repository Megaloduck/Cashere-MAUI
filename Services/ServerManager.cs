using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Cashere.Services
{
    public class ServerManager
    {
        private Process _serverProcess;
        private readonly string _backendPath;
        private readonly int _port;

        public bool IsRunning => _serverProcess != null && !_serverProcess.HasExited;

        public ServerManager(int port = 7102)
        {
            _port = port;

            // Adjust this path to your actual backend location
            // Assuming backend is in parallel folder structure
            string solutionPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
            _backendPath = Path.Combine(solutionPath, "CafePOS");
        }

        public async Task<bool> StartServerAsync()
        {
            try
            {
                if (IsRunning)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Server is already running");
                    return true;
                }

                System.Diagnostics.Debug.WriteLine($"🚀 Starting backend from: {_backendPath}");

                if (!Directory.Exists(_backendPath))
                {
                    throw new DirectoryNotFoundException($"Backend path not found: {_backendPath}");
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "run",
                    WorkingDirectory = _backendPath,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                _serverProcess = new Process { StartInfo = startInfo };

                // Capture output for debugging
                _serverProcess.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                        System.Diagnostics.Debug.WriteLine($"[Backend] {args.Data}");
                };

                _serverProcess.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                        System.Diagnostics.Debug.WriteLine($"[Backend Error] {args.Data}");
                };

                _serverProcess.Start();
                _serverProcess.BeginOutputReadLine();
                _serverProcess.BeginErrorReadLine();

                System.Diagnostics.Debug.WriteLine("⏳ Waiting for server to start...");

                // Wait for server to be ready (check health endpoint)
                for (int i = 0; i < 20; i++) // 20 seconds max
                {
                    await Task.Delay(1000);

                    if (await IsServerResponding())
                    {
                        System.Diagnostics.Debug.WriteLine("✅ Server is running!");
                        return true;
                    }
                }

                System.Diagnostics.Debug.WriteLine("❌ Server did not respond in time");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Failed to start server: {ex.Message}");
                return false;
            }
        }

        public void StopServer()
        {
            try
            {
                if (_serverProcess != null && !_serverProcess.HasExited)
                {
                    System.Diagnostics.Debug.WriteLine("🛑 Stopping server...");
                    _serverProcess.Kill();
                    _serverProcess.WaitForExit(5000);
                    _serverProcess.Dispose();
                    _serverProcess = null;
                    System.Diagnostics.Debug.WriteLine("✅ Server stopped");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Error stopping server: {ex.Message}");
            }
        }

        private async Task<bool> IsServerResponding()
        {
            try
            {
                string ip = ApiConfig.GetLocalIPAddress();
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(2);
                var response = await client.GetAsync($"http://{ip}:{_port}/api/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
