# Pipeline Technical Analysis — V1–V4

Esta pasta reúne os artefatos técnicos utilizados na análise longitudinal das práticas de automação do pipeline das versões V1, V2, V3 e V4 do sistema estudado.

O objetivo é disponibilizar, de forma organizada e reprodutível, os arquivos técnicos utilizados como evidências, o notebook Google Colab responsável pela análise, o notebook utilizado para geração do relatório HTML e o relatório HTML resultante da análise.

## Estrutura da pasta

```text
pipeline/
├── README.md
├── V1/
│   ├── gitlab-ci.yml
│   ├── project.csproj
│   ├── Dockerfile
│   ├── docker-compose.yaml
│   ├── Program.cs
│   ├── appsettings.json
│   ├── .editorconfig
│   ├── .env.example
│   ├── init.sql
│   └── start-db.ps1
├── V2/
│   ├── gitlab-ci.yml
│   ├── project.csproj
│   ├── renovate.json
│   ├── RENOVATE_SETUP.md
│   ├── Dockerfile
│   ├── docker-compose.yaml
│   ├── Program.cs
│   ├── appsettings.json
│   ├── .editorconfig
│   ├── .env.example
│   ├── init.sql
│   ├── seed.sql
│   └── start-db.ps1
├── V3/
│   └── [artefatos correspondentes à V3]
├── V4/
│   └── [artefatos correspondentes à V4]
├── notebooks/
│   ├── pipeline-technical-analysis.ipynb
│   └── generate-analysis-html.ipynb
└── reports/
    └── pipeline-technical-analysis.html
```

A presença dos arquivos pode variar entre as versões. Um artefato ausente em determinada versão não deve ser criado artificialmente apenas para uniformizar a estrutura.

## Versões analisadas

A análise considera quatro entregas sucessivas: **V1**, correspondente ao MVP, seguida por **V2**, **V3** e **V4**, esta última representando o recorte temporal mais recente da análise.

Cada diretório contém os artefatos correspondentes à respectiva versão, preservando a rastreabilidade temporal e permitindo distinguir práticas presentes desde a primeira entrega daquelas introduzidas ou modificadas posteriormente.

## Artefatos técnicos

Os diretórios `V1`, `V2`, `V3` e `V4` contêm as fontes primárias da análise. Entre os principais artefatos estão:

- `gitlab-ci.yml` — definição versionada do pipeline GitLab CI;
- `project.csproj` — configuração do projeto .NET;
- `Dockerfile` — processo de construção da imagem;
- `docker-compose.yaml` — configuração declarativa dos serviços;
- `renovate.json` — gerenciamento automatizado de dependências, quando presente;
- `RENOVATE_SETUP.md` — documentação relacionada ao Renovate, quando presente;
- `Program.cs` — configuração da aplicação e integrações em runtime;
- `appsettings.json` — parâmetros de configuração;
- `.editorconfig` — regras de padronização e análise estática;
- `.env.example` e equivalentes — parametrização de ambiente;
- `init.sql`, `seed.sql` e `start-db.ps1` — scripts de preparação do ambiente e banco.

Os scripts são mantidos diretamente no diretório da respectiva versão, junto aos demais artefatos, para simplificar sua seleção e upload no Google Colab. Esses scripts não constituem, isoladamente, evidência de deploy automatizado.

## Sanitização dos artefatos

Antes da publicação, os artefatos devem ser sanitizados para remover ou substituir credenciais, tokens, senhas, dados pessoais, nomes de usuários, endereços internos, URLs institucionais não públicas e outras informações sensíveis.

A sanitização deve preservar as estruturas necessárias para que as evidências técnicas permaneçam verificáveis.

## Notebook de análise

O arquivo `notebooks/pipeline-technical-analysis.ipynb` contém o processo reproduzível de análise:

```text
artefatos técnicos
        ↓
upload dos artefatos
        ↓
identificação e validação
        ↓
extração objetiva de evidências
        ↓
classificação determinística por critérios
        ↓
estruturação das evidências
        ↓
interpretação assistida por IA
        ↓
armazenamento dos resultados da versão
        ↓
análise longitudinal V1–V4
```

Cada versão é analisada individualmente. Seus resultados são armazenados e posteriormente utilizados como entrada da comparação longitudinal.

## Classificação das evidências

A análise utiliza três estados:

| Símbolo | Significado |
|---|---|
| `✓` | Evidência identificada no artefato analisado |
| `—` | Capacidade não identificada no artefato analisado |
| `?` | Evidência indeterminada ou dependente de informação adicional |

A ausência de evidência nos artefatos analisados não deve ser automaticamente interpretada como inexistência da prática fora do escopo da análise.

## Configuração, orquestração e execução

Quando necessário, são distinguidos três níveis de evidência:

1. **Configuração** — capacidade declarada em artefato versionado.
2. **Orquestração** — capacidade explicitamente incorporada ou invocada pelo pipeline.
3. **Execução efetiva** — existência de evidências de runtime demonstrando a execução.

Por exemplo, `renovate.json` demonstra configuração do Renovate; um job explícito demonstra orquestração; a execução efetiva requer evidências adicionais, como histórico de jobs, logs, branches ou Merge Requests.

## Classificação da maturidade do pipeline

Após a extração, critérios determinísticos classificam as práticas de automação do pipeline em quatro níveis:

1. **Ausente/Tradicional**;
2. **Inicial/Ad hoc**;
3. **Parcial/Emergente**;
4. **Avançado/Consolidado**.

A escala se refere especificamente às práticas de automação do pipeline no recorte analisado e não representa, isoladamente, a maturidade DevOps global do projeto ou da organização.

## Interpretação assistida por IA

A Inteligência Artificial apoia a interpretação das evidências previamente extraídas e classificadas. Ela não realiza a identificação primária das práticas nem substitui a classificação determinística.

A IA é utilizada para interpretar evidências, avaliar se sustentam a classificação, elaborar justificativas técnicas, identificar lacunas e limitações e apoiar a síntese acadêmica.

## Análise longitudinal V1–V4

Após as análises individuais, os resultados são consolidados:

```text
V1 → análise → armazenamento
V2 → análise → armazenamento
V3 → análise → armazenamento
V4 → análise → armazenamento
                 ↓
       resultados consolidados
                 ↓
       matriz longitudinal
                 ↓
       comparação V1–V4
                 ↓
 interpretação longitudinal
```

A análise longitudinal permite identificar práticas persistentes, capacidades introduzidas posteriormente, mudanças de configuração ou orquestração, lacunas persistentes e evolução da automação do pipeline.

## Notebook de geração do HTML

O arquivo `notebooks/generate-analysis-html.ipynb` é utilizado para gerar a representação HTML da análise.

Assim, são separados:

- o **notebook de análise**, destinado à execução e reprodução;
- o **notebook de geração**, destinado à produção do HTML;
- o **HTML**, destinado à leitura dos resultados.

## Relatório HTML

O arquivo `reports/pipeline-technical-analysis.html` é a representação estática da análise e pode ser aberto diretamente em um navegador.

Ele permite consultar evidências, classificações, interpretações técnicas, matriz longitudinal, comparação V1–V4, limitações e síntese dos resultados.

Para reproduzir a análise, deve-se utilizar `pipeline-technical-analysis.ipynb`.

## Reprodutibilidade

Para reproduzir o procedimento:

1. abra `pipeline-technical-analysis.ipynb` no Google Colab;
2. execute a configuração do ambiente;
3. faça upload dos artefatos da versão;
4. execute a identificação e validação;
5. execute a extração automática das evidências;
6. execute a classificação determinística;
7. prepare as evidências para interpretação;
8. execute, quando aplicável, a interpretação assistida por IA;
9. armazene os resultados da versão;
10. repita o procedimento para V1, V2, V3 e V4;
11. execute a análise longitudinal;
12. utilize o notebook de geração para produzir o HTML atualizado.

## Limitações

A análise está limitada aos artefatos disponibilizados. Arquivos de configuração demonstram capacidades declaradas, mas não necessariamente execução efetiva.

Dockerfile e Docker Compose não comprovam, isoladamente, deploy automatizado; scripts de preparação não comprovam deploy; observabilidade não representa automação de entrega; configuração do Renovate não comprova execução efetiva; e a ausência de deploy, promoção ou rollback nos artefatos não prova que essas atividades não ocorram por mecanismos externos ao escopo analisado.

Quando necessária, a comprovação da execução depende de evidências adicionais, como logs, histórico de pipelines, branches, Merge Requests ou registros de implantação.

## Finalidade

Os artefatos são disponibilizados como material complementar ao estudo para favorecer:

- **transparência**;
- **rastreabilidade**;
- **auditabilidade**;
- **reprodutibilidade**;
- **verificabilidade**.

Os resultados devem ser interpretados dentro do recorte temporal e técnico definido no estudo.
