namespace Core;

//DTO = Data Transfer Object. Dette er formen på dataen slik den kommer INN
//over HTTP, ikke slik vi lagrer den internt.
//
//Hvorfor ikke bare ta imot en TodoItem rett i controlleren?
//Fordi klienten da kunne sendt med sin egen Id, sin egen CreatedAt, og påstått
//at oppgaven allerede var fullført. En DTO gjør det fysisk umulig å sende inn
//felt som vår applikasjon selv skal bestemme.
//I tillegg blir kontrakten mot omverdenen frikoblet fra modellen: vi kan endre
//TodoItem uten å rote for klientene, og motsatt.
//
//record gir oss to ting gratis:
//      - verdilikhet, to records med samme innhold er == like
//      - init-only properties, altså immutabilitet
//Denne kortformen (positional record) genererer konstruktør, properties,
//Deconstruct, Equals og ToString for oss.
public record PostTodoDTO(string Title, string Body);

