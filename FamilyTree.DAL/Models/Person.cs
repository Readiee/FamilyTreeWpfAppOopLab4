namespace FamilyTree.DAL.Models;

public class Person
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public List<Relative> Relatives { get; set; } = new List<Relative>();

    public override string ToString()
    {
        return $"{FullName} ({Gender}, {DateOfBirth:dd.MM.yyyy})";
    }
}

public enum Gender
{
    Male,
    Female
}