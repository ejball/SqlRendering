using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using static SqlRendering.Tests.FluentAction;

namespace SqlRendering.Tests
{
	[TestFixture]
	[SuppressMessage("ReSharper", "InterpolatedStringExpressionIsNotIFormattable")]
	public class SqlRendererTests
	{
		[Test]
		public void NullLiteral()
		{
			Sql.Null.ToString().Should().Be("NULL");
			Sql.LiteralOrNull(null).ToString().Should().Be("NULL");
		}

		[Test]
		public void StringLiterals()
		{
			Sql.Literal("text").ToString().Should().Be("'text'");
			Sql.Literal("").ToString().Should().Be("''");
			Sql.Literal("Bob's").ToString().Should().Be("'Bob''s'");
		}

		[Test]
		public void BooleanLiterals()
		{
			Sql.Literal(true).ToString().Should().Be("1");
			Sql.Literal(false).ToString().Should().Be("0");
		}

		[Test]
		public void NumberLiterals()
		{
			Sql.Literal(42).ToString().Should().Be("42");
			Sql.Literal(-42L).ToString().Should().Be("-42");
			Sql.Literal(short.MinValue).ToString().Should().Be("-32768");
			Sql.Literal(3.14f).ToString().Should().Be("3.14");
			Sql.Literal(3.1415).ToString().Should().Be("3.1415");
			Sql.Literal(867.5309m).ToString().Should().Be("867.5309");
		}

		[Test]
		public void BadLiterals()
		{
			Invoking(() => Sql.Literal(new object())).Should().Throw<ArgumentException>();
			Invoking(() => Sql.LiteralOrNull(new object())).Should().Throw<ArgumentException>();
		}

		[Test]
		public void ListOfLiterals()
		{
			Sql.List(Sql.Literal("hi"), Sql.Null, Sql.Literal(42)).ToString().Should().Be("'hi', NULL, 42");
		}

		[Test]
		public void EmptyList()
		{
			Invoking(() => Sql.List()).Should().Throw<ArgumentException>();
		}

		[Test]
		public void FormatLiteral()
		{
			var id = 123;
			var name = "it's";
			var select = "select * from widgets";
			Sql.Format($"{select:raw} where id = {id:literal} and name = {name:literal}")
				.Should().Be("select * from widgets where id = 123 and name = 'it''s'");
		}

		[Test]
		public void RawMustBeString()
		{
			Invoking(() => Sql.Format($"select * from widgets where created = {DateTime.UtcNow:raw}"))
				.Should().Throw<FormatException>();
		}

		[Test]
		public void FormatFragment()
		{
			Sql.Format($"select * from widgets where created is {Sql.Null};")
				.Should().Be("select * from widgets where created is NULL;");
		}

		[Test]
		public void FormatList()
		{
			var ids = new List<string> { "one", "two", "three" };
			Sql.Format($"select * from widgets where id in ({ids.Select(Sql.Literal):list});")
				.Should().Be("select * from widgets where id in ('one', 'two', 'three');");
			Sql.Format($"select * from widgets where id in ({ids:literal-list});")
				.Should().Be("select * from widgets where id in ('one', 'two', 'three');");
		}

		[Test]
		public void MissingFormat()
		{
			var name = "it's";
			Invoking(() => Sql.Format($"select * from widgets where name = {name}"))
				.Should().Throw<FormatException>();
		}

		[Test]
		public void UnknownFormat()
		{
			var name = "it's";
			Invoking(() => Sql.Format($"select * from widgets where name = {name:liberal}"))
				.Should().Throw<FormatException>();
		}

		private static SqlRenderer Sql { get; } = new SqlRenderer();
	}
}
