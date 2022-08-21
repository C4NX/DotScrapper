namespace DotScrapper
{
    /// <summary>
    /// Arguments utils.
    /// </summary>
    public static class Arguments
    {
        /// <summary>
        /// Check if an arguments is here.
        /// Argument is like -(key) or --(key) -...(key), smallKey can be combined like -a -b -c -> -abc
        /// </summary>
        /// <param name="args">Main(string[] args) arguments</param>
        /// <param name="key">The normal key to check.</param>
        /// <param name="smallKey">The small key to check or null if there is no small key.</param>
        /// <returns>If an argument of that key or that smallKey is found in args.</returns>
        public static bool HasArguments(string[] args, string key, string? smallKey = null)
            => args.Any(x =>
            {
                if (x.Contains('-'))
                {
                    var currentKeyName = x.ToLowerInvariant()
                        .Replace("-", null);

                    return currentKeyName == key
                           || (smallKey != null && currentKeyName.Contains(smallKey, StringComparison.OrdinalIgnoreCase));
                }

                return false;
            });

        /// <summary>
        /// Get the data value of an argument, more likely the right value of a key.
        /// Example: 
        /// <example>
        ///     --key "My Value"
        /// </example>
        /// </summary>
        /// <param name="args">Main(string[] args) arguments</param>
        /// <param name="key">The main key to get.</param>
        /// <param name="smallKey">The small key to get.</param>
        /// <returns>data argument or null</returns>
        public static string? GetArgumentData(string[] args, string key, string? smallKey = null)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Contains('-'))
                {
                    var currentKeyName = args[i].ToLower()
                        .Replace("-", null);
                    if (currentKeyName == key
                        || (smallKey != null && currentKeyName.Contains(smallKey, StringComparison.OrdinalIgnoreCase)))
                    {
                        if ((args.Length - 1) == i)
                            return null;
                        else
                            return args[i + 1];
                    }
                }
            }

            return null;
        }
    }
}
