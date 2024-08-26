namespace MauiTestApp.Models;

public sealed class TestItem
{
    public int Id { get; }
    public string Name { get; }

    public TestItem(int id)
    {
        Id = id;
        Name = $"Item {id}";
    }
}
