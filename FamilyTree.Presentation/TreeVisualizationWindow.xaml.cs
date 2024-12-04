using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FamilyTree.BLL.Services;
using FamilyTree.DAL.Models;

namespace FamilyTree.Presentation;

public partial class TreeVisualizationWindow
{
    private readonly IGenealogyService _service;

    public TreeVisualizationWindow(IGenealogyService service, Person root)
    {
        InitializeComponent();
        Title = $"Семейное дерево - {root.FullName}";
        _service = service;
        RenderTree(root);
    }

    private void RenderTree(Person root)
    {
        TreeCanvas.Children.Clear();
        var layout = _service.GetTreeLayout(root, 350, 50, 150, 100);

        // Отрисовка узлов
        foreach (var node in layout)
        {
            var ellipse = new Ellipse
            {
                Width = 50,
                Height = 50,
                Fill = Brushes.White,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            Canvas.SetLeft(ellipse, node.X - 22.5);
            Canvas.SetTop(ellipse, node.Y - 25);
            TreeCanvas.Children.Add(ellipse);

            var text = new TextBlock
            {
                Text = string.Join(Environment.NewLine, node.Person.FullName.Split(' ')),
                Foreground = Brushes.Black,
                FontSize = 11,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 45,
                Height= 45
            };

            Canvas.SetLeft(text, node.X - 20);
            Canvas.SetTop(text, node.Y - 10);
            TreeCanvas.Children.Add(text);
        }

        // Отрисовка связей
        foreach (var node in layout)
        {
            var children = _service.GetChildren(node.Person);
            foreach (var child in children)
            {
                var childNode = layout.First(n => n.Person == child);
                var line = new Line
                {
                    X1 = node.X,
                    Y1 = node.Y + 25,
                    X2 = childNode.X,
                    Y2 = childNode.Y - 25,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };

                TreeCanvas.Children.Add(line);
            }
        }
    }
}