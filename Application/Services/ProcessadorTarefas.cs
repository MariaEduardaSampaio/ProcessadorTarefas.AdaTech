using Domain.Enums;
using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text;

namespace Application.Services
{
    public class ProcessadorTarefas : IProcessadorTarefas
    {
        private readonly IRepository<Tarefa> _repository;
        private readonly int _tarefasExecutadasEmParalelo;

        public ProcessadorTarefas(IRepository<Tarefa> repository, IConfiguration config)
        {
            _tarefasExecutadasEmParalelo = int.Parse(config["TarefasExecutadasEmParalelo"]!);
            _repository = repository;
        }

        public void CancelarTarefasEmExecucao()
        {
            var tarefasEmExecucao = _repository.GetByStatus(EstadoTarefa.EmExecucao);

            foreach (var tarefa in tarefasEmExecucao.ToList())
            {
                tarefa.Estado = EstadoTarefa.Cancelada;
                _repository.Update(tarefa);
            }
        }

        public async Task Encerrar()
        {
            var tarefasEmExecucao = _repository.GetByStatus(EstadoTarefa.EmExecucao);

            foreach (var item in tarefasEmExecucao.ToList())
            {
                Console.WriteLine($"Tarefa {item.Id} em pausa.");
                item.Estado = EstadoTarefa.EmPausa;
                _repository.Update(item);
            }

            return;
        }

        public async Task AgendarTarefas()
        {
            var tarefas = _repository.GetAll().ToList();
            int quantidadeTarefasAgendadas;

            foreach (var tarefa in tarefas.Where(tarefa => tarefa.Estado.Equals(EstadoTarefa.Criada)).Take(_tarefasExecutadasEmParalelo))
            {
                quantidadeTarefasAgendadas = _repository.GetByStatus(EstadoTarefa.Agendada).Count();

                tarefa.Estado = EstadoTarefa.Agendada;
                _repository.Update(tarefa);
            }
            await Task.Delay(1000);
        }

        public async Task ProcessarTarefas()
        {
            while (true)
            {
                await AgendarTarefas();
                IEnumerable<Tarefa> tarefas = _repository.GetByStatus(EstadoTarefa.Agendada).Concat(_repository.GetByStatus(EstadoTarefa.EmPausa));

                Queue<Tarefa> tarefasParaProcessar = new Queue<Tarefa>(tarefas);

                if (tarefasParaProcessar.Count == 0)
                    break;

                var tasksEmExecucao = new List<Task>();

                while (tasksEmExecucao.Count < _tarefasExecutadasEmParalelo && tarefasParaProcessar.Count > 0)
                {
                    Tarefa tarefa = tarefasParaProcessar.Dequeue();
                    tasksEmExecucao.Add(IniciarTarefa(tarefa));
                }

                await Task.WhenAll(tasksEmExecucao);
            }
        }

        public string ImprimirProgresso(int porcentagem, int tamanhoBarra = 20)
        {
            int completos = (int)Math.Floor((decimal)(porcentagem * tamanhoBarra) / 100);
            int restantes = tamanhoBarra - completos;

            StringBuilder barra = new("[");
            barra.Append('=', completos);
            barra.Append(' ', restantes);
            barra.Append(']');

            string porcentagemFormatada = porcentagem.ToString().PadLeft(3);

            return $"{barra} {porcentagemFormatada}%";
        }

        public async Task ImprimirTarefas()
        {
            Console.Clear();
            var tarefas = _repository.GetAll();

            foreach (var tarefa in tarefas)
                ImprimirTarefa(tarefa);

            await Task.Delay(1000);
        }

        public ConsoleColor DefinirCorTarefa(EstadoTarefa estado)
        {
            ConsoleColor corEstado;
            switch (estado)
            {
                case EstadoTarefa.Criada:
                    corEstado = ConsoleColor.Blue;
                    break;
                case EstadoTarefa.EmExecucao:
                    corEstado = ConsoleColor.Yellow;
                    break;
                case EstadoTarefa.Concluida:
                    corEstado = ConsoleColor.Green;
                    break;
                case EstadoTarefa.Cancelada:
                    corEstado = ConsoleColor.Red;
                    break;
                case EstadoTarefa.EmPausa:
                    corEstado = ConsoleColor.Magenta;
                    break;
                case EstadoTarefa.Agendada:
                    corEstado = ConsoleColor.Cyan;
                    break;
                default:
                    corEstado = ConsoleColor.White;
                    break;
            }

            return corEstado;
        }
        public void ImprimirTarefa(Tarefa tarefa)
        {
            int progresso = 0;

            int totalSubtarefasExecutadas = tarefa.SubtarefasExecutadas!.Count();
            int totalSubtarefas = tarefa.SubtarefasPendentes!.Count() + tarefa.SubtarefasExecutadas!.Count();

            if (totalSubtarefas != 0)
                progresso = (int)((totalSubtarefasExecutadas / (double)totalSubtarefas) * 100);

            Console.ForegroundColor = DefinirCorTarefa(tarefa.Estado);

            Console.Write($"\nTarefa {tarefa.Id}\t - Estado: {tarefa.Estado}\t - Iniciada em: {tarefa.IniciadaEm}\t - Encerrada em: {tarefa.EncerradaEm}");
            Console.Write($"\tProgresso: {ImprimirProgresso(progresso)} ({totalSubtarefasExecutadas}/{totalSubtarefas})\n");
            Console.ResetColor();
        }

        public async Task IniciarSubtarefa(Subtarefa subtarefa)
        {
            await Task.Delay(subtarefa.Duracao);
        }

        public async Task IniciarTarefa(Tarefa tarefa)
        {
            tarefa.Estado = EstadoTarefa.EmExecucao;
            tarefa.IniciadaEm = DateTime.Now;

            _repository.Update(tarefa);
            //ImprimirTarefa(tarefa);

            int contadorSubtarefas = 0;
            var duracaoExecucaoTarefa = new Stopwatch();
            var duracaoExecucaoSubtarefa = new Stopwatch();
            duracaoExecucaoTarefa.Start();
            int totalSubtarefas = tarefa.SubtarefasPendentes.Count();

            foreach (var subtarefa in tarefa.SubtarefasPendentes!)
            {
                contadorSubtarefas++;
                //Console.WriteLine($"\n => Tarefa {tarefa.Id} => Subtarefa {contadorSubtarefas} iniciada.");

                duracaoExecucaoSubtarefa.Restart();
                await IniciarSubtarefa(subtarefa);
                duracaoExecucaoSubtarefa.Stop();

                var subtarefaExecutada = tarefa.SubtarefasPendentes.First(subtarefaPendente => subtarefaPendente.Equals(subtarefa));

                List<Subtarefa> subtarefasPendentes = tarefa.SubtarefasPendentes.ToList();
                subtarefasPendentes.Remove(subtarefaExecutada);
                tarefa.SubtarefasPendentes = subtarefasPendentes;

                List<Subtarefa> subtarefasExecutadas = tarefa.SubtarefasExecutadas!.ToList();
                subtarefasExecutadas.Add(subtarefaExecutada);
                tarefa.SubtarefasExecutadas = subtarefasExecutadas;
                //Console.WriteLine($"Subtarefa ({contadorSubtarefas} de {totalSubtarefas}) concluída em {duracaoExecucaoSubtarefa.Elapsed.TotalSeconds:F2} segundos.");
            }

            duracaoExecucaoTarefa.Stop();
            tarefa.Estado = EstadoTarefa.Concluida;
            tarefa.EncerradaEm = DateTime.Now;

            _repository.Update(tarefa);
            //ImprimirTarefa(tarefa);
            await Task.Delay(1000);
        }
    }
}
