using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SqlRendering
{
	public class SqlRenderer
	{
		public string Format(FormattableString formattable) => formattable.ToString(new SqlFormatProvider(this));

		public SqlFragment Raw(string value) => new SqlFragment(value);

		public SqlFragment Null => Raw(RenderNullCore());

		public SqlFragment Literal(object value)
		{
			switch (value ?? throw new ArgumentNullException(nameof(value)))
			{
			case string stringValue:
				return Raw(RenderStringCore(stringValue));
			case bool boolValue:
				return Raw(RenderBooleanCore(boolValue));
			case IFormattable formattable when value is int || value is long || value is short || value is float || value is double || value is decimal:
				return Raw(formattable.ToString(null, CultureInfo.InvariantCulture));
			default:
				throw new ArgumentException($"Type cannot be rendered as a literal: {value.GetType().FullName}", nameof(value));
			}
		}

		public SqlFragment LiteralOrNull(object? value) => value is null ? Null : Literal(value);

		public SqlFragment List(params SqlFragment[] fragments) => List(fragments.AsEnumerable());

		public SqlFragment List(IEnumerable<SqlFragment> fragments)
		{
			var sqls = fragments.Select(x => x.ToString()).ToList();
			if (sqls.Count == 0)
				throw new ArgumentException("List must not be empty.", nameof(fragments));
			return Raw(string.Join(", ", sqls));
		}

		protected virtual string RenderNullCore() => "NULL";

		protected virtual string RenderStringCore(string value) => $"'{value.Replace("'", "''")}'";

		protected virtual string RenderBooleanCore(bool value) => value ? "1" : "0";

		private sealed class SqlFormatProvider : IFormatProvider, ICustomFormatter
		{
			public SqlFormatProvider(SqlRenderer renderer) => m_renderer = renderer;

			public object GetFormat(Type formatType) => this;

			public string Format(string? format, object? arg, IFormatProvider formatProvider)
			{
				switch (format)
				{
				case "raw":
					if (arg is string stringValue)
						return stringValue;
					throw new FormatException("Format 'raw' can only be used with strings.");

				case "literal":
					return m_renderer.LiteralOrNull(arg).ToString();

				case "list":
					if (arg is IEnumerable<SqlFragment> list)
						return m_renderer.List(list).ToString();
					throw new FormatException("Format 'list' can only be used with a collection of fragments.");

				case "literal-list":
					if (arg is IEnumerable<object> literalList)
						return m_renderer.List(literalList.Select(m_renderer.LiteralOrNull)).ToString();
					throw new FormatException("Format 'literal-list' can only be used with a collection of literals.");

				default:
					if (format is object)
						throw new FormatException($"Format '{format}' is not supported.");
					if (arg is SqlFragment fragment)
						return fragment.ToString();
					throw new FormatException("Argument requires a format, e.g. {value:literal}.");
				}
			}

			private readonly SqlRenderer m_renderer;
		}
	}
}
