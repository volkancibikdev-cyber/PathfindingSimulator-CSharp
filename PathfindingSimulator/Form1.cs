using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PathfindingSimulator
{
    public partial class Form1 : Form
    {
        // Arayüz ayarları
        private int _nodeSize = 25; // Karelerin piksel boyutu. Bunu değiştirirsen ızgara otomatik ayarlanır.
        private int _rows;
        private int _cols;

        // Derleyicinin "Bu boş kalabilir" uyarılarını susturmak için "= null!;" ekliyoruz, 
        // çünkü biz bunların içini form yüklenirken (InitializeGrid içinde) kesin dolduracağız.
        private Node[,] _grid = null!;
        private Bitmap _bitmap = null!;
        private Graphics _graphics = null!;
        private Node _startNode = null!;
        private Node _endNode = null!;

        // Fırçalar ve Kalemler (Çizim işlemlerini hızlandırmak için bir kere oluşturup hafızada tutuyoruz)
        private Brush _wallBrush = Brushes.Black;
        private Brush _startBrush = Brushes.Green;
        private Brush _endBrush = Brushes.Red;
        private Brush _emptyBrush = Brushes.White;
        private Brush _pathBrush = Brushes.DodgerBlue; // Bulunan kesin yol
        private Brush _visitedBrush = Brushes.LightCyan; // Algoritmanın taradığı/düşündüğü yerler
        private Pen _gridPen = Pens.LightGray;

        // Fare hareketleri kontrolden çıkmasın diye kullandığımız güvenlik bayrakları
        private bool _isDrawingWall = false;
        private bool _isErasingWall = false;

        // Bulunan en kısa yolu hafızada tutacağımız liste
        private List<Node> _finalPath = new List<Node>();

        public Form1()
        {
            InitializeComponent();
            InitializeGrid();

            // Fare ve çizim olaylarını (event) formdaki PictureBox'a bağlıyoruz
            pbxGrid.Paint += PbxGrid_Paint;
            pbxGrid.MouseDown += PbxGrid_MouseDown;
            pbxGrid.MouseMove += PbxGrid_MouseMove;
            pbxGrid.MouseUp += PbxGrid_MouseUp;
        }

        private void InitializeGrid()
        {
            // Ekran boyutuna göre sığabilecek satır ve sütun sayısını hesapla
            _cols = pbxGrid.Width / _nodeSize;
            _rows = pbxGrid.Height / _nodeSize;

            _grid = new Node[_cols, _rows];

            // Izgarayı Node nesneleriyle ilmek ilmek dokuyoruz
            for (int x = 0; x < _cols; x++)
            {
                // BURADAKİ DİZGİ HATASI DÜZELTİLDİ: x < _rows yerine y < _rows yapıldı.
                for (int y = 0; y < _rows; y++)
                {
                    _grid[x, y] = new Node(x, y);
                }
            }

            // Başlangıç ve bitiş noktalarını ekranda estetik duracak şekilde yerleştir
            _startNode = _grid[2, _rows / 2];
            _startNode.IsStart = true;

            _endNode = _grid[_cols - 3, _rows / 2];
            _endNode.IsEnd = true;

            // Arka plandaki çizim tuvalimizi hazırlıyoruz (GDI+ optimizasyonu)
            _bitmap = new Bitmap(pbxGrid.Width, pbxGrid.Height);
            _graphics = Graphics.FromImage(_bitmap);

            pbxGrid.Image = _bitmap;
            DrawGrid();
        }

        // Manhattan Distance (Kuş uçuşu çapraz gitmek yerine, ızgara üzerindeki en kısa L tipi mesafeyi ölçer)
        private int GetDistance(Node nodeA, Node nodeB)
        {
            int dstX = Math.Abs(nodeA.GridX - nodeB.GridX);
            int dstY = Math.Abs(nodeA.GridY - nodeB.GridY);
            return dstX + dstY;
        }

        // Etraftaki 4 komşuyu (sağ, sol, alt, üst) getiren yardımcı
        private List<Node> GetNeighbors(Node node)
        {
            List<Node> neighbors = new List<Node>();

            // Eksenlerdeki hareket yönleri
            int[] dx = { 0, 1, 0, -1 };
            int[] dy = { -1, 0, 1, 0 };

            for (int i = 0; i < 4; i++)
            {
                int checkX = node.GridX + dx[i];
                int checkY = node.GridY + dy[i];

                // Komşu, haritanın sınırları içinde mi diye bakıyoruz 
                // (Örn: en sol üst köşedeysek sol komşu aramayız)
                if (checkX >= 0 && checkX < _cols && checkY >= 0 && checkY < _rows)
                {
                    neighbors.Add(_grid[checkX, checkY]);
                }
            }
            return neighbors;
        }

        // Hedefi bulunca ebeveyn düğümleri takip edip ipi geriye doğru saran metot
        private void RetracePath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);

                // Uyarıyı susturmak ve güvenceye almak için ufak bir null kontrolü
                if (currentNode.ParentNode != null)
                {
                    currentNode = currentNode.ParentNode;
                }
                else
                {
                    break;
                }
            }

            path.Reverse(); // Yolu baştan sona doğru düzelttik
            _finalPath = path;
        }

        // İşin kalbi: A* Algoritması motoru (Animasyonlu yapıda)
        private async Task FindPathAsync()
        {
            // Yeni aramaya başlamadan önce eski mavi yolu ve ziyaret geçmişini temizle
            _finalPath.Clear();
            for (int x = 0; x < _cols; x++)
            {
                for (int y = 0; y < _rows; y++)
                {
                    _grid[x, y].ResetAlgorithmState();
                }
            }

            List<Node> openSet = new List<Node>(); // Henüz incelenmemiş ama sırada olanlar
            HashSet<Node> closedSet = new HashSet<Node>(); // İşimiz biten, kapatılmış hücreler

            _startNode.GCost = 0;
            _startNode.HCost = GetDistance(_startNode, _endNode);
            openSet.Add(_startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet[0];

                // O anki listemizde hedefe "en mantıklı" (F-Cost'u en düşük) olan hücreyi seç
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].FCost < currentNode.FCost ||
                       (openSet[i].FCost == currentNode.FCost && openSet[i].HCost < currentNode.HCost))
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                // Algoritmanın nereleri taradığını göstermek için ziyaret edildi diye işaretliyoruz
                if (currentNode != _startNode && currentNode != _endNode)
                {
                    currentNode.IsVisited = true;
                }

                // Ekranda su dalgası gibi yayılma animasyonunu sağlayan kısım
                DrawGrid();
                await Task.Delay(10); // Milisaniye cinsinden hız. Düşürürsen simülasyon hızlanır.

                // Hedefe ulaştıysak döngüyü kır, yolu çiz ve metottan çık
                if (currentNode == _endNode)
                {
                    RetracePath(_startNode, _endNode);
                    DrawGrid();
                    return;
                }

                // Komşuları tek tek dolaş ve hangisine gitmek daha mantıklı hesapla
                foreach (Node neighbor in GetNeighbors(currentNode))
                {
                    // Duvara çarptıysak veya zaten baktığımız bir yerse pas geçiyoruz
                    if (neighbor.IsWall || closedSet.Contains(neighbor)) continue;

                    int newMovementCostToNeighbor = currentNode.GCost + 1;

                    // Eğer yeni bulduğumuz yol, komşunun eski yolundan daha kısaysa güncellemeleri yap
                    if (newMovementCostToNeighbor < neighbor.GCost || !openSet.Contains(neighbor))
                    {
                        neighbor.GCost = newMovementCostToNeighbor;
                        neighbor.HCost = GetDistance(neighbor, _endNode);
                        neighbor.ParentNode = currentNode;

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }
        }

        // Tüm sahneyi sıfırdan çizen ressamımız
        private void DrawGrid()
        {
            _graphics.Clear(Color.White);

            for (int x = 0; x < _cols; x++)
            {
                for (int y = 0; y < _rows; y++)
                {
                    Node node = _grid[x, y];
                    Brush currentBrush = _emptyBrush;

                    // Hangi hücre ne renge boyanacak? Buradaki öncelik sırası çok önemlidir.
                    if (node.IsStart) currentBrush = _startBrush;
                    else if (node.IsEnd) currentBrush = _endBrush;
                    else if (node.IsWall) currentBrush = _wallBrush;
                    else if (_finalPath.Contains(node)) currentBrush = _pathBrush;
                    else if (node.IsVisited) currentBrush = _visitedBrush;

                    // Hücrenin içini doldur ve etrafına ince bir çerçeve at
                    _graphics.FillRectangle(currentBrush, x * _nodeSize, y * _nodeSize, _nodeSize, _nodeSize);
                    _graphics.DrawRectangle(_gridPen, x * _nodeSize, y * _nodeSize, _nodeSize, _nodeSize);
                }
            }

            // --- KULLANICI BİLGİLENDİRME (HUD) PANELİ ---
            // Arkaya cam efekti (yarı saydam) koyu bir şerit çekiyoruz (Alpha: 140)
            using (Brush hudBackground = new SolidBrush(Color.FromArgb(140, 20, 20, 20)))
            {
                _graphics.FillRectangle(hudBackground, 0, 0, pbxGrid.Width, 35);
            }

            // Metni şeridin içine basıyoruz. using bloğu sayesinde işi biten font hemen bellekten silinir.
            using (Font infoFont = new Font("Segoe UI", 10, FontStyle.Bold))
            {
                string instructions = "Sol Tık: Duvar Koy | Sağ Tık: Duvar Sil | Yeşil: Başlangıç | Kırmızı: Hedef | Koyu Mavi: Rota";
                _graphics.DrawString(instructions, infoFont, Brushes.White, 15, 7);
            }
            // ---------------------------------------------

            pbxGrid.Refresh(); // Yeni kareyi ekrana bas
        }

        // --- FARE OLAYLARI (EVENTS) ---
        // Null referans uyarılarını çözmek için object? (soru işareti) kullandık.
        private void PbxGrid_MouseDown(object? sender, MouseEventArgs e)
        {
            // Farenin tıkladığı pikseli al, ızgaradaki kaça kaçlık kareye denk geldiğini bul
            int x = e.X / _nodeSize;
            int y = e.Y / _nodeSize;

            // Yanlışlıkla formun dışına tıklanırsa çökmesini engelliyoruz
            if (x < 0 || x >= _cols || y < 0 || y >= _rows) return;

            Node clickedNode = _grid[x, y];

            // Başlangıç ve bitişin üstüne duvar çizilmez!
            if (clickedNode.IsStart || clickedNode.IsEnd) return;

            if (e.Button == MouseButtons.Left)
            {
                _isDrawingWall = true;
                clickedNode.IsWall = true;
            }
            else if (e.Button == MouseButtons.Right)
            {
                _isErasingWall = true;
                clickedNode.IsWall = false;
            }

            DrawGrid(); // Çizimi güncelle
        }

        private void PbxGrid_MouseMove(object? sender, MouseEventArgs e)
        {
            // Sadece tıklılı tutup sürüklüyorsa işlem yapıyoruz, yoksa CPU'yu yormaya gerek yok
            if (!_isDrawingWall && !_isErasingWall) return;

            int x = e.X / _nodeSize;
            int y = e.Y / _nodeSize;

            if (x < 0 || x >= _cols || y < 0 || y >= _rows) return;

            Node hoveredNode = _grid[x, y];

            if (hoveredNode.IsStart || hoveredNode.IsEnd) return;

            bool stateChanged = false;

            // Zaten duvar olan yere bir daha duvar atamaya çalışıp boşuna ekranı yenilemiyoruz
            if (_isDrawingWall && !hoveredNode.IsWall)
            {
                hoveredNode.IsWall = true;
                stateChanged = true;
            }
            else if (_isErasingWall && hoveredNode.IsWall)
            {
                hoveredNode.IsWall = false;
                stateChanged = true;
            }

            if (stateChanged)
            {
                DrawGrid();
            }
        }

        private void PbxGrid_MouseUp(object? sender, MouseEventArgs e)
        {
            // Parmağını fareden çektiğinde fırçayı bırakıyoruz
            _isDrawingWall = false;
            _isErasingWall = false;
        }

        private void PbxGrid_Paint(object? sender, PaintEventArgs e)
        {
            // Biz kendi çizim sistemimizi (GDI+) kurduğumuz için varsayılan paint metodu boş kalıyor.
        }

        // Arama Butonuna (button1) tıklanınca
        private async void button1_Click(object? sender, EventArgs e)
        {
            Button? btn = sender as Button;

            // Simülasyon akarken kullanıcının butona art arda tıklayıp programı bozmasını engelliyoruz
            if (btn != null) btn.Enabled = false;

            await FindPathAsync();

            if (btn != null) btn.Enabled = true;
        }


    }
}