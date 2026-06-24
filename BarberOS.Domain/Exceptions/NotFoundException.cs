namespace BarberOS.Domain.Exceptions
{
    public class NotFoundException : DomainException
    {
        public NotFoundException(string message) : base(message) { }

        public static NotFoundException For(string entityName, object id) =>
            new($"No se encontró {entityName} con identificador '{id}'.");
    }
}
