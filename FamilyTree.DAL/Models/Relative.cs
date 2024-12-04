namespace FamilyTree.DAL.Models;

public class Relative
{
    public Person Person { get; set; }
    public RelationshipType Type { get; set; }
}

public enum RelationshipType
{
    Parent,
    Child,
    Spouse,
    Sibling
}