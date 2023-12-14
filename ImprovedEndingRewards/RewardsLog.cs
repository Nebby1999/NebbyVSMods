using BepInEx.Logging;
using Moonstorm.Loaders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImprovedEndingRewards
{
    public class RewardsLog : LogLoader<RewardsLog>
    {
        public override ManualLogSource LogSource { get; protected set; }

        public override BreakOnLog BreakOn => BreakOnLog.None;
        public RewardsLog(ManualLogSource logSource) : base(logSource)
        {
        }

    }
}
