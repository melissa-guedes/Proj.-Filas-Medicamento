using System;
using System.Collections.Generic;
using System.Linq;

namespace Medicamento
{
    public class Senha
    {
        public int Id { get; private set; }
        public DateTime DataGerac { get; private set; }
        public DateTime HoraGerac { get; private set; }
        public DateTime? DataAtend { get; set; }
        public DateTime? HoraAtend { get; set; }

        public Senha(int id)
        {
            Id = id;
            DataGerac = DateTime.Now.Date;
            HoraGerac = DateTime.Now;
            DataAtend = null;
            HoraAtend = null;
        }

        public string DadosParciais()
        {
            return $"{Id} - {DataGerac:dd/MM/yyyy} - {HoraGerac:HH:mm:ss}";
        }

        public string DadosCompletos()
        {
            string dataAt = DataAtend.HasValue ? DataAtend.Value.ToString("dd/MM/yyyy") : "-";
            string horaAt = HoraAtend.HasValue ? HoraAtend.Value.ToString("HH:mm:ss") : "-";
            return $"{Id} - {DataGerac:dd/MM/yyyy} - {HoraGerac:HH:mm:ss} - {dataAt} - {horaAt}";
        }
    }

    public class Senhas
    {
        public int ProximoAtendimento { get; private set; }
        public Queue<Senha> FilaSenhas { get; private set; }

        public Senhas()
        {
            FilaSenhas = new Queue<Senha>();
            ProximoAtendimento = 1;
        }

        public void Gerar()
        {
            var s = new Senha(ProximoAtendimento++);
            FilaSenhas.Enqueue(s);
        }

        public List<string> ListarParciais()
        {
            return FilaSenhas.Select(s => s.DadosParciais()).ToList();
        }

        public int Contar() => FilaSenhas.Count;
    }

    public class Guiche
    {
        public int Id { get; private set; }
        public Queue<Senha> Atendimentos { get; private set; }

        public Guiche() : this(0) { }

        public Guiche(int id)
        {
            Id = id;
            Atendimentos = new Queue<Senha>();
        }

        public bool Chamar(Queue<Senha> filaSenhas)
        {
            if (filaSenhas == null) return false;
            if (filaSenhas.Count == 0) return false;

            var s = filaSenhas.Dequeue();
            s.DataAtend = DateTime.Now.Date;
            s.HoraAtend = DateTime.Now;
            Atendimentos.Enqueue(s);
            return true;
        }

        public List<string> ListarAtendimentosCompletos()
        {
            return Atendimentos.Select(a => a.DadosCompletos()).ToList();
        }

        public int TotalAtendimentos() => Atendimentos.Count;
    }

    public class Guiches
    {
        public List<Guiche> ListaGuiches { get; private set; }

        public Guiches()
        {
            ListaGuiches = new List<Guiche>();
        }

        public void Adicionar(Guiche g)
        {
            ListaGuiches.Add(g);
        }

        public Guiche BuscarPorId(int id)
        {
            return ListaGuiches.FirstOrDefault(x => x.Id == id);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var senhas = new Senhas();
            var guiches = new Guiches();

            bool sair = false;
            while (!sair)
            {
                Console.Clear();
                Console.WriteLine("=== Sistema de Atendimento ===");
                Console.WriteLine($"Senhas na fila: {senhas.Contar()}");
                Console.WriteLine($"Guichês cadastrados: {guiches.ListaGuiches.Count}");
                Console.WriteLine();
                Console.WriteLine("0 - Sair");
                Console.WriteLine("1 - Gerar nova senha");
                Console.WriteLine("2 - Listar senhas aguardando");
                Console.WriteLine("3 - Adicionar guichê");
                Console.WriteLine("4 - Chamar próxima senha (por guichê)");
                Console.WriteLine("5 - Listar atendimentos de todos os guichês");
                Console.WriteLine("6 - Listar atendimentos de um guichê");
                Console.WriteLine("7 - Ver estado detalhado");
                Console.Write("Escolha uma opção: ");

                string opc = Console.ReadLine();
                Console.WriteLine();

                switch (opc.Trim())
                {
                    case "0":
                        sair = true;
                        break;

                    case "1":
                        senhas.Gerar();
                        Console.WriteLine("Senha gerada: " + (senhas.ProximoAtendimento - 1));
                        Pause();
                        break;

                    case "2":
                        if (senhas.Contar() == 0)
                        {
                            Console.WriteLine("Não há senhas na fila.");
                        }
                        else
                        {
                            Console.WriteLine("Senhas aguardando (parciais):");
                            foreach (var linha in senhas.ListarParciais())
                                Console.WriteLine("  " + linha);
                        }
                        Pause();
                        break;

                    case "3":
                        int novoId = guiches.ListaGuiches.Count + 1;
                        guiches.Adicionar(new Guiche(novoId));
                        Console.WriteLine($"Guichê {novoId} adicionado.");
                        Pause();
                        break;

                    case "4":
                        if (guiches.ListaGuiches.Count == 0)
                        {
                            Console.WriteLine("Nenhum guichê cadastrado. Adicione um primeiro.");
                            Pause();
                            break;
                        }
                        Console.Write("Informe o id do guichê: ");
                        if (!int.TryParse(Console.ReadLine(), out int idGuiche))
                        {
                            Console.WriteLine("Id inválido.");
                            Pause();
                            break;
                        }
                        var g = guiches.BuscarPorId(idGuiche);
                        if (g == null)
                        {
                            Console.WriteLine("Guichê não encontrado.");
                            Pause();
                            break;
                        }
                        bool sucesso = g.Chamar(senhas.FilaSenhas);
                        if (sucesso)
                        {
                            var ultima = g.Atendimentos.Last();
                            Console.WriteLine($"Guichê {g.Id} chamou a senha {ultima.Id} (Hora: {ultima.HoraAtend:HH:mm:ss}).");
                        }
                        else
                        {
                            Console.WriteLine("Não há senhas para chamar.");
                        }
                        Pause();
                        break;

                    case "5":
                        if (guiches.ListaGuiches.Count == 0)
                        {
                            Console.WriteLine("Nenhum guichê cadastrado.");
                        }
                        else
                        {
                            Console.WriteLine("Atendimentos realizados por guichê:");
                            foreach (var gu in guiches.ListaGuiches)
                            {
                                Console.WriteLine($"Guichê {gu.Id} - total atendimentos: {gu.TotalAtendimentos()}");
                                foreach (var linha in gu.ListarAtendimentosCompletos())
                                    Console.WriteLine("   " + linha);
                                Console.WriteLine();
                            }
                        }
                        Pause();
                        break;

                    case "6":
                        Console.Write("Informe o id do guichê: ");
                        if (!int.TryParse(Console.ReadLine(), out int idG))
                        {
                            Console.WriteLine("Id inválido.");
                            Pause();
                            break;
                        }
                        var guicheSel = guiches.BuscarPorId(idG);
                        if (guicheSel == null)
                        {
                            Console.WriteLine("Guichê não encontrado.");
                        }
                        else
                        {
                            Console.WriteLine($"Atendimentos do Guichê {guicheSel.Id}:");
                            if (guicheSel.TotalAtendimentos() == 0) Console.WriteLine("  Nenhum atendimento ainda.");
                            foreach (var linha in guicheSel.ListarAtendimentosCompletos())
                                Console.WriteLine("  " + linha);
                        }
                        Pause();
                        break;

                    case "7":
                        Console.WriteLine("=== Estado detalhado ===");
                        Console.WriteLine($"Senhas na fila: {senhas.Contar()}");
                        Console.WriteLine($"Guichês: {guiches.ListaGuiches.Count}");
                        if (guiches.ListaGuiches.Count > 0)
                        {
                            foreach (var gu in guiches.ListaGuiches)
                            {
                                Console.WriteLine($"Guichê {gu.Id} -> atendimentos: {gu.TotalAtendimentos()}");
                            }
                        }
                        Pause();
                        break;

                    default:
                        Console.WriteLine("Opção inválida.");
                        Pause();
                        break;
                }
            }

            Console.WriteLine("Encerrando... Obrigado!");
        }

        static void Pause()
        {
            Console.WriteLine();
            Console.WriteLine("Pressione ENTER para continuar...");
            Console.ReadLine();
        }
    }
}
