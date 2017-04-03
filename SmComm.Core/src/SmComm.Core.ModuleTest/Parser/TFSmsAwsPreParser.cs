#if DEBUG
using System;
using System.Collections.Generic;
using Xunit;
using SmComm.Core.Parser;

namespace SmComm.Core.Parser.ModuleTest
{
	
	public class TFSmsAwsPreParser {
	

		[Fact]
		public void ParserTest1() {
			// SmsAwsPreParser parser = new SmsAwsPreParser();
		    var s = string.Empty;
            IList<string> results = SmsAwsPreParser.FindSmsAwsSubMessages(s);
			Assert.True(results.Count==0);
			s  = "TO:HMEINMARSATC1@VAISALA.COM\r\n";
			s += "SUBJECT:JABALALKAWR\r\n";
			s += "(S:JABALALKAWR;D:050914;T:170000;PR:0.6\r\n"; 
			s += "S:JABALALKAWR;D:050914;T:180000;PR:0.4\r\n"; 
			s += "S:JABALALKAWR;D:050914;T:190000;PR:0.0\r\n"; 
			s += "S:JABALALKAWR;D:050914;T:200000;PR:0.0\r\n"; 
			s += "S:JABALALKAWR;D:050914;T:210000;PR:0.0\r\n";
			s += "S:JABALALKAWR;D:050914;T:220000;PR:0.0\r\n"; 
			s += "S:JABALALKAWR;D:050914;T:230000;PR:0.0\r\n"; 
			s += "S:JABALALKAWR;D:050915;T:000000;PR:0.0)";

            results = SmsAwsPreParser.FindSmsAwsSubMessages(s);
			Assert.True(results.Count==8);
	
		} // ParserTest1


		[Fact]
		public void ParserTest2() {
			// SmsAwsPreParser parser = new SmsAwsPreParser();

           
			var s  = "TO:HMEINMARSATC1@VAISALA.COM\r\n";
			s += "SUBJECT:JABALALKAWR\r\n";
			s += "(S:JABALMISHT;D:050912;T:000200;ERR:0;BAT:13.5;PRS:0)"; 
			
			var results = SmsAwsPreParser.FindSmsAwsSubMessages(s);
			Assert.True(results.Count==1);
            //foreach (var s1 in results) {
            //    Assert.True(s1.Length>0);
            //    Assert.True(!s1[0].Equals('('), "( at beginning");
            //    Assert.True(!s1[s1.Length-1].Equals(')'), ") at end");
            //}	
		} // ParserTest2

		[Fact]
		public void ParserTest3() {
			// SmsAwsPreParser parser = new SmsAwsPreParser();
			var s   = "TO:HMEINMARSATC1@VAISALA.COM\r\n";
			s += "SUBJECT:JABALALKAWR\r\n";
			s += "(S:JABALALKAWR;D:050914;T:090000;PR:0.2\r\n"; 
			s += "S:JABALALKAWR;D:050914;T:100000;PR:0.4\r\n"; 
			s += "S:JABALALKAWR;D:050914;T:110000;PR:0.0\r\n"; 
			s += "S:JABALALKAWR;D:050914;T:120000;PR:0.0\r\n"; 
			s += "S:JABALALKAWR;D:050914;T:130000;PR:0.0\r\n";
			s += "S:JABALALKAWR;D:050914;T:140000;PR:0.0\r\n"; 
			s += "S:JABALALKAWR;D:050914;T:150000;PR:0.0 \r\n"; 
			s += "S:JABALALKAWR;D:050914;T:160000;PR:0.6)"; 
			
			var results = SmsAwsPreParser.FindSmsAwsSubMessages(s);
			Assert.True(results.Count==8);

		} // ParserTest3

		[Fact]
		public void  ParserTest4() {
			// SmsAwsPreParser parser = new SmsAwsPreParser();
			var s   = "TO:HMEINMARSATC1@VAISALA.COM\r\n";
			s += "SUBJECT:JABALALKAWR\r\n";
			s += "(S:JABALALKAWR;D:050912;T:000200;ERR:0;BAT:13.5;PRS:0)"; 
			
			var results =SmsAwsPreParser.FindSmsAwsSubMessages(s);
			Assert.True(results.Count==1);
            //foreach (var s1 in results)
            //{
            //    Assert.True(s1.Length > 0);
            //    Assert.True(!s1[0].Equals('('), "( at beginning");
            //    Assert.True(!s1[s1.Length - 1].Equals(')'), ") at end");
            //}	
		}  // ParserTest4


		[Fact]
		public void ParserTest5() {
			// SmsAwsPreParser parser = new SmsAwsPreParser();
			var s = String.Empty;
			IList<string> results = SmsAwsPreParser.FindSmsAwsSubMessages(s);
			Assert.True(results.Count==0);
			s  = "TO:HMEINMARSATC1@VAISALA.COM\r\n";
			s += "SUBJECT:JABALALKAWR\r\n";
			s += "(S:JABALALKAWR;D:050914;T:170000;PR:0.6\r\n"; 
			s += "S:JABALALKAWR;D:050914;T:180000;PR:0.4\r\n"; 
			s += "S:JABALALKAWR;D:050914;T:190000;PR:0.0\r\n"; 
			s += "S:JABALALKAWR;D:050914;T:200000;PR:0.0\r\n"; 
			s += "S:JABALALKAWR;D:050914;T:210000;PR:0.0\r\n";
			s += "S:JABALALKAWR;D:050914;T:220000;PR:0.0\r\n"; 
			s += "S:JABALALKAWR;D:050914;T:230000;PR:0.0\r\n"; 
			s += "S:JABALALKAWR;D:050915;T:000000;PR:0.0\r\n)";
			
			results = SmsAwsPreParser.FindSmsAwsSubMessages(s);
			Assert.True(results.Count==8);
	
		} // ParserTest1
	} // class

}
#endif