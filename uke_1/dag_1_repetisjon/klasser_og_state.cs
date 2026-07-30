
var person = new Person
{
    Name = "John",
    Age = 34
};





//I Objektorientert  programmering, handler det om å oppdatere tilstanden til objektet
//slik at det representerer virkeligheten slik vi ønsker å representere den

//La oss si personen over har bursdag, i objektorientert programmering matcher vi dette
//Ved å endre tilstanden til objektet, for å reflektere den nye virkeligheten

person.Age += 1;
Console.WriteLine(person.ToString());


var newAge = PersonService.IncrementAge(person);
Console.WriteLine(newAge.ToString());

//I funksjonell programmering, har vi aldri lov å endre tilstand til et objekt.
//Vi har kun lov å erstatte det, med et nytt objekt, som reflekterer den nye tilstanden.

var person2 = new ImmutablePerson("Terje", 44);
//I funksjonell programmering, kan du kun lage et nytt objekt, når du skal endre tilstand. 
person2 = person2 with {Age = person2.Age + 1};
Console.WriteLine(person2.ToString());

person3 = new ImmutablePerson(person2);


//En klasse representerer egentlig bare en blueprint, eller en oppskrift over, hvordan du vil en datatype skal se ut. 
//That's it. 

//Du kan lage noen begrensinger på egenhånd, for å gi klassene dine mening. 

//Her representerer eg en person i programmet mitt, som bare inneholder det jeg trenger for å 
//representere en vilkårlig person. 
public class Person
{
    public string Name;
    public int Age;

    public override string ToString()
    {
        return $"{Name} is {Age} years old";
    }
}


//Her lager jeg en samling av metoder, ofte kalt en tjeneste, for å behandle disse person objektene mine. 
public static class PersonService
{
    public static Person IncrementAge(Person person)
    {
        person.Age += 1;
        return person;
    }
}


//En record er bare en fancy klasse, der tilstanden ikke har lov å endre seg, 
//Man må lage et nytt objekt, hver gang man vil oppdatere tilstanden til et felt. 
public record ImmutablePerson(string Name, int Age)
{
    public override string ToString()
    {
        return $"{Name} is {Age} years old";
    }
};