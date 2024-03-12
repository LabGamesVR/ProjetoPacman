using System;

namespace Assets.Scripts
{
    public class Game
    {
        public Game()
        {
            Inicio = DateTime.Now;
            QtdVezesAcima = 0;
            QtdVezesAbaixo = 0;
            QtdVezesDireita = 0;
            QtdVezesEsquerda = 0;
            MaiorAmplitudeAcima = 0;
            MaiorAmplitudeAbaixo = 0;
            MaiorAmplitudeDireita = 0;
            MaiorAmplitudeEsquerda = 0;
        }

        public int PatientId { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        public short QtdVezesAcima { get; set; }
        public short QtdVezesAbaixo { get; set; }
        public short QtdVezesDireita { get; set; }
        public short QtdVezesEsquerda { get; set; }
        public decimal MaiorAmplitudeAcima { get; set; }
        public decimal MaiorAmplitudeAbaixo { get; set; }
        public decimal MaiorAmplitudeDireita { get; set; }
        public decimal MaiorAmplitudeEsquerda { get; set; }
    }
}
