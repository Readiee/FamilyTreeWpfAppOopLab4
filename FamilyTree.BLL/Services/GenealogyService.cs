using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using FamilyTree.DAL.Models;

namespace FamilyTree.BLL.Services;

public class GenealogyService : IGenealogyService
{
    private readonly List<Person> _tree = new List<Person>();

    public void AddPerson(Person person)
    {
        if (_tree.Any(p => p.Id == person.Id || p.FullName == person.FullName))
            throw new InvalidOperationException($"Человек с именем \"{person.FullName}\" уже существует в дереве.");

        _tree.Add(person);
    }

    public void SetParentChildRelationship(Person parent, Person child)
    {
        if (parent.Relatives.Any(r => r.Person == child))
            throw new InvalidOperationException("Отношение \"родитель-ребёнок\" уже существует между этими людьми.");

        if (parent == child)
            throw new InvalidOperationException("Человек не может быть своим собственным родителем.");

        if (!_tree.Contains(parent) || !_tree.Contains(child))
            throw new InvalidOperationException(
                "Оба участника должны быть добавлены в дерево перед установлением отношений.");

        if (parent.DateOfBirth > child.DateOfBirth)
            throw new InvalidOperationException("Родитель должен быть старше ребёнка.");

        if (FindCommonAncestors(parent, child).Any())
            throw new InvalidOperationException("Нельзя установить отношения с родственником.");

        if (GetAllAncestors(child).Contains(parent))
            throw new InvalidOperationException("Предок не может усыновить или удочерить своего потомка.");

        var childSpouse = GetSpouse(child);
        if (childSpouse != null && GetAllAncestors(childSpouse).Any(ancestor => ancestor == parent))
        {
            throw new InvalidOperationException("Предок не может усыновить или удочерить супруга своего потомка.");
        }
        
        // if (GetSpouse(child) != null)
        //     throw new InvalidOperationException(
        //         "Нельзя установить отношение родитель-ребёнок с человеком, который состоит в браке.");

        var siblings = GetChildren(parent).Where(s => s != child).ToList();

        parent.Relatives.Add(new Relative { Person = child, Type = RelationshipType.Child });
        child.Relatives.Add(new Relative { Person = parent, Type = RelationshipType.Parent });

        // Обновление связей "брат/сестра"
        foreach (var sibling in siblings)
        {
            if (!sibling.Relatives.Any(r => r.Person == child))
                sibling.Relatives.Add(new Relative { Person = child, Type = RelationshipType.Sibling });
            if (!child.Relatives.Any(r => r.Person == sibling))
                child.Relatives.Add(new Relative { Person = sibling, Type = RelationshipType.Sibling });
        }
    }

    public void SetSpouseRelationship(Person person1, Person person2)
    {
        if (person1 == person2)
            throw new InvalidOperationException("Человек не может вступить в брак с самим собой.");

        if (!_tree.Contains(person1) || !_tree.Contains(person2))
            throw new InvalidOperationException(
                "Оба участника должны быть добавлены в дерево перед установлением отношений.");

        if (person1.Gender == person2.Gender)
            throw new InvalidOperationException("Брак между людьми одного пола не поддерживается.");

        if (person1.Relatives.Any(r => r.Person == person2))
            throw new InvalidOperationException("Отношение между этими людьми уже существует.");

        if (FindCommonAncestors(person1, person2).Any())
            throw new InvalidOperationException("Нельзя вступить в брак с родственником.");

        if (person1.Relatives.Any(r => r.Type == RelationshipType.Spouse) ||
            person2.Relatives.Any(r => r.Type == RelationshipType.Spouse))
            throw new InvalidOperationException("Один из участников уже состоит в браке.");

        person1.Relatives.Add(new Relative { Person = person2, Type = RelationshipType.Spouse });
        person2.Relatives.Add(new Relative { Person = person1, Type = RelationshipType.Spouse });
    }

    public List<Relative> GetRelatives(Person person)
    {
        return _tree.FirstOrDefault(p => p.Id == person.Id)?.Relatives ?? new List<Relative>();
    }

    public List<Person> GetChildren(Person parent)
    {
        return parent.Relatives
            .Where(r => r.Type == RelationshipType.Child)
            .Select(r => r.Person)
            .ToList();
    }

    public List<Person> GetParents(Person child)
    {
        return child.Relatives
            .Where(r => r.Type == RelationshipType.Parent)
            .Select(r => r.Person)
            .ToList();
    }

    public Person? FindPersonByName(string fullName)
    {
        return _tree.FirstOrDefault(p => p.FullName.Equals(fullName, StringComparison.OrdinalIgnoreCase));
    }

    public int GetAncestorAgeAtBirth(Person ancestor, Person descendant)
    {
        if (ancestor.DateOfBirth >= descendant.DateOfBirth)
            throw new InvalidOperationException("Предок должен быть старше потомка.");

        return (descendant.DateOfBirth - ancestor.DateOfBirth).Days / 365;
    }

    public List<Person> FindCommonAncestors(Person person1, Person person2)
    {
        var ancestors1 = GetAllAncestors(person1);
        var ancestors2 = GetAllAncestors(person2);

        return ancestors1.Intersect(ancestors2).ToList();
    }

    private List<Person> GetAllAncestors(Person person)
    {
        var ancestors = new List<Person>();
        var queue = new Queue<Person>();

        queue.Enqueue(person);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var parents = GetParents(current);

            foreach (var parent in parents)
            {
                if (!ancestors.Contains(parent))
                {
                    ancestors.Add(parent);
                    queue.Enqueue(parent);
                }
            }
        }

        return ancestors;
    }

    public List<Person> GetAllPeople()
    {
        return _tree.ToList();
    }

    public List<TreeNode> GetTreeLayout(Person root, double startX = 350, double startY = 100, double xSpacing = 150,
        double ySpacing = 100)
    {
        var layout = new List<TreeNode>();
        var visited = new HashSet<Person>();

        void Traverse(Person current, double x, double y, bool isSpouse = false)
        {
            if (visited.Contains(current)) return;
            visited.Add(current);

            if (isSpouse)
            {
                layout.Add(new TreeNode { Person = current, X = x, Y = y });
                return;
            }

            layout.Add(new TreeNode { Person = current, X = x, Y = y });

            // супруг(-а)
            var spouse = GetSpouse(current);
            if (spouse != null && !visited.Contains(spouse))
            {
                Traverse(spouse, x - 25 + xSpacing / 2, y, isSpouse: true);
            }

            // дети
            var children = GetChildren(current).Where(c => !visited.Contains(c)).ToList();
            for (int i = 0; i < children.Count; i++)
            {
                double childX = x + i * xSpacing - (children.Count - 1) * xSpacing / 2;
                Traverse(children[i], childX, y + ySpacing);
            }
        }

        Traverse(root, startX, startY);
        return layout;
    }


    private Person? GetSpouse(Person person)
    {
        return person.Relatives.FirstOrDefault(r => r.Type == RelationshipType.Spouse)?.Person;
    }

    
    // Save-Load
    public void SaveTree(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Путь к файлу не может быть пустым.");

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.Preserve
        };

        var json = JsonSerializer.Serialize(_tree, options);
        File.WriteAllText(filePath, json);
    }

    public void LoadTree(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Путь к файлу не может быть пустым.");
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Файл не найден.");

        var options = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve
        };

        var json = File.ReadAllText(filePath);
        var loadedTree = JsonSerializer.Deserialize<List<Person>>(json, options);

        if (loadedTree == null || !loadedTree.Any())
            throw new InvalidDataException("Файл содержит некорректные данные или пуст.");

        _tree.Clear();
        _tree.AddRange(loadedTree);
    }

    public void ClearTree()
    {
        _tree.Clear();
    }
}