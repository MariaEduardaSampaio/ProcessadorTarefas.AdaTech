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

        public Task CancelarTarefa(int id)
        {
            var tarefaEmExecucao = _repository.GetById(id);
            if (tarefaEmExecucao.Estado == EstadoTarefa.EmExecucao)
            {
                Console.WriteLine($"Tarefa {tarefaEmExecucao.Id} cancelada.");
                tarefaEmExecucao.Estado = EstadoTarefa.Cancelada;
                _repository.Update(tarefaEmExecucao);
            }
            else
                Console.WriteLine("Esta tarefa não está em execução.");
            return Task.CompletedTask;
        }

        public Task Encerrar()
        {
            var tarefasEmExecucao = _repository.GetByStatus(EstadoTarefa.EmExecucao);

            foreach (var item in tarefasEmExecucao)
            {
                Console.WriteLine($"Tarefa {item.Id} em pausa.");
                item.Estado = EstadoTarefa.EmPausa;
                _repository.Update(item);
            }

            return Task.CompletedTask;
        }

        public void AgendarTarefas()
        {
            var tarefas = _repository.GetAll().ToList();
            int quantidadeTarefasAgendadas = _repository.GetByStatus(EstadoTarefa.Agendada).Count();

            foreach (var tarefa in tarefas)
            {
                quantidadeTarefasAgendadas = _repository.GetByStatus(EstadoTarefa.Agendada).Count();

                if ((tarefa.Estado.Equals(EstadoTarefa.Criada)
                    && quantidadeTarefasAgendadas < _tarefasExecutadasEmParalelo)
                    || tarefas.All(t => t.Estado.Equals(EstadoTarefa.Agendada)))
                {
                    tarefa.Estado = EstadoTarefa.Agendada;
                    _repository.Update(tarefa);
                    ImprimirTarefa(tarefa);
                }
            }
        }

        public async Task ProcessarTarefas()
        {
            while (true)
            {
                AgendarTarefas();
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
            // Calcula o número de caracteres '|' completos e o número de espaços em branco restantes
            int completos = (int)Math.Floor((decimal)(porcentagem * tamanhoBarra) / 100);
            int restantes = tamanhoBarra - completos;

            // Constrói a barra de progresso
            StringBuilder barra = new StringBuilder("[");
            barra.Append('=', completos); // Adiciona os '|' completos
            barra.Append(' ', restantes); // Adiciona espaços em branco restantes
            barra.Append(']');

            // Formata a porcentagem para exibir na barra de progresso
            string porcentagemFormatada = porcentagem.ToString().PadLeft(3) + "%";

            // Retorna a barra de progresso formatada com a porcentagem
            return $"{barra} {porcentagemFormatada}";
        }

        public async Task ImprimirTarefas()
        {
            var tarefasEmExecucao = _repository.GetByStatus(EstadoTarefa.EmExecucao);
            var tarefasEmPausa = _repository.GetByStatus(EstadoTarefa.EmPausa);
            var tarefas = tarefasEmExecucao.Concat(tarefasEmPausa);

            foreach (var tarefa in tarefas)
                await ImprimirTarefa(tarefa);
        }

        public async Task ImprimirTarefa(Tarefa tarefa)
        {
            int progresso = 0;

            int totalSubtarefasExecutadas = tarefa.SubtarefasExecutadas!.Count();
            int totalSubtarefas = tarefa.SubtarefasPendentes!.Count() + tarefa.SubtarefasExecutadas!.Count();

            if (totalSubtarefas != 0)
            {
                progresso = (int)((totalSubtarefasExecutadas / (double)totalSubtarefas) * 100);
            }

            Console.WriteLine($"\n_____________________________________________________________________________");
            Console.WriteLine($"Tarefa {tarefa.Id}\t - Estado: {tarefa.Estado}");
            Console.WriteLine($"Iniciada em: {tarefa.IniciadaEm}\t - Encerrada em: {tarefa.EncerradaEm}");
            Console.WriteLine($"Progresso: {ImprimirProgresso(progresso)}%");
            Console.WriteLine($"_____________________________________________________________________________\n");
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
            ImprimirTarefa(tarefa);

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
            ImprimirTarefa(tarefa);
            await Task.Delay(200);
        }
    }
}
