using GiftTracker.Model.Repositories;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Your real database password goes in appsettings.Local.json (ignored by git), e.g.
// { "ConnectionStrings": { "GiftTrackerDb": "Host=localhost:5432;Username=postgres;Password=yourpassword;Database=app-gifttrackerdb" } }
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllers();

// Make the repositories available to the controllers through dependency injection
builder.Services.AddScoped<PreferenceTypeRepository, PreferenceTypeRepository>();
builder.Services.AddScoped<ProfileRepository, ProfileRepository>();
builder.Services.AddScoped<GiftGroupRepository, GiftGroupRepository>();
builder.Services.AddScoped<GroupMemberRepository, GroupMemberRepository>();
builder.Services.AddScoped<PreferenceItemRepository, PreferenceItemRepository>();
builder.Services.AddScoped<ExternalLinkRepository, ExternalLinkRepository>();
builder.Services.AddScoped<OccasionRepository, OccasionRepository>();
builder.Services.AddScoped<WishlistRepository, WishlistRepository>();
builder.Services.AddScoped<WishlistShareRepository, WishlistShareRepository>();
builder.Services.AddScoped<WishItemRepository, WishItemRepository>();
builder.Services.AddScoped<ReservationRepository, ReservationRepository>();
builder.Services.AddScoped<GiftRepository, GiftRepository>();
builder.Services.AddScoped<GiftContributionRepository, GiftContributionRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// When the database says no (unique key, foreign key or check constraint broken),
// send a 400 Bad Request with the database's message instead of crashing with a 500.
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (PostgresException ex) when (ex.SqlState.StartsWith("23"))
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync($"Database rule broken ({ex.ConstraintName}): {ex.MessageText}");
    }
});

// Let the Angular frontend (another port) call the API
app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.UseAuthorization();

app.MapControllers();

app.Run();
