using GiftTracker.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

namespace GiftTracker.Model.Repositories;

// gift_contribution is a junction table: a row is identified by (giftid, groupmemberid),
// so every method takes both ids instead of one id. Only the amount can be updated.
public class GiftContributionRepository : BaseRepository
{
    public GiftContributionRepository(IConfiguration configuration) : base(configuration) { }

    public GiftContribution? GetGiftContribution(int giftId, int groupMemberId)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from gift_contribution where giftid = @giftid and groupmemberid = @groupmemberid";
            cmd.Parameters.AddWithValue("@giftid", NpgsqlDbType.Integer, giftId);
            cmd.Parameters.AddWithValue("@groupmemberid", NpgsqlDbType.Integer, groupMemberId);
            using var data = GetData(dbConn, cmd);
            if (data.Read())
            {
                return ReadGiftContribution(data);
            }
            return null;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public List<GiftContribution> GetGiftContributions()
    {
        NpgsqlConnection? dbConn = null;
        var contributions = new List<GiftContribution>();
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from gift_contribution order by giftid, groupmemberid";
            using var data = GetData(dbConn, cmd);
            while (data.Read())
            {
                contributions.Add(ReadGiftContribution(data));
            }
            return contributions;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool InsertGiftContribution(GiftContribution c)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                insert into gift_contribution
                (giftid, groupmemberid, amount)
                values
                (@giftid, @groupmemberid, @amount)";
            cmd.Parameters.AddWithValue("@giftid", NpgsqlDbType.Integer, c.GiftId);
            cmd.Parameters.AddWithValue("@groupmemberid", NpgsqlDbType.Integer, c.GroupMemberId);
            cmd.Parameters.AddWithValue("@amount", NpgsqlDbType.Numeric, c.Amount);
            return InsertData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool UpdateGiftContribution(GiftContribution c)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                update gift_contribution set
                amount = @amount
                where giftid = @giftid and groupmemberid = @groupmemberid";
            cmd.Parameters.AddWithValue("@amount", NpgsqlDbType.Numeric, c.Amount);
            cmd.Parameters.AddWithValue("@giftid", NpgsqlDbType.Integer, c.GiftId);
            cmd.Parameters.AddWithValue("@groupmemberid", NpgsqlDbType.Integer, c.GroupMemberId);
            return UpdateData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool DeleteGiftContribution(int giftId, int groupMemberId)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "delete from gift_contribution where giftid = @giftid and groupmemberid = @groupmemberid";
            cmd.Parameters.AddWithValue("@giftid", NpgsqlDbType.Integer, giftId);
            cmd.Parameters.AddWithValue("@groupmemberid", NpgsqlDbType.Integer, groupMemberId);
            return DeleteData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    private static GiftContribution ReadGiftContribution(NpgsqlDataReader data)
    {
        return new GiftContribution
        {
            GiftId = Convert.ToInt32(data["giftid"]),
            GroupMemberId = Convert.ToInt32(data["groupmemberid"]),
            Amount = Convert.ToDecimal(data["amount"])
        };
    }
}
