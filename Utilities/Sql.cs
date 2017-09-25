namespace Utilities
{
    public static class Sql
    {
        public static string ToSqlString(this string sql)
        {
            return sql.Replace("'", "''");
        }
    }
}