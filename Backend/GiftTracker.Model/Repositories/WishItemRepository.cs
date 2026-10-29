using GiftTracker.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

namespace GiftTracker.Model.Repositories;

public class WishItemRepository : BaseRepository
{
    public WishItemRepository(IConfiguration configuration) : base(configuration) { }

    public WishItem? GetWishItemById(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from wish_item where id = @id";
            cmd.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;
            using var data = GetData(dbConn, cmd);
            if (data.Read())
            {
                return ReadWishItem(data);
            }
            return null;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public List<WishItem> GetWishItems()
    {
        NpgsqlConnection? dbConn = null;
        var wishItems = new List<WishItem>();
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from wish_item order by id";
            using var data = GetData(dbConn, cmd);
            while (data.Read()) // every time the loop runs it reads the next row
            {
                wishItems.Add(ReadWishItem(data));
            }
            return wishItems;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Adds a new wish_item row and returns the id the database gave it
    public int InsertWishItem(WishItem w)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                insert into wish_item
                (wishlistid, name, link, price, priority)
                values
                (@wishlistid, @name, @link, @price, @priority)
                returning id";
            cmd.Parameters.AddWithValue("@wishlistid", NpgsqlDbType.Integer, w.WishlistId);
            cmd.Parameters.AddWithValue("@name", NpgsqlDbType.Text, w.Name);
            cmd.Parameters.AddWithValue("@link", NpgsqlDbType.Text, w.Link);
            cmd.Parameters.AddWithValue("@price", NpgsqlDbType.Numeric, w.Price);
            cmd.Parameters.AddWithValue("@priority", NpgsqlDbType.Integer, w.Priority);
            return InsertAndGetId(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool UpdateWishItem(WishItem w)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                update wish_item set
                wishlistid = @wishlistid,
                name = @name,
                link = @link,
                price = @price,
                priority = @priority
                where id = @id";
            cmd.Parameters.AddWithValue("@wishlistid", NpgsqlDbType.Integer, w.WishlistId);
            cmd.Parameters.AddWithValue("@name", NpgsqlDbType.Text, w.Name);
            cmd.Parameters.AddWithValue("@link", NpgsqlDbType.Text, w.Link);
            cmd.Parameters.AddWithValue("@price", NpgsqlDbType.Numeric, w.Price);
            cmd.Parameters.AddWithValue("@priority", NpgsqlDbType.Integer, w.Priority);
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, w.Id);
            return UpdateData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool DeleteWishItem(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "delete from wish_item where id = @id";
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, id);
            return DeleteData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Turns one row from the database into a WishItem object
    private static WishItem ReadWishItem(NpgsqlDataReader data)
    {
        return new WishItem(Convert.ToInt32(data["id"]))
        {
            WishlistId = Convert.ToInt32(data["wishlistid"]),
            Name = Convert.ToString(data["name"])!,
            Link = Convert.ToString(data["link"])!,
            Price = Convert.ToDecimal(data["price"]),
            Priority = Convert.ToInt32(data["priority"])
        };
    }
}
