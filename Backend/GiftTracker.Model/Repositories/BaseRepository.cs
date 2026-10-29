using Microsoft.Extensions.Configuration;
using Npgsql;

namespace GiftTracker.Model.Repositories;

public class BaseRepository
{
    protected string ConnectionString { get; }

    public BaseRepository(IConfiguration configuration)
    {
        ConnectionString = configuration.GetConnectionString("GiftTrackerDb")
            ?? throw new InvalidOperationException("Connection string 'GiftTrackerDb' is missing in appsettings.json");
    }

    protected NpgsqlDataReader GetData(NpgsqlConnection conn, NpgsqlCommand cmd)
    {
        conn.Open();
        return cmd.ExecuteReader();
    }

    // The three methods below return true only if a row was actually changed
    // (so updating or deleting an id that doesn't exist returns false).
    protected bool InsertData(NpgsqlConnection conn, NpgsqlCommand cmd)
    {
        conn.Open();
        return cmd.ExecuteNonQuery() > 0;
    }

    protected bool UpdateData(NpgsqlConnection conn, NpgsqlCommand cmd)
    {
        conn.Open();
        return cmd.ExecuteNonQuery() > 0;
    }

    protected bool DeleteData(NpgsqlConnection conn, NpgsqlCommand cmd)
    {
        conn.Open();
        return cmd.ExecuteNonQuery() > 0;
    }

    // Inserts a row and returns the id the database gave it (the SQL must end with "returning id").
    protected int InsertAndGetId(NpgsqlConnection conn, NpgsqlCommand cmd)
    {
        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    // ---- Helpers for columns that can be NULL in the database ----

    // Reading: a NULL column comes back as DBNull.Value, which we turn into C# null
    protected static string? ReadNullableString(NpgsqlDataReader data, string column) =>
        data[column] == DBNull.Value ? null : data[column].ToString();

    protected static int? ReadNullableInt(NpgsqlDataReader data, string column) =>
        data[column] == DBNull.Value ? null : Convert.ToInt32(data[column]);

    protected static DateOnly ReadDate(NpgsqlDataReader data, string column) =>
        DateOnly.FromDateTime(Convert.ToDateTime(data[column]));

    protected static DateOnly? ReadNullableDate(NpgsqlDataReader data, string column) =>
        data[column] == DBNull.Value ? null : ReadDate(data, column);

    // Writing: C# null must be sent to the database as DBNull.Value
    protected static object DbValue(object? value) => value ?? DBNull.Value;
}
