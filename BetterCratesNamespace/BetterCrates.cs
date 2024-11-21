using Vintagestory.API.Common;

namespace BetterCratesNamespace;

public class BetterCrates : ModSystem
{
	public override void Start(ICoreAPI api)
	{
		((ModSystem)this).Start(api);
		((ICoreAPICommon)api).RegisterBlockClass("BBetterCrate", typeof(BetterCrateBlock));
		((ICoreAPICommon)api).RegisterBlockEntityClass("BEBetterCrate", typeof(BetterCrateBlockEntity));
	}

	public override void StartPre(ICoreAPI api)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		((ModSystem)this).StartPre(api);
		if ((int)api.Side == 2)
		{
			BetterCratesConfig.Current = api.LoadOrCreateConfig<BetterCratesConfig>("BetterCratesConfig.json");
		}
	}
}
