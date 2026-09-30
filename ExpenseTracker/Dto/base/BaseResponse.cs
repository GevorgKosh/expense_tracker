public class BaseResponse<T>
{
  public int Status { get; set; }
  public string? Error { get; set; }
  public string Message { get; set; } = string.Empty;
  public T? Details { get; set; }
}