using Domain.Entities;

namespace Application.Services.Interfaces
{
    public interface IGerenciadorTarefas
    {
        Task CancelarTarefaCriadaOuAgendada(int idTarefa);
        Task<Tarefa> Consultar(int idTarefa);
        Task<Tarefa> CriarTarefa();
        IEnumerable<Subtarefa> CriarSubtarefas();
        Task<IEnumerable<Tarefa>> ListarAtivas();
        Task<IEnumerable<Tarefa>> ListarInativas();
    }
}
