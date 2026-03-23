using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

// 1. CHYBA: Musíš nejdřív říct, že chceš CORS používat (zaregistrovat službu)
builder.Services.AddCors(); 

var app = builder.Build();

app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

string connString = "server=127.0.0.1;uid=root;pwd=;database=MojeSQL";

// REGISTRACE
app.MapPost("/api/registrace", (UzivatelDTO u) => {
    using var conn = new MySqlConnection(connString);
    conn.Open();
    var cmd = new MySqlCommand("INSERT INTO uzivatele (jmeno, heslo) VALUES (@j, @h)", conn);
    cmd.Parameters.AddWithValue("@j", u.Jmeno);
    cmd.Parameters.AddWithValue("@h", u.Heslo); // V praxi se heslo musí hashovat!
    cmd.ExecuteNonQuery();
    return Results.Ok();
});

// PŘIHLÁŠENÍ (Vrátí ID uživatele, pokud sedí jméno a heslo)
app.MapPost("/api/login", (UzivatelDTO u) => {
    using var conn = new MySqlConnection(connString);
    conn.Open();
    var cmd = new MySqlCommand("SELECT id FROM uzivatele WHERE jmeno=@j AND heslo=@h", conn);
    cmd.Parameters.AddWithValue("@j", u.Jmeno);
    cmd.Parameters.AddWithValue("@h", u.Heslo);
    var id = cmd.ExecuteScalar();
    return id != null ? Results.Ok(id) : Results.Unauthorized();
});

// ÚPRAVA: Načítání úkolů pro konkrétního uživatele
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

// ÚPRAVA: Přidávání úkolu pro konkrétního uživatele
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

// POMOCNÉ STRUKTURY
record UzivatelDTO(string Jmeno, string Heslo);
record NovyUkolDTO(string Text, int UserId);