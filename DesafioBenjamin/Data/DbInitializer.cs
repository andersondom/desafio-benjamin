using DesafioBenjamin.Models;
using Microsoft.EntityFrameworkCore;

namespace DesafioBenjamin.Data;

public static class DbInitializer
{
    public static async Task InicializarAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        var benjamin = await context.Alunos.FirstOrDefaultAsync(a => a.Nome == "Benjamin");
        if (benjamin == null)
        {
            benjamin = new Aluno { Nome = "Benjamin", Ativo = true };
            context.Alunos.Add(benjamin);
            await context.SaveChangesAsync();
        }

        // Cada conteúdo é incluído apenas se ainda não existir.
        var geografiaExiste = await context.Questionarios.AnyAsync(q =>
            q.Titulo == "Relevo e Hidrografia" && q.Capitulo == "Capítulo 9");

        if (!geografiaExiste)
        {

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

        context.Questionarios.Add(questionario);
        await context.SaveChangesAsync();
        }

        var inglesExiste = await context.Questionarios.AnyAsync(q =>
            q.Titulo == "A Song in My Heart" && q.Capitulo == "Unit 3 - Chapter 6");

        if (!inglesExiste)
        {
            var ingles = new Disciplina
            {
                Nome = "Inglês",
                Icone = "🇬🇧",
                Ativa = true
            };

            var inglesQuiz = new Questionario
            {
                Titulo = "A Song in My Heart",
                Descricao = "Revisão da Unit 3 - Chapter 6",
                Capitulo = "Unit 3 - Chapter 6",
                Disciplina = ingles,
                Ativo = true
            };

            AdicionarQuestao(inglesQuiz, 1, "David was the _____ son of Jesse.", "youngest", "oldest", "only", "first", "The story says: David was the youngest son of Jesse.");
            AdicionarQuestao(inglesQuiz, 2, "How many brothers did David have?", "Seven", "Three", "Five", "Ten", "The story says that David had seven brothers.");
            AdicionarQuestao(inglesQuiz, 3, "What did David take care of?", "His father's sheep", "King Saul's horses", "His brothers' house", "A garden", "David took care of his father's sheep.");
            AdicionarQuestao(inglesQuiz, 4, "David loved to play his _____ and sing to the Lord.", "harp", "piano", "guitar", "drums", "The story says David loved to play his harp and sing to the Lord.");
            AdicionarQuestao(inglesQuiz, 5, "King Saul had _____ problems.", "sleeping", "walking", "reading", "traveling", "The story says King Saul had sleeping problems.");
            AdicionarQuestao(inglesQuiz, 6, "Where was Jesse from?", "Bethlehem", "Jerusalem", "Egypt", "Rome", "The king's servants said Jesse was from the town of Bethlehem.");
            AdicionarQuestao(inglesQuiz, 7, "Besides being a good musician, David was also a good _____.", "soldier", "teacher", "farmer", "doctor", "The servants described David as a good musician and a good soldier.");
            AdicionarQuestao(inglesQuiz, 8, "What did David's music help King Saul do?", "Calm down and sleep", "Run and jump", "Work and travel", "Read and write", "David's music helped King Saul calm down and sleep.");
            AdicionarQuestao(inglesQuiz, 9, "A composer is...", "someone who makes or writes songs", "someone who imitates the sound of birds", "someone who plays the piano well", "someone who only listens to music", "The activity defines a composer as someone who makes or writes songs.");
            AdicionarQuestao(inglesQuiz, 10, "Complete the Bible gem: “Sing to the Lord _____.”", "a new song!", "every morning!", "with a harp!", "with your friends!", "The Bible gem shown in the material is: “Sing to the Lord a new song!” — Psalm 96:1.");
            AdicionarQuestao(inglesQuiz, 11, "Which is the past form of COMPOSE?", "composed", "composeed", "compossed", "compose", "The grammar box gives the example compose → composed.");
            AdicionarQuestao(inglesQuiz, 12, "Which is the past form of WALK?", "walked", "walkd", "walking", "walk", "The matching activity pairs WALK with WALKED.");
            AdicionarQuestao(inglesQuiz, 13, "Which is the past form of LOOK?", "looked", "lookd", "looking", "look", "The matching activity pairs LOOK with LOOKED.");
            AdicionarQuestao(inglesQuiz, 14, "Which is the past form of HELP?", "helped", "helpd", "helping", "help", "The matching activity pairs HELP with HELPED.");
            AdicionarQuestao(inglesQuiz, 15, "Which is the past form of ASK?", "asked", "askd", "asking", "ask", "The matching activity pairs ASK with ASKED.");
            AdicionarQuestao(inglesQuiz, 16, "Which is the past form of OPEN?", "opened", "opend", "opening", "open", "The matching activity pairs OPEN with OPENED.");
            AdicionarQuestao(inglesQuiz, 17, "Which is the past form of WORK?", "worked", "workd", "working", "work", "The matching activity pairs WORK with WORKED.");
            AdicionarQuestao(inglesQuiz, 18, "Which is the past form of JUMP?", "jumped", "jumpd", "jumping", "jump", "The matching activity pairs JUMP with JUMPED.");
            AdicionarQuestao(inglesQuiz, 19, "Which is the past form of ANSWER?", "answered", "answerd", "answering", "answer", "The matching activity pairs ANSWER with ANSWERED.");
            AdicionarQuestao(inglesQuiz, 20, "Which is the past form of WATCH?", "watched", "watchd", "watching", "watch", "The matching activity pairs WATCH with WATCHED.");
            AdicionarQuestao(inglesQuiz, 21, "Which is the past form of TALK?", "talked", "talkd", "talking", "talk", "The matching activity pairs TALK with TALKED.");
            AdicionarQuestao(inglesQuiz, 22, "The grammar box says action words are called _____.", "verbs", "adjectives", "nouns", "songs", "The material says: Action words are called verbs.");
            AdicionarQuestao(inglesQuiz, 23, "What does “past” indicate in the grammar lesson?", "The action happened before", "The action is happening now", "The action will happen tomorrow", "The word is a feeling", "The material explains that -ed can show that an action happened in the past.");
            AdicionarQuestao(inglesQuiz, 24, "If a verb ends with “e”, what does the hint tell you to add?", "d", "ed", "ing", "s", "The hint says that if the word ends with an “e”, you just add a “d”.");
            AdicionarQuestao(inglesQuiz, 25, "Which sentence uses the past form shown in the lesson?", "King David played the harp.", "King David play the harp.", "King David playing the harp.", "King David plays the harp tomorrow.", "The grammar example in the material is: “A long time ago, King David played the harp.”");

            context.Questionarios.Add(inglesQuiz);
            await context.SaveChangesAsync();
        }
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