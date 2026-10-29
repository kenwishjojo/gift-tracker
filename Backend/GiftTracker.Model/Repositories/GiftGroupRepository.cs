using GiftTracker.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

namespace GiftTracker.Model.Repositories;

public class GiftGroupRepository : BaseRepository
{
    public GiftGroupRepository(IConfiguration configuration) : base(configuration) { }

    public GiftGroup? GetGiftGroupById(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from gift_group where id = @id";
            cmd.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;
            using var data = GetData(dbConn, cmd);
            if (data.Read())
            {
                return ReadGiftGroup(data);
            }
            return null;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public List<GiftGroup> GetGiftGroups()
    {
        NpgsqlConnection? dbConn = null;
        var giftGroups = new List<GiftGroup>();
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from gift_group order by id";
            using var data = GetData(dbConn, cmd);
            while (data.Read()) // every time the loop runs it reads the next row
            {
                giftGroups.Add(ReadGiftGroup(data));
            }
            return giftGroups;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Adds a new gift_group row and returns the id the database gave it
    public int InsertGiftGroup(GiftGroup g)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                insert into gift_group
                (name, invitecode)
                values
                (@name, @invitecode)
                returning id";
            cmd.Parameters.AddWithValue("@name", NpgsqlDbType.Text, g.Name);
            cmd.Parameters.AddWithValue("@invitecode", NpgsqlDbType.Text, DbValue(g.InviteCode));
            return InsertAndGetId(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool UpdateGiftGroup(GiftGroup g)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                update gift_group set
                name = @name,
                invitecode = @invitecode
                where id = @id";
            cmd.Parameters.AddWithValue("@name", NpgsqlDbType.Text, g.Name);
            cmd.Parameters.AddWithValue("@invitecode", NpgsqlDbType.Text, DbValue(g.InviteCode));
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, g.Id);
            return UpdateData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool DeleteGiftGroup(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "delete from gift_group where id = @id";
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, id);
            return DeleteData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Turns one row from the database into a GiftGroup object
    private static GiftGroup ReadGiftGroup(NpgsqlDataReader data)
    {
        return new GiftGroup(Convert.ToInt32(data["id"]))
        {
            Name = Convert.ToString(data["name"])!,
            InviteCode = ReadNullableString(data, "invitecode"),
            CreatedAt = Convert.ToDateTime(data["createdat"])
        };
    }
}
