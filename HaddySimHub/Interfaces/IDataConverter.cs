namespace HaddySimHub.Interfaces;

public interface IDataConverter<TInput, TOutput>
{
    /// <summary>
    /// Converts input data of type TInput to output data of type TOutput.
    /// </summary>
    /// <param name="input">The input data to convert.</param>
    /// <returns>The converted output data.</returns>
    TOutput Convert(TInput input);
}
