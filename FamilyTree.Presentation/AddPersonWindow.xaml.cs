using System.Windows;
using FamilyTree.BLL.Services;
using FamilyTree.DAL.Models;

namespace FamilyTree.Presentation;

public partial class AddPersonWindow
{
    private IGenealogyService _service;

    public AddPersonWindow(IGenealogyService service)
    {
        InitializeComponent();
        _service = service;
        GenderComboBox.SelectedIndex = 0;
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var name = NameTextBox.Text.Trim();
        var birthDate = BirthDatePicker.SelectedDate;
        var gender = GenderComboBox.SelectedIndex == 0 ? Gender.Male : Gender.Female;

        if (!string.IsNullOrWhiteSpace(name) && birthDate.HasValue)
        {
            var person = new Person
            {
                FullName = name,
                DateOfBirth = birthDate.Value,
                Gender = gender
            };

            try
            {
                _service.AddPerson(person);
                // MessageBox.Show($"Персонаж \"{person.FullName}\" успешно добавлен в дерево.",
                //     "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else
            MessageBox.Show("Заполните все поля.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}