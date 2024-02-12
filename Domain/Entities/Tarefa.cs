using Domain.Interfaces;
using Domain.Enums;

namespace Domain.Entities
{
    public class Tarefa : ITarefa
    {
        public int Id { get; set; }
        public EstadoTarefa Estado { get; set; }
        public DateTime IniciadaEm { get; set; }
        public DateTime EncerradaEm { get; set; }
        public IEnumerable<Subtarefa>? SubtarefasPendentes { get; set; }
        public IEnumerable<Subtarefa>? SubtarefasExecutadas { get; set; }
        public Tarefa(int id, EstadoTarefa estado, DateTime iniciadaEm, DateTime encerradaEm, IEnumerable<Subtarefa>? subtarefasPendentes, IEnumerable<Subtarefa>? subtarefasExecutadas)
        {
            Id = id;
            Estado = estado;
            IniciadaEm = iniciadaEm;
            EncerradaEm = encerradaEm;
            SubtarefasExecutadas = subtarefasExecutadas;
            SubtarefasPendentes = subtarefasPendentes;
        }
    }
}
