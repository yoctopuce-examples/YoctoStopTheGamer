using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration.Install;
using System.Data.Common;
using System.Security.Principal;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;


namespace YoctoStopTheGamer
{
    internal static class Program
    {
        static void ParseArgs(string[] args, out string url, out string hwid, out string message, out string locale)
        {
            message = "Stop playing";
            locale = "";
            if (args.Length < 2) {
                throw new ArgumentException("Missing URL and button HardwareID");
            }
            url = args[0];
            hwid = args[1];
            for (int i = 2; i < args.Length; i++) {
                if (args[i] == "--msg") {
                    if (i + 1 >= args.Length) {
                        throw new ArgumentException("Missing message after --msg");
                    }
                    message = args[i + 1];
                } else if (args[i] == "--locale") {
                    if (i + 1 >= args.Length) {
                        throw new ArgumentException("Missing local after --locale");
                    }
                    locale = args[i + 1];
                }
            }
        }


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            string url;
            string hwid;
            string message;
            string locale;
                if (Environment.UserInteractive) {
                    if (args.Length < 1) {
                        Console.WriteLine("Error: Missing command\n");
                        Usage("");
                        Environment.Exit(1);
                    }
                    if (args.Contains("--help")) {
                        Usage(args[0]);
                        Environment.Exit(0);
                    }
                    string exe_location = Assembly.GetExecutingAssembly().Location;
                    string serviceName = "YoctoStopTheGamerService";
                    string serviceDisplayName = "Gamer Compliance Service";
                    switch (args[0]) {
                        case "--install":
                            CheckAdmin();
                            try {
                                if (WindowsServiceControl.ServiceIsInstalled(serviceName)) {
                                    Console.WriteLine("Service " + serviceName + " is already installed");
                                } else {
                                    try {
                                        ParseArgs(args.Skip(1).ToArray(), out url, out hwid, out message, out locale);
                                    } catch (ArgumentException ex) {
                                        Console.WriteLine(ex.ToString());
                                        Usage(args[0]);
                                        Environment.Exit(1);
                                    }

                                    ParseArgs(args.Skip(1).ToArray(), out url, out hwid, out message, out locale);
                                    string param = "\"" + exe_location + "\" " + url + " " + hwid + " --msg \"" +
                                                   message +
                                                   "\"";
                                    if (locale != "") {
                                        param += " --locale " + locale;
                                    }
                                    WindowsServiceControl.InstallAndStart(serviceName, serviceDisplayName, param);
                                    Console.WriteLine(serviceDisplayName + " installed and started");
                                }
                            } catch (Exception ex) {
                                Console.WriteLine("Error:" + ex.Message);
                                Environment.Exit(1);
                            }

                            break;
                        case "--uninstall":
                            CheckAdmin();
                            try {
                                if (!WindowsServiceControl.ServiceIsInstalled(serviceName)) {
                                    Console.WriteLine("Service " + serviceName + " is not installed");
                                } else {
                                    WindowsServiceControl.Uninstall(serviceName);
                                    Console.WriteLine(serviceDisplayName + " uninstalled");
                                }
                            } catch (Exception ex) {
                                Console.WriteLine("Error:" + ex.Message);
                                Environment.Exit(1);
                            }

                            break;
                        case "--test":
                            try {
                                ParseArgs(args.Skip(1).ToArray(), out url, out hwid, out message, out locale);
                                CheckSongService checkSongService = new CheckSongService(url, hwid, message, locale);
                                try {
                                    checkSongService.TestStartupAndStop(null);
                                } catch (Exception ex) {
                                    Console.WriteLine(ex.ToString());
                                }
                            } catch (ArgumentException ex) {
                                Console.WriteLine(ex.ToString());
                                Usage(args[0]);
                                Environment.Exit(1);
                            }

                            break;
                    }
                } else {
                    ServiceBase[] ServicesToRun;
                    ParseArgs(args, out url, out hwid, out message, out locale);
                    ServicesToRun = new ServiceBase[]
                    {
                        new CheckSongService(url, hwid, message, locale)
                    };
                    ServiceBase.Run(ServicesToRun);
                }
            }

            private static void CheckAdmin()
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator)) {
                    Console.WriteLine("Error: Access Denied");
                    Console.WriteLine(
                        "The application does not have the necessary permissions to perform this action. Please run the application as an administrator.");
                    Environment.Exit(1);
                }
            }

            private static bool IsAdministrator()
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }


            private static void Usage(string cmd)
            {
                string execname = "  YoctoStopTheGamer";
                if (cmd == "test" || cmd == "install") {
                    Console.WriteLine("Usage:");
                    Console.WriteLine(execname + " " + cmd + " <URL> <HwId> [Opt]");
                    Console.WriteLine("Args:");
                    Console.WriteLine("  <URL>  : the URL to connect to the YoctoHub");
                    Console.WriteLine("  <HwId> : the HardwareID or logical name of the button to use");
                } else {
                    Console.WriteLine("Usage:");
                    Console.WriteLine(execname + " --install <URL> <HwId> [Opt] : Install the service");
                    Console.WriteLine(execname + " --uninstall                  : Uninstall the service");
                    Console.WriteLine(execname + " --test <URL> <HwId> [Opt]    : Test service without installing it");
                }
                Console.WriteLine("Options:");
                Console.WriteLine("  --msg <value>    : The message to read if the button is pressed");
                Console.WriteLine("                     Default value is \"Stop playing\"");
                Console.WriteLine("  --locale <value> : The locale of text speech to use");
                Console.WriteLine("  --help           : Help message");
            }
        }
    }