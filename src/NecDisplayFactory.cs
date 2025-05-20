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
            MinimumEssentialsFrameworkVersion = "1.16.0";
            TypeNames = new List<string> { "necDisplay", "necmpsx" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            var config = dc.Properties.ToObject<NecDisplayConfigObject>();
            var comm = CommFactory.CreateCommForDevice(dc);

            if(comm == null)
            {
#if SERIES4
                Debug.LogMessage(Serilog.Events.LogEventLevel.Error, "Unable to create comm device");
#else
                Debug.Console(0, "Unable to create comm device");
#endif
                return null;
            }
            
            return new PdtNecDisplay(dc.Key, dc.Name, comm, config);            
        }
    }   
}