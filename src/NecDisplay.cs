using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Routing;
using Feedback = PepperDash.Essentials.Core.Feedback;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using Newtonsoft.Json;

namespace PDT.NecDisplay.EPI
{
	/// <summary>
	/// 
	/// </summary>
	public class PdtNecDisplay : PepperDash.Essentials.Devices.Common.Displays.TwoWayDisplayBase, IBasicVolumeWithFeedback, ICommunicationMonitor, IBridgeAdvanced
#if SERIES4
		, IHasInputs<string>
#endif
	{
		public IBasicCommunication Communication { get; private set; }
		public CommunicationGather PortGather { get; private set; }
		public StatusMonitorBase CommunicationMonitor { get; private set; }
		private int PollState = 0; 
		#region Command constants

        public const string HeaderCmd = "\x01\x30";
        public const string InputGetCmd = "\x30\x43\x30\x36\x02\x30\x30\x36\x30\x03"; 
        public const string Hdmi1Cmd = "\x30\x45\x30\x41\x02\x31\x31\x30\x36\x30\x30\x31\x31\x03"; 
        public const string Hdmi2Cmd = "\x30\x45\x30\x41\x02\x31\x31\x30\x36\x30\x30\x31\x32\x03"; 
        public const string Hdmi3Cmd = "\x30\x45\x30\x41\x02\x31\x31\x30\x36\x30\x30\x38\x32x03"; 
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
        public const string PowerPoll = "\x30\x41\x30\x36\x02\x30\x31\x64\x36\x03"; 

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

		protected override Func<bool> PowerIsOnFeedbackFunc { get { return () => _PowerIsOn; } }
		protected override Func<bool> IsCoolingDownFeedbackFunc { get { return () => _IsCoolingDown; } }
		protected override Func<bool> IsWarmingUpFeedbackFunc { get { return () => _IsWarmingUp; } }

        protected override Func<string> CurrentInputFeedbackFunc
        {
            get { return () => CurrentInput; }
        }

        public PdtNecDisplay(string key, string name, IBasicCommunication comm, NecDisplayConfigObject props)
    : base(key, name)
        {
			var id = props.ID;
            _ID = id == null ? (byte)0x2A : Convert.ToByte(id);
            Communication = comm;
            Init();
        }



        /// <summary>
        /// Constructor for IBasicCommunication with id passed from the device properties in the config file
        /// </summary>
        public PdtNecDisplay(string key, string name, IBasicCommunication comm, string id)
			: base(key, name)
		{
           _ID = id == null ? (byte)0x2A : Convert.ToByte(id); 
           Communication = comm;
			Init();
		}

        /// <summary>
        /// Constructor for IBasicCommunication when no id is in the properties of the config file
        /// </summary>
        public PdtNecDisplay(string key, string name, IBasicCommunication comm)
            : base(key, name)
        {
            _ID = (byte)0x2A; 
            Communication = comm;
            Init();
        }

		/// <summary>
		/// Constructor for TCP
		/// </summary>
        public PdtNecDisplay(string key, string name, string hostname, int port, string id)
			: base(key, name)
		{
            _ID = id == null ? (byte)0x2A : Convert.ToByte(id);
			Communication = new GenericTcpIpClient(key + "-tcp", hostname, port, 5000);
			Init();
		}

		/// <summary>
		/// Constructor for COM
		/// </summary>
        public PdtNecDisplay(string key, string name, ComPort port, ComPort.ComPortSpec spec, string id)
			: base(key, name)
		{
            _ID = id == null ? (byte)0x2A : Convert.ToByte(id); // If id is null, set default value of 0x2A (all displays command in NEC), otherwise assign value passed in constructor
			Communication = new ComPortController(key + "-com", port, spec);
			Init();
		}

		void Init()
		{
			PortGather = new CommunicationGather(Communication, '\x0d');
			PortGather.LineReceived += this.Port_LineReceived;
			CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 30000, 120000, 300000, Poll);

			InputPorts.Add(new RoutingInputPort(RoutingPortNames.HdmiIn1, eRoutingSignalType.Audio | eRoutingSignalType.Video,
				eRoutingPortConnectionType.Hdmi, new Action(InputHdmi1), this));
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

		public override bool CustomActivate()
		{
			Communication.Connect();
			CommunicationMonitor.StatusChange += (o, a) => { Debug.Console(2, this, "Communication monitor state: {0}", CommunicationMonitor.Status); };
			CommunicationMonitor.Start();
			return true;
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
			switch (PollState)
			{
				case 0: 
					
					break;
				
				default:
					PollState = 0;
					return;
			}
			//PollState++; 
			
		}

		void Port_LineReceived(object dev, GenericCommMethodReceiveTextArgs args)
		{
			if (Debug.Level == 2)
				Debug.Console(2, this, "Received: '{0}'", ComTextHelper.GetEscapedText(args.Text));

			if (args.Text == "DO SOMETHING HERE EVENTUALLY")
			{

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

            if (Convert.ToInt16(_ID) == 0)
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
			Debug.Console(2, this, "Send: '{0}'", ComTextHelper.GetEscapedText(s));
			Communication.SendText(s);
		}


		public override void PowerOn()
		{
            AppendChecksumAndSend(PowerOnCmd);
			if (!PowerIsOnFeedback.BoolValue && !_IsWarmingUp && !_IsCoolingDown)
			{
				_IsWarmingUp = true;
				IsWarmingUpFeedback.FireUpdate();
				// Fake power-up cycle
				WarmupTimer = new CTimer(o =>
				{
					_IsWarmingUp = false;
					_PowerIsOn = true;
					IsWarmingUpFeedback.FireUpdate();
					PowerIsOnFeedback.FireUpdate();
				}, WarmupTime);
			}
		}

		public override void PowerOff()
		{
			// If a display has unreliable-power off feedback, just override this and
			// remove this check.
                AppendChecksumAndSend(PowerOffCmd);
				_IsCoolingDown = true;
				_PowerIsOn = false;
				PowerIsOnFeedback.FireUpdate();
				IsCoolingDownFeedback.FireUpdate();
				// Fake cool-down cycle
				CooldownTimer = new CTimer(o =>
				{
					Debug.Console(2, this, "Cooldown timer ending");
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
		}

		public void InputHdmi2()
		{
            AppendChecksumAndSend(Hdmi2Cmd);
		}

		public void InputHdmi3()
		{
            AppendChecksumAndSend(Hdmi3Cmd);
		}

		public void InputHdmi4()
		{
            AppendChecksumAndSend(Hdmi4Cmd);
		}

		public void InputDisplayPort1()
		{
            AppendChecksumAndSend(Dp1Cmd);
		}

		public void InputDisplayPort2()
		{
            AppendChecksumAndSend(Dp2Cmd);
		}

		public void InputDvi1()
		{
            AppendChecksumAndSend(Dvi1Cmd);
		}

		public void InputVideo1()
		{
            AppendChecksumAndSend(Video1Cmd);
		}

		public void InputVga()
		{
            AppendChecksumAndSend(VgaCmd);
		}

		public void InputRgb()
		{
            AppendChecksumAndSend(RgbCmd);
		}

		public override void ExecuteSwitch(object selector)
		{
			if (selector is Action)
				(selector as Action).Invoke();
			else
				Debug.Console(1, this, "WARNING: ExecuteSwitch cannot handle type {0}", selector.GetType());
			//Send((string)selector);
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
				Items = new Dictionary<string, ISelectableItem>
				{
					{
						Hdmi1Cmd, new NecInput("HDMI1", "HDMI 1", this, Hdmi1Cmd)
					},
					{
						Hdmi2Cmd, new NecInput("HDMI2", "HDMI 2", this, Hdmi2Cmd)
					},
					{
						Dp1Cmd, new NecInput("DP1", "Display Port 1", this, Dp1Cmd)
					},
					{
						Dp2Cmd, new NecInput("DP2", "Display Port 2", this, Dp2Cmd)
					}
				}
			};

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
			//throw new NotImplementedException();
			//#warning need incrementer for these
			SetVolume(_VolumeLevel++);
		}

		public void VolumeUp(bool pressRelease)
		{
			//throw new NotImplementedException();
			SetVolume(_VolumeLevel--);
		}

		#endregion
	}

	public class NecPSXMDisplayFactory : EssentialsDeviceFactory<PdtNecDisplay>
	{
		public NecPSXMDisplayFactory()
		{
			TypeNames = new List<string>() { "necmpsx", "necdisplay" };
		}

		public override EssentialsDevice BuildDevice(DeviceConfig dc)
		{
			Debug.Console(1, "Factory Attempting to create new Generic Comm Device");
			var comm = CommFactory.CreateCommForDevice(dc);
			if (comm != null)
                try
                {
                    var props = JsonConvert.DeserializeObject<NecDisplayConfigObject>(dc.Properties.ToString());

                    return new PdtNecDisplay(dc.Key, dc.Name, comm, props);
                }
                catch
                {
                    
                    return new PdtNecDisplay(dc.Key, dc.Name, comm);
                }
			else
				return null;
		}
	}

}