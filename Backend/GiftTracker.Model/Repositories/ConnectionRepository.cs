using GiftTracker.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

namespace GiftTracker.Model.Repositories;

public class ConnectionRepository : BaseRepository
{
    public ConnectionRepository(IConfiguration configuration) : base(configuration) { }

    public Connection? GetConnectionById(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from connection where id = @id";
            cmd.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;
            using var data = GetData(dbConn, cmd);
            if (data.Read())
            {
                return ReadConnection(data);
            }
            return null;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public List<Connection> GetConnections()
    {
        NpgsqlConnection? dbConn = null;
        var connections = new List<Connection>();
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from connection order by id";
            using var data = GetData(dbConn, cmd);
            while (data.Read()) // every time the loop runs it reads the next row
            {
                connections.Add(ReadConnection(data));
            }
            return connections;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Adds a new connection row and returns the id the database gave it
    public int InsertConnection(Connection c)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                insert into connection
                (requesterid, addresseeid, status)
                values
                (@requesterid, @addresseeid, @status)
                returning id";
            cmd.Parameters.AddWithValue("@requesterid", NpgsqlDbType.Integer, c.RequesterId);
            cmd.Parameters.AddWithValue("@addresseeid", NpgsqlDbType.Integer, c.AddresseeId);
            cmd.Parameters.AddWithValue("@status", NpgsqlDbType.Text, c.Status);
            return InsertAndGetId(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Typically used to accept a friend request (status "pending" -> "accepted")
    public bool UpdateConnection(Connection c)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                update connection set
                requesterid = @requesterid,
                addresseeid = @addresseeid,
                status = @status
                where id = @id";
            cmd.Parameters.AddWithValue("@requesterid", NpgsqlDbType.Integer, c.RequesterId);
            cmd.Parameters.AddWithValue("@addresseeid", NpgsqlDbType.Integer, c.AddresseeId);
            cmd.Parameters.AddWithValue("@status", NpgsqlDbType.Text, c.Status);
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, c.Id);
            return UpdateData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool DeleteConnection(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "delete from connection where id = @id";
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, id);
            return DeleteData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Turns one row from the database into a Connection object
    private static Connection ReadConnection(NpgsqlDataReader data)
    {
        return new Connection(Convert.ToInt32(data["id"]))
        {
            RequesterId = Convert.ToInt32(data["requesterid"]),
            AddresseeId = Convert.ToInt32(data["addresseeid"]),
            Status = Convert.ToString(data["status"])!,
            CreatedAt = Convert.ToDateTime(data["createdat"])
        };
    }
}
