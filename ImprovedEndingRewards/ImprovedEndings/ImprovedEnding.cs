using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImprovedEndingRewards.ImprovedEndings
{
    public interface IImprovedEnding
    {
        public bool IsEnabled { get; }
        public void LogMessage(object data);
        public void SetHooks();
        public void UnsetHooks();
        public void Config();
    }
}
