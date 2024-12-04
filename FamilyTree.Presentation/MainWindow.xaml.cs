using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using FamilyTree.BLL.Services;
using FamilyTree.DAL.Models;

namespace FamilyTree.Presentation;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : INotifyPropertyChanged
{
    private readonly IGenealogyService _service;
    private bool _isPeopleListEmpty;
    public bool IsPeopleListEmpty
    {
        get => _isPeopleListEmpty;
        set
        {
            _isPeopleListEmpty = value;
            OnPropertyChanged();
        }
    }
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        _service = new GenealogyService();
        UpdatePeopleList();
    }

    private void UpdatePeopleList()
    {
        PeopleListBox.ItemsSource = null;
        var people = _service.GetAllPeople();
        PeopleListBox.ItemsSource = people;
        IsPeopleListEmpty = people.Count == 0;
    }

    // Создать сущность “Человек”
    private void AddPersonButton_Click(object sender, RoutedEventArgs e)
    {
        var addPersonWindow = new AddPersonWindow(_service);
        addPersonWindow.ShowDialog();
        UpdatePeopleList();
    }

    // Добавить сущность в древо (Установить отношения "Родитель-ребенок")
    private void SetParentChildButton_Click(object sender, RoutedEventArgs e)
    {
        if (PeopleListBox.SelectedItem is Person parent)
        {
            var selectChildWindow = new SelectPersonWindow(_service, $"Выберите ребенка - {parent.FullName}");
            if (selectChildWindow.ShowDialog() == true)
            {
                var child = selectChildWindow.SelectedPerson;
                try
                {
                    _service.SetParentChildRelationship(parent, child);
                    OutputTextBox.Text =
                        $"Успешно установлено отношение \"Родитель-ребёнок\" между {parent.FullName} и {child.FullName}.";
                    UpdatePeopleList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        else MessageBox.Show("Выберите родителя из списка.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    // Добавить сущность в древо (Установить отношения "Муж-жена")
    private void SetSpouseButton_Click(object sender, RoutedEventArgs e)
    {
        if (PeopleListBox.SelectedItem is Person person)
        {
            var selectSpouseWindow = new SelectPersonWindow(_service, $"Выберите супруга - {person.FullName}");
            if (selectSpouseWindow.ShowDialog() == true)
            {
                var spouse = selectSpouseWindow.SelectedPerson;
                try
                {
                    _service.SetSpouseRelationship(person, spouse);
                    OutputTextBox.Text =
                        $"Успешно установлено отношение \"Супруги\" между {person.FullName} и {spouse.FullName}.";
                    UpdatePeopleList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        else
        {
            MessageBox.Show("Выберите человека из списка.", "Предупреждение", MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    // ** Искать общих предков для двух выбранных людей
    private void GetCommonAncestorsButton_Click(object sender, RoutedEventArgs e)
    {
        if (PeopleListBox.SelectedItem is Person person1)
        {
            var selectSecondPersonWindow = new SelectPersonWindow(_service,
                $"Выберите человека, с которым нужно найти общих предков - {person1.FullName}");
            if (selectSecondPersonWindow.ShowDialog() == true)
            {
                var person2 = selectSecondPersonWindow.SelectedPerson;
                var commonAncestors = _service.FindCommonAncestors(person1, person2);
                UpdatePeopleList();

                OutputTextBox.Text = $"Общие предки для {person1} и {person2}: \n" +
                                     string.Join("\n", commonAncestors.Select(p => $"{p.FullName}"));
            }
        }
        else MessageBox.Show("Выберите человека из списка.");
    }

    // Вычислить возраст предка при рождении потомка
    private void GetAncestorAgeAtBirthButton_Click(object sender, RoutedEventArgs e)
    {
        if (PeopleListBox.SelectedItem is Person ancestor)
        {
            var selectSecondPersonWindow = new SelectPersonWindow(_service,
                $"Выберите потомка, относительно чьего дня рождения нужно посчитать возраст - {ancestor.FullName}");

            if (selectSecondPersonWindow.ShowDialog() == true)
            {
                var descendant = selectSecondPersonWindow.SelectedPerson;

                try
                {
                    var age = _service.GetAncestorAgeAtBirth(ancestor, descendant);
                    OutputTextBox.Text =
                        $"Возраст {ancestor.FullName} в момент рождения {descendant.FullName}:\n{age} лет.";
                    UpdatePeopleList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        else
            MessageBox.Show("Выберите человека из списка.", "Предупреждение", MessageBoxButton.OK,
                MessageBoxImage.Warning);
    }

    // Вывести ближайших родственников 
    private void ShowRelativesButton_Click(object sender, RoutedEventArgs e)
    {
        if (PeopleListBox.SelectedItem is Person person)
        {
            var relatives = _service.GetRelatives(person);
            OutputTextBox.Text = $"Родственники {person.FullName}:\n" +
                                 string.Join("\n", relatives.Select(r => $"{r.Person.FullName} ({r.Type})"));
        }
        else MessageBox.Show("Выберите человека из списка.");
    }


    // Показать получившееся древо;
    private void ShowTreeButton_Click(object sender, RoutedEventArgs e)
    {
        if (PeopleListBox.SelectedItem is Person selectedPerson)
        {
            var window = new TreeVisualizationWindow(_service, selectedPerson);
            window.Show();
        }
        else
            MessageBox.Show("Выберите человека для отображения дерева.", "Предупреждение", MessageBoxButton.OK,
                MessageBoxImage.Warning);
    }

    private void GenerateRandomFamily_Click(object sender, RoutedEventArgs e)
    {
        var random = new Random();

        var parent1 = new Person
        {
            FullName = $"Иван",
            DateOfBirth = new DateTime(1950 + random.Next(20), random.Next(1, 12), random.Next(1, 28)),
            Gender = Gender.Male
        };

        var parent2 = new Person
        {
            FullName = $"Мария",
            DateOfBirth = new DateTime(1950 + random.Next(20), random.Next(1, 12), random.Next(1, 28)),
            Gender = Gender.Female
        };

        _service.AddPerson(parent1);
        _service.AddPerson(parent2);
        _service.SetSpouseRelationship(parent1, parent2);

        int numberOfChildren = 3;
        for (int i = 0; i < numberOfChildren; i++)
        {
            var child = new Person
            {
                FullName = $"Женя-{i}",
                DateOfBirth = new DateTime(parent1.DateOfBirth.Year + 20 + random.Next(10), random.Next(1, 12),
                    random.Next(1, 28)),
                Gender = random.Next(2) == 0 ? Gender.Male : Gender.Female
            };

            _service.AddPerson(child);
            _service.SetParentChildRelationship(parent1, child);
            _service.SetParentChildRelationship(parent2, child);
        }

        UpdatePeopleList();
        
    }

    private void ClearTree_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Вы уверены, что хотите очистить дерево?",
            "Подтверждение",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning
        );

        if (result == MessageBoxResult.Yes)
        {
            _service.ClearTree();
            OutputTextBox.Clear();
            UpdatePeopleList();
        }
    }

    private void SaveTree_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            FileName = "GenealogyTree",
            DefaultExt = ".json",
            Filter = "JSON файлы (.json)|*.json|Все файлы (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                _service.SaveTree(dialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении дерева: {ex.Message}", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    private void LoadTree_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            DefaultExt = ".json",
            Filter = "JSON файлы (.json)|*.json|Все файлы (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                _service.LoadTree(dialog.FileName);
                OutputTextBox.Clear();
                UpdatePeopleList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке дерева: {ex.Message}", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
    
    
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}