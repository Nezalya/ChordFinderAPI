namespace ChordFinderAPI.Models
{
    public class Escala
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty; 
        public string Tipo { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;

        // Relación muchos a muchos con Acordes
        public List<Acorde> Acordes { get; set; } = new();
    }
}
