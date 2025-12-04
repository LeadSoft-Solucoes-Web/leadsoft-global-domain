using LeadSoft.Common.GlobalDomain.Helpers;
using LeadSoft.Common.Library;
using LeadSoft.Common.Library.Extensions;

using Microsoft.AspNetCore.Mvc;

namespace LeadSoft.Common.GlobalDomain.DTOs
{
    /// <summary>
    /// Generic paged list response Methods
    /// </summary>
    public partial class DTOPagedListResponse<T>
    {

        /// <summary>
        /// Paged list constructor.
        /// Sets inner constructor with paged list.
        /// </summary>
        /// <param name="aPagedList">Paged list of objects to be set</param>
        public DTOPagedListResponse(PagedList<T> aPagedList) : base(aPagedList)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DTOPagedListResponse{T}"/> class with the specified list, paged
        /// request, and total results count.
        /// </summary>
        /// <param name="aList">The list of items to include in the paged response.</param>
        /// <param name="pagedRequest">The paged request details, including page number and size.</param>
        /// <param name="aTotalResults">The total number of results available across all pages.</param>
        public DTOPagedListResponse(IEnumerable<T> aList, PagedRequest pagedRequest, long aTotalResults) : base(aList, pagedRequest, aTotalResults)
        {
        }

        /// <summary>
        /// Constructor based on an existing list, that sets inner base constructor with informed data.
        /// </summary>
        /// <param name="aList">List of objects</param>
        /// <param name="aPageSize">Page size number</param>
        /// <param name="aCurrentPage">Current page number</param>
        /// <param name="aTotalResults">Count of total results</param>
        public DTOPagedListResponse(IEnumerable<T> aList, int aPageSize, int aCurrentPage, long aTotalResults) : base(aList, aPageSize, aCurrentPage, aTotalResults)
        {
        }

        /// <summary>
        /// Generates a sanitized paging header as a JSON string for the current page context.
        /// </summary>
        /// <remarks>The method constructs a <see cref="PagingMetadata"/> object using the current page
        /// context and generates links for the previous and next pages if they exist. The resulting metadata is
        /// serialized to a JSON string.</remarks>
        /// <typeparam name="TRequest">The type of the paged request, which must inherit from <see cref="PagedRequest"/>.</typeparam>
        /// <param name="aUrl">The URL helper used to generate links for pagination.</param>
        /// <param name="aRouteName">The name of the route to use for generating pagination links.</param>
        /// <param name="aDtoPagedRequestInheritance">The paged request object containing pagination parameters.</param>
        /// <returns>A JSON string representing the paging metadata, including links to the current, previous, and next pages if
        /// applicable.</returns>
        /// <exception cref="ArgumentException">Thrown if <typeparamref name="TRequest"/> does not inherit from <see cref="PagedRequest"/>.</exception>
        public string GetSanitizedPagingHeader<TRequest>(IUrlHelper aUrl, string aRouteName, TRequest aDtoPagedRequestInheritance) where TRequest : PagedRequest
        {
            if (aUrl is null)
                return string.Empty;

            if (!typeof(TRequest).IsSubclassOf(typeof(PagedRequest)))
                throw new ArgumentException(string.Format(ApplicationStatusMessage.ArgumentMustInheritFromDTOPagedRequest, nameof(aDtoPagedRequestInheritance)));

            PagingMetadata metadata = new(CurrentPage, PageSize, TotalResults, TotalPages);

            if (HasPreviousPage)
                metadata.SetPreviousPageLink(aUrl.Link(aRouteName, aDtoPagedRequestInheritance.GetPrevious()));

            if (HasNextPage)
                metadata.SetNextPageLink(aUrl.Link(aRouteName, aDtoPagedRequestInheritance.GetNext()));

            return metadata.SetCurrentPageLink(aUrl.Link(aRouteName, aDtoPagedRequestInheritance))
                           .ToJson();
        }
    }
}