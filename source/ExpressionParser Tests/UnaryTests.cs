﻿using NUnit.Framework;
using Stareater.Utils.Collections;

namespace ExpressionParser_Tests
{
	[TestFixture]
	public class UnaryTests
	{
		[Test]
		public void NegateConstNegative()
		{
			var test = new ParserTester("-5", null, null, -5);
			Assert.IsTrue(test.IsOK, test.Message);
		}
		
		[Test]
		public void NegateConstPositive()
		{
			var test = new ParserTester("-(-5)", null, null, 5);
			Assert.IsTrue(test.IsOK, test.Message);
		}
		
		[Test]
		public void NegateVarNegative()
		{
			var test = new ParserTester("-a", null, new Var("a", 4), -4);
			Assert.IsTrue(test.IsOK, test.Message);
		}
		
		[Test]
		public void NegateVarPositive()
		{
			var test = new ParserTester("-a", null, new Var("a", -4), 4);
			Assert.IsTrue(test.IsOK, test.Message);
		}
		
		[Test]
		public void NegateToBooleanConst()
		{
			var test = new ParserTester("-'5", null, null, -1);
			Assert.IsTrue(test.IsOK, test.Message);
		}
		
		[Test]
		public void NegateToBooleanVar()
		{
			var test = new ParserTester("-'x", null, new Var("x", -2), 1);
			Assert.IsTrue(test.IsOK, test.Message);
		}
		
		[Test]
		public void ToBooleanConstNegative()
		{
			var test = new ParserTester("'5", null, null, 1);
			Assert.IsTrue(test.IsOK, test.Message);
		}
		
		[Test]
		public void ToBooleanConstPositive()
		{
			var test = new ParserTester("'(-5)", null, null, -1);
			Assert.IsTrue(test.IsOK, test.Message);
		}
		
		[Test]
		public void ToBooleanConstZero()
		{
			var test = new ParserTester("'0", null, null, 1);
			Assert.IsTrue(test.IsOK, test.Message);
		}
		
		[Test]
		public void ToBooleanVarNegative()
		{
			var test = new ParserTester("'a", null, new Var("a", 4), 1);
			Assert.IsTrue(test.IsOK, test.Message);
		}
		
		[Test]
		public void ToBooleanVarPositive()
		{
			var test = new ParserTester("'a", null, new Var("a", -4), -1);
			Assert.IsTrue(test.IsOK, test.Message);
		}
	}
}
