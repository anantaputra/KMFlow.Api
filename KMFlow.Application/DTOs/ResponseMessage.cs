public class BaseResponse
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }

    public BaseResponse()
    {
        Status = true;
        Message = "Success";
    }

    public BaseResponse(bool status, string message)
    {
        Status = status;
        Message = message;
    }
}

public class Response<T> : BaseResponse
{
    public T? Data { get; set; }

    public Response() : base() { }

    public Response(T data) : base()
    {
        Data = data;
    }

    public Response(bool status, string message, T? data = default)
        : base(status, message)
    {
        Data = data;
    }
}

public class ResponseList<T> : BaseResponse
{
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public int TotalCount { get; set; }

    public ResponseList(bool status, string message, IEnumerable<T> data)
        : base(status, message)
    {
        Data = data;
        TotalCount = data.Count();
    }

    public ResponseList(IEnumerable<T> data) : base()
    {
        Data = data;
        TotalCount = data.Count();
    }

    public ResponseList(IEnumerable<T> data, int totalCount) : base()
    {
        Data = data;
        TotalCount = totalCount;
    }
}

public class PagedResponse<T> : ResponseList<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;

    public PagedResponse(IEnumerable<T> data, int totalCount, int pageNumber, int pageSize)
        : base(data, totalCount)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
