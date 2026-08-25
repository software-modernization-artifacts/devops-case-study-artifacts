Evidências e Classificação DevOps

O arquivo evidence/devops_classification.xlsx contém o artefato utilizado para registrar e fundamentar a classificação dos elementos da taxonomia DevOps harmonizada proposta por Díaz et al.

A planilha está organizada em três perspectivas principais:

Team: aspectos relacionados às equipes, comunicação, colaboração e autonomia;
Management: aspectos gerenciais associados à gestão de produtos, projetos e responsabilidades;
Technology: aspectos tecnológicos relacionados à automação, infraestrutura, plataforma e ciclo de vida das aplicações.

Para cada elemento da taxonomia são registrados:

as classificações atribuídas aos cenários de legado e modernização;
os tipos previstos pela taxonomia;
as justificativas associadas às classificações;
as evidências utilizadas na análise.

As abas Motivo Legado e Motivo Modernização armazenam as justificativas detalhadas utilizadas para fundamentar cada classificação, permitindo rastrear a relação entre as evidências coletadas, as decisões de classificação e os elementos representados nos diagramas de instanciação.

O arquivo evidence/evidence_matrix.xlsx complementa essa rastreabilidade, relacionando as evidências técnicas coletadas aos elementos analisados e aos respectivos artefatos comprobatórios.

Instanciação da Taxonomia

A pasta taxonomy/ contém os diagramas correspondentes às instanciações da taxonomia nos dois cenários analisados.

Cenário Legado
taxonomy_instantiation_legacy.pdf
taxonomy_instantiation_legacy.png

Representa a instanciação da taxonomia correspondente ao cenário anterior ao processo de modernização.

Cenário de Modernização
taxonomy_instantiation_modernization.pdf
taxonomy_instantiation_modernization.png

Representa a instanciação da taxonomia correspondente ao cenário de modernização analisado no estudo.

Os diagramas foram construídos a partir das classificações, justificativas e evidências registradas durante a análise. Sua disponibilização em PDF e PNG permite a inspeção detalhada e a comparação entre os dois cenários.

Evidências de Pipeline e CI/CD

A pasta pipeline/ contém artefatos utilizados para verificar os mecanismos de automação do ciclo de desenvolvimento e entrega observados no cenário modernizado.

gitlab-ci.yml: configuração anonimizada do pipeline analisado; pipeline-execution-evidence.pdf: evidências anonimizadas de execuções do pipeline utilizadas durante a análise.

Esses artefatos permitem verificar as práticas de automação efetivamente observadas e apoiar a caracterização do estágio de adoção de DevOps.

Artefatos de Configuração

A pasta configuration/ contém configurações técnicas analisadas como evidências complementares das práticas adotadas no ambiente modernizado.

docker-compose.yml: configuração anonimizada utilizada na conteinerização e definição do ambiente;
renovate.json: configuração anonimizada relacionada à automação do gerenciamento de dependências.
Rastreabilidade das Evidências

Os artefatos disponibilizados foram organizados de modo a permitir a rastreabilidade entre as evidências técnicas, as decisões de classificação e as instanciações da taxonomia:

Artefatos técnicos
(pipeline/ e configuration/)
          ↓
Evidências e justificativas
(evidence/)
          ↓
Classificação dos elementos
          ↓
Instanciação da taxonomia
(taxonomy/)

Essa organização permite inspecionar as evidências utilizadas na análise e compreender como elas fundamentaram a comparação entre os cenários de legado e modernização.