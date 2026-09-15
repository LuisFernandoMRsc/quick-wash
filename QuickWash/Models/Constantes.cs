namespace QuickWash.Models;

public static class Roles
{
    public const string Estudiante = "Estudiante";
    public const string Personal = "Personal";
}

public static class EstadosReserva
{
    public const string Pendiente = "Pendiente";
    public const string Proceso = "Proceso";
    public const string Finalizada = "Finalizada";
    public const string Cancelada = "Cancelada";

    public static readonly string[] Todos = [Pendiente, Proceso, Finalizada, Cancelada];
}

public static class EstadosMaquina
{
    public const string Operativa = "Operativa";
    public const string Mantenimiento = "Mantenimiento";
}
