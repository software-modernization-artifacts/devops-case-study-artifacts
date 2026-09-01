# Renovate - Atualização Automática de Dependências

## 📋 Visão Geral

Este projeto utiliza o **Renovate** para automatizar as atualizações de dependências **NuGet e Docker**. O Renovate é uma ferramenta madura, amplamente utilizada e com excelente suporte ao GitLab.

## ⚙️ Como Funciona

1. **Detecção Automática**: O Renovate verifica por atualizações (agendamento configurável)
2. **Criação de MRs**: Cria automaticamente Merge Requests **individuais** para cada dependência
3. **Configuração Simplificada**: Configuração mínima e confiável

## 🚀 Configuração Atual

### Pipeline GitLab CI

#### Job Renovate (Detecção e Criação de MRs)
```yaml
renovate:
  stage: maintenance
  image: renovate/renovate:latest
  variables:
    RENOVATE_PLATFORM: "gitlab"
    RENOVATE_ENDPOINT: "https://git.embrapa.io/api/v4/"
    RENOVATE_TOKEN: "$RENOVATE_GITLAB_ACCESS_TOKEN"
    RENOVATE_AUTODISCOVER: "false"
    RENOVATE_DRY_RUN: "false"
    LOG_LEVEL: "info"
    RENOVATE_BRANCH_PREFIX: "renovate/"
    RENOVATE_PR_CONCURRENT_LIMIT: "10"
    RENOVATE_LABELS: "dependencies,renovate,automated"
  script:
    - renovate $CI_PROJECT_PATH
  rules:
    - if: $CI_PIPELINE_SOURCE == "schedule"
    - if: $CI_PIPELINE_SOURCE == "web"
      when: manual
```

#### Job Sonar (Executa Apenas na Main)
```yaml
sonar-analysis:
  rules:
    - if: $CI_COMMIT_BRANCH == 'main' && $CI_PIPELINE_SOURCE == 'push'
```

### Execução
- **Agendamento**: Configurável via GitLab CI/CD Schedules
- **Manual**: Execução manual via pipeline

## 🔧 Setup Necessário

### 1. Variável de Ambiente
Configure no GitLab em **Settings > CI/CD > Variables**:

- **Key**: `RENOVATE_GITLAB_ACCESS_TOKEN`
- **Value**: Personal Access Token com escopo `api`
- **Flags**: ☑️ Protected, ☑️ Masked

### 2. Personal Access Token
1. Acesse: `https://git.embrapa.io/-/profile/personal_access_tokens`
2. Crie token com:
   - **Nome**: `renovate-automation`
   - **Escopo**: ☑️ `api`
   - **Validade**: 365 dias (ou conforme política)

### 3. Schedule (Opcional)
Para execução automática, configure em **CI/CD > Schedules**:
- **Description**: "Renovate Weekly Update"
- **Interval Pattern**: `0 15 * * 4` (quinta, 15:00)
- **Target Branch**: `main`

## 📊 Funcionalidades

### ✅ Managers Habilitados
- **NuGet**: Arquivos `.csproj`, `.sln`, `packages.config`
- **Dockerfile**: `Dockerfile`, `docker-compose.yml`, imagens Docker
- **Suporte a múltiplos projetos** na mesma solução

### ✅ Merge Requests Individuais
- **Um MR por dependência** (sem agrupamentos)
- Títulos descritivos: `renovate(deps): update BootstrapBlazor to v9.10.2`
- Changelog automático com links para releases
- Labels automáticos: `dependencies`, `renovate`, `automated`



### ✅ Configuração Simplificada
- Configuração mínima e confiável
- Sem agrupamentos complexos
- Foco na estabilidade

## 🔄 Workflow Típico

### Fluxo Semanal
1. **Quinta, 15:00**: Renovate verifica atualizações
2. **Análise**: Detecta pacotes desatualizados
3. **MRs**: Cria branches `renovate/nuget-Microsoft.AspNetCore.App-9.x`
4. **Review**: Aguarda aprovação da equipe
5. **Merge**: Após aprovação, integração na branch principal

### Exemplos de MRs Criados
```
renovate(deps): update BootstrapBlazor to v9.10.2
renovate(deps): update Sentry.AspNetCore to v5.15.1
renovate(deps): update mcr.microsoft.com/dotnet/sdk Docker tag to v9.1
renovate(deps): update postgres Docker tag to v17
```

### Tipos de Atualizações
- ✅ **Minor/Patch**: Atualizações compatíveis (9.10.1 → 9.10.2)
- ❌ **Major**: Bloqueadas para NuGet (estabilidade)
- ✅ **Major Docker**: Permitidas para imagens Docker
- 🚨 **Security**: Priorizadas independente da versão

## 🛠️ Configuração Atual

### Arquivo renovate.json
```json
{
  "$schema": "https://docs.renovatebot.com/renovate-schema.json",
  "extends": ["config:base"],
  "platform": "gitlab",
  "gitlabci": {"enabled": true},
  "labels": ["dependencies", "renovate", "automated"],
  "prConcurrentLimit": 10,
  "branchPrefix": "renovate/",
  "commitMessagePrefix": "renovate(deps): ",
  "semanticCommits": "enabled",
  "enabledManagers": ["nuget", "dockerfile"],
  "nuget": {"enabled": true},
  "dockerfile": {"enabled": true},
  "packageRules": [
    {
      "description": "Ignorar major versions para estabilidade",
      "matchUpdateTypes": ["major"],
      "enabled": false
    },
    {
      "description": "Configuração específica para imagens Docker",
      "matchManagers": ["dockerfile"],
      "matchUpdateTypes": ["major"],
      "enabled": true
    }
  ],
  "vulnerabilityAlerts": {"enabled": true},
  "ignoreDeps": ["Microsoft.NETCore.App"]
}
```

### Principais Configurações

| Configuração | Valor | Descrição |
|--------------|-------|-----------|
| **Managers** | `nuget`, `dockerfile` | Gerenciadores habilitados |
| **Concurrent Limit** | `10` | Máximo 10 MRs simultâneos |
| **Branch Prefix** | `renovate/` | Prefixo das branches |
| **Major Updates** | `false` (NuGet), `true` (Docker) | Controle de breaking changes |
| **Security Alerts** | `true` | Alertas de vulnerabilidade |
| **Labels** | `dependencies`, `renovate`, `automated` | Labels automáticos |

### Arquivos Monitorados

#### NuGet (.NET)
```bash
*.csproj       # Projetos C#
*.fsproj       # Projetos F#
*.vbproj       # Projetos VB.NET
packages.config # Packages legados
Directory.Packages.props # Central package management
```

#### Docker (Imagens)
```bash
Dockerfile         # Dockerfile principal
Dockerfile.*       # Dockerfiles nomeados
docker/Dockerfile  # Dockerfiles em subpastas  
*.dockerfile       # Arquivos .dockerfile
docker-compose.yml # Docker Compose (se habilitado)
```

## 🔍 Monitoramento

### Verificar Execuções
1. **Pipeline History**: `CI/CD > Pipelines` - Ver execuções do job `renovate`
2. **MR List**: Filtrar por label `renovate` - Ver MRs criados
3. **Debug Pipeline**: Job `debug-pipeline` mostra variáveis da execução
4. **Logs Detalhados**: `LOG_LEVEL: "info"` nas execuções

### Comandos Úteis
```bash
# Verificar pacotes desatualizados localmente
dotnet list package --outdated

# Ver dependências atuais
dotnet list package

# Restaurar dependências
dotnet restore
```

## 🚨 Troubleshooting

### Renovate Não Executa
1. **Token**: Verificar se `RENOVATE_GITLAB_ACCESS_TOKEN` está configurado
2. **Permissões**: Confirmar token tem escopo `api`
3. **Schedule**: Verificar se schedule está ativo

### MRs Não São Criados
1. **Dependências**: Verificar se há pacotes desatualizados
2. **Limite**: Verificar `prConcurrentLimit` (padrão: 10)
3. **Branches**: Verificar se branches antigas foram limpas

### Pipeline de Validação Falha
1. **Build**: Verificar se projeto compila localmente
2. **Dependencies**: Confirmar compatibilidade de versões
3. **Restore**: Verificar se `dotnet restore` funciona

## 📈 Benefícios vs Dependabot

| Aspecto | Dependabot | **Renovate** |
|---------|------------|--------------|
| **Maturidade** | ❌ Implementação GitLab experimental | ✅ Solução madura e estável |
| **Configuração** | ❌ Complexa e propensa a erros | ✅ Simples e confiável |
| **Suporte GitLab** | ❌ Limitado | ✅ Nativo e completo |
| **Documentação** | ❌ Fragmentada | ✅ Extensiva e clara |
| **Comunidade** | ❌ Pequena | ✅ Grande e ativa |
| **Funcionalidades** | ❌ Básicas | ✅ Avançadas |

## ✅ Status Atual

1. **✅ Configuração Simplificada**: Renovate configurado com dependências individuais
2. **✅ Managers Habilitados**: NuGet + Dockerfile 
3. **✅ Pipeline Otimizada**: Jobs executam apenas quando necessário
4. **✅ Sonar Integrado**: Análise apenas após merge na main

## 🎯 Próximas Ações

1. **Configure Token**: `RENOVATE_GITLAB_ACCESS_TOKEN` nas variáveis CI/CD
2. **Configure Schedule**: Agendamento semanal via GitLab Schedules  
3. **Teste Execução**: Execute pipeline manualmente para validar
4. **Monitore MRs**: Acompanhe MRs individuais criados
5. **Refine Configuração**: Ajuste conforme necessidades específicas

## 📚 Recursos Adicionais

- [Renovate Documentation](https://docs.renovatebot.com/)
- [GitLab Integration](https://docs.renovatebot.com/gitlab/)
- [NuGet Manager](https://docs.renovatebot.com/modules/manager/nuget/)
- [Configuration Options](https://docs.renovatebot.com/configuration-options/)

---

**Renovate é a escolha certa**: Solução madura, confiável e amplamente utilizada pela comunidade .NET e GitLab. 🚀