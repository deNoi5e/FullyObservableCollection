using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace observable
{
    class Observable
    {
        static void Main(string[] args)
        {
            List<ItemPropertyChangedEventArgs> ItemEventList;
            ItemEventList = new List<ItemPropertyChangedEventArgs>();


            var items = new FullyObservableCollection<Entry>();
            items.ItemPropertyChanged += (o, e) => ItemEventList.Add(e);

            var entry1 = new Entry(1, "One");
            var entry2 = new Entry(2, "Two");

            items.Add(entry1);
            items.Add(entry2);

            foreach (var entry in items)
            {
                Console.WriteLine($"Entry: {entry.Id} Name: {entry.Name}");
            }

            Console.ReadKey();

            items[1].Name = "Three";


            foreach (var entry in items)
            {
                Console.WriteLine($"Entry: {entry.Id} Name: {entry.Name}");
            }

            Console.ReadKey();

            foreach (var item in ItemEventList)
            {
                Console.WriteLine($"Item: {item.CollectionEntry.Name} Field {item.Property}");
            }
        }

        public class FullyObservableCollection<T> : ObservableCollection<T>
            where T : INotifyPropertyChanged
        {
            /// <summary>
            /// Occurs when a property is changed within an item.
            /// </summary>
            public event EventHandler<ItemPropertyChangedEventArgs> ItemPropertyChanged;

            public FullyObservableCollection() : base()
            {
            }

            public FullyObservableCollection(List<T> list) : base(list)
            {
                ObserveAll();
            }

            public FullyObservableCollection(IEnumerable<T> enumerable) : base(enumerable)
            {
                ObserveAll();
            }

            protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                if (e.Action == NotifyCollectionChangedAction.Remove ||
                    e.Action == NotifyCollectionChangedAction.Replace)
                {
                    foreach (T item in e.OldItems)
                        item.PropertyChanged -= ChildPropertyChanged;
                }

                if (e.Action == NotifyCollectionChangedAction.Add ||
                    e.Action == NotifyCollectionChangedAction.Replace)
                {
                    foreach (T item in e.NewItems)
                        item.PropertyChanged += ChildPropertyChanged;
                }

                base.OnCollectionChanged(e);
            }


            protected void OnItemPropertyChanged(Entry e, string prop)
            {
                ItemPropertyChanged?.Invoke(this, new ItemPropertyChangedEventArgs(e, prop));
            }


            protected override void ClearItems()
            {
                foreach (T item in Items)
                    item.PropertyChanged -= ChildPropertyChanged;

                base.ClearItems();
            }

            private void ObserveAll()
            {
                foreach (T item in Items)
                    item.PropertyChanged += ChildPropertyChanged;
            }

            private void ChildPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                Entry typedSender = (Entry) sender;

                OnItemPropertyChanged(typedSender, e.PropertyName);
            }
        }

        /// <summary>
        /// Provides data for the <see cref="FullyObservableCollection{T}.ItemPropertyChanged"/> event.
        /// </summary>
        public class ItemPropertyChangedEventArgs : EventArgs
        {
            /// <summary>
            /// Gets the index in the collection for which the property change has occurred.
            /// </summary>
            /// <value>
            /// Index in parent collection.
            /// </value>
            // public int CollectionIndex { get; }
            public Entry CollectionEntry { get; }

            public string Property { get; }


            /// <summary>
            /// Initializes a new instance of the <see cref="ItemPropertyChangedEventArgs"/> class.
            /// </summary>
            /// <param name="index">The index in the collection of changed item.</param>
            /// <param name="name">The name of the property that changed.</param>
            public ItemPropertyChangedEventArgs(Entry changedEntry, string prop)
            {
                CollectionEntry = changedEntry;
                Property = prop;
            }
        }
    }

    internal class Entry : INotifyPropertyChanged
    {
        private int _id;
        private string _name;

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public Entry()
        {
        }

        public Entry(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}