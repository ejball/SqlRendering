using System;

namespace SqlRendering
{
	public sealed class SqlFragment : IEquatable<SqlFragment>
	{
		public SqlFragment(string sql) => m_sql = sql ?? throw new ArgumentNullException(nameof(sql));

		public override string ToString() => m_sql;

		public bool Equals(SqlFragment? other) => other is object && m_sql == other.m_sql;

		public override bool Equals(object? obj) => obj is SqlFragment fragment && Equals(fragment);

		public override int GetHashCode() => m_sql.GetHashCode();

		private readonly string m_sql;
	}
}
