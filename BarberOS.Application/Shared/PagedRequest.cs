namespace BarberOS.Application.Shared
{
    public record PagedRequest(int Page = 1, int PageSize = 20)
    {
        public int Skip => (Math.Max(1, Page) - 1) * Math.Clamp(PageSize, 1, 100);
        public int Take => Math.Clamp(PageSize, 1, 100);
    }
}
