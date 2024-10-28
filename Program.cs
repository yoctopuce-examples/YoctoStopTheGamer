using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace YoctoStopTheGamer
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            if (Environment.UserInteractive)
            {
                CheckSongService checkSongService = new CheckSongService();
                checkSongService.TestStartupAndStop();
            } else {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new CheckSongService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
