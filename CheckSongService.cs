using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Media;
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
            _message = "The meal is ready";

            if (args.Length < 2) {
                string errorMessage = "CheckSongService require at least two arguments URL + hardwareID";
                EventLog.WriteEntry("CheckSongService", errorMessage, EventLogEntryType.Error);
                throw new ApplicationException("Missing arguments");
            }
            _url = args[0];
            _hwid = args[1];
            string errmsg = "";
            int res = YAPI.PreregisterHub(_url, ref errmsg);
            if (res != YAPI.SUCCESS) {
                string errorMessage = "YAPI.PreregisterHub failed:" + errmsg;
                EventLog.WriteEntry("CheckSongService", errorMessage, EventLogEntryType.Error);
                throw new ApplicationException(errorMessage);
            }
            _button = YAnButton.FindAnButton(_hwid);
            _synth = new SpeechSynthesizer();
            if (args.Length > 2) {
                _message = args[2];
            }
            if (args.Length > 3) {
                var readOnlyCollection = _synth.GetInstalledVoices();
                foreach (var voice in readOnlyCollection) {
                    var info = voice.VoiceInfo;
                    if (voice.VoiceInfo.Culture.Name.StartsWith(args[3])) {
                        _synth.SelectVoice(info.Name);
                        break;
                    }
                }
            }
            Console.WriteLine("Selected voice:" + _synth.Voice.Name);
            Console.WriteLine(" Name:          " + _synth.Voice.Name);
            Console.WriteLine(" Culture:       " + _synth.Voice.Culture);
            Console.WriteLine(" Age:           " + _synth.Voice.Age);
            Console.WriteLine(" Gender:        " + _synth.Voice.Gender);
            Console.WriteLine(" Description:   " + _synth.Voice.Description);

            _timer = new Timer(CheckApi, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        private void CheckApi(object state)
        {
            try {
                int isPressed = _button.get_isPressed();

                if (isPressed == YAnButton.ISPRESSED_TRUE) {
                    _synth.Speak(_message);
                }
            } catch (YAPI_Exception ex) {
                EventLog.WriteEntry("CheckSongService", ex.Message, EventLogEntryType.Error);
            }
        }

        protected override void OnStop()
        {
            _timer?.Dispose();
            _synth?.Dispose();
            _button = null;
            YAPI.FreeAPI();
        }

        public void TestStartupAndStop()
        {
            string[] strings = new[]
                { "user:kapockapoc@localhost", "YBUTTON1-2072D.anButton1", "Le repas est prêt", "fr" };
            this.OnStart(strings);
            Thread.Sleep(TimeSpan.FromMinutes(1));
            this.OnStop();
        }
    }
}