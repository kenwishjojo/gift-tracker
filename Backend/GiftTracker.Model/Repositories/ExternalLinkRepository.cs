using GiftTracker.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

namespace GiftTracker.Model.Repositories;

public class ExternalLinkRepository : BaseRepository
{
    public ExternalLinkRepository(IConfiguration configuration) : base(configuration) { }

    public ExternalLink? GetExternalLinkById(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from external_link where id = @id";
            cmd.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;
            using var data = GetData(dbConn, cmd);
            if (data.Read())
            {
                return ReadExternalLink(data);
            }
            return null;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public List<ExternalLink> GetExternalLinks()
    {
        NpgsqlConnection? dbConn = null;
        var externalLinks = new List<ExternalLink>();
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from external_link order by id";
            using var data = GetData(dbConn, cmd);
            while (data.Read()) // every time the loop runs it reads the next row
            {
                externalLinks.Add(ReadExternalLink(data));
            }
            return externalLinks;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Adds a new external_link row and returns the id the database gave it
    public int InsertExternalLink(ExternalLink e)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                insert into external_link
                (profileid, service, url)
                values
                (@profileid, @service, @url)
                returning id";
            cmd.Parameters.AddWithValue("@profileid", NpgsqlDbType.Integer, e.ProfileId);
            cmd.Parameters.AddWithValue("@service", NpgsqlDbType.Text, e.Service);
            cmd.Parameters.AddWithValue("@url", NpgsqlDbType.Text, e.Url);
            return InsertAndGetId(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool UpdateExternalLink(ExternalLink e)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                update external_link set
                profileid = @profileid,
                service = @service,
                url = @url
                where id = @id";
            cmd.Parameters.AddWithValue("@profileid", NpgsqlDbType.Integer, e.ProfileId);
            cmd.Parameters.AddWithValue("@service", NpgsqlDbType.Text, e.Service);
            cmd.Parameters.AddWithValue("@url", NpgsqlDbType.Text, e.Url);
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, e.Id);
            return UpdateData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool DeleteExternalLink(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "delete from external_link where id = @id";
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, id);
            return DeleteData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Turns one row from the database into a ExternalLink object
    private static ExternalLink ReadExternalLink(NpgsqlDataReader data)
    {
        return new ExternalLink(Convert.ToInt32(data["id"]))
        {
            ProfileId = Convert.ToInt32(data["profileid"]),
            Service = Convert.ToString(data["service"])!,
            Url = Convert.ToString(data["url"])!
        };
    }
}
