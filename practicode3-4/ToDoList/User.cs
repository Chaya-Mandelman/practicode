namespace TodoApi;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    // נשמרת כ-hash, לא כטקסט גלוי
    public string Password { get; set; } = null!;
}
