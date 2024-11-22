using System;
using Vintagestory.API.Common;

namespace BetterCratesNamespace
{    public class BetterCrates : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockClass("BBetterCrate", typeof(BetterCrateBlock));
            api.RegisterBlockEntityClass("BEBetterCrate", typeof(BetterCrateBlockEntity));
        }

        public override void StartPre(ICoreAPI api)
        {
            base.StartPre(api);
            if (api.Side == EnumAppSide.Client)
            {
                BetterCratesConfig.Current = api.LoadOrCreateConfig<BetterCratesConfig>("BetterCratesConfig.json");
            }
        }
    }
}
