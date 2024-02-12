using Domain.Entities;

namespace Application.Services.Interfaces
{
    internal interface IProcessadorTarefas
    {
        Task ImprimirTarefas();
        Task ProcessarTarefas();
        Task IniciarTarefa(Tarefa tarefa);
        Task ImprimirTarefa(Tarefa tarefa);
        Task CancelarTarefa(int id);
        Task Encerrar();
    }
}
