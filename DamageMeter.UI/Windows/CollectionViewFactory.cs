using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

namespace Nostrum.Factories
{
    /// <summary>
    /// Factory class to create <see cref="ICollectionView"/>s and <see cref="ICollectionViewLiveShaping"/>s and automatically bind them to the contained empty event handler. This is required to prevent GC to dispose them when they're still needed.
    /// <para>
    /// Created views can be freed by calling the Free() extension method, allowing the GC to collect them.
    /// </para>
    /// </summary>
    public static class CollectionViewFactory
    {
        public static void Holder(object sender, EventArgs ev) { }

        /// <summary>
        /// Creates a new <see cref="ICollectionView"/> from the given <paramref name="source"/> and binds it to the <see cref="Holder"/> handler.
        /// <para>
        /// Optional <paramref name="predicate"/> and a <paramref name="sortDescr"/> can be passed in to filter and sort the list.
        /// </para>
        /// </summary>
        public static ICollectionView CreateCollectionView<T>(IEnumerable<T> source,
            Predicate<T> predicate = null,
            IEnumerable<SortDescription> sortDescr = null)
        {
            var view = new CollectionViewSource { Source = source }.View;
            if (predicate == null) view.Filter = null;
            else view.Filter = o => predicate.Invoke((T)o);

            if (sortDescr != null)
            {
                foreach (var sd in sortDescr)
                {
                    view.SortDescriptions.Add(sd);
                }
            }

            view.CollectionChanged += Holder;
            return view;
        }

        /// <summary>
        /// Creates a new <see cref="ICollectionViewLiveShaping"/> from the given <paramref name="source"/> and binds it to the <see cref="Holder"/> handler.
        /// <para>
        /// Optional <paramref name="predicate"/>, <paramref name="filters"/> and <paramref name="sortFilters"/> can be passed in to filter and sort the list.
        /// </para>
        /// </summary>
        public static ICollectionViewLiveShaping CreateLiveCollectionView<T>(IEnumerable<T> source,
            Predicate<T> predicate = null,
            string[] filters = null,
            SortDescription[] sortFilters = null)
        {
            var cv = new CollectionViewSource { Source = source }.View;

            if (predicate == null) cv.Filter = null;
            else cv.Filter = o => predicate.Invoke((T)o);

            if (!(cv is ICollectionViewLiveShaping liveView)) return null;
            if (!liveView.CanChangeLiveFiltering) return null;
            if (filters?.Length > 0)
            {
                foreach (var filter in filters)
                {
                    liveView.LiveFilteringProperties.Add(filter);
                }
                liveView.IsLiveFiltering = true;
            }
            ((ICollectionView)liveView).CollectionChanged += Holder;

            if (sortFilters == null || sortFilters.Length <= 0) return liveView;

            foreach (var filter in sortFilters)
            {
                ((ICollectionView)liveView).SortDescriptions.Add(filter);
                liveView.LiveSortingProperties.Add(filter.PropertyName);
            }

            liveView.IsLiveSorting = true;

            return liveView;
        }
    }
}