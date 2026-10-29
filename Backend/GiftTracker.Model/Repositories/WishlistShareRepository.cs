using GiftTracker.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

namespace GiftTracker.Model.Repositories;

// wishlist_share is a junction table: a row is identified by (wishlistid, groupid),
// so every method takes both ids instead of one id. There is no update – to change
// a share you delete it and add a new one.
public class WishlistShareRepository : BaseRepository
{
    public WishlistShareRepository(IConfiguration configuration) : base(configuration) { }

    public WishlistShare? GetWishlistShare(int wishlistId, int groupId)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from wishlist_share where wishlistid = @wishlistid and groupid = @groupid";
            cmd.Parameters.AddWithValue("@wishlistid", NpgsqlDbType.Integer, wishlistId);
            cmd.Parameters.AddWithValue("@groupid", NpgsqlDbType.Integer, groupId);
            using var data = GetData(dbConn, cmd);
            if (data.Read())
            {
                return ReadWishlistShare(data);
            }
            return null;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public List<WishlistShare> GetWishlistShares()
    {
        NpgsqlConnection? dbConn = null;
        var shares = new List<WishlistShare>();
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from wishlist_share order by wishlistid, groupid";
            using var data = GetData(dbConn, cmd);
            while (data.Read())
            {
                shares.Add(ReadWishlistShare(data));
            }
            return shares;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool InsertWishlistShare(WishlistShare s)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                insert into wishlist_share
                (wishlistid, groupid)
                values
                (@wishlistid, @groupid)";
            cmd.Parameters.AddWithValue("@wishlistid", NpgsqlDbType.Integer, s.WishlistId);
            cmd.Parameters.AddWithValue("@groupid", NpgsqlDbType.Integer, s.GroupId);
            return InsertData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool DeleteWishlistShare(int wishlistId, int groupId)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "delete from wishlist_share where wishlistid = @wishlistid and groupid = @groupid";
            cmd.Parameters.AddWithValue("@wishlistid", NpgsqlDbType.Integer, wishlistId);
            cmd.Parameters.AddWithValue("@groupid", NpgsqlDbType.Integer, groupId);
            return DeleteData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    private static WishlistShare ReadWishlistShare(NpgsqlDataReader data)
    {
        return new WishlistShare
        {
            WishlistId = Convert.ToInt32(data["wishlistid"]),
            GroupId = Convert.ToInt32(data["groupid"])
        };
    }
}
