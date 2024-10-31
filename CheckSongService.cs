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
        private string _locale;

        public CheckSongService(string url, string hwid, string message, string locale)
        {
            InitializeComponent();
            _message = message;
            _locale = locale;
            _hwid = hwid;
            _url = url;
        }

        protected override void OnStart(string[] args)
        {
            Console.WriteLine("Yoctopuce Lib version is :" + YAPI.GetAPIVersion());
            Console.WriteLine("Hub URL : " + _url);
            Console.WriteLine("Button  : " + _hwid);
            Console.WriteLine("Message : " + _message);
            if (_locale != "") {
                Console.WriteLine("Locale  : " + _locale);
            }
            // Register the connection to the YoctoHub
            string errmsg = "";
            if (YAPI.PreregisterHub(_url, ref errmsg) != YAPI.SUCCESS) {
                FatalError("YAPI.PreregisterHub failed:" + errmsg);
            }
            // Instantiate YAnButton object to interact with button
            _button = YAnButton.FindAnButton(_hwid);

            // Configure Text2Speech
            _synth = new SpeechSynthesizer();
            if (_locale != "") {
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
            _timer = new Timer(CheckButton, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        private void CheckButton(object state)
        {
            if (_button.isOnline()) {
                int isPressed = _button.get_isPressed();

                if (isPressed == YAnButton.ISPRESSED_TRUE) {
                    _synth.Speak(_message);
                }
            } else {
                string msg = "button \"" + _hwid + "\" is offline. Check arguments and connections";
                Console.WriteLine(msg);
                EventLog.WriteEntry("YoctoStopTheGamerService", msg, EventLogEntryType.Error);
            }
        }

        private static void FatalError(string errorMessage)
        {
            EventLog.WriteEntry("YoctoStopTheGamerService", errorMessage, EventLogEntryType.Error);
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