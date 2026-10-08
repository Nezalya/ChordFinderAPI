namespace ChordFinderAPI.Models
{
    public class Acorde
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;

        // Relación muchos a muchos con Escalas
        public List<Escala> Escalas { get; set; } = new();
    }
}
