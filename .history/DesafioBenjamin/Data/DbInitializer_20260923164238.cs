using DesafioBenjamin.Models;
using Microsoft.EntityFrameworkCore;

namespace DesafioBenjamin.Data;

public static class DbInitializer
{
    public static async Task InicializarAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        // Evita cadastrar tudo novamente a cada execução.
        if (await context.Alunos.AnyAsync())
            return;

        var benjamin = new Aluno
        {
            Nome = "Benjamin",
            Ativo = true
        };

        var geografia = new Disciplina
        {
            Nome = "Geografia",
            Icone = "🌎",
            Ativa = true
        };

        var questionario = new Questionario
        {
            Titulo = "Relevo e Hidrografia",
            Descricao = "Revisão do Capítulo 9 — páginas 216 a 224",
            Capitulo = "Capítulo 9",
            Disciplina = geografia,
            Ativo = true
        };

        // =========================================================
        // FORMAS DE RELEVO
        // =========================================================

        AdicionarQuestao(
            questionario,
            1,
            "Qual forma de relevo apresenta altitude a partir do nível do mar e não ultrapassa 200 metros?",
            "Planície",
            "Montanha",
            "Planalto",
            "Depressão",
            "Segundo o material, a planície apresenta altitude a partir do nível do mar (0 metro) e não ultrapassa 200 metros."
        );

        AdicionarQuestao(
            questionario,
            2,
            "Qual forma de relevo possui elevações acentuadas nas encostas e picos pontiagudos?",
            "Montanhas",
            "Planícies",
            "Depressões",
            "Praias",
            "O livro apresenta as montanhas como elevações do relevo acentuadas nas encostas e com picos pontiagudos."
        );

        AdicionarQuestao(
            questionario,
            3,
            "Uma área mais baixa, localizada entre elevações de morros, corresponde a uma:",
            "Depressão",
            "Planície",
            "Montanha",
            "Duna",
            "A depressão é apresentada como uma área mais baixa entre elevações."
        );

        AdicionarQuestao(
            questionario,
            4,
            "Segundo o livro, qual forma de relevo apresenta altitudes elevadas, acima de 300 metros?",
            "Planalto",
            "Planície",
            "Praia",
            "Depressão",
            "O planalto apresenta altitudes elevadas, acima de 300 metros, podendo ser relativamente plano ou possuir elevações."
        );

        AdicionarQuestao(
            questionario,
            5,
            "Qual destas NÃO aparece entre as quatro principais formas de relevo apresentadas na página 217?",
            "Vale",
            "Planície",
            "Montanha",
            "Planalto",
            "As quatro formas apresentadas são planície, montanhas, depressão e planalto. O vale aparece posteriormente no diagrama."
        );

        // =========================================================
        // TERRAS EMERSAS E SUBMERSAS
        // =========================================================

        AdicionarQuestao(
            questionario,
            6,
            "Como o livro chama as partes do relevo que vemos acima do nível da água do mar?",
            "Terras emersas",
            "Terras submersas",
            "Bacias oceânicas",
            "Plataformas continentais",
            "Morros, pedras, ilhas e falésias acima do nível da água do mar são apresentados como terras emersas."
        );

        AdicionarQuestao(
            questionario,
            7,
            "Como são chamadas as partes do relevo que ficam abaixo da água?",
            "Terras submersas",
            "Terras emersas",
            "Planícies",
            "Encostas",
            "O material denomina terras submersas aquelas que estão abaixo da água."
        );

        AdicionarQuestao(
            questionario,
            8,
            "Qual ciência mede a profundidade dos oceanos?",
            "Batimetria",
            "Geografia",
            "Altimetria",
            "Hidrografia",
            "O quadro 'Você sabia?' explica que a batimetria é a ciência que mede a profundidade dos oceanos."
        );

        AdicionarQuestao(
            questionario,
            9,
            "Segundo o material, a profundidade dos oceanos pode ser medida por meio de sondas em:",
            "Navios, satélites e submarinos",
            "Apenas navios",
            "Apenas aviões",
            "Carros, bicicletas e navios",
            "O livro cita sondas em navios, satélites e submarinos."
        );

        // =========================================================
        // RELEVO COSTEIRO
        // =========================================================

        AdicionarQuestao(
            questionario,
            10,
            "Qual é a área junto à água com areia acumulada?",
            "Praia",
            "Falésia",
            "Baía",
            "Talude",
            "No exercício do relevo costeiro, essa definição corresponde à praia."
        );

        AdicionarQuestao(
            questionario,
            11,
            "Qual é a parte elevada junto à água?",
            "Falésia",
            "Praia",
            "Delta",
            "Duna",
            "O exercício identifica a falésia como a parte elevada junto à água."
        );

        AdicionarQuestao(
            questionario,
            12,
            "Qual formação é arredondada, com praia, junto ao mar?",
            "Baía",
            "Delta",
            "Duna",
            "Talude",
            "A formação arredondada, com praia e junto ao mar, é identificada como baía."
        );

        AdicionarQuestao(
            questionario,
            13,
            "Como é chamada a formação de ilhas de um rio que desemboca no mar?",
            "Delta",
            "Baía",
            "Falésia",
            "Planalto",
            "O exercício da página 220 relaciona essa formação ao delta."
        );

        AdicionarQuestao(
            questionario,
            14,
            "Como é chamado o acúmulo de areia mais afastado da água?",
            "Dunas",
            "Praia",
            "Falésia",
            "Bacia oceânica",
            "No relevo costeiro, o acúmulo de areia mais afastado da água corresponde às dunas."
        );

        // =========================================================
        // RELEVO OCEÂNICO
        // =========================================================

        AdicionarQuestao(
            questionario,
            15,
            "Qual é a parte menos profunda do relevo oceânico, próxima à costa?",
            "Plataforma continental",
            "Talude",
            "Bacia oceânica",
            "Dorsal meso-oceânica",
            "A plataforma continental é apresentada como a parte menos profunda próxima à costa."
        );

        AdicionarQuestao(
            questionario,
            16,
            "Qual é a área extensa e mais profunda do oceano?",
            "Bacia oceânica",
            "Plataforma continental",
            "Praia",
            "Baía",
            "O exercício identifica a área extensa e mais profunda como bacia oceânica."
        );

        AdicionarQuestao(
            questionario,
            17,
            "Como é chamado o conjunto de montanhas submarinas?",
            "Dorsal meso-oceânica",
            "Talude",
            "Plataforma continental",
            "Delta",
            "O conjunto de montanhas submarinas recebe o nome de dorsal meso-oceânica."
        );

        AdicionarQuestao(
            questionario,
            18,
            "Qual é a inclinação acentuada entre a plataforma continental e a bacia oceânica?",
            "Talude",
            "Duna",
            "Falésia",
            "Delta",
            "O talude corresponde à inclinação acentuada entre a plataforma continental e a bacia."
        );

        // =========================================================
        // SOCIEDADE E RELEVO
        // =========================================================

        AdicionarQuestao(
            questionario,
            19,
            "Qual construção humana é destacada pelo livro como de grande importância nos litorais?",
            "Porto",
            "Escola",
            "Hospital",
            "Estádio",
            "O material destaca os portos como construções humanas de grande importância nos litorais."
        );

        AdicionarQuestao(
            questionario,
            20,
            "Os portos oferecem estrutura principalmente para:",
            "Atracação de navios e trabalho de embarque e desembarque",
            "Plantação em montanhas",
            "Construção de morros",
            "Formação de rios",
            "Segundo o livro, os portos oferecem ampla estrutura para atracação dos navios e para embarque e desembarque."
        );

        // =========================================================
        // RIOS E BACIAS HIDROGRÁFICAS
        // =========================================================

        AdicionarQuestao(
            questionario,
            21,
            "Os rios geralmente se deslocam das áreas de:",
            "Maior altitude para as de menor altitude",
            "Menor altitude para as de maior altitude",
            "Mesma altitude o tempo inteiro",
            "Menor altitude para o topo das montanhas",
            "O material explica que os rios seguem das áreas de maior altitude para as de menor altitude."
        );

        AdicionarQuestao(
            questionario,
            22,
            "Segundo o livro, qual força faz o rio seguir seu curso das áreas mais altas para as mais baixas?",
            "Gravidade",
            "Vento",
            "Calor",
            "Luz",
            "O texto afirma que os rios seguem seu curso por força da gravidade."
        );

        AdicionarQuestao(
            questionario,
            23,
            "Os rios interligados e as regiões por onde correm, delimitadas pelos divisores de águas, formam uma:",
            "Bacia hidrográfica",
            "Praia",
            "Falésia",
            "Plataforma continental",
            "Essa é a definição de bacia hidrográfica apresentada na página 222."
        );

        AdicionarQuestao(
            questionario,
            24,
            "Os divisores de águas também são chamados no material de:",
            "Interflúvios",
            "Afluentes",
            "Estuários",
            "Deltas",
            "O texto utiliza a expressão 'divisores de águas, ou interflúvios'."
        );

        AdicionarQuestao(
            questionario,
            25,
            "Qual destes elementos aparece identificado no diagrama de uma bacia hidrográfica?",
            "Afluente",
            "Plataforma continental",
            "Falésia",
            "Duna",
            "O diagrama da página 222 identifica, entre outros elementos, nascente, afluente, subafluente, confluência, curso, leito, margens e foz."
        );

        context.Alunos.Add(benjamin);
        context.Questionarios.Add(questionario);

        await context.SaveChangesAsync();
    }

    private static void AdicionarQuestao(
        Questionario questionario,
        int ordem,
        string enunciado,
        string correta,
        string errada1,
        string errada2,
        string errada3,
        string explicacao)
    {
        var questao = new Questao
        {
            Ordem = ordem,
            Enunciado = enunciado,
            Explicacao = explicacao
        };

        questao.Alternativas.Add(new Alternativa
        {
            Texto = correta,
            Correta = true
        });

        questao.Alternativas.Add(new Alternativa
        {
            Texto = errada1,
            Correta = false
        });

        questao.Alternativas.Add(new Alternativa
        {
            Texto = errada2,
            Correta = false
        });

        questao.Alternativas.Add(new Alternativa
        {
            Texto = errada3,
            Correta = false
        });

        questionario.Questoes.Add(questao);
    }
}