using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Net.Configuration;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Speech.Synthesis;

namespace YoctoStopTheGamer
{
    public partial class CheckSongService : ServiceBase
    {
        private Timer _timer;
        private SpeechSynthesizer _synth;
        private String _message;
        private string _hwid;
        private string _url;
        private YAnButton _button;

        public CheckSongService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _message = "Stop playing";
            string locale = "";
            if (args.Length < 2) {
                FatalError("Missing URL and button HardwareID");
            }
            _url = args[0];
            _hwid = args[1];
            for (int i = 2; i < args.Length; i++) {
                if (args[i] == "--msg") {
                    if (i + 1 >= args.Length) {
                        FatalError("Missing message after --msg");
                    }
                    _message = args[i + 1];
                } else if (args[i] == "--locale") {
                    if (i + 1 >= args.Length) {
                        FatalError("Missing local after --locale");
                    }
                    locale = args[i + 1];
                }
            }
            Console.WriteLine("Yoctopuce Lib version is :" + YAPI.GetAPIVersion());
            Console.WriteLine("Hub URL :" + _url);
            Console.WriteLine("Button  :" + _hwid);
            Console.WriteLine("Message :" + _message);
            Console.WriteLine("Locale  :" + locale);
            // Register the connection to the YoctoHub
            string errmsg = "";
            if (YAPI.PreregisterHub(_url, ref errmsg) != YAPI.SUCCESS) {
                FatalError("YAPI.PreregisterHub failed:" + errmsg);
            }
            // Instantiate YAnButton object to interact with button
            _button = YAnButton.FindAnButton(_hwid);

            // Configure Text2Speech
            _synth = new SpeechSynthesizer();
            if (locale != "") {
                var readOnlyCollection = _synth.GetInstalledVoices();
                foreach (var voice in readOnlyCollection) {
                    var info = voice.VoiceInfo;
                    if (voice.VoiceInfo.Culture.Name.StartsWith(args[3])) {
                        _synth.SelectVoice(info.Name);
                        break;
                    }
                }
            }
            Console.WriteLine("Selected voice:" + _synth.Voice.Name + " (lang=" + _synth.Voice.Culture + ")");
            _timer = new Timer(CheckApi, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        private void CheckApi(object state)
        {
            if (_button.isOnline()) {
                int isPressed = _button.get_isPressed();

                if (isPressed == YAnButton.ISPRESSED_TRUE) {
                    _synth.Speak(_message);
                }
            } else {
                string msg = "button \"" + _hwid + "\" is offline. Check arguments and connections";
                Console.WriteLine(msg);
                EventLog.WriteEntry("CheckSongService", msg, EventLogEntryType.Error);
            }
        }

        private static void FatalError(string errorMessage)
        {
            EventLog.WriteEntry("YoctoStopTheGamer", errorMessage, EventLogEntryType.Error);
            throw new ApplicationException("Missing arguments");
        }

        protected override void OnStop()
        {
            _timer?.Dispose();
            _synth?.Dispose();
            _button = null;
            YAPI.FreeAPI();
        }

        public void TestStartupAndStop(string[] args)
        {
            this.OnStart(args);
            Thread.Sleep(TimeSpan.FromMinutes(1));
            this.OnStop();
        }
    }
}