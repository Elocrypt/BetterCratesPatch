using System;
using System.Runtime.CompilerServices;
using Vintagestory.API.Common;

namespace BetterCratesNamespace
{
    public static class ApiExtensions
    {
        public static TConfig LoadOrCreateConfig<TConfig>(this ICoreAPI api, string filename) where TConfig : new()
        {
            try
            {
                TConfig loadedConfig = api.LoadModConfig<TConfig>(filename);
                if (loadedConfig != null)
                {
                    return loadedConfig;
                }
            }
            catch (Exception e)
            {
                ILogger logger = api.World.Logger;
                string text = "{0}";
                object[] array = new object[1];
                int num = 0;
                DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(55, 2);
                defaultInterpolatedStringHandler.AppendLiteral("Failed loading file (");
                defaultInterpolatedStringHandler.AppendFormatted(filename);
                defaultInterpolatedStringHandler.AppendLiteral("), error ");
                defaultInterpolatedStringHandler.AppendFormatted<Exception>(e);
                defaultInterpolatedStringHandler.AppendLiteral(". Will initialize new one");
                array[num] = defaultInterpolatedStringHandler.ToStringAndClear();
                logger.Error(text, array);
            }
            TConfig newConfig = Activator.CreateInstance<TConfig>();
            api.StoreModConfig<TConfig>(newConfig, filename);
            return newConfig;
        }
    }
}
