namespace BinarySearchTree.Tests;
using BinarySearchTree.Core.Models;

//En unit-test er egentlig bare en helt vanlig metode som kjører litt av koden vår,
//og som sier ifra hvis resultatet ikke ble det vi forventet.
//Poenget er ikke å bevise at koden er riktig, men å oppdage at den *slutter* å være riktig.
//
//Alle testene under følger samme oppskrift, ofte kalt AAA:
//      Arrange -> sett opp det du trenger (lag treet, legg inn verdier)
//      Act     -> gjør DEN ene tingen du faktisk vil teste (kall Insert / BinaryTreeSearch)
//      Assert  -> sjekk at resultatet ble som forventet
//
//Legg merke til navngivningen: Klasse_Scenario_ForventetResultat.
//Grunnen er at det er navnet du får se i terminalen når noe feiler.
//"BinarySearchTree_InsertSmallerValue_LeftNodeIsMade" forteller deg hva som er galt
//uten at du trenger å åpne testen. "Test1" gjør ikke det.
//
//Merk også at hver test lager sitt EGET tre. Tester skal aldri dele tilstand,
//da blir de avhengige av hvilken rekkefølge de kjøres i, og det vil du ikke feilsøke.
public class BinarySearchTreeTest
{
    //[Fact] betyr "dette er en test uten input, den skal alltid gi samme svar".
    //Testrunneren finner den automatisk via attributtet, vi kaller den aldri selv.
    //Denne første testen tester ingenting av vår egen kode, den er bare en
    //røykprøve på at testoppsettet i det hele tatt kjører.
    [Fact]
    public void GenericTest()
    {
        var result = 2 + 2;
        //Assert.Equal(forventet, faktisk). Rekkefølgen av parameterene i Assert betyr noe for feilmeldingen du får.
        Assert.Equal(4, result);
    }

    //Denne feiler med vilje. Det er nyttig å se hvordan en rød test ser ut,
    //og hva xUnit skriver ut når Expected og Actual ikke stemmer overens.
    [Fact]
    public void GenericFailingTest()
    {
        var result = 2+3;
        Assert.Equal(4, result);
    }

    //Det aller enkleste vi kan kreve: et nytt tre skal eksistere, og det skal være TOMT.
    //Root == null er selve definisjonen på "tomt tre" i implementasjonen vår.
    //Vell og merke, i de fleste tilfeller, slik som dette, der vi har en
    //ganske standard Constructor, er ikke denne testen veldig nyttig. 
    [Fact]
    public void BinarySearchTree_TestConstruction_NotNull()
    {
        var binaryTree = new BinarySearchTree<int>();
        Assert.NotNull(binaryTree);
        Assert.Null(binaryTree.Root);
    }

    //Her lager vi tests for INSERT.
    //Vi tester alle grener INSERT kan ta (if)


    //Første Insert treffer spesialtilfellet i koden: treet er tomt, så verdien blir roten.
    //Vi sjekker både at roten finnes, og at den har RIKTIG verdi.
    //Assert.NotNull er også nyttig for Compileren: etter den linjen vet den at Root
    //ikke er null, så vi slipper advarsel når vi leser Root.Value.
    [Fact]
    public void BinarySearchTree_TestInsert_OnEmptyTree_RootIsNewNode()
    {
        var binaryTree = new BinarySearchTree<int>();
        var value = 4;
        binaryTree.Insert(value);
        Assert.NotNull(binaryTree.Root);
        Assert.Equal(value, binaryTree.Root.Value);
    }

    //Treet vårt tillater ikke duplikater (comparison == 0 -> return).
    //Her tester vi denne grenen.
    //Roten skal fortsatt være uten children. Hadde duplikatet blitt satt inn,
    //måtte det ha dukket opp som enten Left eller Right.
    [Fact]
    public void BinarySearchTree_InsertExistingValue_NoNewNodeMade()
    {
        var binaryTree = new BinarySearchTree<int>();
        binaryTree.Insert(4);
        binaryTree.Insert(4);
        Assert.NotNull(binaryTree.Root);
        Assert.Null(binaryTree.Root.Left);
        Assert.Null(binaryTree.Root.Right);

    }

    //Her tester vi insert av mindre verdi. mindre verdi skal havne til VENSTRE (Left).
    //Merk at vi også asserter Null på Right. Det er like viktig som resten,
    //vi vil vite at verdien havnet på riktig side, ikke bare at den havnet et sted.
    [Fact]
    public void BinarySearchTree_InsertSmallerValue_LeftNodeIsMade()
    {
        var tree = new BinarySearchTree<int>();
        tree.Insert(4);
        tree.Insert(2);
        Assert.NotNull(tree.Root);
        Assert.NotNull(tree.Root.Left);
        Assert.Null(tree.Root.Right);
        Assert.Equal(2, tree.Root.Left.Value);
    }

    //Speilvendt av testen over: større verdi skal havne til HØYRE (Right).
    //To nesten identiske tester er helt greit her, de dekker hver sin gren i Insert.
    [Fact]
    public void BinarySearchTree_InsertLargerValue_RightNodeIsMade()
    {
        var tree = new BinarySearchTree<int>();
        tree.Insert(4);
        tree.Insert(6);
        Assert.NotNull(tree.Root);
        Assert.NotNull(tree.Root.Right);
        Assert.Null(tree.Root.Left);
        Assert.Equal(6, tree.Root.Right.Value);
    }

    //[Theory] er en test som tar INPUT. Den kjøres én gang per datasett,
    //og hvert datasett blir sin egen linje i testresultatet - så du ser nøyaktig
    //hvilke tall som feilet, ikke bare at "testen feilet".
    //
    //[MemberData] peker på en static metode i klassen som leverer dataene.
    //nameof(...) brukes i stedet for strengen "GetTestData" slik at kompilatoren
    //fanger opp skrivefeil og omdøping.
    //Vi kan stable flere [MemberData] oppå hverandre, da slås datasettene sammen.
    //
    //Dette er en mer en Demo test: med tilfeldige tall kan vi ikke forutsi treets form,
    //så selve nyttigheten med denne testen kan diskuteres.
    [Theory]
    [MemberData(nameof(GetTestData))]
    [MemberData(nameof(GetRandomGeneratedStepArrays))]
    public void BinarySearchTree_InsertSeveralElements_CorrectNodesMade(params int[] ints)
    {
        var tree = new BinarySearchTree<int>();
        foreach (var num in ints) tree.Insert(num);
        foreach(var num in ints)
        {
            //Vi pakker ut tuppelen fra BinaryTreeSearch og kaster Steps med _,
            //fordi antall steg ikke er det denne testen bryr seg om.
            var (Found, _) = tree.BinaryTreeSearch(num);
            Assert.True(Found);
        }
    }

    //Her tester vi ikke bare OM verdien finnes, men hvor DYRT det var å finne den.
    //Hvert datasett er (verdiene vi setter inn, verdien vi leter etter, forventet antall steg).
    //
    //Eksempel: [1,2,3,4,5] settes inn i stigende rekkefølge. Da blir treet en skjev "lenke"
    //nedover til høyre - i praksis en liste. Å finne 3 koster da 3 steg (1 -> 2 -> 3).
    [Theory]
    [MemberData(nameof(GetStepsTestData))]
    public void BinarySearchTree_FindingInsertedValues_ProducesCorrectSteps((int[] data, int value, int predictedSteps) values)
    {
        var tree = new BinarySearchTree<int>();
        foreach(var num in values.data)
        {
            tree.Insert(num);
        }
        var (Found, Steps) = tree.BinaryTreeSearch(values.value);
        Assert.True(Found);
        Assert.Equal(values.predictedSteps, Steps);
    }

    //TheoryData<T> er xUnit sin typesikre måte å levere testdata på.
    //Fordelen framfor IEnumerable<object[]> er at Compileren sjekker at typene
    //stemmer med parameterne i testmetoden. Skriver du feil, får du en feil ved bygging
    //i stedet for en kryptisk feil ved kjøring.
    //Faste, håndplukkede datasett som disse er verdifulle nettopp fordi de er forutsigbare.
    //De kan ta en stund å skrive, så noen prøver å slippe unda med generatorer e.l.
    public static TheoryData<int[]> GetTestData() => [
        [1,2,3,4,5],
        [1234,532452,642346457],
        [5463234, 8456234, 421356],
        [34522346,998345, 42345]
    ];

    //Her lager vi datasettene til steg-testen.
    //Vi lager en Truple med tre verdier:
    //(Datasettet vi skal legge inn i treet, verdien vi prøver å finne, antall steg vi forventer å ta)
    //  ([1,2,3,4,5], 3, 3)     -> skjevt tre mot høyre, tallet 3 ligger på dybde 3
    //  ([32, 42, 21, 8], 8, 3) -> 32 blir rot, 21 til venstre for 32, 8 til venstre for 21
    public static TheoryData<(int[], int, int)> GetStepsTestData() => new() {
        ([1,2,3,4,5], 3, 3),
        ([32, 42, 21, 8], 8, 3)
    };

    //Genererer 10 tilfeldige datasett med 10 tall i hvert.
    //Fordi verdiene er tilfeldige, kan vi ikke si noe om treets form eller antall steg,
    //bare kreve egenskapen "det jeg la inn, finner jeg igjen".
    //
    //Baksiden ved tilfeldige data i tester: en feil kan dukke opp én kjøring og være borte
    //den neste. Skjer det, noter tallene som feilet og lag en fast [Fact] av dem.
    public static IEnumerable<object[]> GetRandomGeneratedStepArrays()
    {
        for (var i = 0; i < 10; i++)
        {
            //yield return leverer ett datasett om gangen. Se for deg yield som en "bookmark"
            //som lar oss returnere en verdi fra en loop, så fortsette loopen fra der den stoppet
            //neste gang denne metoden kalles.
            //Random.Shared.Next(0, 10000) kan gi duplikater, og det er helt greit:
            //treet skal jo håndtere at samme verdi settes inn to ganger.
            yield return [..Enumerable.Range(0, 10)
                                    .Select(num => Random.Shared.Next(0, 10000))
                        ];
        }
    }
}
