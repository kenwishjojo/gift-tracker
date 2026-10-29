using GiftTracker.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

namespace GiftTracker.Model.Repositories;

public class GiftGroupRepository : BaseRepository
{
    public GiftGroupRepository(IConfiguration configuration) : base(configuration) { }

    // The owner is not a column in gift_group, so we join group_member to find the member
    // with role 'owner'. "left join" keeps a group in the result even if no owner is found.
    private const string SelectWithOwner = @"
        select g.id, g.giftid, g.name, g.description, g.createdat, m.profileid as ownerid
        from gift_group g
        left join group_member m on m.groupid = g.id and m.role = 'owner'";

    public GiftGroup? GetGiftGroupById(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = SelectWithOwner + " where g.id = @id";
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
        var groups = new List<GiftGroup>();
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = SelectWithOwner + " order by g.id";
            using var data = GetData(dbConn, cmd);
            while (data.Read()) // every time the loop runs it reads the next row
            {
                groups.Add(ReadGiftGroup(data));
            }
            return groups;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Adds a new gift_group row AND its owner (a group_member row with role 'owner'),
    // so a group never exists without an owner. Returns the id of the new group.
    // A transaction makes the two inserts "all or nothing": if the second one fails
    // (e.g. the owner id doesn't exist), the first one is undone too.
    public int InsertGiftGroup(GiftGroup g)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            dbConn.Open();
            using var transaction = dbConn.BeginTransaction();

            var cmd = dbConn.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = @"
                insert into gift_group
                (giftid, name, description)
                values
                (@giftid, @name, @description)
                returning id";
            cmd.Parameters.AddWithValue("@giftid", NpgsqlDbType.Integer, g.GiftId);
            cmd.Parameters.AddWithValue("@name", NpgsqlDbType.Text, g.Name);
            cmd.Parameters.AddWithValue("@description", NpgsqlDbType.Text, DbValue(g.Description));
            int newId = Convert.ToInt32(cmd.ExecuteScalar());

            var ownerCmd = dbConn.CreateCommand();
            ownerCmd.Transaction = transaction;
            ownerCmd.CommandText = @"
                insert into group_member
                (groupid, profileid, role)
                values
                (@groupid, @profileid, 'owner')";
            ownerCmd.Parameters.AddWithValue("@groupid", NpgsqlDbType.Integer, newId);
            ownerCmd.Parameters.AddWithValue("@profileid", NpgsqlDbType.Integer, g.OwnerId!);
            ownerCmd.ExecuteNonQuery();

            // Only now are both rows saved. If an exception happened above, Commit is never
            // reached and the transaction is rolled back when it is disposed.
            transaction.Commit();
            return newId;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Updates the group details. The owner is not changed here (OwnerId is ignored).
    public bool UpdateGiftGroup(GiftGroup g)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                update gift_group set
                giftid = @giftid,
                name = @name,
                description = @description
                where id = @id";
            cmd.Parameters.AddWithValue("@giftid", NpgsqlDbType.Integer, g.GiftId);
            cmd.Parameters.AddWithValue("@name", NpgsqlDbType.Text, g.Name);
            cmd.Parameters.AddWithValue("@description", NpgsqlDbType.Text, DbValue(g.Description));
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, g.Id);
            return UpdateData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Also deletes the group's members (on delete cascade in the database)
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
            GiftId = Convert.ToInt32(data["giftid"]),
            Name = Convert.ToString(data["name"])!,
            Description = ReadNullableString(data, "description"),
            OwnerId = ReadNullableInt(data, "ownerid"),
            CreatedAt = Convert.ToDateTime(data["createdat"])
        };
    }
}
