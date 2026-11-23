namespace TodoApi;

public partial class Item
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public bool? IsComplete { get; set; }

    // המשתמש שיצר את המשימה - כל משתמש רואה רק את המשימות שלו
    public int? UserId { get; set; }
}
