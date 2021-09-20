using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core.Config;
using System.Collections;
using PepperDash.Essentials.Bridges;
using PepperDash.Essentials.Core;

namespace PDT.NecDisplay.EPI
{
	public class PdtNecDisplayEpi
	{
		public static void LoadPlugin()
		{
			PepperDash.Essentials.Core.DeviceFactory.AddFactoryForType("necDisplay", PdtNecDisplayEpi.BuildDevice);
		}

		public static string MinimumEssentialsFrameworkVersion = "1.4.23";

		public static PdtNecDisplay BuildDevice(DeviceConfig dc)
		{
			var config = JsonConvert.DeserializeObject<DeviceConfig>(dc.Properties.ToString());
			var comm = CommFactory.CreateCommForDevice(dc);
            try
            {
               // if there is no id in the config file an exception is thrown
               var newMe = new PdtNecDisplay(dc.Key, dc.Name, comm, dc.Properties["id"].Value<string>());
               return newMe;
            }
            catch
            {
                // if there is no id in the config file an exception is thrown.  id will default to (0x2a) the all displays command
                var newMe = new PdtNecDisplay(dc.Key, dc.Name, comm);
                return newMe;
            }
		}
	}
}