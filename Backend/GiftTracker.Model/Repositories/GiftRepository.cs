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

    // All gifts for one occasion (used by OccasionController to check the buyer rule)
    public List<Gift> GetGiftsByOccasionId(int occasionId)
    {
        NpgsqlConnection? dbConn = null;
        var gifts = new List<Gift>();
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from gift where occasionid = @occasionid order by id";
            cmd.Parameters.Add("@occasionid", NpgsqlDbType.Integer).Value = occasionId;
            using var data = GetData(dbConn, cmd);
            while (data.Read())
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
                (occasionid, buyerid, name, description, price, link, status)
                values
                (@occasionid, @buyerid, @name, @description, @price, @link, @status)
                returning id";
            cmd.Parameters.AddWithValue("@occasionid", NpgsqlDbType.Integer, g.OccasionId);
            cmd.Parameters.AddWithValue("@buyerid", NpgsqlDbType.Integer, g.BuyerId);
            cmd.Parameters.AddWithValue("@name", NpgsqlDbType.Text, g.Name);
            cmd.Parameters.AddWithValue("@description", NpgsqlDbType.Text, DbValue(g.Description));
            cmd.Parameters.AddWithValue("@price", NpgsqlDbType.Numeric, DbValue(g.Price));
            cmd.Parameters.AddWithValue("@link", NpgsqlDbType.Text, DbValue(g.Link));
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
                buyerid = @buyerid,
                name = @name,
                description = @description,
                price = @price,
                link = @link,
                status = @status
                where id = @id";
            cmd.Parameters.AddWithValue("@occasionid", NpgsqlDbType.Integer, g.OccasionId);
            cmd.Parameters.AddWithValue("@buyerid", NpgsqlDbType.Integer, g.BuyerId);
            cmd.Parameters.AddWithValue("@name", NpgsqlDbType.Text, g.Name);
            cmd.Parameters.AddWithValue("@description", NpgsqlDbType.Text, DbValue(g.Description));
            cmd.Parameters.AddWithValue("@price", NpgsqlDbType.Numeric, DbValue(g.Price));
            cmd.Parameters.AddWithValue("@link", NpgsqlDbType.Text, DbValue(g.Link));
            cmd.Parameters.AddWithValue("@status", NpgsqlDbType.Text, g.Status);
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, g.Id);
            return UpdateData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Also deletes the gift's gift_group and its members (on delete cascade in the database)
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
            BuyerId = Convert.ToInt32(data["buyerid"]),
            Name = Convert.ToString(data["name"])!,
            Description = ReadNullableString(data, "description"),
            Price = ReadNullableDecimal(data, "price"),
            Link = ReadNullableString(data, "link"),
            Status = Convert.ToString(data["status"])!
        };
    }
}
