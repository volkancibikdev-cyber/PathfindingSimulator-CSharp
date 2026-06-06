using System;

namespace PathfindingSimulator
{
    public class Node
    {
        // Izgaradaki kordinatları (hangi satır/sütunda duruyor)
        public int GridX { get; set; }
        public int GridY { get; set; }

        // Hücrenin o anki kimliği (Duvar mı? Hedef mi? vs.)
        public bool IsWall { get; set; }
        public bool IsStart { get; set; }
        public bool IsEnd { get; set; }
        public bool IsVisited { get; set; }

        // A* algoritması için gerekli maliyet hesapları
        // GCost: Başlangıçtan buraya gelmek ne kadar sürdü?
        public int GCost { get; set; }

        // HCost: Buradan hedefe (kuş uçuşu) ne kadar mesafe kaldı?
        public int HCost { get; set; }

        // F-Cost, bu iki değerin toplamıdır. Algoritma hep en düşük F-Cost'a gitmeye çalışır.
        // Sadece okuma (get) işlemi yapıyoruz, değer istendikçe kendi kendini hesaplıyor.
        public int FCost => GCost + HCost;

        // Hedefi bulduğumuzda "ben buraya nereden geldim?" diyebilmesi için
        // bir önceki hücreyi hafızada tutuyoruz. (Başlangıçta boş olabileceği için '?' koyduk)
        public Node? ParentNode { get; set; }

        public Node(int gridX, int gridY)
        {
            GridX = gridX;
            GridY = gridY;

            // İlk yaratıldığında her hücre sıradan, boş bir karedir
            IsWall = false;
            IsStart = false;
            IsEnd = false;
            IsVisited = false;
        }

        // Yeni bir arama başlattığımızda eski yolun izlerini silmek için pratik bir metot
        public void ResetAlgorithmState()
        {
            IsVisited = false;
            ParentNode = null;

            // Algoritmalar her zaman mesafeyi önce "sonsuz" kabul ederek başlar.
            // Biz de int'in alabileceği en büyük değeri veriyoruz.
            GCost = int.MaxValue;
            HCost = 0;
        }
    }
}