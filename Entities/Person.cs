namespace Darts_for_people.Entities
{
    public class Person
    {
        public string Name { get; set; } = null!;
        public string Surname { get; set; } = null!;
        public string Id { get; set; } = null!;
        public DateOnly BirthDate { get; set; }
    }
}