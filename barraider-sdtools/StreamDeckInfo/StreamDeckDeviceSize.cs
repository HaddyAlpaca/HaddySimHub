using Newtonsoft.Json;

namespace BarRaider.SdTools
{
    /// <summary>
    /// Layout of the keys on the StreamDeck hardware device
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="rows"></param>
    /// <param name="cols"></param>
    public class StreamDeckDeviceSize(int rows, int cols)
    {
        /// <summary>
        /// Number of key rows on the StreamDeck hardware device
        /// </summary>
        [JsonProperty(PropertyName = "rows")]
        public int Rows { get; private set; } = rows;

        /// <summary>
        /// Number of key columns on the StreamDeck hardware device
        /// </summary>
        [JsonProperty(PropertyName = "columns")]
        public int Cols { get; private set; } = cols;

        /// <summary>
        /// Shows class information as string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Rows: {Rows} Columns: {Cols}";
        }
    }
}
