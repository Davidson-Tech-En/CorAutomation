# Projeto de Automação QA: Correios

Este projeto implementa testes para validação da busca de CEP e rastreamento no site dos Correios.

# Tecnologias Utilizadas

* **Linguagem:** C# (.NET 7.0)
* **BDD/Test Framework:** SpecFlow (Gherkin) e NUnit
* **Automação UI:** Selenium WebDriver

# Estratégia

A automação direta do site dos Correios é inviabilizada pela presença de **CAPTCHA dinâmico** na busca. Utilizar OCR para CAPTCHA é uma **má prática** (instável e lenta).

**Solução Adotada (Mocking):**
Para garantir **estabilidade e velocidade**, o teste utiliza **injeção de código JavaScript** para simular os resultados de sucesso e falha (CEP e Rastreio) diretamente no DOM (Document Object Model).

* **Validação Visual (Rastreio):** A mensagem simulada de **Rastreio Incorreto** é injetada com **estilos CSS destacados (azul e amarelo)** e mantida visível por 2 segundos.

# Boas Práticas

Embora o Mocking garanta a robustez e entrega do teste, a melhor prática arquitetural para validar a lógica de CEP/Rastreio seria o **Teste de API/Serviço**. O teste de UI implementado foca na prova de conceito e na superação do desafio do CAPTCHA.
Em um cenário real de produção com acesso à API Rest, o código está pronto para ser **modularizado** e migrar a validação de dados para o Teste de Serviço.

# Cenário Implementado

O projeto cobre a avaliação completa em um único cenário BDD, atendendo a todos os requisitos técnicos (incluindo checks por ID, XPath e CSS implícito).

Adicione dentro do arquivo Hooks uma sessão para que você tenha como optar se deseja visualizar o navegador ou não. (true = sim / false = não).