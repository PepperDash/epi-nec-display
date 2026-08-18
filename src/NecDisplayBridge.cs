using System;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PDT.NecDisplay.EPI
{
    public static class PdtNecDisplayBridge
    {
        private const uint TunerChannelUpJoin = 31;
        private const uint TunerChannelDownJoin = 32;

        public static void LinkToApiExt(this PdtNecDisplay displayDevice, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            if (joinStart == 0) throw new ArgumentOutOfRangeException(nameof(joinStart));
            _ = joinMapKey;

            var joinOffset = joinStart - 1;

            trilist.SetSigTrueAction(TunerChannelUpJoin + joinOffset, displayDevice.TunerChannelUp);
            trilist.SetSigTrueAction(TunerChannelDownJoin + joinOffset, displayDevice.TunerChannelDown);
        }
    }
}
