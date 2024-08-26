using MauiTestApp.Models;

namespace MauiTestApp.Pages;

public partial class CollectionViewRangeTest : ContentPage
{
    public ObservableRangeCollection<TestItem> ItemsSource { get; }

    public CollectionViewRangeTest()
    {
        InitializeComponent();

        ItemsSource = [];
        TestCollectionView.ItemsSource = ItemsSource;
    }

    private static void PrependItems(ObservableRangeCollection<TestItem> source, IEnumerable<TestItem> items)
    {
        var x = items.Reverse().ToList();
        source.PrependRange(x);
    }

    private static void AddItems(ObservableRangeCollection<TestItem> source, IEnumerable<TestItem> items)
    {
        source.AddRange(items.ToList());
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        AddItems(ItemsSource, YieldItems(1, i => i + 1));
    }

    private void Button_OnClicked(object? sender, EventArgs e)
    {
        ItemsSource.ReplaceRange(YieldItems(1, i => i + 1));
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
