using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

// Registrace CORS - nezbytné pro propojení Frontendu a Backendů na různých adresách
builder.Services.AddCors(); 

// NASTAVENÍ PORTU PRO CLOUD (Render)
// Render používá proměnnou PORT, pokud není, použije se 5221 pro lokální vývoj
var port = Environment.GetEnvironmentVariable("PORT") ?? "5221";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// Tohle povolí tvému webu z Vercelu mluvit s tvým Macem
app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

// NASTAVENÍ DATABÁZE
// Heslo se načte z Environment Variables na Renderu. 
// PRO LOKÁLNÍ TEST: Můžeš si místo "TVOJE_HESLO_Z_AIVENU" napsat své heslo, 
// ale GitHub tě může znovu zablokovat. Nejlepší je heslo po Pushnutí smazat.
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "AVNS_6HxgAgi6xPEpPJcwzHI";
string connString = $"server=mojesql-mujprojekt.a.aivencloud.com;port=10341;uid=avnadmin;pwd={dbPassword};database=defaultdb;SslMode=Required";

// --- API ENDPOINTY ---

// REGISTRACE
app.MapPost("/api/registrace", (UzivatelDTO u) => {
    using var conn = new MySqlConnection(connString);
    conn.Open();
    var cmd = new MySqlCommand("INSERT INTO uzivatele (jmeno, heslo) VALUES (@j, @h)", conn);
    cmd.Parameters.AddWithValue("@j", u.Jmeno);
    cmd.Parameters.AddWithValue("@h", u.Heslo); 
    cmd.ExecuteNonQuery();
    return Results.Ok();
});

// PŘIHLÁŠENÍ
app.MapPost("/api/login", (UzivatelDTO u) => {
    using var conn = new MySqlConnection(connString);
    conn.Open();
    var cmd = new MySqlCommand("SELECT id FROM uzivatele WHERE jmeno=@j AND heslo=@h", conn);
    cmd.Parameters.AddWithValue("@j", u.Jmeno);
    cmd.Parameters.AddWithValue("@h", u.Heslo);
    var id = cmd.ExecuteScalar();
    return id != null ? Results.Ok(id) : Results.Unauthorized();
});

// NAČÍTÁNÍ ÚKOLŮ
app.MapGet("/api/ukoly/{userId}", (int userId) => {
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
});

// PŘIDÁVÁNÍ ÚKOLU
app.MapPost("/api/ukoly", (NovyUkolDTO n) => {
    using var conn = new MySqlConnection(connString);
    conn.Open();
    var cmd = new MySqlCommand("INSERT INTO ukoly (text_ukolu, uzivatel_id) VALUES (@t, @uid)", conn);
    cmd.Parameters.AddWithValue("@t", n.Text);
    cmd.Parameters.AddWithValue("@uid", n.UserId);
    cmd.ExecuteNonQuery();
    return Results.Ok();
});

app.Run();

// POMOCNÉ STRUKTURY (DTOs)
record UzivatelDTO(string Jmeno, string Heslo);
record NovyUkolDTO(string Text, int UserId);