using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using System.Collections.Generic;

namespace PDT.NecDisplay.EPI
{
    public class NecDisplayFactory : EssentialsPluginDeviceFactory<PdtNecDisplay>
    {
        public NecDisplayFactory()
        {
            TypeNames = new List<string> { "necDisplay", "necmpsx" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            var config = dc.Properties.ToObject<NecDisplayConfigObject>();
            var comm = CommFactory.CreateCommForDevice(dc);

            if(comm == null)
            {
                Debug.LogMessage(Serilog.Events.LogEventLevel.Error, "Unable to create comm device");
                return null;
            }
            
            return new PdtNecDisplay(dc.Key, dc.Name, comm, config);            
        }
    }   
}