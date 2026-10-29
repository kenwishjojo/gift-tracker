using GiftTracker.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

namespace GiftTracker.Model.Repositories;

public class OccasionRepository : BaseRepository
{
    public OccasionRepository(IConfiguration configuration) : base(configuration) { }

    public Occasion? GetOccasionById(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from occasion where id = @id";
            cmd.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;
            using var data = GetData(dbConn, cmd);
            if (data.Read())
            {
                return ReadOccasion(data);
            }
            return null;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public List<Occasion> GetOccasions()
    {
        NpgsqlConnection? dbConn = null;
        var occasions = new List<Occasion>();
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from occasion order by id";
            using var data = GetData(dbConn, cmd);
            while (data.Read()) // every time the loop runs it reads the next row
            {
                occasions.Add(ReadOccasion(data));
            }
            return occasions;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Adds a new occasion row and returns the id the database gave it
    public int InsertOccasion(Occasion o)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                insert into occasion
                (createdbyid, recipientid, name, eventdate, type, notes)
                values
                (@createdbyid, @recipientid, @name, @eventdate, @type, @notes)
                returning id";
            cmd.Parameters.AddWithValue("@createdbyid", NpgsqlDbType.Integer, o.CreatedById);
            cmd.Parameters.AddWithValue("@recipientid", NpgsqlDbType.Integer, o.RecipientId);
            cmd.Parameters.AddWithValue("@name", NpgsqlDbType.Text, o.Name);
            cmd.Parameters.AddWithValue("@eventdate", NpgsqlDbType.Date, o.EventDate);
            cmd.Parameters.AddWithValue("@type", NpgsqlDbType.Text, o.Type);
            cmd.Parameters.AddWithValue("@notes", NpgsqlDbType.Text, DbValue(o.Notes));
            return InsertAndGetId(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool UpdateOccasion(Occasion o)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                update occasion set
                createdbyid = @createdbyid,
                recipientid = @recipientid,
                name = @name,
                eventdate = @eventdate,
                type = @type,
                notes = @notes
                where id = @id";
            cmd.Parameters.AddWithValue("@createdbyid", NpgsqlDbType.Integer, o.CreatedById);
            cmd.Parameters.AddWithValue("@recipientid", NpgsqlDbType.Integer, o.RecipientId);
            cmd.Parameters.AddWithValue("@name", NpgsqlDbType.Text, o.Name);
            cmd.Parameters.AddWithValue("@eventdate", NpgsqlDbType.Date, o.EventDate);
            cmd.Parameters.AddWithValue("@type", NpgsqlDbType.Text, o.Type);
            cmd.Parameters.AddWithValue("@notes", NpgsqlDbType.Text, DbValue(o.Notes));
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, o.Id);
            return UpdateData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Also deletes the occasion's gifts (on delete cascade in the database)
    public bool DeleteOccasion(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "delete from occasion where id = @id";
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, id);
            return DeleteData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Turns one row from the database into an Occasion object
    private static Occasion ReadOccasion(NpgsqlDataReader data)
    {
        return new Occasion(Convert.ToInt32(data["id"]))
        {
            CreatedById = Convert.ToInt32(data["createdbyid"]),
            RecipientId = Convert.ToInt32(data["recipientid"]),
            Name = Convert.ToString(data["name"])!,
            EventDate = ReadDate(data, "eventdate"),
            Type = Convert.ToString(data["type"])!,
            Notes = ReadNullableString(data, "notes")
        };
    }
}
