﻿using Application.Services;
using Microsoft.Extensions.Configuration;
using Domain.Entities;
using Domain.Interfaces;
using Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Application;

class Program
{
    private static readonly IServiceProvider serviceProvider = new ServiceCollection()
            .ApplicationDependencyInjection().BuildServiceProvider();
    private static readonly IGerenciadorTarefas? gerenciador = serviceProvider.GetService<IGerenciadorTarefas>();
    private static readonly IProcessadorTarefas? processador = serviceProvider.GetService<IProcessadorTarefas>();

    private static void ListarOpcoes()
    {
        Console.WriteLine("1. Criar tarefa.");
        Console.WriteLine("2. Cancelar tarefa criada ou agendada.");
        Console.WriteLine("3. Listar tarefas ativas.");
        Console.WriteLine("4. Listar tarefas inativas.");
        Console.WriteLine("5. Executar tarefas.");
        Console.WriteLine("6. Sair do programa.");
    }

    private static int ReceberOpcaoValida()
    {
        int opcao;
        bool valida;
        do
        {
            Console.WriteLine("Opção: ");
            valida = int.TryParse(Console.ReadLine(), out opcao);
        } while (!valida || opcao < 1 || opcao > 6);
        return opcao;
    }

    private static async Task RealizarOpcao(int opcao)
    {
        switch (opcao)
        {
            case 1:
                var tarefaCriada = await gerenciador!.CriarTarefa();
                Console.WriteLine("Tarefa criada com sucesso!");
                processador.ImprimirTarefa(tarefaCriada);
                break;

            case 2:
                bool deuCerto;
                int idTarefaParaCancelar;
                do
                {
                    Console.WriteLine("Entre com o ID da tarefa criada ou agendada que deseja cancelar:");
                    deuCerto = int.TryParse(Console.ReadLine(), out idTarefaParaCancelar);
                } while (!deuCerto);
                await gerenciador!.CancelarTarefaCriadaOuAgendada(idTarefaParaCancelar);
                break;

            case 3:
                var tarefasAtivas = await gerenciador!.ListarAtivas();
                if (tarefasAtivas != null)
                {
                    Console.WriteLine("\tTAREFAS ATIVAS:");
                    foreach (var tarefa in tarefasAtivas)
                        processador.ImprimirTarefa(tarefa);
                }
                else
                    Console.WriteLine("Não existem tarefas ativas no momento.");
                break;

            case 4:
                var tarefasInativas = await gerenciador!.ListarInativas();
                if (tarefasInativas != null)
                {
                    Console.WriteLine("\tTAREFAS INATIVAS:");
                    foreach (var tarefa in tarefasInativas)
                        processador.ImprimirTarefa(tarefa);
                }
                else
                    Console.WriteLine("Não existem tarefas inativas no momento.");
                break;

            case 5:
                Task processamento = processador!.ProcessarTarefas();
                bool isProcessing = true;

                while (isProcessing)
                {
                    // Aguarda a conclusão de uma das tarefas: processamento ou entrada do usuário
                    await Task.WhenAny(processamento, Task.Delay(80));

                    Console.Clear();
                    await processador.ImprimirTarefas();
                    Console.WriteLine("Menu:");
                    Console.WriteLine("1. Cancelar tarefas em execução.");
                    Console.WriteLine("2. Encerrar execução de tarefas.");
                    Console.WriteLine("Escolha uma opção:");

                    if (Console.KeyAvailable)
                    {
                        int opcaoProcessamento = int.Parse(Console.ReadLine());

                        switch (opcaoProcessamento)
                        {
                            case 1:
                                bool conversaoValida;
                                int idTarefa;
                                do
                                {
                                    Console.WriteLine("Entre com o ID da tarefa em execução que deseja cancelar:");
                                    conversaoValida = int.TryParse(Console.ReadLine(), out idTarefa);
                                } while (!conversaoValida);
                                await processador!.CancelarTarefa(idTarefa);
                                break;

                            case 2:
                                Console.WriteLine("Tarefas pausadas!");
                                await processador!.Encerrar();
                                isProcessing = false; // Sai do loop
                                break;

                            default:
                                Console.WriteLine("Opção inválida.");
                                break;
                        }
                    }
                    else if (processamento.IsCompleted)
                    {
                        isProcessing = false; // Sai do loop
                    }
                }

                break;

            case 6:
                Console.WriteLine("Saindo do programa...");
                Environment.Exit(1);
                break;

            default:
                Console.WriteLine("Opção inválida!");
                break;
        }
    }

    static async Task Main()
    {
        try
        {
            while (true)
            {
                Console.Clear();
                ListarOpcoes();
                Console.WriteLine("Entre com alguma opção: ");
                int opcao = ReceberOpcaoValida();

                await RealizarOpcao(opcao);

                Console.WriteLine("Pressione qualquer letra para continuar...");
                Console.ReadKey();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ocorreu um erro ao executar o TaskManager: {ex}");
        }
    }
}
