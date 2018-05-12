namespace Dungeons_and_Dragons_Helper
{
    public static class Rozmiar
    {
        public enum RozmiarEnum:int
        {
            Maly = 1,
            Sredni = 2,
            Duzy = 3,
            Brak=0
        }

        public static RozmiarEnum getRozmiarById(int id)
        {
            switch (id)
            {
                case 1: return RozmiarEnum.Maly;
                case 2: return RozmiarEnum.Sredni;
                case 3: return RozmiarEnum.Duzy;
            }

            return RozmiarEnum.Brak;
        }
    }
}