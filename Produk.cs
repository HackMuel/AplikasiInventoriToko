/*
    Nama    : Samuel Pardamean Pardosi
    NIM     : 2381008
    Pemrograman Framework .NET (PROJECT MID)
    Aplikasi Inventori Toko Sederhana

    File : Produk.cs
    -----------------------------------------
    Berisi dua class utama :
    1. class Produk --> representasi data produk (model)
    2. class ProdukManager --> menangani semua operasi CRUD produk
*/

using System;
using System.Buffers;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;

// representasi data 1 produk
public class Produk
{
    public string Kode { get; set; } = "";
    public string Nama { get; set; } = "";
    public decimal Harga { get; set; }
    public int Stok { get; set; }

    // mengubah objek produk jadi string untuk disimpan ke file
    public string Serialize()
        => $"{Kode}|{Nama}|{Harga.ToString("0.##", CultureInfo.InvariantCulture)}|{Stok}";

    // mengubah 1 baris teks dari file jadi objek Produk
    public static bool TryDeserialize(string line, out Produk p)
    {
        p = new Produk();
        try
        {
            var parts = line.Split('|');
            if (parts.Length != 4) return false;

            p.Kode = parts[0].Trim();
            p.Nama = parts[1].Trim();
            p.Harga = decimal.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
            p.Stok = int.Parse(parts[3].Trim());

            return true;
        }
        catch
        {
            return false;
        }
    }
}

// manajer untuk CRUD Produk
public class ProdukManager
{
    private string filePath;
    private Produk[] data = Array.Empty<Produk>();
    public bool Changed { get; private set; } = false;

    public ProdukManager(string path)
    {
        filePath = path;
        data = LoadData();
    }

    // --MENU PRODUK--
    public void MenuProduk()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("==== MENU PRODUK ====");
            Console.WriteLine("1. Lihat Semua Produk");
            Console.WriteLine("2. Tambah Produk Baru");
            Console.WriteLine("3. Edit Produk");
            Console.WriteLine("4. Hapus Produk");
            Console.WriteLine("5. Simpan ke File");
            Console.WriteLine("0. Kembali ke Menu Utama");
            Console.Write("Pilih Menu :");
            string? pilihan = Console.ReadLine();

            switch (pilihan)
            {
                case "1": TampilTabel(); Pause(); break;
                case "2": TambahProduk(); Pause(); break;
                case "3": EditProduk(); Pause(); break;
                case "4": HapusProduk(); Pause(); break;
                case "5": SimpanData(); Pause(); break;
                case "0": return;
                default:
                    Console.WriteLine("Pilihan tidak valid.");
                    Pause();
                    break;
            }
        }
    }
    // --CRUD OPERATIONS--
    private void TampilTabel()
    {
        Console.WriteLine("\nKode\tNama\tHarga\t\tStok");
        Console.WriteLine("-------------------------------------------------");

        if (data.Length == 0)
        {
            Console.WriteLine("Belum ada data produk.");
            return;
        }

        foreach (var p in data)
        {
            Console.WriteLine($"{p.Kode}\t{p.Nama.PadRight(15)}\tRp {p.Harga:N0}\t{p.Stok}");
        }
    }

    private void TambahProduk()
    {
        Console.WriteLine("\n==== Tambah Produk Baru ====");

        Produk p = new Produk();
        p.Kode = BacaString("Kode Produk :");
        p.Nama = BacaString("Nama Produk :");
        p.Harga = BacaDecimal("Harga :");
        p.Stok = BacaInt("Stok :");

        // tambahkan ke array
        var baru = new Produk[data.Length + 1];
        Array.Copy(data, baru, data.Length);
        baru[data.Length] = p;
        data = baru;

        Changed = true;
        Console.WriteLine(">> Produk berhasil ditambahkan!");
    }

    private void EditProduk()
    {
        if (data.Length == 0)
        {
            Console.WriteLine("Belum ada data produk.");
            return;
        }

        TampilTabel();
        string kode = BacaString("\nMasukan kode produk yang ingin di edit :");
        int idx = Array.FindIndex(data, p => p.Kode.Equals(kode, StringComparison.OrdinalIgnoreCase));

        if (idx == -1)
        {
            Console.WriteLine("Produk tidak ditemukan!");
            return;
        }

        var p = data[idx];
        Console.WriteLine($"\nEdit Produk : {p.Nama}");
        Console.WriteLine("1. Ubah Nama");
        Console.WriteLine("2. Ubah Harga");
        Console.WriteLine("3. Ubah Stok");
        Console.WriteLine("Lainnya : Batal");
        Console.Write("Pilih :");
        string? pilih = Console.ReadLine();

        switch (pilih)
        {
            case "1": p.Nama = BacaString("Nama baru :"); break;
            case "2": p.Harga = BacaDecimal("Harga baru :"); break;
            case "3": p.Stok = BacaInt("Stok Baru :"); break;
            default: Console.WriteLine("Batal edit."); return;
        }

        data[idx] = p;
        Changed = true;
        Console.WriteLine(">> Data produk berhasil diperbarui!");
    }

    private void HapusProduk()
    {
        if (data.Length == 0)
        {
            Console.WriteLine("Tidak ad data untuk dihapus!");
            return;
        }

        TampilTabel();
        string kode = BacaString("\nMasukan kode produk yang ingin di hapus :");
        int idx = Array.FindIndex(data, p => p.Kode.Equals(kode, StringComparison.OrdinalIgnoreCase));

        if (idx == -1)
        {
            Console.WriteLine("Produk tidak ditemukan!");
            return;
        }

        var baru = new Produk[data.Length - 1];
        Array.Copy(data, 0, baru, 0, idx);
        Array.Copy(data, idx + 1, baru, idx, data.Length - idx - 1);
        data = baru;

        Changed = true;
        Console.WriteLine(">> Produk berhasil dihapus!");
    }
    // --FILE HANDLING--
    private Produk[] LoadData()
    {
        if (!File.Exists(filePath)) return Array.Empty<Produk>();

        try
        {
            var lines = File.ReadAllLines(filePath);
            var list = new System.Collections.Generic.List<Produk>();
            foreach (var ln in lines)
            {
                if (Produk.TryDeserialize(ln, out var p))
                    list.Add(p);
            }
            return list.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Gagal membaca file : {ex.Message}");
            return Array.Empty<Produk>();
        }
    }

    public void SimpanData()
    {
        try
        {
            using var sw = new StreamWriter(filePath);
            foreach (var p in data)
                sw.WriteLine(p.Serialize());
            Changed = false;
            Console.WriteLine(">> Data produk berhasil disimpan!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Gagal menyimpan file : {ex.Message}");
        }
    }
    // --HELPER FUNCTIONS
    private string BacaString(string msg)
    {
        while (true)
        {
            Console.Write(msg);
            string? s = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
            Console.WriteLine("Input tidak boleh kosong!");
        }
    }
    private int BacaInt(string msg)
    {
        while (true)
        {
            Console.Write(msg);
            if (int.TryParse(Console.ReadLine(), out int val) && val >= 0)
                return val;
            Console.WriteLine("Masukan angka valid (>= 0)!");
        }
    }
    private decimal BacaDecimal(string msg)
    {
        while (true)
        {
            Console.Write(msg);
            if (decimal.TryParse(Console.ReadLine(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal val) && val >= 0)
                return val;
            Console.WriteLine("MAsukan nilai desimal valid (>= 0)!");
        }
    }
    private void Pause()
    {
        Console.WriteLine("\nTekan tombol apapun untuk melanjutkan...");
        Console.ReadKey();
    }
    // mengambil produk berdasarkan kode
    public Produk? GetProdukByKode(string kode)
    {
        foreach (var p in data)
            if (p.Kode.Equals(kode, StringComparison.OrdinalIgnoreCase))
                return p;
        return null;
    }

    // mengurangi stok setelah transaksi
    public void KurangiStok(string kode, int jumlah)
    {
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i].Kode.Equals(kode, StringComparison.OrdinalIgnoreCase))
            {
                data[i].Stok -= jumlah;
                Changed = true;
                return;
            }
        }
    }
} // NO CODE BELOW THIS LINE