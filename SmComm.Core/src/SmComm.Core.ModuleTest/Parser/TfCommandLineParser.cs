#if DEBUG

using Xunit;
using SmSimple.Core;
using SmComm.Core.Parser;

namespace SmComm.Core.Parser.ModuleTest
{
	
	public class TFCommandLineParser {
		

		[Fact]
		public void ParserTest()
		{

			string[] args = { @"-size = 100", @"/height:'400'" ,"-param1 \"Nice stuff !\"", "--debug"};
			var commandLineParser = new CommandLineParser();
			var ok = commandLineParser.Parse(args);
			Assert.True(ok);

			//Assert.True(commandLineParser["param1"] == "Nice stuff !" );
			Assert.True(commandLineParser["height"] == "400");
			Assert.True(commandLineParser["width"] == null);
			Assert.True(commandLineParser["size"] == "100");
			Assert.True(commandLineParser["debug"] == "true");

		} // ParserTest1

        /*
        private static void ValidateResults(IEnumerable<string> results)
		{
			foreach (var s1 in results)
			{
				Assert.True(s1.Length > 0);
				Assert.True(s1.StartsWith("MEAS\t"), "Does not start with MEAS\t");
				Assert.True(s1.EndsWith(Environment.NewLine), "Does not end properly");
				var m = new MeasMsg();
				Assert.True(m.Initialise(s1), "Could not initialise measmsg");
			}
			
		}
*/

	} // class
}
#endif