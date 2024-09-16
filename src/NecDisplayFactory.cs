using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using System.Collections.Generic;

namespace PDT.NecDisplay.EPI
{
    public class NecDisplayFactory : EssentialsPluginDeviceFactory<PdtNecDisplay>
    {
        public NecDisplayFactory()
        {
            TypeNames = new List<string> { "necDisplay" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            var config = dc.Properties.ToObject<NecDisplayConfigObject>();
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