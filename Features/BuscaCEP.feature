Feature: Busca e validação de CEP e rastreamento

  Scenario: Avaliação completa dos Correios
    Given que estou no site dos Correios
    When eu buscar pelo CEP "80700000"
    Then o sistema deve informar que o CEP não existe
    When eu voltar para a tela inicial
    And eu buscar pelo CEP "01013-001"
    Then o endereco retornado deve ser "Rua Quinze de Novembro, São Paulo/SP"
    When eu voltar para a tela inicial
    And eu buscar o codigo de rastreio "SS987654321BR"
    Then o sistema deve informar que o codigo esta incorreto
    And eu fecho o navegador
