using GiftTracker.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

namespace GiftTracker.Model.Repositories;

public class GroupMemberRepository : BaseRepository
{
    public GroupMemberRepository(IConfiguration configuration) : base(configuration) { }

    public GroupMember? GetGroupMemberById(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from group_member where id = @id";
            cmd.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;
            using var data = GetData(dbConn, cmd);
            if (data.Read())
            {
                return ReadGroupMember(data);
            }
            return null;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public List<GroupMember> GetGroupMembers()
    {
        NpgsqlConnection? dbConn = null;
        var groupMembers = new List<GroupMember>();
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from group_member order by id";
            using var data = GetData(dbConn, cmd);
            while (data.Read()) // every time the loop runs it reads the next row
            {
                groupMembers.Add(ReadGroupMember(data));
            }
            return groupMembers;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Adds a new group_member row and returns the id the database gave it
    public int InsertGroupMember(GroupMember g)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                insert into group_member
                (groupid, profileid, guestname, role)
                values
                (@groupid, @profileid, @guestname, @role)
                returning id";
            cmd.Parameters.AddWithValue("@groupid", NpgsqlDbType.Integer, g.GroupId);
            cmd.Parameters.AddWithValue("@profileid", NpgsqlDbType.Integer, DbValue(g.ProfileId));
            cmd.Parameters.AddWithValue("@guestname", NpgsqlDbType.Text, DbValue(g.GuestName));
            cmd.Parameters.AddWithValue("@role", NpgsqlDbType.Text, g.Role);
            return InsertAndGetId(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool UpdateGroupMember(GroupMember g)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                update group_member set
                groupid = @groupid,
                profileid = @profileid,
                guestname = @guestname,
                role = @role
                where id = @id";
            cmd.Parameters.AddWithValue("@groupid", NpgsqlDbType.Integer, g.GroupId);
            cmd.Parameters.AddWithValue("@profileid", NpgsqlDbType.Integer, DbValue(g.ProfileId));
            cmd.Parameters.AddWithValue("@guestname", NpgsqlDbType.Text, DbValue(g.GuestName));
            cmd.Parameters.AddWithValue("@role", NpgsqlDbType.Text, g.Role);
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, g.Id);
            return UpdateData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool DeleteGroupMember(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "delete from group_member where id = @id";
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, id);
            return DeleteData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Turns one row from the database into a GroupMember object
    private static GroupMember ReadGroupMember(NpgsqlDataReader data)
    {
        return new GroupMember(Convert.ToInt32(data["id"]))
        {
            GroupId = Convert.ToInt32(data["groupid"]),
            ProfileId = ReadNullableInt(data, "profileid"),
            GuestName = ReadNullableString(data, "guestname"),
            Role = Convert.ToString(data["role"])!
        };
    }
}
