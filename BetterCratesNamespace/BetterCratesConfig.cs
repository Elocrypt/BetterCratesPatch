namespace BetterCratesNamespace;

public class BetterCratesConfig
{
	public static BetterCratesConfig Current { get; set; }

	public int LabelInfoMaxRenderDistanceInBlocks { get; set; } = 50;
}
