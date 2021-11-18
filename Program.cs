using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<OdyssDb>(opt => opt.UseInMemoryDatabase("OdyssList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

app.MapGet("/", () => "Type three-letter destination code after domain adrress in URL bar, for example 'odyss-api.com/PAN'");

app.MapGet("/{destination}", async (string destination, OdyssDb db) =>
{
    var route = await db.Routes.Where(d => d.Destination == destination).FirstAsync();
    var response = "{" + "\n" + "\tdestination: \'" + route.Destination?.ToString() + "\',\n" + "\tlist: " + route.List?.ToString() + "\n" + "}";

    return response;
});

app.MapGet("/routes", async (OdyssDb db) =>
    await db.Routes.ToListAsync());


app.MapGet("/routes/{id}", async (int id, OdyssDb db) =>
    await db.Routes.FindAsync(id)
        is Route route
            ? Results.Ok(route)
            : Results.NotFound());

app.MapPost("/routes", async (Route route, OdyssDb db) =>
{
    db.Routes.Add(route);
    await db.SaveChangesAsync();

    return Results.Created($"/destinations/{route.Id}", route);
});

app.MapPut("/routes/{id}", async (int id, Route inputRoute, OdyssDb db) =>
{
    var route = await db.Routes.FindAsync(id);

    if (route is null) return Results.NotFound();

    route.Destination = inputRoute.Destination;
    route.List = inputRoute.List;

    await db.SaveChangesAsync();

    return Results.NoContent();
});


app.MapDelete("/routes/{id}", async (int id, OdyssDb db) =>
{
    if (await db.Routes.FindAsync(id) is Route destination)
    {
        db.Routes.Remove(destination);
        await db.SaveChangesAsync();
        return Results.Ok(destination);
    }

    return Results.NotFound();
});

app.Run();

class Route
{
    public int Id { get; set; }
    public string? Destination { get; set; }
    public string? List { get; set; }
}

class OdyssDb : DbContext
{
    public OdyssDb(DbContextOptions<OdyssDb> options)
        : base(options) { }

    public DbSet<Route> Routes => Set<Route>();
}