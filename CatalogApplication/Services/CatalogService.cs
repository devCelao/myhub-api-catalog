using CatalogDomain.Dtos;
using CatalogDomain.Entities;
using CatalogInfrastructure.Repositories;
using MicroserviceCore.Respostas;
using MicroserviceCore.Services;

namespace CatalogApplication.Services;

public interface ICatalogService
{
    // Plano
    Task<RespostaProcessamento> CriarPlanoAsync(PlanoRequest request);
    Task<RespostaProcessamento> AtualizarPlanoAsync(PlanoRequest request);
    Task<RespostaProcessamento> ObterPlanoAsync(string codPlano);
    Task<RespostaProcessamento> ListarPlanosAsync();
    Task<RespostaProcessamento> ExcluirPlanoAsync(string codPlano);

    // Serviço
    Task<RespostaProcessamento> CriarServicoAsync(ServicoRequest request);
    Task<RespostaProcessamento> AtualizarServicoAsync(ServicoRequest request);
    Task<RespostaProcessamento> ObterServicoAsync(string codServico);
    Task<RespostaProcessamento> ListarServicosAsync();
    Task<RespostaProcessamento> ExcluirServicoAsync(string codServico);

    // Vínculo Plano-Serviço
    Task<RespostaProcessamento> VincularServicosAoPlanoAsync(PlanoServicosRequest planoServicos);
    Task<RespostaProcessamento> ListarServicosDoPlanoAsync(string codPlano);
    Task<RespostaProcessamento> ListarPlanosDoServicoAsync(string codServico);

    // Funcao
    Task<RespostaProcessamento> CriarFuncaoAsync(string codServico, FuncaoRequest request);
    Task<RespostaProcessamento> AtualizarFuncaoAsync(string codServico, FuncaoRequest request);
    Task<RespostaProcessamento> ListarFuncoesDoServicoAsync(string codServico);
    Task<RespostaProcessamento> ExcluirFuncaoAsync(string codServico, string codFuncao);
    Task<RespostaProcessamento> ReordenarFuncoesAsync(string codServico, FuncaoRequest[] funcoes);
}

public class CatalogService(ICatalogRepository repository) : BaseContextService, ICatalogService
{
    private readonly ICatalogRepository repository = repository;

    #region Métodos de Plano

    public async Task<RespostaProcessamento> CriarPlanoAsync(PlanoRequest request)
    {
        // 1. Validar dados do plano
        if (!ValidarDadosPlano(request)) return RetornaProcessamento();

        // 2. Verificar se o plano já existe
        var planoExistente = await repository.ObterPlanoPorCodigo(request.CodPlano);
        if (planoExistente is not null)
        {
            AddErroProcessamento($"Plano com código {request.CodPlano} já existe.");
            return RetornaProcessamento();
        }

        // 3. Criar o plano (sem serviços)
        var plano = new Plano(request.CodPlano, request.NomePlano, request.ValorBase);
        plano.DefinirGeraCobranca(request.IndGeraCobranca);
        plano.AlteraStatusPlano(request.IndAtivo);
        repository.AdicionarPlano(plano);

        // 4. Salvar mudanças
        if (!await repository.UnitOfWork.Commit())
        {
            AddErroProcessamento("Erro interno ao criar plano. Tente novamente.");
            return RetornaProcessamento();
        }

        // 5. Retornar dados do plano criado
        var planoDto = await repository.ObterPlanoComServicos(request.CodPlano);
        AdicionaRetorno(planoDto);
        return RetornaProcessamento();
    }

    public async Task<RespostaProcessamento> AtualizarPlanoAsync(PlanoRequest request)
    {
        // 1. Buscar plano existente
        var plano = await repository.ObterPlanoParaEdicao(request.CodPlano);
        if (plano is null)
        {
            AddErroProcessamento($"Plano com código {request.CodPlano} não encontrado.");
            return RetornaProcessamento();
        }

        // 2. Atualizar dados do plano (sem mexer nos serviços)
        plano.AlterarNome(request.NomePlano);
        plano.DefinirValorBase(request.ValorBase);
        plano.DefinirGeraCobranca(request.IndGeraCobranca);
        plano.AlteraStatusPlano(request.IndAtivo);
        repository.AtualizarPlano(plano);

        // 3. Salvar mudanças
        if (!await repository.UnitOfWork.Commit())
        {
            AddErroProcessamento("Erro interno ao atualizar plano. Tente novamente.");
            return RetornaProcessamento();
        }

        // 4. Retornar dados atualizados
        var planoAtualizado = await repository.ObterPlanoComServicos(request.CodPlano);
        AdicionaRetorno(planoAtualizado);
        return RetornaProcessamento();
    }

    public async Task<RespostaProcessamento> ObterPlanoAsync(string codPlano)
    {
        var plano = await repository.ObterPlanoComServicos(codPlano);
        if (plano is null)
        {
            AddErroProcessamento($"Plano com código {codPlano} não encontrado.");
            return RetornaProcessamento();
        }

        AdicionaRetorno(plano);
        return RetornaProcessamento();
    }

    public async Task<RespostaProcessamento> ListarPlanosAsync()
    {
        var planos = await repository.ListarPlanosComServicos();
        AdicionaRetorno(planos);
        return RetornaProcessamento();
    }

    public async Task<RespostaProcessamento> ExcluirPlanoAsync(string codPlano)
    {
        var plano = await repository.ObterPlanoParaEdicao(codPlano);
        if (plano is null)
        {
            AddErroProcessamento($"Plano com código {codPlano} não encontrado.");
            return RetornaProcessamento();
        }

        repository.RemoverPlano(plano);
        
        if (!await repository.UnitOfWork.Commit())
        {
            AddErroProcessamento("Erro interno ao excluir plano. Tente novamente.");
            return RetornaProcessamento();
        }
        AdicionaRetorno($"Plano {codPlano} excluído com sucesso!");
        return RetornaProcessamento();
    }

    #endregion

    #region Métodos de Serviço

    public async Task<RespostaProcessamento> CriarServicoAsync(ServicoRequest request)
    {
        // 1. Validar dados do serviço
        if (!ValidarDadosServico(request)) return RetornaProcessamento();

        // 2. Verificar se o serviço já existe
        var servicoExistente = await repository.ObterServicoPorCodigo(request.CodServico);
        if (servicoExistente is not null)
        {
            AddErroProcessamento($"Serviço com código {request.CodServico} já existe.");
            return RetornaProcessamento();
        }

        // 3. Criar o serviço
        var servico = new Servico(request.CodServico, request.NomeServico);
        if (!string.IsNullOrEmpty(request.Descricao))
            servico.AlterarDescricao(request.Descricao);

        repository.AdicionarServico(servico);

        // 4. Salvar mudanças
        if (!await repository.UnitOfWork.Commit())
        {
            AddErroProcessamento("Erro interno ao criar serviço. Tente novamente.");
            return RetornaProcessamento();
        }

        // 5. Retornar dados do serviço criado
        var servicoDto = await repository.ObterServicoPorCodigo(request.CodServico);
        AdicionaRetorno(servicoDto);
        return RetornaProcessamento();
    }

    public async Task<RespostaProcessamento> AtualizarServicoAsync(ServicoRequest request)
    {
        // 1. Buscar serviço existente
        var servico = await repository.ObterServicoParaEdicao(request.CodServico);
        if (servico is null)
        {
            AddErroProcessamento($"Serviço com código {request.CodServico} não encontrado.");
            return RetornaProcessamento();
        }

        // 2. Atualizar dados do serviço
        servico.AlterarNome(request.NomeServico);
        servico.AlterarDescricao(request.Descricao);
        repository.AtualizarServico(servico);

        // 3. Salvar mudanças
        if (!await repository.UnitOfWork.Commit())
        {
            AddErroProcessamento("Erro interno ao atualizar serviço. Tente novamente.");
            return RetornaProcessamento();
        }

        // 4. Retornar dados atualizados
        var servicoAtualizado = await repository.ObterServicoPorCodigo(request.CodServico);
        AdicionaRetorno(servicoAtualizado);
        return RetornaProcessamento();
    }

    public async Task<RespostaProcessamento> ObterServicoAsync(string codServico)
    {
        var servico = await repository.ObterServicoPorCodigo(codServico);
        if (servico is null)
        {
            AddErroProcessamento($"Serviço com código {codServico} não encontrado.");
            return RetornaProcessamento();
        }

        AdicionaRetorno(servico);
        return RetornaProcessamento();
    }

    public async Task<RespostaProcessamento> ListarServicosAsync()
    {
        var servicos = await repository.ListarServicos();
        AdicionaRetorno(servicos);
        return RetornaProcessamento();
    }

    public async Task<RespostaProcessamento> ExcluirServicoAsync(string codServico)
    {
        var servico = await repository.ObterServicoParaEdicao(codServico);
        if (servico is null)
        {
            AddErroProcessamento($"Serviço com código {codServico} não encontrado.");
            return RetornaProcessamento();
        }

        repository.RemoverServico(servico);
        
        if (!await repository.UnitOfWork.Commit())
        {
            AddErroProcessamento("Erro interno ao excluir serviço. Tente novamente.");
            return RetornaProcessamento();
        }
        AdicionaRetorno($"Serviço {codServico} removido");
        return RetornaProcessamento();
    }

    #endregion

    #region Métodos Privados - Validação

    private bool ValidarDadosPlano(PlanoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CodPlano))
        {
            AddErroProcessamento("Código do plano é obrigatório.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.NomePlano))
        {
            AddErroProcessamento("Nome do plano é obrigatório.");
            return false;
        }

        if (request.ValorBase < 0)
        {
            AddErroProcessamento("Valor base não pode ser negativo.");
            return false;
        }

        return true;
    }

    private bool ValidarDadosServico(ServicoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CodServico))
        {
            AddErroProcessamento("Código do serviço é obrigatório.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.NomeServico))
        {
            AddErroProcessamento("Nome do serviço é obrigatório.");
            return false;
        }

        return true;
    }

    #endregion

    #region Métodos de Vínculo Plano-Serviço

    public async Task<RespostaProcessamento> VincularServicoAoPlanoAsync(string codPlano, string codServico)
    {
        // 1. Verificar se o plano existe
        var plano = await repository.ObterPlanoPorCodigo(codPlano);
        if (plano is null)
        {
            AddErroProcessamento($"Plano com código {codPlano} não encontrado.");
            return RetornaProcessamento();
        }

        // 2. Verificar se o serviço existe
        var servico = await repository.ObterServicoPorCodigo(codServico);
        if (servico is null)
        {
            AddErroProcessamento($"Serviço com código {codServico} não encontrado.");
            return RetornaProcessamento();
        }

        // 3. Verificar se já existe o vínculo
        var vínculoExistente = await repository.ObterPlanoServico(codPlano, codServico);
        if (vínculoExistente is not null)
        {
            AddErroProcessamento($"Serviço {codServico} já está vinculado ao plano {codPlano}.");
            return RetornaProcessamento();
        }

        // 4. Criar o vínculo
        var planoServico = new PlanoServico
        {
            CodPlano = codPlano,
            CodServico = codServico
        };
        repository.AdicionarPlanoServico(planoServico);

        // 5. Salvar mudanças
        if (!await repository.UnitOfWork.Commit())
        {
            AddErroProcessamento("Erro interno ao vincular serviço ao plano. Tente novamente.");
            return RetornaProcessamento();
        }

        AdicionaRetorno($"Serviço {codServico} agregado com sucesso ao plano {codPlano}");

        return RetornaProcessamento();
    }

    public async Task<RespostaProcessamento> DesvincularServicoDoPlanoAsync(string codPlano, string codServico)
    {
        // 1. Verificar se o vínculo existe
        var vínculo = await repository.ObterPlanoServico(codPlano, codServico);
        if (vínculo is null)
        {
            AddErroProcessamento($"Agregaçao do serviço {codServico} removida do plano {codPlano}.");
            return RetornaProcessamento();
        }

        // 2. Remover o vínculo
        repository.RemoverPlanoServico(vínculo);

        // 3. Salvar mudanças
        if (!await repository.UnitOfWork.Commit())
        {
            AddErroProcessamento("Erro interno ao desvincular serviço do plano. Tente novamente.");
            return RetornaProcessamento();
        }

        AdicionaRetorno($"Serviço {codServico}, removido com sucesso ao plano {codPlano}");

        return RetornaProcessamento();
    }

    public async Task<RespostaProcessamento> VincularServicosAoPlanoAsync(PlanoServicosRequest planoServicos)
    {
        // 1. Verificar se o plano existe
        var plano = await repository.ObterPlanoPorCodigo(planoServicos.CodPlano);
        if (plano is null)
        {
            AddErroProcessamento($"Plano com código {planoServicos.CodPlano} não encontrado.");
            return RetornaProcessamento();
        }

        // 2. É desvinculo de servicos?
        var servicosPlano = await repository.ObterServicosDoPlano(planoServicos.CodPlano);

        if (!planoServicos.HasServices)
        {
            if(servicosPlano.Count == 0)
            {
                AdicionaRetorno(await repository.ObterPlanoComServicos(planoServicos.CodPlano));
                return RetornaProcessamento();
            }

            // 2.1 Remover todos os vínculos
            foreach (var servico in servicosPlano)
            {
                var vínculo = await repository.ObterPlanoServico(planoServicos.CodPlano, servico.CodServico);
                if (vínculo is not null)
                {
                    repository.RemoverPlanoServico(vínculo);
                }
            }
            if (!await repository.UnitOfWork.Commit())
            {
                AddErroProcessamento("Erro interno ao desvincular serviços do plano. Tente novamente.");
                return RetornaProcessamento();
            }

            AdicionaRetorno(await repository.ObterPlanoComServicos(planoServicos.CodPlano));
            return RetornaProcessamento();
        }

        // 3. Atualiza vínculos de serviços
        var servicosExistentesNoPlano = servicosPlano.Select(s => s.CodServico).ToList(); // do banco
        var servicosParaAdicionar = planoServicos.CodServicos!
            .Where(cod => !servicosExistentesNoPlano.Contains(cod))
            .ToList();
        var servicosParaRemover = servicosExistentesNoPlano
            .Where(cod => !planoServicos.CodServicos!.Contains(cod))
            .ToList();

        // 3.1 Verifica se há algo para fazer
        if (servicosParaAdicionar.Count == 0 && servicosParaRemover.Count == 0)
        {
            AdicionaRetorno(await repository.ObterPlanoComServicos(planoServicos.CodPlano));
            return RetornaProcessamento();
        }
        // 3.1 Adicionar novos vínculos
        foreach (var codServico in servicosParaAdicionar)
        {
            var planoServico = new PlanoServico
            {
                CodPlano = planoServicos.CodPlano,
                CodServico = codServico
            };
            repository.AdicionarPlanoServico(planoServico);
        }

        // 3.2 Remover vínculos não mais desejados
        foreach (var codServico in servicosParaRemover)
        {
            var vínculo = await repository.ObterPlanoServico(planoServicos.CodPlano, codServico);
            if (vínculo is not null)
            {
                repository.RemoverPlanoServico(vínculo);
            }
        }

        // 4. Salvar mudanças
        if (!await repository.UnitOfWork.Commit())
        {
            AddErroProcessamento("Erro interno ao vincular serviços ao plano. Tente novamente.");
            return RetornaProcessamento();
        }

        AdicionaRetorno(await repository.ObterPlanoComServicos(planoServicos.CodPlano));
        return RetornaProcessamento();
    }

    public async Task<RespostaProcessamento> DesvincularTodosServicosDoPlanoAsync(string codPlano)
    {
        // 1. Verificar se o plano existe
        var plano = await repository.ObterPlanoPorCodigo(codPlano);
        if (plano is null)
        {
            AddErroProcessamento($"Plano com código {codPlano} não encontrado.");
            return RetornaProcessamento();
        }

        // 2. Remover todos os vínculos
        await repository.RemoverServicosDoPlano(codPlano);

        // 3. Salvar mudanças
        if (!await repository.UnitOfWork.Commit())
        {
            AddErroProcessamento("Erro interno ao desvincular serviços do plano. Tente novamente.");
            return RetornaProcessamento();
        }

        AdicionaRetorno(await repository.ObterPlanoComServicos(codPlano));

        return RetornaProcessamento();
    }

    public async Task<RespostaProcessamento> ListarServicosDoPlanoAsync(string codPlano)
    {
        // 1. Verificar se o plano existe
        var plano = await repository.ObterPlanoPorCodigo(codPlano);
        if (plano is null)
        {
            AddErroProcessamento($"Plano com código {codPlano} não encontrado.");
            return RetornaProcessamento();
        }

        // 2. Buscar serviços do plano
        var servicos = await repository.ObterServicosDoPlano(codPlano);
        AdicionaRetorno(servicos);
        return RetornaProcessamento();
    }

    public async Task<RespostaProcessamento> ListarPlanosDoServicoAsync(string codServico)
    {
        // 1. Verificar se o serviço existe
        var servico = await repository.ObterServicoPorCodigo(codServico);
        if (servico is null)
        {
            AddErroProcessamento($"Serviço com código {codServico} não encontrado.");
            return RetornaProcessamento();
        }

        // 2. Buscar planos do serviço
        var planos = await repository.ObterPlanosDoServico(codServico);
        AdicionaRetorno(planos);
        return RetornaProcessamento();
    }

    #endregion

    #region Métodos de Funcao

    public async Task<RespostaProcessamento> CriarFuncaoAsync(string codServico, FuncaoRequest request)
    {
        // 1. Validar dados da função
        if (!ValidarDadosFuncao(request)) return RetornaProcessamento();

        // 2. Verificar se o serviço existe
        var servico = await repository.ObterServicoPorCodigo(codServico);
        if (servico is null)
        {
            AddErroProcessamento($"Serviço com código {codServico} não encontrado.");
            return RetornaProcessamento();
        }

        // 3. Verificar se a função já existe
        var funcaoExistente = await repository.ObterFuncaoPorCodigo(request.CodFuncao);
        if (funcaoExistente is not null)
        {
            AddErroProcessamento($"Função com código {request.CodFuncao} já existe.");
            return RetornaProcessamento();
        }

        // 4. Verificar se a ordem já está em uso e encontrar a próxima disponível
        var funcoesDoServico = await repository.ListarFuncoesDoServico(codServico);
        var ordensEmUso = funcoesDoServico.Select(f => f.NumOrdem).OrderBy(o => o).ToList();
        
        int ordemFinal = request.NumOrdem;
        if (ordensEmUso.Contains(ordemFinal))
        {
            // Encontrar a próxima ordem disponível
            ordemFinal = 1;
            while (ordensEmUso.Contains(ordemFinal))
            {
                ordemFinal++;
            }
        }

        // 5. Criar a função
        var funcao = new Funcao(request.CodFuncao, codServico, request.Label);
        funcao.AlterarDescricao(request.Descricao);
        funcao.AlterarIcone(request.Icone);
        funcao.AlterarOrdem(ordemFinal);
        funcao.AlterarStatus(request.IndAtivo);

        repository.AdicionarFuncao(funcao);

        // 6. Salvar mudanças
        if (!await repository.UnitOfWork.Commit())
        {
            AddErroProcessamento("Erro interno ao criar função. Tente novamente.");
            return RetornaProcessamento();
        }

        // 7. Retornar dados da função criada
        var funcaoCriada = await repository.ObterFuncaoPorCodigo(request.CodFuncao);
        AdicionaRetorno(funcaoCriada);
        return RetornaProcessamento();
    }

    public async Task<RespostaProcessamento> AtualizarFuncaoAsync(string codServico, FuncaoRequest request)
    {
        // 1. Validar dados da função
        if (!ValidarDadosFuncao(request)) return RetornaProcessamento();

        // 2. Verificar se o serviço existe
        var servico = await repository.ObterServicoPorCodigo(codServico);
        if (servico is null)
        {
            AddErroProcessamento($"Serviço com código {codServico} não encontrado.");
            return RetornaProcessamento();
        }

        // 3. Buscar função existente
        var funcao = await repository.ObterFuncaoParaEdicao(request.CodFuncao);
        if (funcao is null)
        {
            AddErroProcessamento($"Função com código {request.CodFuncao} não encontrada.");
            return RetornaProcessamento();
        }

        // 4. Verificar se a função pertence ao serviço
        if (funcao.CodServico != codServico)
        {
            AddErroProcessamento($"Função {request.CodFuncao} não pertence ao serviço {codServico}.");
            return RetornaProcessamento();
        }

        // 5. Atualizar dados da função
        funcao.AlterarLabel(request.Label);
        funcao.AlterarDescricao(request.Descricao);
        funcao.AlterarIcone(request.Icone);
        funcao.AlterarOrdem(request.NumOrdem);
        funcao.AlterarStatus(request.IndAtivo);

        repository.AtualizarFuncao(funcao);

        // 6. Salvar mudanças
        if (!await repository.UnitOfWork.Commit())
        {
            AddErroProcessamento("Erro interno ao atualizar função. Tente novamente.");
            return RetornaProcessamento();
        }

        // 7. Retornar dados atualizados
        var funcaoAtualizada = await repository.ObterFuncaoPorCodigo(request.CodFuncao);
        AdicionaRetorno(funcaoAtualizada);
        return RetornaProcessamento();
    }

    public async Task<RespostaProcessamento> ListarFuncoesDoServicoAsync(string codServico)
    {
        // 1. Verificar se o serviço existe
        var servico = await repository.ObterServicoPorCodigo(codServico);
        if (servico is null)
        {
            AddErroProcessamento($"Serviço com código {codServico} não encontrado.");
            return RetornaProcessamento();
        }

        // 2. Buscar funções do serviço
        var funcoes = await repository.ListarFuncoesDoServico(codServico);
        AdicionaRetorno(funcoes);
        return RetornaProcessamento();
    }

    public async Task<RespostaProcessamento> ExcluirFuncaoAsync(string codServico, string codFuncao)
    {
        // 1. Verificar se o serviço existe
        var servico = await repository.ObterServicoPorCodigo(codServico);
        if (servico is null)
        {
            AddErroProcessamento($"Serviço com código {codServico} não encontrado.");
            return RetornaProcessamento();
        }

        // 2. Buscar função existente
        var funcao = await repository.ObterFuncaoParaEdicao(codFuncao);
        if (funcao is null)
        {
            AddErroProcessamento($"Função com código {codFuncao} não encontrada.");
            return RetornaProcessamento();
        }

        // 3. Verificar se a função pertence ao serviço
        if (funcao.CodServico != codServico)
        {
            AddErroProcessamento($"Função {codFuncao} não pertence ao serviço {codServico}.");
            return RetornaProcessamento();
        }

        // 4. Remover função
        repository.RemoverFuncao(funcao);

        // 5. Salvar mudanças
        if (!await repository.UnitOfWork.Commit())
        {
            AddErroProcessamento("Erro interno ao excluir função. Tente novamente.");
            return RetornaProcessamento();
        }

        AdicionaRetorno($"Função {codFuncao} excluída com sucesso!");
        return RetornaProcessamento();
    }

    public async Task<RespostaProcessamento> ReordenarFuncoesAsync(string codServico, FuncaoRequest[] funcoes)
    {
        // 1. Verificar se o serviço existe
        var servico = await repository.ObterServicoPorCodigo(codServico);
        if (servico is null)
        {
            AddErroProcessamento($"Serviço com código {codServico} não encontrado.");
            return RetornaProcessamento();
        }

        // 2. Validar se o array de funções não está vazio
        if (funcoes == null || funcoes.Length == 0)
        {
            AddErroProcessamento("Lista de funções não pode estar vazia.");
            return RetornaProcessamento();
        }

        // 3. Validar se todas as funções possuem código
        var funcoesInvalidas = funcoes.Where(f => string.IsNullOrWhiteSpace(f.CodFuncao)).ToList();
        if (funcoesInvalidas.Any())
        {
            AddErroProcessamento("Todas as funções devem ter um código válido.");
            return RetornaProcessamento();
        }

        // 4. Validar se todas as funções pertencem ao serviço
        var codigosFuncoes = funcoes.Select(f => f.CodFuncao).Where(cod => !string.IsNullOrWhiteSpace(cod)).ToList();
        var funcoesExistentes = await repository.ListarFuncoesDoServico(codServico);
        var codigosFuncoesExistentes = funcoesExistentes.Select(f => f.CodFuncao).ToHashSet();

        var funcoesNaoEncontradas = codigosFuncoes.Where(cod => !codigosFuncoesExistentes.Contains(cod)).ToList();
        if (funcoesNaoEncontradas.Count > 0)
        {
            AddErroProcessamento($"Funções não encontradas no serviço: {string.Join(", ", funcoesNaoEncontradas)}");
            return RetornaProcessamento();
        }

        // 5. Atualizar a ordem de cada função
        for (int i = 0; i < funcoes.Length; i++)
        {
            var funcaoRequest = funcoes[i];
            var funcao = await repository.ObterFuncaoParaEdicao(funcaoRequest.CodFuncao);
            
            if (funcao != null && funcao.CodServico == codServico)
            {
                funcao.AlterarOrdem(i + 1); // Ordem começa em 1
                repository.AtualizarFuncao(funcao);
            }
        }

        // 6. Salvar mudanças
        if (!await repository.UnitOfWork.Commit())
        {
            AddErroProcessamento("Erro interno ao reordenar funções. Tente novamente.");
            return RetornaProcessamento();
        }

        // 7. Retornar lista atualizada
        var funcoesAtualizadas = await repository.ListarFuncoesDoServico(codServico);
        AdicionaRetorno(funcoesAtualizadas);
        return RetornaProcessamento();
    }

    private bool ValidarDadosFuncao(FuncaoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CodFuncao))
        {
            AddErroProcessamento("Código da função é obrigatório.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.Label))
        {
            AddErroProcessamento("Label da função é obrigatório.");
            return false;
        }

        if (request.NumOrdem < 0)
        {
            AddErroProcessamento("Número de ordem não pode ser negativo.");
            return false;
        }

        return true;
    }

    #endregion
}
