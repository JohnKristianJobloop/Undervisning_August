//Denne using-en trengs fordi PostTodoDTO ligger i det globale namespacet
//mens TodoItem ligger i Core. Ryddigere: gi DTO-en samme namespace som resten.
using Core;

//Builderen har en jobb: oversette fra DTO (språket det ytre laget
//snakker) til domenemodell (språket det indre laget snakker).
//Det kan ofte være lurt å separere domenemodellene våre, fra de modellene vi eksponerer ut.
//
//
//Den gjør ikke så mye nå, men vi kunne lett utvidet dette med mer datavalidering.
//
//Dette er S-en i SOLID i praksis, Single Responsibility:
//controlleren ruter, builderen oversetter, repositoriet lagrer.
//Tre grunner til å endre koden, tre klasser.
public class TodoItemBuilder
{
    //Metoden er stateless, den bare leser dto og lager et nytt objekt.
    //Da kunne den like gjerne vært static. Grunnen til at den ikke er det, er
    //at vi vil injisere den via DI (se Program.cs): static klasser kan ikke
    //injiseres, og kan heller ikke byttes ut med noe annet i en test.
    public TodoItem FromDto(PostTodoDTO dto) => new TodoItem(dto.Title, dto.Body);
}
