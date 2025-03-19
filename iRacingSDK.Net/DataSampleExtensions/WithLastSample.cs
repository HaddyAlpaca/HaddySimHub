namespace iRacingSDK;

public static partial class DataSampleExtensions
{
	/// <summary>
	/// Mixes in the LastSample field 
	/// Also disconnect the link list - so only the immediate sample has ref to last sample.
	/// </summary>
	public static IEnumerable<DataSample> WithLastSample(this IEnumerable<DataSample> samples)
	{
		DataSample lastDataSample = null;

		foreach (var data in samples)
		{
			data.LastSample = lastDataSample;
			if (lastDataSample != null)
				lastDataSample.LastSample = null;
			lastDataSample = data;

			yield return data;
		}
	}
}

