﻿using NUnit.Framework;
using Stareater.Utils.Collections;

namespace ExpressionParser_Tests
{
	[TestFixture]
	public class SummationTest
	{
		[Test]
		public void AdditionConst()
		{
			var test = new ParserTester("2+2", null, null, 4);
			Assert.IsTrue(test.IsOK, test.Message);
		}

		[Test]
		public void AdditionVarBoth()
		{
			var test = new ParserTester("x+y", null, new Var("x", 2).And("y", 3), 5);
			Assert.IsTrue(test.IsOK, test.Message);
		}

		[Test]
		public void AdditionVarLeft()
		{
			var test = new ParserTester("x+2", null, new Var("x", 2), 4);
			Assert.IsTrue(test.IsOK, test.Message);
		}

		[Test]
		public void AdditionVarRight()
		{
			var test = new ParserTester("2+x", null, new Var("x", 3), 5);
			Assert.IsTrue(test.IsOK, test.Message);
		}

		[Test]
		public void SubstractionConst()
		{
			var test = new ParserTester("2-5", null, null, -3);
			Assert.IsTrue(test.IsOK, test.Message);
		}

		[Test]
		public void SubstractionVarBoth()
		{
			var test = new ParserTester("x-y", null, new Var("x", 2).And("y", 5), -3);
			Assert.IsTrue(test.IsOK, test.Message);
		}

		[Test]
		public void SubstractionVarLeft()
		{
			var test = new ParserTester("x-5", null, new Var("x", 2), -3);
			Assert.IsTrue(test.IsOK, test.Message);
		}

		[Test]
		public void SubstractionVarRight()
		{
			var test = new ParserTester("2-x", null, new Var("x", 5), -3);
			Assert.IsTrue(test.IsOK, test.Message);
		}

		[Test]
		public void SegmentSummationConst()
		{
			var test = new ParserTester("1-2+3+6-1+9", null, null, 16);
			Assert.IsTrue(test.IsOK, test.Message);
		}

		[Test]
		public void SegmentSummationVar()
		{
			var test = new ParserTester("a-2+b+6-a+9", null, new Var("a", 1).And("b", 3), 16);
			Assert.IsTrue(test.IsOK, test.Message);
		}
	}
}
