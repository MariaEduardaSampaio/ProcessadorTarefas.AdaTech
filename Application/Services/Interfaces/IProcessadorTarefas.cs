using Domain.Entities;

namespace Application.Services.Interfaces
{
    internal interface IProcessadorTarefas
    {
        Task ProcessarTarefas();
        Task IniciarTarefa(Tarefa tarefa);
        Task ImprimirTarefas();
        void ImprimirTarefa(Tarefa tarefa);
        void CancelarTarefasEmExecucao();
        Task Encerrar();
        Task AgendarTarefas();
    }
}
