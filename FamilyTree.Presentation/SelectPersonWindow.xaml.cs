using System.Windows;
using FamilyTree.BLL.Services;
using FamilyTree.DAL.Models;

namespace FamilyTree.Presentation;

public partial class SelectPersonWindow
{
    private readonly IGenealogyService _service;
    public Person SelectedPerson { get; private set; }
    public SelectPersonWindow(IGenealogyService service, string title)
    {   
        InitializeComponent();
        _service = service;
        Title = title;
        LoadPeople();
    }
    
    private void LoadPeople()
    {
        PeopleListBox.ItemsSource = null;
        PeopleListBox.ItemsSource = _service.GetAllPeople();
    }

    private void SelectButton_Click(object sender, RoutedEventArgs e)
    {
        if (PeopleListBox.SelectedItem is Person selected)
        {
            SelectedPerson = selected;
            DialogResult = true;
            Close();
        }
        else MessageBox.Show("Выберите человека из списка.");
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}