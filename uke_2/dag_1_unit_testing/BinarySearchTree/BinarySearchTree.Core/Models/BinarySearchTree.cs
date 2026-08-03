namespace BinarySearchTree.Core.Models;


//Et binært søketre er en datastruktur som holder verdiene sine sortert *mens* de settes inn.
//Regelen (invarianten) er enkel, og gjelder for HVER eneste node i treet:
//      alt til venstre for meg er mindre enn meg
//      alt til høyre for meg er større enn meg
//
//Fordelen: når vi leter etter en verdi, kan vi kaste bort halve treet for hver sammenligning.
//I en List<T> må vi i verste fall se på alle 100 000 elementene -> O(n).
//I et balansert tre må vi i verste fall se på ~17 -> O(log n).

//Legg merke til "where T : IComparable<T>". Dette er en generisk begrensning (constraint).
//Uten den vet ikke kompilatoren om T i det hele tatt KAN sammenlignes, og vi får ikke lov
//å kalle value.CompareTo(...). Vi sier altså: "T kan være hva som helst, så lenge det er
//noe man kan si er større eller mindre enn noe annet av samme type."
public class BinarySearchTree<T> where T : IComparable<T>
{

    //Roten er inngangsdøren til treet, tenk stammen på treet. Er den null, er treet tomt.
    public BinaryTreeNode? Root = null;

    public int _hidden = 42;

    //Selve treet er egentlig bare noder som peker på hverandre.
    //En node kjenner ikke til hele treet, den kjenner bare sin egen verdi og sine to children.
    //Fordi Left og Right er av samme type som noden selv, blir strukturen rekursiv:
    //hvert children er i praksis roten i sitt eget lille undertre.
    //Enhver gren i et tre, er stammen til sine undergrener.
    public class BinaryTreeNode(T value)
    {
        public T? Value {get;set;} = value;

        //null betyr "ingen children her". En node uten children kalles et leafnode/leaf.
        public BinaryTreeNode? Left {get;set;} = null;
        public BinaryTreeNode? Right {get;set;} = null;
    }

    //Insert plasserer en ny verdi der den MÅ ligge for at regelen over skal holde.
    //Vi bestemmer aldri plasseringen selv, vi lar sammenligningene bestemme veien.
    public void Insert(T value)
    {
        //Spesialtilfellet: tomt tre. Da blir den første verdien roten.
        if (Root is null)
        {
            Root = new(value);
            return;
        }

        //"current" er noden vi står i akkurat nå. Vi starter på toppen og vandrer nedover.
        var current = Root;

        while (true)
        {
            //CompareTo gir oss tre svar:
            //  negativt tall -> value er MINDRE enn current.Value
            //  0             -> de er LIKE
            //  positivt tall -> value er STØRRE enn current.Value
            var comparison = value.CompareTo(current.Value);

            //Verdien finnes allerede. Dette treet tillater ikke duplikater,
            //så vi gjør ingenting og går ut.
            if (comparison == 0) return;

            if (comparison < 0)
            {
                //Verdien skal til venstre. Er det ledig plass, setter vi den inn og er ferdig.
                if (current.Left is null)
                {
                    current.Left = new(value);
                    return;
                }
                //Ellers er plassen opptatt, og vi flytter oss ett hakk ned til venstre
                //og stiller det samme spørsmålet på nytt.
                current = current.Left;
            }
            if (comparison > 0)
            {
                //Speilvendt av over: verdien skal til høyre.
                if (current.Right is null)
                {
                    current.Right = new(value);
                    return;
                }
                current = current.Right;
            }
        }
    }

    //Dette er "inngangen" til søket. 
    //Det er denne delen APIet (eller kontrakten til) treet vårt eksponerer.
    //Den som bruker treet skal slippe å vite at det er rekursivt, og slippe å sende inn
    //en node og en teller. De skal bare kunne spørre: "finnes denne verdien?"
    //Vi starter derfor på Root, med steg-teller 1 (vi teller selve rot-besøket som steg 1).
    public (bool Found, int Steps) BinaryTreeSearch(T value) => BinaryTreeSearchR(value, Root, 1);

    //Den rekursive hjelpemetoden. "R" står for rekursiv, og den er private
    //fordi den er en implementasjonsdetalj.
    //Returnerer en tuple: (ble den funnet?, hvor mange steg brukte vi nedover treet?)
    private (bool, int) BinaryTreeSearchR(T value, BinaryTreeNode? node, int count)
    {
        //Base case / stoppbetingelsen. Uten denne ville rekursjonen aldri tatt slutt.
        //Vi har gått forbi et løvnode og står i "ingenting" -> verdien finnes ikke i treet.
        if (node is null) return (false, count);

        //switch-uttrykk på resultatet av sammenligningen.
        //Merk at dette er nøyaktig samme logikk som i Insert: vi leter der verdien
        //MÅTTE ha blitt lagt inn, hvis den fantes. Derfor kan vi ignorere den andre halvdelen.
        return value.CompareTo(node.Value) switch
        {
            //Mindre enn noden vi står i -> finnes verdien, må den eksistere til venstre. 
            < 0 => BinaryTreeSearchR(value, node.Left, ++count),

            //Større enn noden vi står i -> let til høyre.
            > 0 => BinaryTreeSearchR(value, node.Right, ++count),

            //Treff. Vi returnerer true, og hvor dypt ned vi måtte gå for å finne den.
            0 => (true, count)
        };
    }
}
