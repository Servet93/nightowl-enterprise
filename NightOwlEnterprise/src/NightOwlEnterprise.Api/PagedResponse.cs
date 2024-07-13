using Microsoft.AspNetCore.WebUtilities;

namespace NightOwlEnterprise.Api;

public class PagedResponse<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public Uri? FirstPage { get; set; }
    public Uri? LastPage { get; set; }

    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
    public Uri? NextPage { get; set; }
    public Uri? PreviousPage { get; set; }

    public IEnumerable<T> Data { get; set; }


    public PagedResponse(IEnumerable<T> data, int pageNumber, int pageSize)
    {
        this.PageNumber = pageNumber;
        this.PageSize = pageSize;
        this.Data = data;
    }

    public static PagedResponse<T> CreatePagedResponse(IEnumerable<T> pagedData, int totalRecords,
        PaginationFilter validFilter, PaginationUriBuilder paginationUriBuilder, string route)
    {
        var response = new PagedResponse<T>(pagedData, validFilter.PageNumber, validFilter.PageSize);
        var totalPages = ((double)totalRecords / (double)validFilter.PageSize);
        int roundedTotalPages = Convert.ToInt32(Math.Ceiling(totalPages));

        response.NextPage =
            validFilter.PageNumber >= 1 && validFilter.PageNumber < roundedTotalPages
                ? paginationUriBuilder.GetPageUri(
                    new PaginationFilter(validFilter.PageNumber + 1, validFilter.PageSize), route)
                : null;

        response.PreviousPage =
            validFilter.PageNumber - 1 >= 1 && validFilter.PageNumber <= roundedTotalPages
                ? paginationUriBuilder.GetPageUri(
                    new PaginationFilter(validFilter.PageNumber - 1, validFilter.PageSize), route)
                : null;

        response.FirstPage = paginationUriBuilder.GetPageUri(new PaginationFilter(1, validFilter.PageSize), route);
        response.LastPage =
            paginationUriBuilder.GetPageUri(new PaginationFilter(roundedTotalPages, validFilter.PageSize), route);
        response.TotalPages = roundedTotalPages;
        response.TotalRecords = totalRecords;

        response.PageNumber = validFilter.PageNumber;

        return response;
    }
}

public class PaginationFilter
{
    private const int MaxPageSize = 20;
    
    public int PageNumber { get; set; }
    
    public int PageSize { get; set; }
    
    public PaginationFilter()
    {
        this.PageNumber = 1;
        this.PageSize = MaxPageSize;
    }
    
    public PaginationFilter(int? pageNumber, int? pageSize)
    {
        if (pageNumber.HasValue)
        {
            this.PageNumber = pageNumber.Value < 1 ? 1 : pageNumber.Value;    
        }
        else
        {
            this.PageNumber = 1;
        }

        if (pageSize.HasValue)
        {
            this.PageSize = pageSize.Value > MaxPageSize ? MaxPageSize : pageSize.Value;    
        }
        else
        {
            this.PageSize = MaxPageSize;
        }
    }

    public PaginationFilter(int pageNumber, int pageSize, bool checkPageSize)
    {
        if (checkPageSize)
        {
            this.PageNumber = pageNumber < 1 ? 1 : pageNumber;
            this.PageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;
        }
        else
        {
            this.PageNumber = pageNumber < 1 ? 1 : pageNumber;
            this.PageSize = pageSize;
        }
    }
}

public class PaginationUriBuilder
{
    private readonly string _baseUri;
    
    public PaginationUriBuilder(string baseUri)
    {
        _baseUri = baseUri;
    }
    
    public Uri GetPageUri(PaginationFilter filter, string route)
    {
        var enpointUri = new Uri(string.Concat(_baseUri, route));
        var modifiedUri = QueryHelpers.AddQueryString(enpointUri.ToString(), "pageNumber", filter.PageNumber.ToString());
        modifiedUri = QueryHelpers.AddQueryString(modifiedUri, "pageSize", filter.PageSize.ToString());
        return new Uri(modifiedUri);
    }

    public string GetCoachProfilePhotoUri(Guid coachId) => $"{_baseUri}/coachs/{coachId.ToString()}/profile-photo";
    
    public string GetStudentProfilePhotoUri(Guid studentId) => $"{_baseUri}/students/{studentId.ToString()}/profile-photo";
    
}