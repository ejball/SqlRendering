using System;
using System.Globalization;

namespace SqlRendering
{
	public class SqlRenderer
	{
		public string Format(FormattableString formattable) => formattable.ToString(new SqlFormatProvider(this));

		public string RenderNull() => RenderNullCore();

		public string RenderString(string value) => RenderStringCore(value ?? throw new ArgumentNullException(nameof(value)));

		public string RenderBoolean(bool value) => RenderBooleanCore(value);

		public string RenderLiteral(object value)
		{
			if (TryRenderLiteral(value, out string text))
				return text;

			throw new ArgumentException($"Type cannot be rendered as a literal: {value.GetType().FullName}", nameof(value));
		}

		public bool TryRenderLiteral(object value, out string text)
		{
			text = null;

			if (value == null)
				text = RenderNullCore();
			else if (value is string stringValue)
				text = RenderStringCore(stringValue);
			else if (value is bool boolValue)
				text = RenderBooleanCore(boolValue);
			else if (value is IFormattable formattable && (value is int || value is long || value is short || value is float || value is double || value is decimal))
				text = formattable.ToString(null, CultureInfo.InvariantCulture);

			return text != null;
		}

		protected virtual string RenderNullCore() => "NULL";

		protected virtual string RenderStringCore(string value) => $"'{value.Replace("'", "''")}'";

		protected virtual string RenderBooleanCore(bool value) => value ? "1" : "0";

		private sealed class SqlFormatProvider : IFormatProvider, ICustomFormatter
		{
			public SqlFormatProvider(SqlRenderer sqlRenderer)
			{
				m_sqlRenderer = sqlRenderer;
			}

			public object GetFormat(Type formatType) => this;

			public string Format(string format, object arg, IFormatProvider formatProvider)
			{
				if (format == "raw")
				{
					if (arg is string stringValue)
						return stringValue;
					if (arg == null)
						return "";

					throw new FormatException("Format 'raw' can only be used with strings.");
				}
				else if (format == "literal")
				{
					if (m_sqlRenderer.TryRenderLiteral(arg, out string text))
						return text;

					throw new FormatException($"Format 'literal' cannot be used with type: {arg.GetType().FullName}");
				}
				else if (format != null)
				{
					throw new FormatException($"Format '{format}' is not supported.");
				}
				else
				{
					throw new FormatException($"Argument of type '{arg?.GetType().FullName}' requires a format, e.g. {{value:literal}}.");
				}
			}

			private readonly SqlRenderer m_sqlRenderer;
		}
	}
}
