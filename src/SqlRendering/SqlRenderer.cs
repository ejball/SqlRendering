using System;
using System.Globalization;

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

		protected virtual string RenderNullCore() => "NULL";

		protected virtual string RenderStringCore(string value) => $"'{value.Replace("'", "''")}'";

		protected virtual string RenderBooleanCore(bool value) => value ? "1" : "0";

		private sealed class SqlFormatProvider : IFormatProvider, ICustomFormatter
		{
			public SqlFormatProvider(SqlRenderer renderer) => m_renderer = renderer;

			public object GetFormat(Type formatType) => this;

			public string Format(string? format, object? arg, IFormatProvider formatProvider)
			{
				if (format == "raw")
					return arg is string stringValue ? stringValue : throw new FormatException("Format 'raw' can only be used with strings.");
				else if (format == "literal")
					return m_renderer.LiteralOrNull(arg).ToString();
				else if (format is object)
					throw new FormatException($"Format '{format}' is not supported.");
				else if (arg is SqlFragment fragment)
					return fragment.ToString();
				else
					throw new FormatException("Argument requires a format, e.g. {value:literal}.");
			}

			private readonly SqlRenderer m_renderer;
		}
	}
}
