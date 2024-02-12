using Domain.Interfaces;

namespace Domain.Entities
{
    public class Subtarefa: ISubtarefa
    {
        public TimeSpan Duracao { get; set; }
        public Subtarefa(TimeSpan duracao)
        {
            Duracao = duracao;
        }
    }
}
