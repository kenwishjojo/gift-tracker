using GiftTracker.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

namespace GiftTracker.Model.Repositories;

public class ProfileRepository : BaseRepository
{
    public ProfileRepository(IConfiguration configuration) : base(configuration) { }

    // passwordhash is never selected, so it can't be sent back by the API by mistake
    public Profile? GetProfileById(int id)
    {
        NpgsqlConnection? dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select id, fullname, email, dateofbirth, profilepictureurl, createdat from profile where id = @id";
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
            cmd.CommandText = "select id, fullname, email, dateofbirth, profilepictureurl, createdat from profile order by id";
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
                (fullname, email, passwordhash, dateofbirth, profilepictureurl)
                values
                (@fullname, @email, @passwordhash, @dateofbirth, @profilepictureurl)
                returning id";
            cmd.Parameters.AddWithValue("@fullname", NpgsqlDbType.Text, p.FullName);
            cmd.Parameters.AddWithValue("@email", NpgsqlDbType.Text, p.Email);
            cmd.Parameters.AddWithValue("@dateofbirth", NpgsqlDbType.Date, p.DateOfBirth);
            cmd.Parameters.AddWithValue("@profilepictureurl", NpgsqlDbType.Text, DbValue(p.ProfilePictureUrl));
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
                profilepictureurl = @profilepictureurl
                where id = @id";
            cmd.Parameters.AddWithValue("@fullname", NpgsqlDbType.Text, p.FullName);
            cmd.Parameters.AddWithValue("@email", NpgsqlDbType.Text, p.Email);
            cmd.Parameters.AddWithValue("@dateofbirth", NpgsqlDbType.Date, p.DateOfBirth);
            cmd.Parameters.AddWithValue("@profilepictureurl", NpgsqlDbType.Text, DbValue(p.ProfilePictureUrl));
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
            DateOfBirth = ReadDate(data, "dateofbirth"),
            ProfilePictureUrl = ReadNullableString(data, "profilepictureurl"),
            CreatedAt = Convert.ToDateTime(data["createdat"])
        };
    }
}
