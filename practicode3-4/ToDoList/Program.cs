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
// הסודות האמיתיים נמצאים ב-appsettings.Development.json (לא נשמר ב-git)
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    throw new InvalidOperationException("Jwt:Key must be set (at least 32 chars) in appsettings.Development.json or environment variables");
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

// מזהה המשתמש המחובר מתוך הטוקן
int GetUserId(ClaimsPrincipal user) =>
    int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

// שליפת המשימות של המשתמש המחובר
items.MapGet("/", async (ToDoDbContext db, ClaimsPrincipal user) =>
{
    var userId = GetUserId(user);
    return await db.Items.Where(i => i.UserId == userId).ToListAsync();
});

// הוספת משימה חדשה למשתמש המחובר
items.MapPost("/", async (ToDoDbContext db, ClaimsPrincipal user, Item input) =>
{
    var item = new Item
    {
        Name = input.Name,
        IsComplete = input.IsComplete ?? false,
        UserId = GetUserId(user)
    };
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
});

// עדכון משימה - רק אם היא שייכת למשתמש
items.MapPut("/{id}", async (ToDoDbContext db, ClaimsPrincipal user, int id, Item input) =>
{
    var userId = GetUserId(user);
    var item = await db.Items.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
    if (item is null) return Results.NotFound();

    if (input.Name is not null) item.Name = input.Name;
    if (input.IsComplete is not null) item.IsComplete = input.IsComplete;
    await db.SaveChangesAsync();
    return Results.Ok(item);
});

// מחיקת משימה - רק אם היא שייכת למשתמש
items.MapDelete("/{id}", async (ToDoDbContext db, ClaimsPrincipal user, int id) =>
{
    var userId = GetUserId(user);
    var item = await db.Items.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
    if (item is null) return Results.NotFound();

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

// גוף הבקשה של הרשמה והתחברות
record LoginRequest(string Username, string Password);
