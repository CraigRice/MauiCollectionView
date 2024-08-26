using System.Collections.ObjectModel;
using MauiTestApp.Models;

namespace MauiTestApp.Pages;

public partial class CollectionViewTest : ContentPage
{
    public ObservableCollection<TestItem> ItemsSource { get; }

    public CollectionViewTest()
    {
        InitializeComponent();

        ItemsSource = [];
        TestCollectionView.ItemsSource = ItemsSource;
    }

    private static void PrependItems(ObservableCollection<TestItem> source, IEnumerable<TestItem> items)
    {
        foreach (var item in items)
        {
            source.Insert(0, item);
        }
    }

    private static void AddItems(ObservableCollection<TestItem> source, IEnumerable<TestItem> items)
    {
        foreach (var item in items)
        {
            source.Add(item);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        AddItems(ItemsSource, YieldItems(1, i => i + 1));
    }

    private void Button_OnClicked(object? sender, EventArgs e)
    {
        ItemsSource.Clear();
        AddItems(ItemsSource, YieldItems(1, i => i + 1));
    }

    private void ButtonPrepend_OnClicked(object? sender, EventArgs e)
    {
        var min = ItemsSource[0];
        PrependItems(ItemsSource, YieldItems(min.Id - 1, i => i - 1));
    }

    private void ButtonAppend_OnClicked(object? sender, EventArgs e)
    {
        var max = ItemsSource[^1];
        AddItems(ItemsSource, YieldItems(max.Id + 1, i => i + 1));
    }

    private static IEnumerable<TestItem> YieldItems(int start, Func<int, int> next)
    {
        const int quantity = 10;

        var current = start;
        for (var i = 0; i < quantity; i++)
        {
            yield return new TestItem(current);
            current = next(current);
        }
    }
}
