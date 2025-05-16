using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.GeneralIO;
using Newtonsoft.Json;
using PDT.NecDisplay.EPI;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Queues;
#if SERIES3
using PepperDash.Essentials.Core.Routing;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Feedback = PepperDash.Essentials.Core.Feedback;
#if SERIES4
using TwoWayDisplayBase = PepperDash.Essentials.Devices.Common.Displays.TwoWayDisplayBase;
#else
using TwoWayDisplayBase = PepperDash.Essentials.Core.TwoWayDisplayBase;
#endif


namespace PDT.NecDisplay.EPI
{
    /// <summary>
    /// 
    /// </summary>
	/// 


    public class PdtNecDisplay : TwoWayDisplayBase, IBasicVolumeWithFeedback, ICommunicationMonitor, IBridgeAdvanced
#if SERIES4
		, IHasInputs<string>
#endif
	{
		public IBasicCommunication Communication { get; private set; }
		public CommunicationGather PortGather { get; private set; }
		public StatusMonitorBase CommunicationMonitor { get; private set; }
		private int PollState = 0;
        private NecDisplayConfigObject _config;


        #region Command constants

        private GenericQueue _transmitQueue;

        public const string HeaderCmd = "\x01\x30";
        public const string InputGetCmd = "\x30\x43\x30\x36\x02\x30\x30\x36\x30\x03";
		public const string InputGetCmdAlt = "\x30\x43\x30\x36\x02\x31\x31\x30\x36\x03";
        public const string Hdmi1Cmd = "\x30\x45\x30\x41\x02\x31\x31\x30\x36\x30\x30\x31\x31\x03"; 
        public const string Hdmi2Cmd = "\x30\x45\x30\x41\x02\x31\x31\x30\x36\x30\x30\x31\x32\x03"; 
        public const string Hdmi3Cmd = "\x30\x45\x30\x41\x02\x31\x31\x30\x36\x30\x30\x38\x32\x03"; 
        public const string Hdmi4Cmd = "\x30\x45\x30\x41\x02\x31\x31\x30\x36\x30\x30\x38\x33\x03"; 
        public const string Dp1Cmd = "\x30\x45\x30\x41\x02\x30\x30\x36\x30\x30\x30\x30\x46\x03"; 
        public const string Dp2Cmd = "\x30\x45\x30\x41\x02\x30\x30\x36\x30\x30\x30\x31\x30\x03"; 
        public const string Dvi1Cmd = "\x30\x45\x30\x41\x02\x30\x30\x36\x30\x30\x30\x30\x33\x03"; 
        public const string Video1Cmd = "\x30\x45\x30\x41\x02\x30\x30\x36\x30\x30\x30\x30\x35\x03"; 
        public const string VgaCmd = "\x30\x45\x30\x41\x02\x30\x30\x36\x30\x30\x30\x30\x31\x03"; 
        public const string RgbCmd = "\x30\x45\x30\x41\x02\x30\x30\x36\x30\x30\x30\x30\x32\x03"; 

        public const string PowerOnCmd = "\x30\x41\x30\x43\x02\x43\x32\x30\x33\x44\x36\x30\x30\x30\x31\x03"; 
        public const string PowerOffCmd = "\x30\x41\x30\x43\x02\x43\x32\x30\x33\x44\x36\x30\x30\x30\x34\x03"; 
        public const string PowerToggleIrCmd = "\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x30\x33\x30\x33\x03"; 
        public const string PowerPoll = "\x30\x41\x30\x36\x02\x30\x31\x44\x36\x03"; 

        public const string MuteOffCmd = "\x30\x45\x30\x41\x02\x30\x30\x38\x44\x30\x30\x30\x30\x03"; 
        public const string MuteOnCmd = "\x30\x45\x30\x41\x02\x30\x30\x38\x44\x30\x30\x30\x31\x03"; 
        public const string MuteToggleIrCmd = "\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x31\x42\x30\x33\x03"; 
        public const string MuteGetCmd = "\x30\x43\x30\x36\x02\x30\x30\x38\x44\x03";

        public const string PictureMuteOnCmd = "\x30\x45\x30\x41\x02\x31\x30\x42\x36\x30\x30\x30\x31\x03";
        public const string PictureMuteOffCmd = "\x30\x45\x30\x41\x02\x31\x30\x42\x36\x30\x30\x30\x32\x03";

        public const string MatrixModeOnCmd = "\x30\x45\x30\x41\x02\x30\x32\x44\x33\x30\x30\x30\x32\x03";
        public const string MatrixModeOffCmd = "\x30\x45\x30\x41\x02\x30\x32\x44\x33\x30\x30\x30\x31\x03";

        public const string VolumeGetCmd = "\x30\x43\x30\x36\x02\x30\x30\x36\x32\x03"; 
        public const string VolumeLevelPartialCmd = "\x30\x45\x30\x41\x02\x30\x30\x36\x32"; 
        public const string VolumeUpCmd = "\x30\x45\x30\x41\x02\x31\x30\x41\x44\x30\x30\x30\x31\x03"; 
        public const string VolumeDownCmd = "\x30\x45\x30\x41\x02\x31\x30\x41\x44\x30\x30\x30\x32\x03"; 

        public const string MenuIrCmd = "\x41\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x32\x30\x30\x33\x03\x03\x0D";
        public const string UpIrCmd = "\x41\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x31\x35\x30\x33\x03\x05\x0D";
        public const string DownIrCmd = "\x41\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x31\x34\x30\x33\x03\x04\x0D";
        public const string LeftIrCmd = "\x41\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x32\x31\x30\x33\x03\x02\x0D";
        public const string RightIrCmd = "\x41\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x32\x32\x30\x33\x03\x01\x0D";
        public const string SelectIrCmd = "\x41\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x32\x33\x30\x33\x03\x00\x0D";
        public const string ExitIrCmd = "\x41\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x31\x46\x30\x33\x03\x76\x0D";
		#endregion

		bool _PowerIsOn;
		bool _IsWarmingUp;
		bool _IsCoolingDown;
		ushort _VolumeLevel;
		string _CurrentInput;
		bool _IsMuted;
		byte _ID;

		bool _VideoIsMuted;
		public bool VideoIsMuted
		{
			get
			{
				return _VideoIsMuted;
			}
			set
			{
				_VideoIsMuted = value;
				VideoIsMutedFeedback.FireUpdate();
			}
		}
		public BoolFeedback VideoIsMutedFeedback;


       public string CurrentInput {
           get
           {
               return _CurrentInput; 

           } 
           set
           {
               _CurrentInput = value; 
                CurrentInputFeedback.FireUpdate(); 
           }
       }

		protected override Func<bool> PowerIsOnFeedbackFunc
        {
            get
            {
                return () =>
                {                    
                    return _PowerIsOn;
                };
            }
        }

		protected override Func<bool> IsCoolingDownFeedbackFunc { get { return () => _IsCoolingDown; } }
		protected override Func<bool> IsWarmingUpFeedbackFunc { get { return () => _IsWarmingUp; } }

        protected override Func<string> CurrentInputFeedbackFunc
        {
            get { return () => CurrentInput; }
        }

        public PdtNecDisplay(string key, string name, IBasicCommunication comm, NecDisplayConfigObject props)
    : base(key, name)
        {			
            _ID = string.IsNullOrEmpty(props.Id) ? (byte)0x2A : Convert.ToByte(props.Id);
            Communication = comm;

            _config = props;


            CooldownTime = props.CooldownTime ?? 5000;
            WarmupTime = props.WarmupTime ?? 5000;

            Init();
        }

		void Init()
		{

			_transmitQueue = new GenericQueue(600, "transmit");

            PortGather = new CommunicationGather(Communication, '\x0D');
			PortGather.LineReceived += this.Port_LineReceived;
            CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 15000, 120000, 300000, Poll);




			InputPorts.Add(new RoutingInputPort(RoutingPortNames.HdmiIn1, eRoutingSignalType.Audio | eRoutingSignalType.Video,
				eRoutingPortConnectionType.Hdmi, new Action(InputHdmi1), this));


            var (hdmi1Name, hdmi1Hide) = GetFriendlyName(RoutingPortNames.HdmiIn1, "HDMI 1");
            if (!hdmi1Hide)
                {
                InputPorts.Add(new RoutingInputPort(
                    RoutingPortNames.HdmiIn1,
                    eRoutingSignalType.Audio | eRoutingSignalType.Video,
                    eRoutingPortConnectionType.Hdmi,
                    new Action(InputHdmi1),
                   // hdmi1Name,
                    this));
                }


            InputPorts.Add(new RoutingInputPort(RoutingPortNames.HdmiIn2, eRoutingSignalType.Audio | eRoutingSignalType.Video,
				eRoutingPortConnectionType.Hdmi, new Action(InputHdmi2), this));
			InputPorts.Add(new RoutingInputPort(RoutingPortNames.HdmiIn3, eRoutingSignalType.Audio | eRoutingSignalType.Video,
				eRoutingPortConnectionType.Hdmi, new Action(InputHdmi3), this));
			InputPorts.Add(new RoutingInputPort(RoutingPortNames.HdmiIn4, eRoutingSignalType.Audio | eRoutingSignalType.Video,
				eRoutingPortConnectionType.Hdmi, new Action(InputHdmi4), this));
			InputPorts.Add(new RoutingInputPort(RoutingPortNames.DisplayPortIn1, eRoutingSignalType.Audio | eRoutingSignalType.Video,
				eRoutingPortConnectionType.DisplayPort, new Action(InputDisplayPort1), this));
			InputPorts.Add(new RoutingInputPort(RoutingPortNames.DisplayPortIn2, eRoutingSignalType.Audio | eRoutingSignalType.Video,
				eRoutingPortConnectionType.DisplayPort, new Action(InputDisplayPort2), this));
			InputPorts.Add(new RoutingInputPort(RoutingPortNames.DviIn, eRoutingSignalType.Audio | eRoutingSignalType.Video,
				eRoutingPortConnectionType.Dvi, new Action(InputDvi1), this));
			InputPorts.Add(new RoutingInputPort(RoutingPortNames.CompositeIn, eRoutingSignalType.Audio | eRoutingSignalType.Video,
				eRoutingPortConnectionType.Composite, new Action(InputVideo1), this));
			InputPorts.Add(new RoutingInputPort(RoutingPortNames.VgaIn, eRoutingSignalType.Video,
				eRoutingPortConnectionType.Vga, new Action(InputVga), this));
			InputPorts.Add(new RoutingInputPort(RoutingPortNames.RgbIn, eRoutingSignalType.Video,
				eRoutingPortConnectionType.Rgb, new Action(new Action(InputRgb)), this));

			VolumeLevelFeedback = new IntFeedback(() => { return _VolumeLevel; });
			MuteFeedback = new BoolFeedback(() => _IsMuted);
			VideoIsMutedFeedback = new BoolFeedback(() => VideoIsMuted);

#if SERIES4
            SetupInputs();
#endif
		}

        ~PdtNecDisplay()
		{
			PortGather = null;
		}


        public override void Initialize()
        {
            Communication.Connect();            
            CommunicationMonitor.Start();
        }        

		public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
		{
			//PdtNecDisplayBridge.LinkToApiExt(this, trilist, joinStart, joinMapKey);
			LinkDisplayToApi(this, trilist, joinStart, joinMapKey, bridge);
		}

		public override FeedbackCollection<Feedback> Feedbacks
		{
			get
			{
				var list = base.Feedbacks;
				list.AddRange(new List<Feedback>
				{

				});
				return list;
			}
		}
		public void Poll()
		{
			AppendChecksumAndSend(PowerPoll);
			AppendChecksumAndSend(InputGetCmdAlt);
			switch (PollState)
			{
				case 0: 
					
					break;
				
				default:
					PollState = 0;
					return;
			}		
		}

		void Port_LineReceived(object dev, GenericCommMethodReceiveTextArgs args)
		{
			//Debug.Console(2, this, "Gathered: '{0}'", ComTextHelper.GetEscapedText(args.Text));

			var bytes = Encoding.GetEncoding(28591).GetBytes(args.Text);

			if (bytes[3] == _ID + 0x40)
			{
				//Debug.Console(2, this, "ID Match!");

				if (bytes.Length > 20 && (bytes[9] == 0x32 && bytes[10] == 0x30 && bytes[11] == 0x30 && bytes[12] == 0x44 && bytes[13] == 0x36))
				{
					//Debug.Console(2, this, "Power State Response...");

					switch (bytes[23])
					{
                        //case 0x31:
                        //	{
                        //		Debug.Console(0, this, "Device is On");
                        //		_PowerIsOn = true;
                        //		PowerIsOnFeedback.FireUpdate();
                        //		break;
                        //	}
                        case 0x31:
                            {
                                _PowerIsOn = true;
                                PowerIsOnFeedback.FireUpdate();
                                break;
                            }
						case 0x32:
							{
								//Debug.Console(2, this, "Device is in Standby");
								_PowerIsOn = false;
								PowerIsOnFeedback.FireUpdate();
								break;
							}
						case 0x34:
							{
								//Debug.Console(2, this, "Device is Off");
								_PowerIsOn = false;
								PowerIsOnFeedback.FireUpdate();
								break;
							}
						default:
							break;
					}
				}
				else if (bytes.Length > 20 && (bytes[9] == 0x30 && bytes[10] == 0x43 && bytes[11] == 0x32 && bytes[12] == 0x30 && bytes[13] == 0x33))
				{
					//Debug.Console(2, this, "Power State Response...");

					switch (bytes[19])
					{
						case 0x31:
							{
								//Debug.Console(2, this, "Device is On");
								_PowerIsOn = true;
								PowerIsOnFeedback.FireUpdate();
								break;
							}
						default:
							break;
					}
				}
				else if (bytes.Length > 20 && (bytes[10] == 0x31 && bytes[11] == 0x31 && bytes[12] == 0x30 && bytes[13] == 0x36 ||
					bytes[10] == 0x30 && bytes[11] == 0x30 && bytes[12] == 0x36 && bytes[13] == 0x30))
				{
					//Debug.Console(2, this, "Input State Response...");
#if SERIES4
					// Compare the relevant portion of the response to the key for each input to find a match
					foreach (var input in Inputs.Items)
					{
						if (input.Key.Equals(Encoding.GetEncoding(28591).GetString(bytes, 14, 10)))
						{
							//Debug.Console(2, this, "Input Match: {0}", input.Value.Name);
							input.Value.IsSelected = true;
							Inputs.CurrentItem = input.Value.Name;
							CurrentInput = input.Value.Name;
						}
						else
						{
							input.Value.IsSelected = false;
						}
					}
#else
#endif
				}
			}
		}

        int CalculateChecksum(string s)
        {
            int x = 0;
            for (int i = 1; i < s.Length; i++)
                x = x ^ s[i];

            return x;
        }

		public void AppendChecksumAndSend(string s)
		{
            int x;

            if (Convert.ToInt16(_ID) == 0 || _ID == 0x2A)
            {
               s = HeaderCmd + "\x2a" + s;
            }
            else
            {
               s = HeaderCmd + Convert.ToChar(Convert.ToInt16(_ID) + Convert.ToInt16('\x40')).ToString() + s;
            }
 
            x = CalculateChecksum(s);

			string send = s + (char)x + '\x0d';
			Send(send);
		}

		void Send(string s)
		{
            // Temp debug
            //Debug.Console(2, this, "Send: '{0}'", ComTextHelper.GetEscapedText(s));
			var bytes = Encoding.GetEncoding(28591).GetBytes(s);
			_transmitQueue.Enqueue(new ComsMessage(Communication, bytes));
			//Communication.SendText(s);
		}


		public override void PowerOn()
		{
            if (_PowerIsOn)
            {
                Debug.Console(1, this, "Display is on");
                return;
            }

            if(_IsCoolingDown || _IsWarmingUp)
            {
                Debug.Console(1,this, "State is changing");
                return;
            }

            AppendChecksumAndSend(PowerOnCmd);
			Poll();
			
			
			_IsWarmingUp = true;
			IsWarmingUpFeedback.FireUpdate();
			// Fake power-up cycle
			WarmupTimer = new CTimer(o =>
			{
                Debug.Console(1, this, "Warmup complete");
				_IsWarmingUp = false;
				IsWarmingUpFeedback.FireUpdate();
			}, WarmupTime);
			
		}

		public override void PowerOff()
		{
            if(!_PowerIsOn)
            {
                Debug.Console(1,this, "Display is off");
                return;
            }

            if(_IsWarmingUp || _IsCoolingDown)
            {
                Debug.Console(1, this, "State is changing");
                return;
            }
			// If a display has unreliable-po
            //
            // appdebug 1
            // er off feedback, just override this and
			// remove this check.
            AppendChecksumAndSend(PowerOffCmd);
			Poll();

			_IsCoolingDown = true;
			IsCoolingDownFeedback.FireUpdate();
			// Fake cool-down cycle
			CooldownTimer = new CTimer(o =>
			{
                Debug.Console(1, this, "Cooldown complete");
				_IsCoolingDown = false;
				IsCoolingDownFeedback.FireUpdate();
			}, CooldownTime);
		}

		public override void PowerToggle()
		{
			if (PowerIsOnFeedback.BoolValue && !IsWarmingUpFeedback.BoolValue)
				PowerOff();
			else if (!PowerIsOnFeedback.BoolValue && !IsCoolingDownFeedback.BoolValue)
				PowerOn();
		}

        public void PictureMuteOn()
        {
            AppendChecksumAndSend(PictureMuteOnCmd);
			VideoIsMuted = true;
        }

        public void PictureMuteOff()
        {
            AppendChecksumAndSend(PictureMuteOffCmd);
			VideoIsMuted = false;
        }

        public void PictureMuteToggle()
        {
			Debug.Console(2, this, "PictureMuteToggle: '{0}'", VideoIsMuted);
			if (!VideoIsMuted)
			{
				PictureMuteOn();
			}
			else
			{
				PictureMuteOff();
			}

        }

        public void MatrixModeOn()
        {
            AppendChecksumAndSend(MatrixModeOnCmd);
        }

        public void MatrixModeOff()
        {
            AppendChecksumAndSend(MatrixModeOffCmd);
        }

		public void InputHdmi1()
		{
            AppendChecksumAndSend(Hdmi1Cmd);
			Poll();
		}

		public void InputHdmi2()
		{
            AppendChecksumAndSend(Hdmi2Cmd);
            Poll();
        }

        public void InputHdmi3()
		{
            AppendChecksumAndSend(Hdmi3Cmd);
            Poll();
        }

        public void InputHdmi4()
		{
            AppendChecksumAndSend(Hdmi4Cmd);
            Poll();
        }

        public void InputDisplayPort1()
		{
            AppendChecksumAndSend(Dp1Cmd);
            Poll();
        }

        public void InputDisplayPort2()
		{
            AppendChecksumAndSend(Dp2Cmd);
            Poll();
        }

        public void InputDvi1()
		{
            AppendChecksumAndSend(Dvi1Cmd);
            Poll();
        }

        public void InputVideo1()
		{
            AppendChecksumAndSend(Video1Cmd);
            Poll();
        }

        public void InputVga()
		{
            AppendChecksumAndSend(VgaCmd);
            Poll();
        }

        public void InputRgb()
		{
            AppendChecksumAndSend(RgbCmd);
            Poll();
        }

        public override void ExecuteSwitch(object selector)
		{
            if (_PowerIsOn)
            {
#if SERIES4
                if (!(selector is Action switchInput)) return;
#else
                var switchInput = selector as Action;

                if(switchInput == null)
                {
                    return;
                }
#endif

                switchInput();

                return;
            }
#if SERIES4
            void handler(object sender, FeedbackEventArgs args)
            {
                if (_IsWarmingUp)
                    return;

                IsWarmingUpFeedback.OutputChange -= handler;


                if (!(selector is Action switchInput)) return;
                switchInput = selector as Action;                

                switchInput();
            }
#else
            EventHandler<FeedbackEventArgs> handler = null;

            handler = (o, a) => {
                if(_IsWarmingUp)
                    return;

                IsWarmingUpFeedback.OutputChange -= handler;

                var switchInput = selector as Action;

                if(switchInput == null)
                {
                    return;
                }

                switchInput();
            };
#endif

            IsWarmingUpFeedback.OutputChange += handler;

            PowerOn();
		}

		void SetVolume(ushort level)
		{
			var levelString = string.Format("{0}{1:X4}\x03", VolumeLevelPartialCmd, level);
			AppendChecksumAndSend(levelString);
			//Debug.Console(2, this, "Volume:{0}", ComTextHelper.GetEscapedText(levelString));
			_VolumeLevel = level;
			VolumeLevelFeedback.FireUpdate();
		}

		#region IBasicVolumeWithFeedback Members

		public IntFeedback VolumeLevelFeedback { get; private set; }

		public BoolFeedback MuteFeedback { get; private set; }

#if SERIES4
        public ISelectableItems<string> Inputs{ get; private set; }

        private void SetupInputs()
            {
            Inputs = new NecInputs
                {
                Items = new Dictionary<string, ISelectableItem>()
                };

            void AddInput(string inputKey, string defaultName, string protocolKey, string command)
                {
                var (friendlyName, hide) = GetFriendlyName(inputKey, defaultName);
                if (!hide)
                    {
                    Inputs.Items.Add(protocolKey, new NecInput(inputKey, friendlyName, this, command));
                    }
                }

            AddInput("HDMI1", "HDMI 1", "\x30\x30\x30\x30\x38\x38\x30\x30\x31\x31", Hdmi1Cmd);
            AddInput("HDMI2", "HDMI 2", "\x30\x30\x30\x30\x38\x38\x30\x30\x31\x32", Hdmi2Cmd);
            AddInput("DP1", "Display Port 1", "\x30\x30\x30\x30\x38\x38\x30\x30\x30\x46", Dp1Cmd);
            }

#endif

        public void MuteOff()
		{
            AppendChecksumAndSend(MuteOffCmd);
		}

		public void MuteOn()
		{
            AppendChecksumAndSend(MuteOnCmd);
		}

		void IBasicVolumeWithFeedback.SetVolume(ushort level)
		{
			SetVolume(level);
		}

		#endregion

		#region IBasicVolumeControls Members

		public void MuteToggle()
		{
			Send(MuteToggleIrCmd);
		}

		public void VolumeDown(bool pressRelease)
		{			
			//#warning need incrementer for these
			SetVolume(_VolumeLevel++);
		}

		public void VolumeUp(bool pressRelease)
		{			
			SetVolume(_VolumeLevel--);
		}
        private (string name, bool hide) GetFriendlyName(string inputKey, string defaultName)
            {
            if (_config?.FriendlyNames != null)
                {
                var match = _config.FriendlyNames.FirstOrDefault(f => f.InputKey == inputKey);
                if (match != null)
                    return (match.Name ?? defaultName, match.HideInput);
                }
            return (defaultName, false);
            }

		#endregion
	}
}