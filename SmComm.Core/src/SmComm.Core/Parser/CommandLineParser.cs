using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using SmSimple.Core;

namespace SmComm.Core.Parser
{
    /// <summary>
    /// Arguments class
    /// </summary>
    public sealed class CommandLineParser{
        // Variables
		private readonly Dictionary<string, string>  parameters = new Dictionary<string, string>();


      
		public bool Parse(string[] args)
		{
			try
			{
				DoParse(args);
				return true;
			}
			catch (Exception ex)
			{
				ExceptionRecorder.RecordException("Exception in command line parser " + ex.Message);
				return false;
			}
		}



        private void DoParse(IEnumerable<string> args)
		{
			var splitter = new Regex(@"^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled);

			var remover = new Regex(@"^['""]?(.*?)['""]?$",
				RegexOptions.IgnoreCase | RegexOptions.Compiled);

			string Parameter = null;
			 

			// Valid parameters forms:
			// {-,/,--}param{ ,=,:}((",')value(",'))
			// Examples: 
			// -param1 value1 --param2 /param3:"Test-:-work" 
			//   /param4=happy -param5 '--=nice=--'
			foreach (var txt in args)
			{
				// Look for new parameters (-,/ or --) and a
				// possible enclosed value (=,:)
                var Parts = splitter.Split(txt, 3);

				switch (Parts.Length)
				{
					// Found a value (for the last parameter 
					// found (space separator))
					case 1:
						if (Parameter != null)
						{
							if (!parameters.ContainsKey(Parameter))
							{
								Parts[0] =remover.Replace(Parts[0], "$1");

								AddParam(Parameter, Parts[0]);
							}
							Parameter = null;
						}
						// else Error: no parameter waiting for a value (skipped)
						break;

					// Found just a parameter
					case 2:
						// The last parameter is still waiting. 
						// With no value, set it to true.
						if (Parameter != null)
						{
							if (!parameters.ContainsKey(Parameter))
								AddParam(Parameter, "true");
						}
						Parameter = Parts[1];
						break;

					// Parameter with enclosed value
					case 3:
						// The last parameter is still waiting. 
						// With no value, set it to true.
						if (Parameter != null)
						{
							if (!parameters.ContainsKey(Parameter))
								AddParam(Parameter, "true");
						}

						Parameter = Parts[1];

						// Remove possible enclosing characters (",')
						if (!parameters.ContainsKey(Parameter))
						{
							Parts[2] = remover.Replace(Parts[2], "$1");
							AddParam(Parameter, Parts[2]);
						}

						Parameter = null;
						break;
				}
			}
			// In case a parameter is still waiting
			if (Parameter != null)
			{
				if (!parameters.ContainsKey(Parameter))
					AddParam(Parameter, "true");
			}
		}

		private void AddParam(string key, string data) {
			parameters.Add(key.Trim(), data.Trim());
		}

        // Retrieve a parameter value if it exists 
        // (overriding C# indexer property)
        public string this [string key]
        {
            get
            {
				if (parameters.ContainsKey(key))
					return(parameters[key]);
				return null;
            }
        }

		public int Count { get { return parameters.Count; } }
    }
}
