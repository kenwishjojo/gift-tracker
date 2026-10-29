using GiftTracker.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

namespace GiftTracker.Model.Repositories;

public class PreferenceTypeRepository : BaseRepository
{
    public PreferenceTypeRepository(IConfiguration configuration) : base(configuration) { }

    public PreferenceType? GetPreferenceTypeById(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from preference_type where id = @id";
            cmd.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;
            using var data = GetData(dbConn, cmd);
            if (data.Read())
            {
                return ReadPreferenceType(data);
            }
            return null;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public List<PreferenceType> GetPreferenceTypes()
    {
        NpgsqlConnection? dbConn = null;
        var preferenceTypes = new List<PreferenceType>();
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from preference_type order by id";
            using var data = GetData(dbConn, cmd);
            while (data.Read()) // every time the loop runs it reads the next row
            {
                preferenceTypes.Add(ReadPreferenceType(data));
            }
            return preferenceTypes;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Adds a new preference_type row and returns the id the database gave it
    public int InsertPreferenceType(PreferenceType pt)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                insert into preference_type
                (name)
                values
                (@name)
                returning id";
            cmd.Parameters.AddWithValue("@name", NpgsqlDbType.Text, pt.Name);
            return InsertAndGetId(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool UpdatePreferenceType(PreferenceType pt)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                update preference_type set
                name = @name
                where id = @id";
            cmd.Parameters.AddWithValue("@name", NpgsqlDbType.Text, pt.Name);
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, pt.Id);
            return UpdateData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool DeletePreferenceType(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "delete from preference_type where id = @id";
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, id);
            return DeleteData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Turns one row from the database into a PreferenceType object
    private static PreferenceType ReadPreferenceType(NpgsqlDataReader data)
    {
        return new PreferenceType(Convert.ToInt32(data["id"]))
        {
            Name = Convert.ToString(data["name"])!
        };
    }
}
