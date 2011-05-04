using System;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

using log4net;


namespace com.johnruiz.minecraft
{
    public partial class MinecraftService : ServiceBase
    {
        private StreamWriter _standardInput;
        private ILog _log;
        private bool _isStopping;


        public MinecraftService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _log = LogManager.GetLogger(this.GetType());
            _isStopping = false;

            try
            {
                _log.Info("Starting minecraft");

                MinecraftConfig config = ConfigurationManager.GetSection(
                    typeof(MinecraftConfig).Name) as MinecraftConfig;

                Process startCmd = new Process();
                startCmd.StartInfo.FileName = config.JavaExecutable;
                startCmd.StartInfo.Arguments = string.Format("-Xmx{0}M -Xms{1}M -jar {2}\\minecraft_server.jar nogui",
                    config.MaxHeapInMegabytes, config.InitialHeapInMegabytes, config.MinecraftJarDirectory);
                startCmd.StartInfo.WorkingDirectory = config.MinecraftJarDirectory;
                startCmd.StartInfo.UseShellExecute = false;
                startCmd.StartInfo.RedirectStandardInput = true;
                startCmd.StartInfo.RedirectStandardOutput = true;
                startCmd.StartInfo.RedirectStandardError = true;
                startCmd.EnableRaisingEvents = true;

                startCmd.OutputDataReceived += new DataReceivedEventHandler(startCmd_OutputDataReceived);
                startCmd.ErrorDataReceived += new DataReceivedEventHandler(startCmd_ErrorDataReceived);
                startCmd.Exited += new EventHandler(startCmd_Exited);

                _log.InfoFormat("Executing '{0} {1}'", startCmd.StartInfo.FileName, startCmd.StartInfo.Arguments);

                startCmd.Start();

                startCmd.BeginOutputReadLine();
                startCmd.BeginErrorReadLine();

                _standardInput = startCmd.StandardInput;
            }
            catch (Exception ex)
            {
                _log.Fatal("Failed to start minecraft", ex);
            }
        }

        void startCmd_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            _log.Info(e.Data);
        }

        void startCmd_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            _log.Info(e.Data);
        }

        void startCmd_Exited(object sender, EventArgs e)
        {
            if (!_isStopping)
            {
                _log.Error("The minecraft service has detected that the minecraft server has stopped " +
                    "even though the service wasn't asked to shut down the server.");
                this.OnStop();
            }
            else
            {
                _log.Info("Minecraft server stopped normally.");
            }
        }

        protected override void OnStop()
        {
            _isStopping = true;
            _log.Info("Stopping minecraft.");

            try
            {
                _standardInput.WriteLine("stop");
            }
            catch (Exception ex)
            {
                _log.Warn("Caught an exception while stopping the minecraft server.", ex);
            }
        }
    }
}
