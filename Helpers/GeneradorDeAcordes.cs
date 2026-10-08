using ChordFinderAPI.Models;

namespace ChordFinderAPI.Helpers
{
    public static class GeneradorDeAcordes
    {
        private static readonly string[] NotasCromaticas =
            { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        private static readonly Dictionary<string, int[]> FormulasAcordes = new()
        {
            { "", new[] { 0, 4, 7 } },        // Mayor
            { "m", new[] { 0, 3, 7 } },       // Menor
            { "dim", new[] { 0, 3, 6 } },     // Diminuto
            { "7", new[] { 0, 4, 7, 10 } },    // Séptima Dominante
            { "maj7", new[] { 0, 4, 7, 11 } }  // Mayor Séptima
        };

        // Estructura de intervalos de las escalas
        private static readonly Dictionary<string, int[]> FormulasEscalas = new()
        {
            { "Mayor", new[] { 0, 2, 4, 5, 7, 9, 11 } },
            { "Menor", new[] { 0, 2, 3, 5, 7, 8, 10 } }
        };

        // Grados para la escala Mayor: I(M), ii(m), iii(m), IV(M), V(M), vi(m), vii°(dim)
        private static readonly string[] GradosEscalaMayor = { "", "m", "m", "", "", "m", "dim" };

        // Grados para la escala Menor: i(m), ii°(dim), III(M), iv(m), v(m), VI(M), VII(M)
        private static readonly string[] GradosEscalaMenor = { "m", "dim", "", "m", "m", "", "" };

        // Genera todas las variaciones de acordes
        public static Dictionary<string, Acorde> GenerarAcordes()
        {
            var diccAcordes = new Dictionary<string, Acorde>();

            for (int i = 0; i < NotasCromaticas.Length; i++)
            {
                string tonica = NotasCromaticas[i];

                foreach (var (sufijo, intervalos) in FormulasAcordes)
                {
                    var notasDelAcorde = intervalos
                        .Select(intervalo => NotasCromaticas[(i + intervalo) % 12]);

                    string nombreAcorde = $"{tonica}{sufijo}";

                    diccAcordes[nombreAcorde] = new Acorde
                    {
                        Nombre = nombreAcorde,
                        Notas = string.Join(", ", notasDelAcorde)
                    };
                }
            }

            return diccAcordes;
        }

        // Genera las escalas y vincula sus acordes armonizados en orden armónico estricto
        public static List<Escala> GenerarEscalasConArmonia(Dictionary<string, Acorde> acordesMap)
        {
            var listaEscalas = new List<Escala>();

            for (int i = 0; i < NotasCromaticas.Length; i++)
            {
                string tonica = NotasCromaticas[i];

                foreach (var (tipoEscala, intervalos) in FormulasEscalas)
                {
                    // Obtener las 7 notas de la escala en orden
                    var notasEscala = intervalos
                        .Select(intervalo => NotasCromaticas[(i + intervalo) % 12])
                        .ToList();

                    var escala = new Escala
                    {
                        Nombre = $"{tonica} {tipoEscala}",
                        Tipo = tipoEscala,
                        Notas = string.Join(", ", notasEscala)
                    };

                    // Determinar qué esquema de grados aplicar según el tipo de escala
                    string[] gradosAplicables = (tipoEscala == "Mayor")
                        ? GradosEscalaMayor
                        : GradosEscalaMenor;

                    // Recorrer los 7 grados en orden armónico estricto (del grado 1 al 7)
                    for (int grado = 0; grado < 7; grado++)
                    {
                        string notaRaizAcorde = notasEscala[grado];
                        string sufijoAcorde = gradosAplicables[grado];
                        string claveBuscada = $"{notaRaizAcorde}{sufijoAcorde}";

                        if (acordesMap.TryGetValue(claveBuscada, out var acordeEncontrado))
                        {
                            escala.Acordes.Add(acordeEncontrado);
                        }
                    }

                    listaEscalas.Add(escala);
                }
            }

            return listaEscalas;
        }
    }
}