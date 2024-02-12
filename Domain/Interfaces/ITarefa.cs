using Domain.Enums;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ITarefa
    {
        int Id { get; }
        EstadoTarefa Estado { get; }
        DateTime IniciadaEm { get; }
        DateTime EncerradaEm { get; }
        IEnumerable<Subtarefa>? SubtarefasPendentes { get; }
        IEnumerable<Subtarefa>? SubtarefasExecutadas { get; }
    }

}
