namespace Aureo.Domain.Common
{
    public static class DataUtc
    {
        // Npgsql (timestamptz) rejeita Kind Unspecified/Local.
        public static DateTime Normalizar(DateTime data) => data.Kind switch
        {
            DateTimeKind.Utc => data,
            DateTimeKind.Local => data.ToUniversalTime(),
            _ => DateTime.SpecifyKind(data, DateTimeKind.Utc)
        };
    }
}
