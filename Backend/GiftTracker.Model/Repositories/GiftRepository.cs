using GiftTracker.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

namespace GiftTracker.Model.Repositories;

public class GiftRepository : BaseRepository
{
    public GiftRepository(IConfiguration configuration) : base(configuration) { }

    public Gift? GetGiftById(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from gift where id = @id";
            cmd.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;
            using var data = GetData(dbConn, cmd);
            if (data.Read())
            {
                return ReadGift(data);
            }
            return null;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public List<Gift> GetGifts()
    {
        NpgsqlConnection? dbConn = null;
        var gifts = new List<Gift>();
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from gift order by id";
            using var data = GetData(dbConn, cmd);
            while (data.Read()) // every time the loop runs it reads the next row
            {
                gifts.Add(ReadGift(data));
            }
            return gifts;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Adds a new gift row and returns the id the database gave it
    public int InsertGift(Gift g)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                insert into gift
                (occasionid, recipientid, wishitemid, description, purchasetype, status)
                values
                (@occasionid, @recipientid, @wishitemid, @description, @purchasetype, @status)
                returning id";
            cmd.Parameters.AddWithValue("@occasionid", NpgsqlDbType.Integer, g.OccasionId);
            cmd.Parameters.AddWithValue("@recipientid", NpgsqlDbType.Integer, g.RecipientId);
            cmd.Parameters.AddWithValue("@wishitemid", NpgsqlDbType.Integer, DbValue(g.WishItemId));
            cmd.Parameters.AddWithValue("@description", NpgsqlDbType.Text, g.Description);
            cmd.Parameters.AddWithValue("@purchasetype", NpgsqlDbType.Text, g.PurchaseType);
            cmd.Parameters.AddWithValue("@status", NpgsqlDbType.Text, g.Status);
            return InsertAndGetId(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool UpdateGift(Gift g)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                update gift set
                occasionid = @occasionid,
                recipientid = @recipientid,
                wishitemid = @wishitemid,
                description = @description,
                purchasetype = @purchasetype,
                status = @status
                where id = @id";
            cmd.Parameters.AddWithValue("@occasionid", NpgsqlDbType.Integer, g.OccasionId);
            cmd.Parameters.AddWithValue("@recipientid", NpgsqlDbType.Integer, g.RecipientId);
            cmd.Parameters.AddWithValue("@wishitemid", NpgsqlDbType.Integer, DbValue(g.WishItemId));
            cmd.Parameters.AddWithValue("@description", NpgsqlDbType.Text, g.Description);
            cmd.Parameters.AddWithValue("@purchasetype", NpgsqlDbType.Text, g.PurchaseType);
            cmd.Parameters.AddWithValue("@status", NpgsqlDbType.Text, g.Status);
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, g.Id);
            return UpdateData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool DeleteGift(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "delete from gift where id = @id";
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, id);
            return DeleteData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Turns one row from the database into a Gift object
    private static Gift ReadGift(NpgsqlDataReader data)
    {
        return new Gift(Convert.ToInt32(data["id"]))
        {
            OccasionId = Convert.ToInt32(data["occasionid"]),
            RecipientId = Convert.ToInt32(data["recipientid"]),
            WishItemId = ReadNullableInt(data, "wishitemid"),
            Description = Convert.ToString(data["description"])!,
            PurchaseType = Convert.ToString(data["purchasetype"])!,
            Status = Convert.ToString(data["status"])!
        };
    }
}
