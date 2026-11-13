using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TodoApi;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// הזרקת ה-Context ל-Services
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ToDoDB"),
        ServerVersion.Parse("8.4.9-mysql")));

// הגדרות JWT מתוך appsettings.json
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

// הזדהות: כל בקשה עם Authorization: Bearer <token> נבדקת כאן
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = signingKey
        };
    });
builder.Services.AddAuthorization();

// הצפנת סיסמאות (hash) - לא שומרים סיסמה כטקסט גלוי
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

// יצירת טוקן JWT למשתמש
string CreateToken(User user)
{
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username)
    };

    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(2),
        signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

    return new JwtSecurityTokenHandler().WriteToken(token);
}

// הרשמה - משתמש חדש
app.MapPost("/register", async (ToDoDbContext db, IPasswordHasher<User> hasher, LoginRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest("Username and password are required");

    if (await db.Users.AnyAsync(u => u.Username == req.Username))
        return Results.Conflict("Username already exists");

    var user = new User { Username = req.Username };
    user.Password = hasher.HashPassword(user, req.Password);
    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Ok(new { token = CreateToken(user) });
});

// התחברות - משתמש קיים
app.MapPost("/login", async (ToDoDbContext db, IPasswordHasher<User> hasher, LoginRequest req) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
    if (user is null ||
        hasher.VerifyHashedPassword(user, user.Password, req.Password ?? "") == PasswordVerificationResult.Failed)
        return Results.Unauthorized();

    return Results.Ok(new { token = CreateToken(user) });
});

// כל ה-routes של המשימות דורשים טוקן תקין
var items = app.MapGroup("/items").RequireAuthorization();

// שליפת כל המשימות
items.MapGet("/", async (ToDoDbContext db) =>
    await db.Items.ToListAsync());

// הוספת משימה חדשה
items.MapPost("/", async (ToDoDbContext db, Item item) =>
{
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
});

// עדכון משימה
items.MapPut("/{id}", async (ToDoDbContext db, int id, Item input) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    if (input.Name is not null) item.Name = input.Name;
    item.IsComplete = input.IsComplete;
    await db.SaveChangesAsync();
    return Results.Ok(item);
});

// מחיקת משימה
items.MapDelete("/{id}", async (ToDoDbContext db, int id) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

// גוף הבקשה של הרשמה והתחברות
record LoginRequest(string Username, string Password);
