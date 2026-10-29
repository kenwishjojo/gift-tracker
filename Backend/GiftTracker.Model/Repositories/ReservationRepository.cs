using GiftTracker.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

namespace GiftTracker.Model.Repositories;

public class ReservationRepository : BaseRepository
{
    public ReservationRepository(IConfiguration configuration) : base(configuration) { }

    public Reservation? GetReservationById(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from reservation where id = @id";
            cmd.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;
            using var data = GetData(dbConn, cmd);
            if (data.Read())
            {
                return ReadReservation(data);
            }
            return null;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public List<Reservation> GetReservations()
    {
        NpgsqlConnection? dbConn = null;
        var reservations = new List<Reservation>();
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from reservation order by id";
            using var data = GetData(dbConn, cmd);
            while (data.Read()) // every time the loop runs it reads the next row
            {
                reservations.Add(ReadReservation(data));
            }
            return reservations;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Adds a new reservation row and returns the id the database gave it
    public int InsertReservation(Reservation r)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                insert into reservation
                (wishitemid, groupmemberid)
                values
                (@wishitemid, @groupmemberid)
                returning id";
            cmd.Parameters.AddWithValue("@wishitemid", NpgsqlDbType.Integer, r.WishItemId);
            cmd.Parameters.AddWithValue("@groupmemberid", NpgsqlDbType.Integer, r.GroupMemberId);
            return InsertAndGetId(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool UpdateReservation(Reservation r)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                update reservation set
                wishitemid = @wishitemid,
                groupmemberid = @groupmemberid
                where id = @id";
            cmd.Parameters.AddWithValue("@wishitemid", NpgsqlDbType.Integer, r.WishItemId);
            cmd.Parameters.AddWithValue("@groupmemberid", NpgsqlDbType.Integer, r.GroupMemberId);
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, r.Id);
            return UpdateData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool DeleteReservation(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "delete from reservation where id = @id";
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, id);
            return DeleteData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Turns one row from the database into a Reservation object
    private static Reservation ReadReservation(NpgsqlDataReader data)
    {
        return new Reservation(Convert.ToInt32(data["id"]))
        {
            WishItemId = Convert.ToInt32(data["wishitemid"]),
            GroupMemberId = Convert.ToInt32(data["groupmemberid"]),
            ReservedAt = Convert.ToDateTime(data["reservedat"])
        };
    }
}
