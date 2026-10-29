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

    // The member with role 'owner' in a group, or null if the group has none
    // (used by GroupMemberController to check the "exactly one owner" rule)
    public GroupMember? GetOwnerOfGroup(int groupId)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from group_member where groupid = @groupid and role = 'owner'";
            cmd.Parameters.Add("@groupid", NpgsqlDbType.Integer).Value = groupId;
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
        var members = new List<GroupMember>();
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from group_member order by id";
            using var data = GetData(dbConn, cmd);
            while (data.Read()) // every time the loop runs it reads the next row
            {
                members.Add(ReadGroupMember(data));
            }
            return members;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Adds a new group_member row and returns the id the database gave it
    public int InsertGroupMember(GroupMember m)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                insert into group_member
                (groupid, profileid, role)
                values
                (@groupid, @profileid, @role)
                returning id";
            cmd.Parameters.AddWithValue("@groupid", NpgsqlDbType.Integer, m.GroupId);
            cmd.Parameters.AddWithValue("@profileid", NpgsqlDbType.Integer, m.ProfileId);
            cmd.Parameters.AddWithValue("@role", NpgsqlDbType.Text, m.Role);
            return InsertAndGetId(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool UpdateGroupMember(GroupMember m)
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
                role = @role
                where id = @id";
            cmd.Parameters.AddWithValue("@groupid", NpgsqlDbType.Integer, m.GroupId);
            cmd.Parameters.AddWithValue("@profileid", NpgsqlDbType.Integer, m.ProfileId);
            cmd.Parameters.AddWithValue("@role", NpgsqlDbType.Text, m.Role);
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, m.Id);
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
            ProfileId = Convert.ToInt32(data["profileid"]),
            Role = Convert.ToString(data["role"])!,
            JoinedAt = Convert.ToDateTime(data["joinedat"])
        };
    }
}
