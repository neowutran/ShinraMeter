using System.ComponentModel;
using Nostrum.Factories;

namespace Nostrum
{
    public static class ICollectionViewExtensions
    {
        /// <summary>
        /// Unsubscribes the <see cref="ICollectionView"/> from the <see cref="CollectionViewFactory.Holder"/> static handler.
        /// </summary>
        public static void Free(this ICollectionView view)
        {
            view.CollectionChanged -= CollectionViewFactory.Holder;
        }
    }

}