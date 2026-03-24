using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

// Povolení CORS
builder.Services.AddCors(); 

var port = Environment.GetEnvironmentVariable("PORT") ?? "5221";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

// --- NASTAVENÍ DATABÁZE (Opravená adresa serveru) ---
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "AVNS_6HxgAgi6xPEpPJcwzHI";
// Opraveno z -a.aivencloud na .a.aivencloud
string connString = $"server=mojesql-mujprojekt.a.aivencloud.com;port=10341;uid=avnadmin;pwd={dbPassword};database=defaultdb;SslMode=Required";

// API ENDPOINTY

app.MapPost("/api/registrace", (UzivatelDTO u) => {
    try {
        using var conn = new MySqlConnection(connString);
        conn.Open();
        var cmd = new MySqlCommand("INSERT INTO uzivatele (jmeno, heslo) VALUES (@j, @h)", conn);
        cmd.Parameters.AddWithValue("@j", u.Jmeno);
        cmd.Parameters.AddWithValue("@h", u.Heslo); 
        cmd.ExecuteNonQuery();
        return Results.Ok();
    } catch (Exception ex) { return Results.Problem(ex.Message); }
});

app.MapPost("/api/login", (UzivatelDTO u) => {
    try {
        using var conn = new MySqlConnection(connString);
        conn.Open();
        var cmd = new MySqlCommand("SELECT id FROM uzivatele WHERE jmeno=@j AND heslo=@h", conn);
        cmd.Parameters.AddWithValue("@j", u.Jmeno);
        cmd.Parameters.AddWithValue("@h", u.Heslo);
        var id = cmd.ExecuteScalar();
        return id != null ? Results.Ok(id) : Results.Unauthorized();
    } catch (Exception ex) { return Results.Problem(ex.Message); }
});

app.MapGet("/api/ukoly/{userId}", (int userId) => {
    try {
        var ukoly = new List<object>();
        using var conn = new MySqlConnection(connString);
        conn.Open();
        var cmd = new MySqlCommand("SELECT id, text_ukolu FROM ukoly WHERE uzivatel_id = @uid", conn);
        cmd.Parameters.AddWithValue("@uid", userId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) {
            ukoly.Add(new { id = reader.GetInt32(0), text = reader.GetString(1) });
        }
        return Results.Ok(ukoly);
    } catch (Exception ex) { return Results.Problem(ex.Message); }
});

app.MapPost("/api/ukoly", (NovyUkolDTO n) => {
    try {
        using var conn = new MySqlConnection(connString);
        conn.Open();
        var cmd = new MySqlCommand("INSERT INTO ukoly (text_ukolu, uzivatel_id) VALUES (@t, @uid)", conn);
        cmd.Parameters.AddWithValue("@t", n.Text);
        cmd.Parameters.AddWithValue("@uid", n.UserId);
        cmd.ExecuteNonQuery();
        return Results.Ok();
    } catch (Exception ex) { return Results.Problem(ex.Message); }
});

app.MapDelete("/api/ukoly/{id}", (int id) => {
    try {
        using var conn = new MySqlConnection(connString);
        conn.Open();
        var cmd = new MySqlCommand("DELETE FROM ukoly WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
        return Results.Ok();
    } catch (Exception ex) { return Results.Problem(ex.Message); }
});

app.Run();

record UzivatelDTO(string Jmeno, string Heslo);
record NovyUkolDTO(string Text, int UserId);