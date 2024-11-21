using System;
using Vintagestory.API.Common;

namespace BetterCratesNamespace;

public static class ApiExtensions
{
	public static TConfig LoadOrCreateConfig<TConfig>(this ICoreAPI api, string filename) where TConfig : new()
	{
		try
		{
			TConfig loadedConfig = ((ICoreAPICommon)api).LoadModConfig<TConfig>(filename);
			if (loadedConfig != null)
			{
				return loadedConfig;
			}
		}
		catch (Exception value)
		{
			api.World.Logger.Error("{0}", new object[1] { $"Failed loading file ({filename}), error {value}. Will initialize new one" });
		}
		TConfig newConfig = new TConfig();
		((ICoreAPICommon)api).StoreModConfig<TConfig>(newConfig, filename);
		return newConfig;
	}
}
