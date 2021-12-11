namespace AOEMods.Essence;

public static class StreamEnumerableUtil
{
    /// <summary>
    /// Wraps an enumerable and restores the stream's position to its original position every time
    /// the enumerable is iterated.
    /// </summary>
    /// <typeparam name="TResult">Enumerable result type</typeparam>
    /// <param name="stream">Stream for which to restore the position when iterating the enumerable.</param>
    /// <param name="enumerable">Enumerable to wrap.</param>
    /// <returns>Wrapped enumerable that restores its stream's position to the original position.</returns>
    public static IEnumerable<TResult> WithStreamPosition<TResult>(Stream stream, IEnumerable<TResult> enumerable)
        => WithPosition(stream, stream.Position, enumerable);

    /// <summary>
    /// Wraps an enumerable and sets the stream's position to the given position every time
    /// the enumerable is iterated.
    /// </summary>
    /// <typeparam name="TResult">Enumerable result type</typeparam>
    /// <param name="stream">Stream for which to set the position when iterating the enumerable.</param>
    /// <param name="position">Position to set stream's position to before iterating the enumerable.</param>
    /// <param name="enumerable">Enumerable to wrap.</param>
    /// <returns>Wrapped enumerable that restores its stream's position to the given position.</returns>
    public static IEnumerable<TResult> WithPosition<TResult>(Stream stream, long position, IEnumerable<TResult> enumerable)
    {
        IEnumerable<TResult> WrappedEnumerable()
        {
            stream.Position = position;
            foreach (var item in enumerable)
            {
                yield return item;
            }
        }
        return WrappedEnumerable();
    }
}
