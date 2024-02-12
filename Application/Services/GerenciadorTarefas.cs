using Domain.Enums;
using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Application.Services
{
    internal class GerenciadorTarefas : IGerenciadorTarefas
    {
        private static int ContadorId = 0;
        private readonly IRepository<Tarefa> _repository;
        private readonly int _quantidadeTarefasEstaticas;
        private readonly int _quantidadeMinimaSubtarefas;
        private readonly int _quantidadeMaximaSubtarefas;
        private readonly int _tempoMinimoExecucaoSubtarefa;
        private readonly int _tempoMaximoExecucaoSubtarefa;

        public GerenciadorTarefas(IRepository<Tarefa> repository, IConfiguration config)
        {
            _repository = repository;
            _quantidadeTarefasEstaticas = int.Parse(config["QuantidadeTarefasEstaticas"]);
            _quantidadeMinimaSubtarefas = int.Parse(config["QuantidadeMinimaSubtarefas"]);
            _quantidadeMaximaSubtarefas = int.Parse(config["QuantidadeMaximaSubtarefas"]);
            _tempoMinimoExecucaoSubtarefa = int.Parse(config["TempoMinimoExecucaoSubtarefa"]);
            _tempoMaximoExecucaoSubtarefa = int.Parse(config["TempoMaximoExecucaoSubtarefa"]);

            for (int i = 0; i < _quantidadeTarefasEstaticas; i++)
                CriarTarefa();
        }

        public Task<Tarefa> CriarTarefa()
        {
            var tarefaCriada = new Tarefa(ContadorId++, EstadoTarefa.Criada, 
                default, default, CriarSubtarefas(), new List<Subtarefa>());

            _repository.Add(tarefaCriada);

            return Task.FromResult(tarefaCriada);
        }


        public async Task CancelarTarefaCriadaOuAgendada(int idTarefa)
        {
            var tarefa = _repository.GetById(idTarefa);
            if (tarefa!.Estado == EstadoTarefa.Criada ||
                tarefa.Estado == EstadoTarefa.Agendada)
            {
                if (tarefa.IniciadaEm == default)
                    tarefa.IniciadaEm = DateTime.Now;
                tarefa.EncerradaEm = DateTime.Now;
                tarefa.Estado = EstadoTarefa.Cancelada;

                _repository.Update(tarefa);

                Task.FromResult(tarefa);
            }
            else
                Console.WriteLine("Não é possível cancelar uma tarefa que não tenha sido criada ou agendada.");
        }

        public Task<Tarefa> Consultar(int idTarefa)
        {
            var tarefa = _repository.GetById(idTarefa);
            return Task.FromResult(tarefa);
        }

        public IEnumerable<Subtarefa> CriarSubtarefas()
        {
            List<Subtarefa> subtasks = new();
            int quantidadeTarefas = new Random().Next(_quantidadeMinimaSubtarefas, _quantidadeMaximaSubtarefas);

            for (int i = 0; i < quantidadeTarefas; i++)
            {
                int duracao = new Random().Next(_tempoMinimoExecucaoSubtarefa, _tempoMaximoExecucaoSubtarefa);
                Subtarefa novaSubtask = new(TimeSpan.FromSeconds(duracao));
                subtasks.Add(novaSubtask);
            }

            return subtasks.AsEnumerable();
        }

        public Task<IEnumerable<Tarefa>> ListarAtivas()
        {
            var tarefas = _repository.GetAll().Where(t => t.Estado != EstadoTarefa.Cancelada && t.Estado != EstadoTarefa.Concluida);
            return Task.FromResult(tarefas);
        }

        public Task<IEnumerable<Tarefa>> ListarInativas()
        {
            var tarefas = _repository.GetAll().Where(t => t.Estado == EstadoTarefa.Cancelada || t.Estado == EstadoTarefa.Concluida);
            return Task.FromResult(tarefas);
        }
    }
}
