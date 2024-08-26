// Coming to .NET some time in the future: https://github.com/dotnet/runtime/issues/18087, though the issue has been open since August 2016.
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace MauiTestApp;

/// <summary>
/// Represents a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObservableRangeCollection<T> : ObservableCollection<T>
{
    /// <summary>
    /// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class.
    /// </summary>
    public ObservableRangeCollection() { }

    /// <summary>
    /// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class that contains elements copied from the specified collection.
    /// </summary>
    /// <param name="collection">collection: The collection from which the elements are copied.</param>
    /// <exception cref="ArgumentNullException">The collection parameter cannot be null.</exception>
    public ObservableRangeCollection(IEnumerable<T> collection)
        : base(collection)
    {
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of the ObservableCollection(Of T).
    /// </summary>
    public void AddRange(IReadOnlyList<T> collection, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Add)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (collection.Count == 0) { return; }

        if (notificationMode != NotifyCollectionChangedAction.Add && notificationMode != NotifyCollectionChangedAction.Reset)
        {
            throw new ArgumentException(@"Mode must be either Add or Reset for AddRange.", nameof(notificationMode));
        }

        CheckReentrancy();

        var startIndex = Count;

        AddRangeCore(collection);

        if (notificationMode == NotifyCollectionChangedAction.Reset)
        {
            RaiseChangeNotificationEventsAsReset();

            return;
        }

        RaiseChangeNotificationEvents(
            action: NotifyCollectionChangedAction.Add,
            changedItems: collection,
            startingIndex: startIndex);
    }

    /// <summary>
    /// Adds the elements of the specified collection to the start of the ObservableCollection(Of T).
    /// </summary>
    public void PrependRange(IReadOnlyList<T> collection, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Add)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (collection.Count == 0) { return; }

        if (notificationMode != NotifyCollectionChangedAction.Add && notificationMode != NotifyCollectionChangedAction.Reset)
        {
            throw new ArgumentException($@"Mode must be either Add or Reset for {nameof(PrependRange)}.", nameof(notificationMode));
        }

        InsertRangeAt(collection, 0, notificationMode);
    }

    public void InsertRangeAt(IReadOnlyList<T> collection, int startIndex, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Add)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (collection.Count == 0) { return; }

        if (notificationMode != NotifyCollectionChangedAction.Add && notificationMode != NotifyCollectionChangedAction.Reset)
        {
            throw new ArgumentException($@"Mode must be either Add or Reset for {nameof(InsertRangeAt)}.", nameof(notificationMode));
        }

        CheckReentrancy();

        var itemsAdded = InsertAtCore(collection, startIndex);

        if (!itemsAdded)
        {
            return;
        }

        if (notificationMode == NotifyCollectionChangedAction.Reset)
        {
            RaiseChangeNotificationEventsAsReset();

            return;
        }

        RaiseChangeNotificationEvents(
            action: NotifyCollectionChangedAction.Add,
            changedItems: collection,
            startingIndex: startIndex);
    }

    /// <summary>
    /// Removes the first occurrence of each item in the specified collection from ObservableCollection(Of T). NOTE: with notificationMode = Remove, removed items starting index is not set because items are not guaranteed to be consecutive.
    /// </summary>
    public void RemoveRange(IEnumerable<T> collection, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Reset)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (notificationMode != NotifyCollectionChangedAction.Remove && notificationMode != NotifyCollectionChangedAction.Reset)
        {
            throw new ArgumentException($@"Mode must be either Remove or Reset for {nameof(RemoveRange)}.", nameof(notificationMode));
        }

        CheckReentrancy();

        if (notificationMode == NotifyCollectionChangedAction.Reset)
        {
            var raiseEvents = false;
            foreach (var item in collection)
            {
                if (Items.Remove(item))
                {
                    raiseEvents = true;
                }
            }

            if (raiseEvents)
            {
                RaiseChangeNotificationEventsAsReset();
            }

            return;
        }

        var changedItems = new List<T>(collection);
        for (var i = 0; i < changedItems.Count; i++)
        {
            if (!Items.Remove(changedItems[i]))
            {
                changedItems.RemoveAt(i); //Can't use a foreach because changedItems is intended to be (carefully) modified
                i--;
            }
        }

        if (changedItems.Count == 0)
        {
            return;
        }

        RaiseChangeNotificationEvents(
            action: NotifyCollectionChangedAction.Remove,
            changedItems: changedItems,
            startingIndex: -1);
    }

    /// <summary>
    /// Clears the current collection and replaces it with the specified collection.
    /// </summary>
    public void ReplaceRange(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        CheckReentrancy();

        var initiallyEmpty = Items.Count == 0;

        if (!initiallyEmpty)
        {
            Items.Clear();
        }

        AddRangeCore(collection);

        var currentlyEmpty = Items.Count == 0;

        if (initiallyEmpty && currentlyEmpty)
        {
            return;
        }

        RaiseChangeNotificationEventsAsReset();
    }

    private void AddRangeCore(IEnumerable<T> collection)
    {
        foreach (var item in collection)
        {
            Items.Add(item);
        }
    }

    private bool InsertAtCore(IEnumerable<T> collection, int startingIndex)
    {
        var i = startingIndex;
        foreach (var item in collection)
        {
            Items.Insert(i, item);
            i++;
        }
        return i != startingIndex;
    }

    private void RaiseChangeNotificationEventsAsReset()
    {
        OnCountPropertyChanged();
        OnIndexerPropertyChanged();
        OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
    }

    private void RaiseChangeNotificationEvents(NotifyCollectionChangedAction action, IReadOnlyList<T> changedItems, int startingIndex)
    {
        ArgumentNullException.ThrowIfNull(changedItems);

        OnCountPropertyChanged();
        OnIndexerPropertyChanged();

        var itemsAsIList = new List<T>(changedItems.Count);
        itemsAsIList.AddRange(changedItems);

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, changedItems: itemsAsIList, startingIndex: startingIndex));
    }

    /// <summary>
    /// Helper to raise a PropertyChanged event for the Count property
    /// </summary>
    private void OnCountPropertyChanged() => OnPropertyChanged(EventArgsCache.CountPropertyChanged);

    /// <summary>
    /// Helper to raise a PropertyChanged event for the Indexer property
    /// </summary>
    private void OnIndexerPropertyChanged() => OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
}

internal static class EventArgsCache
{
    public static readonly PropertyChangedEventArgs CountPropertyChanged = new("Count");
    public static readonly PropertyChangedEventArgs IndexerPropertyChanged = new("Item[]");
    public static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new(NotifyCollectionChangedAction.Reset);
}
