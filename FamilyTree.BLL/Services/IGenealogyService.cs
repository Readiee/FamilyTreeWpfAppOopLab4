using FamilyTree.DAL.Models;

namespace FamilyTree.BLL.Services;

public interface IGenealogyService
{
    void AddPerson(Person person);
    void SetParentChildRelationship(Person parent, Person child);
    void SetSpouseRelationship(Person person1, Person person2);
    List<Relative> GetRelatives(Person person);
    List<Person> GetChildren(Person parent);
    List<Person> GetParents(Person child);
    Person FindPersonByName(string fullName);
    int GetAncestorAgeAtBirth(Person ancestor, Person descendant);
    List<Person> GetAllPeople();
    List<TreeNode> GetTreeLayout(Person root, double startX, double startY, double xSpacing, double ySpacing);
    
    // ** Искать общих предков для двух выбранных людей
    List<Person> FindCommonAncestors(Person person1, Person person2);
    
    void SaveTree(string filePath);
    void LoadTree(string filePath);
    public void ClearTree();
}