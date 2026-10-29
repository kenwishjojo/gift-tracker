using GiftTracker.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

namespace GiftTracker.Model.Repositories;

public class PreferenceItemRepository : BaseRepository
{
    public PreferenceItemRepository(IConfiguration configuration) : base(configuration) { }

    public PreferenceItem? GetPreferenceItemById(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from preference_item where id = @id";
            cmd.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;
            using var data = GetData(dbConn, cmd);
            if (data.Read())
            {
                return ReadPreferenceItem(data);
            }
            return null;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public List<PreferenceItem> GetPreferenceItems()
    {
        NpgsqlConnection? dbConn = null;
        var preferenceItems = new List<PreferenceItem>();
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from preference_item order by id";
            using var data = GetData(dbConn, cmd);
            while (data.Read()) // every time the loop runs it reads the next row
            {
                preferenceItems.Add(ReadPreferenceItem(data));
            }
            return preferenceItems;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Adds a new preference_item row and returns the id the database gave it
    public int InsertPreferenceItem(PreferenceItem pi)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                insert into preference_item
                (profileid, preferencetypeid, subject, value)
                values
                (@profileid, @preferencetypeid, @subject, @value)
                returning id";
            cmd.Parameters.AddWithValue("@profileid", NpgsqlDbType.Integer, pi.ProfileId);
            cmd.Parameters.AddWithValue("@preferencetypeid", NpgsqlDbType.Integer, pi.PreferenceTypeId);
            cmd.Parameters.AddWithValue("@subject", NpgsqlDbType.Text, DbValue(pi.Subject));
            cmd.Parameters.AddWithValue("@value", NpgsqlDbType.Text, pi.Value);
            return InsertAndGetId(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool UpdatePreferenceItem(PreferenceItem pi)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                update preference_item set
                profileid = @profileid,
                preferencetypeid = @preferencetypeid,
                subject = @subject,
                value = @value
                where id = @id";
            cmd.Parameters.AddWithValue("@profileid", NpgsqlDbType.Integer, pi.ProfileId);
            cmd.Parameters.AddWithValue("@preferencetypeid", NpgsqlDbType.Integer, pi.PreferenceTypeId);
            cmd.Parameters.AddWithValue("@subject", NpgsqlDbType.Text, DbValue(pi.Subject));
            cmd.Parameters.AddWithValue("@value", NpgsqlDbType.Text, pi.Value);
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, pi.Id);
            return UpdateData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool DeletePreferenceItem(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "delete from preference_item where id = @id";
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, id);
            return DeleteData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Turns one row from the database into a PreferenceItem object
    private static PreferenceItem ReadPreferenceItem(NpgsqlDataReader data)
    {
        return new PreferenceItem(Convert.ToInt32(data["id"]))
        {
            ProfileId = Convert.ToInt32(data["profileid"]),
            PreferenceTypeId = Convert.ToInt32(data["preferencetypeid"]),
            Subject = ReadNullableString(data, "subject"),
            Value = Convert.ToString(data["value"])!
        };
    }
}
