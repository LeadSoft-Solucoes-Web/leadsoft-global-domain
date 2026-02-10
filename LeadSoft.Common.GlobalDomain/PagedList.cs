using LeadSoft.Common.Library.Extensions;

namespace LeadSoft.Common.GlobalDomain
{
    /// <summary>
    /// Represents a paginated list of items that supports navigation through pages of data.
    /// </summary>
    /// <remarks>The <see cref="PagedList{T}"/> class provides functionality to handle paginated data, 
    /// including properties to determine the current page, total number of pages, and whether  there are previous or
    /// next pages available. It extends the <see cref="List{T}"/> class,  allowing it to be used like a standard list
    /// while also supporting pagination.</remarks>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public class PagedList<T> : List<T> where T : class
    {
        /// <summary>
        /// Gets the current page number in the pagination sequence.
        /// </summary>
        public virtual int CurrentPage { get; set; }

        /// <summary>
        /// Gets the number of items to be displayed on a single page.
        /// </summary>
        public virtual int PageSize { get; set; }

        /// <summary>
        /// Gets the total number of results available from the query.
        /// </summary>
        public virtual long TotalResults { get; set; }

        /// <summary>
        /// Gets the total number of pages available.
        /// </summary>
        public virtual long TotalPages { get; set; }

        /// <summary>
        /// Defines if this paged list has suffered merge operations.
        /// </summary>
        /// <remarks>
        /// It means that the content of the list is different from the original content and the Count of objects can be greater than the requested page size.
        /// Use <see cref="PagedList{T}.Count"/> to verify the current total count of objects.
        /// </remarks>
        public virtual bool HasMerge { get => MergedItemsCount > 0; }

        /// <summary>
        /// Gets the total number of items that have been merged.
        /// </summary>
        public virtual int MergedItemsCount { get; private set; }

        /// <summary>
        /// Gets a value indicating whether there is a previous page available.
        /// </summary>
        public virtual bool HasPreviousPage => CurrentPage > 1;

        /// <summary>
        /// Gets a value indicating whether there is a subsequent page available.
        /// </summary>
        public virtual bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}"/> class.
        /// </summary>
        /// <remarks>This constructor creates an empty instance of the <see cref="PagedList{T}"/>
        /// class.</remarks>
        public PagedList()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}"/> class by copying the properties and items from
        /// an existing paged list.
        /// </summary>
        /// <remarks>The properties <see cref="PageSize"/>, <see cref="CurrentPage"/>, and <see
        /// cref="TotalResults"/> are set to the absolute values of the corresponding properties in <paramref
        /// name="aPagedList"/>. The <see cref="TotalPages"/> is calculated based on the <see cref="TotalResults"/> and
        /// <see cref="PageSize"/>.</remarks>
        /// <param name="aPagedList">The paged list to copy from. Must not be null.</param>
        public PagedList(PagedList<T> aPagedList)
        {
            PageSize = Math.Abs(aPagedList.PageSize);
            CurrentPage = Math.Abs(aPagedList.CurrentPage);
            TotalResults = Math.Abs(aPagedList.TotalResults);
            TotalPages = (long)Math.Ceiling(TotalResults / (double)PageSize);

            if (!aPagedList.IsNull())
                AddRange(aPagedList);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}"/> class with the specified list, paging request,
        /// and total results count.
        /// </summary>
        /// <remarks>The <see cref="PagedList{T}"/> class calculates the total number of pages based on
        /// the total results and page size. If the provided list is not null, its elements are added to the current
        /// page.</remarks>
        /// <param name="aList">The collection of items to include in the current page. This collection can be empty but not null.</param>
        /// <param name="pagedRequest">The paging request containing the desired page size and current page number. The values are converted to
        /// their absolute values.</param>
        /// <param name="aTotalResults">The total number of results available across all pages. This value is converted to its absolute value.</param>
        public PagedList(IEnumerable<T> aList, PagedRequest pagedRequest, long aTotalResults)
        {
            PageSize = Math.Abs(pagedRequest.PageSize);
            CurrentPage = Math.Abs(pagedRequest.CurrentPage);
            TotalResults = Math.Abs(aTotalResults);
            TotalPages = (long)Math.Ceiling(TotalResults / (double)PageSize);

            if (!aList.IsNull())
                AddRange(aList);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}"/> class with the specified list, page size,
        /// current page, and total results.
        /// </summary>
        /// <remarks>The <see cref="PagedList{T}"/> class calculates the total number of pages based on
        /// the total results and page size.</remarks>
        /// <param name="aList">The collection of items to include in the current page. This can be an empty collection if there are no
        /// items for the page.</param>
        /// <param name="aPageSize">The number of items per page. Must be a positive integer.</param>
        /// <param name="aCurrentPage">The current page number. Must be a positive integer.</param>
        /// <param name="aTotalResults">The total number of results available across all pages. Must be a non-negative integer.</param>
        public PagedList(IEnumerable<T> aList, int aPageSize, int aCurrentPage, long aTotalResults)
        {
            PageSize = Math.Abs(aPageSize);
            CurrentPage = Math.Abs(aCurrentPage);
            TotalResults = Math.Abs(aTotalResults);
            TotalPages = (long)Math.Ceiling(TotalResults / (double)aPageSize);

            if (!aList.IsNull())
                AddRange(aList);
        }

        /// <summary>
        /// Converts the specified enumerable list to a new <see cref="PagedList{D}"/> instance.
        /// </summary>
        /// <remarks>The pagination properties such as <c>PageSize</c>, <c>CurrentPage</c>,
        /// <c>TotalResults</c>, and <c>TotalPages</c> are derived from the current instance's state.</remarks>
        /// <typeparam name="D">The type of elements in the list, which must be a reference type.</typeparam>
        /// <param name="aList">The enumerable list of items to be converted into a paged list. If the list is null, an empty paged list is
        /// returned.</param>
        /// <returns>A new <see cref="PagedList{D}"/> containing the elements from the specified list, with pagination properties
        /// set based on the current instance's configuration.</returns>
        public PagedList<D> ToNew<D>(IEnumerable<D> aList) where D : class
        {
            PagedList<D> pagedList = new()
            {
                PageSize = Math.Abs(PageSize),
                CurrentPage = Math.Abs(CurrentPage),
                TotalResults = Math.Abs(TotalResults),
                TotalPages = (long)Math.Ceiling(TotalResults / (double)PageSize)
            };

            if (!aList.IsNull())
                pagedList.AddRange(aList);

            return pagedList;
        }

        public PagedList<T> Merge(IEnumerable<T> aList)
        {
            if (aList is null || !aList.Any())
                return this;

            AddRange(aList);
            MergedItemsCount = aList.Count();

            return this;
        }

        /// <summary>
        /// Method that checks if list is empty
        /// </summary>
        /// <returns>True if empty</returns>
        public bool IsEmpty() => !this.Any();
    }
}
