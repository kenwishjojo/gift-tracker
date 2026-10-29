using GiftTracker.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

namespace GiftTracker.Model.Repositories;

public class WishlistRepository : BaseRepository
{
    public WishlistRepository(IConfiguration configuration) : base(configuration) { }

    public Wishlist? GetWishlistById(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from wishlist where id = @id";
            cmd.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;
            using var data = GetData(dbConn, cmd);
            if (data.Read())
            {
                return ReadWishlist(data);
            }
            return null;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public List<Wishlist> GetWishlists()
    {
        NpgsqlConnection? dbConn = null;
        var wishlists = new List<Wishlist>();
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from wishlist order by id";
            using var data = GetData(dbConn, cmd);
            while (data.Read()) // every time the loop runs it reads the next row
            {
                wishlists.Add(ReadWishlist(data));
            }
            return wishlists;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Adds a new wishlist row and returns the id the database gave it
    public int InsertWishlist(Wishlist w)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                insert into wishlist
                (ownerid, occasionid, title)
                values
                (@ownerid, @occasionid, @title)
                returning id";
            cmd.Parameters.AddWithValue("@ownerid", NpgsqlDbType.Integer, w.OwnerId);
            cmd.Parameters.AddWithValue("@occasionid", NpgsqlDbType.Integer, DbValue(w.OccasionId));
            cmd.Parameters.AddWithValue("@title", NpgsqlDbType.Text, w.Title);
            return InsertAndGetId(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool UpdateWishlist(Wishlist w)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                update wishlist set
                ownerid = @ownerid,
                occasionid = @occasionid,
                title = @title
                where id = @id";
            cmd.Parameters.AddWithValue("@ownerid", NpgsqlDbType.Integer, w.OwnerId);
            cmd.Parameters.AddWithValue("@occasionid", NpgsqlDbType.Integer, DbValue(w.OccasionId));
            cmd.Parameters.AddWithValue("@title", NpgsqlDbType.Text, w.Title);
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, w.Id);
            return UpdateData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool DeleteWishlist(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "delete from wishlist where id = @id";
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, id);
            return DeleteData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Turns one row from the database into a Wishlist object
    private static Wishlist ReadWishlist(NpgsqlDataReader data)
    {
        return new Wishlist(Convert.ToInt32(data["id"]))
        {
            OwnerId = Convert.ToInt32(data["ownerid"]),
            OccasionId = ReadNullableInt(data, "occasionid"),
            Title = Convert.ToString(data["title"])!
        };
    }
}
