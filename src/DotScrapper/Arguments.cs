//TODO: Optimize code.

namespace DotScrapper
{
    /// <summary>
    /// Arguments utils.
    /// </summary>
    public static class Arguments
    {
        private static readonly List<ArgumentDefinition> _arguments 
            = new();

        public static IList<ArgumentDefinitionRef>? Default { get; set; }

        public static ArgumentDefinition Add(ArgumentDefinition def)
        {
            _arguments.Add(def);
            return def;
        }

        public static void Clear()
            => _arguments.Clear();

        public static IEnumerable<ArgumentDefinition> GetEnumerable()
            => _arguments
                .AsEnumerable();

        public static IList<ArgumentDefinitionRef> LoadArguments(string[] args)
        {
            List<ArgumentDefinitionRef> refs = new List<ArgumentDefinitionRef>();

            for (var i = 0; i < args.Length; i++)
            {
                if (!args[i].Contains('-')) 
                    continue;

                var lowerKey = args[i]
                    .ToLowerInvariant()
                    .Replace("-", null);

                // search for argument
                var argument 
                    = _arguments
                        .FirstOrDefault(x => x.Name.ToLowerInvariant() == lowerKey);

                if (argument != null)
                {
                    refs.Add(new ArgumentDefinitionRef(argument, i
                        // It will try to get the next value for the argument if it's in range, and check if the next argument is not an argument name.
                        , (args.Length - 1) == i ? null : args[i + 1].Contains("-") ? null : args[i + 1]));
                }
            }

            // second pass for small key.
            for (var i = 0; i < args.Length; i++)
            {
                // skip if the argument is a key and he was not processed in the first-pass.
                if(args[i].Contains('-') && refs.Exists(x => x.Index == i))
                    continue;

                if (args[i].Contains('-') && !refs.Exists(x => x.Index == i))
                {
                    var lowerKey = args[i]
                        .ToLowerInvariant()
                        .Replace("-", null);

                    // will match all arguments registered with small key, and add it to the refs.
                    refs.AddRange(
                        _arguments.Where(x =>
                                x.SmallName != null &&
                                lowerKey.Contains(x.SmallName, StringComparison.OrdinalIgnoreCase))
                            .Select(argument => new ArgumentDefinitionRef(argument, i,
                                // It will try to get the next value for the argument if it's in range, and check if the next argument is not an argument name.
                                (args.Length - 1) == i ? null : args[i + 1].Contains("-") ? null : args[i + 1])));
                }
            }

            return refs;
        }

        public static bool HasArguments(ArgumentDefinition def)
            => Default != null && Default.Any(x => x.Ref == def);

        public static IEnumerable<ArgumentDefinitionRef>? GetArguments(ArgumentDefinition def)
            => Default?.Where(x => x.Ref == def);

        public static string? GetArgumentsData(ArgumentDefinition def)
            => GetArguments(def)?.FirstOrDefault(x => x.Value != null)?.Value;

        /*/// <summary>
        /// Check if an arguments is here.
        /// Argument is like -(key) or --(key) -...(key), smallKey can be combined like -a -b -c -> -abc
        /// </summary>
        /// <param name="args">Main(string[] args) arguments</param>
        /// <param name="key">The normal key to check.</param>
        /// <param name="smallKey">The small key to check or null if there is no small key.</param>
        /// <returns>If an argument of that key or that smallKey is found in args.</returns>
        public static bool HasArguments(string[] args, string key, string? smallKey = null)
        {
            var lowerKey = key.ToLowerInvariant();
            var lowerSmallKey = smallKey?.ToLowerInvariant();

            var keys = args
                .Where(x => x.Contains("-"))
                .Select(x => x.ToLowerInvariant().Replace("-", null))
                .ToArray();

            return keys.Any(x => x == lowerKey) || lowerSmallKey != null && keys.Any(x=>x.Contains(lowerSmallKey, StringComparison.OrdinalIgnoreCase));
        }

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
        }*/
    }

    public class ArgumentDefinition
    {
        public string Name { get; set; }
        public string? SmallName { get; set; }

        public ArgumentDefinition(string name, string? smallName)
        {
            if(name == null)
                throw new ArgumentNullException(nameof(name));

            Name = name;
            SmallName = smallName;
        }

        public bool IsPresent()
            => Arguments.HasArguments(this);

        public string? GetActualData(string? @default = null)
            => Arguments.GetArgumentsData(this) ?? @default;
    }

    public class ArgumentDefinitionRef
    {
        public ArgumentDefinition Ref { get; }
        public string? Value { get; }

        public int Index { get; }

        public ArgumentDefinitionRef(ArgumentDefinition def, int index, string? value)
        {
            Ref = def;
            Index = index;
            Value = value;
        }

        public override string ToString()
            => $"--{Ref.Name} at {Index} is {Value ?? "<null>"}";
    }
}
