using GiftTracker.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

namespace GiftTracker.Model.Repositories;

public class ProfileRepository : BaseRepository
{
    public ProfileRepository(IConfiguration configuration) : base(configuration) { }

    public Profile? GetProfileById(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select id, fullname, email, dateofbirth, phone, street, postalcode, city, giftnotes, createdat from profile where id = @id";
            cmd.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;
            using var data = GetData(dbConn, cmd);
            if (data.Read())
            {
                return ReadProfile(data);
            }
            return null;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public List<Profile> GetProfiles()
    {
        NpgsqlConnection? dbConn = null;
        var profiles = new List<Profile>();
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select id, fullname, email, dateofbirth, phone, street, postalcode, city, giftnotes, createdat from profile order by id";
            using var data = GetData(dbConn, cmd);
            while (data.Read()) // every time the loop runs it reads the next row
            {
                profiles.Add(ReadProfile(data));
            }
            return profiles;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Adds a new profile row and returns the id the database gave it
    public int InsertProfile(Profile p)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                insert into profile
                (fullname, email, passwordhash, dateofbirth, phone, street, postalcode, city, giftnotes)
                values
                (@fullname, @email, @passwordhash, @dateofbirth, @phone, @street, @postalcode, @city, @giftnotes)
                returning id";
            cmd.Parameters.AddWithValue("@fullname", NpgsqlDbType.Text, p.FullName);
            cmd.Parameters.AddWithValue("@email", NpgsqlDbType.Text, p.Email);
            cmd.Parameters.AddWithValue("@dateofbirth", NpgsqlDbType.Date, DbValue(p.DateOfBirth));
            cmd.Parameters.AddWithValue("@phone", NpgsqlDbType.Text, DbValue(p.Phone));
            cmd.Parameters.AddWithValue("@street", NpgsqlDbType.Text, DbValue(p.Street));
            cmd.Parameters.AddWithValue("@postalcode", NpgsqlDbType.Text, DbValue(p.PostalCode));
            cmd.Parameters.AddWithValue("@city", NpgsqlDbType.Text, DbValue(p.City));
            cmd.Parameters.AddWithValue("@giftnotes", NpgsqlDbType.Text, DbValue(p.GiftNotes));
            // Never store the plain password – only a BCrypt hash of it
            cmd.Parameters.AddWithValue("@passwordhash", NpgsqlDbType.Text, BCrypt.Net.BCrypt.HashPassword(p.Password!));
            return InsertAndGetId(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Updates the profile details. The password is not changed here.
    public bool UpdateProfile(Profile p)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
                update profile set
                fullname = @fullname,
                email = @email,
                dateofbirth = @dateofbirth,
                phone = @phone,
                street = @street,
                postalcode = @postalcode,
                city = @city,
                giftnotes = @giftnotes
                where id = @id";
            cmd.Parameters.AddWithValue("@fullname", NpgsqlDbType.Text, p.FullName);
            cmd.Parameters.AddWithValue("@email", NpgsqlDbType.Text, p.Email);
            cmd.Parameters.AddWithValue("@dateofbirth", NpgsqlDbType.Date, DbValue(p.DateOfBirth));
            cmd.Parameters.AddWithValue("@phone", NpgsqlDbType.Text, DbValue(p.Phone));
            cmd.Parameters.AddWithValue("@street", NpgsqlDbType.Text, DbValue(p.Street));
            cmd.Parameters.AddWithValue("@postalcode", NpgsqlDbType.Text, DbValue(p.PostalCode));
            cmd.Parameters.AddWithValue("@city", NpgsqlDbType.Text, DbValue(p.City));
            cmd.Parameters.AddWithValue("@giftnotes", NpgsqlDbType.Text, DbValue(p.GiftNotes));
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, p.Id);
            return UpdateData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool DeleteProfile(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "delete from profile where id = @id";
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, id);
            return DeleteData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // Turns one row from the database into a Profile object
    private static Profile ReadProfile(NpgsqlDataReader data)
    {
        return new Profile(Convert.ToInt32(data["id"]))
        {
            FullName = Convert.ToString(data["fullname"])!,
            Email = Convert.ToString(data["email"])!,
            DateOfBirth = ReadNullableDate(data, "dateofbirth"),
            Phone = ReadNullableString(data, "phone"),
            Street = ReadNullableString(data, "street"),
            PostalCode = ReadNullableString(data, "postalcode"),
            City = ReadNullableString(data, "city"),
            GiftNotes = ReadNullableString(data, "giftnotes"),
            CreatedAt = Convert.ToDateTime(data["createdat"])
        };
    }
}
