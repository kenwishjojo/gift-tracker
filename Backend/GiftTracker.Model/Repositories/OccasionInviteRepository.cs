using GiftTracker.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

namespace GiftTracker.Model.Repositories;

// SQL for the occasion_invite table – same pattern as the other repositories
public class OccasionInviteRepository : BaseRepository
{
    public OccasionInviteRepository(IConfiguration configuration) : base(configuration) { }

    public OccasionInvite? GetInviteById(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from occasion_invite where id = @id";
            cmd.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;
            using var data = GetData(dbConn, cmd);
            if (data.Read())
            {
                return ReadInvite(data);
            }
            return null;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public List<OccasionInvite> GetInvites()
    {
        NpgsqlConnection? dbConn = null;
        var invites = new List<OccasionInvite>();
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from occasion_invite order by id";
            using var data = GetData(dbConn, cmd);
            while (data.Read()) // every time the loop runs it reads the next row
            {
                invites.Add(ReadInvite(data));
            }
            return invites;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Adds one invite and returns the id the database gave it
    public int InsertInvite(OccasionInvite i)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                insert into occasion_invite
                (occasionid, profileid)
                values
                (@occasionid, @profileid)
                returning id";
            cmd.Parameters.AddWithValue("@occasionid", NpgsqlDbType.Integer, i.OccasionId);
            cmd.Parameters.AddWithValue("@profileid", NpgsqlDbType.Integer, i.ProfileId);
            return InsertAndGetId(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool DeleteInvite(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "delete from occasion_invite where id = @id";
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, id);
            return DeleteData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Replaces all invites of one occasion with a new list of profiles, in ONE transaction:
    // first delete the old invites, then insert one row per profile. If any insert fails
    // (e.g. a wrong profile id), Commit is never reached and the old invites are kept.
    // Used by the occasion form, which sends the whole list of ticked friends when saving.
    public void ReplaceInvites(int occasionId, List<int> profileIds)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            dbConn.Open();
            using var transaction = dbConn.BeginTransaction();

            var deleteCmd = dbConn.CreateCommand();
            deleteCmd.Transaction = transaction;
            deleteCmd.CommandText = "delete from occasion_invite where occasionid = @occasionid";
            deleteCmd.Parameters.AddWithValue("@occasionid", NpgsqlDbType.Integer, occasionId);
            deleteCmd.ExecuteNonQuery();

            foreach (int profileId in profileIds)
            {
                var insertCmd = dbConn.CreateCommand();
                insertCmd.Transaction = transaction;
                insertCmd.CommandText = @"
                    insert into occasion_invite
                    (occasionid, profileid)
                    values
                    (@occasionid, @profileid)";
                insertCmd.Parameters.AddWithValue("@occasionid", NpgsqlDbType.Integer, occasionId);
                insertCmd.Parameters.AddWithValue("@profileid", NpgsqlDbType.Integer, profileId);
                insertCmd.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Turns one row from the database into an OccasionInvite object
    private static OccasionInvite ReadInvite(NpgsqlDataReader data)
    {
        return new OccasionInvite(Convert.ToInt32(data["id"]))
        {
            OccasionId = Convert.ToInt32(data["occasionid"]),
            ProfileId = Convert.ToInt32(data["profileid"]),
            InvitedAt = Convert.ToDateTime(data["invitedat"])
        };
    }
}
