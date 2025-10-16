/*
    Nama    : Samuel Pardamean Pardosi
    NIM     : 2381008
    Pemrograman Framework .NET (PROJECT MID)
    Aplikasi Inventori Toko Sederhana

    File : Transaksi.cs
    -----------------------------------------
    Berisi dua class :
    1. class Transaksi --> representasi data transaksi
    2. class TransaksiManager --> menangani CRUD transaksi
        dan berinteraksi dengan ProdukManager.
*/

using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
// --MODEL TRANSAKSI--
public class Transaksi
{
    public string IDTransaksi { get; set; } = "";
    public string KodeProduk { get; set; } = "";
    public string NamaProduk { get; set; } = "";
    public int Jumlah { get; set; }
    public decimal TotalHarga { get; set; }

    // Serelize --> ubah jadi 1 baris teks untuk disimpan
    public string Serialize()
        => $"{IDTransaksi}|{KodeProduk}|{NamaProduk}|{Jumlah}|{TotalHarga.ToString("0.##", CultureInfo.InvariantCulture)}";
    // TryDeserialize --> parsing dari 1 baris file ke objek
    public static bool TryDeserialize(string line, out Transaksi t)
    {
        t = new Transaksi();
        try
        {
            var parts = line.Split('|');
            if (parts.Length != 5) return false;

            t.IDTransaksi = parts[0].Trim();
            t.KodeProduk = parts[1].Trim();
            t.NamaProduk = parts[2].Trim();
            t.Jumlah = int.Parse(parts[3].Trim());
            t.TotalHarga = decimal.Parse(parts[4].Trim(), CultureInfo.InvariantCulture);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
// --MANAJER TRANSAKSI (CRUD)
public class TransaksiManager
{
    private string filePath;
    private List<Transaksi> data = new List<Transaksi>();
    private ProdukManager produkManager;
    public bool Changed { get; private set; } = false;

    public TransaksiManager(string path, ProdukManager pm)
    {
        filePath = path;
        produkManager = pm;
        data = new List<Transaksi>(LoadData());
    }
    // --MENU TRANSAKSI--
    public void MenuTransaksi()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("==== MENU TRANSAKSI ====");
            Console.WriteLine("1. Lihat semua Transaksi");
            Console.WriteLine("2. Tambah Transaksi Baru");
            Console.WriteLine("3. Hapus Transaksi");
            Console.WriteLine("4. Simpan ke File");
            Console.WriteLine("0. Kembali ke Menu Utama");
            Console.Write("Pilihan Menu :");
            string? pilihan = Console.ReadLine();

            switch (pilihan)
            {
                case "1": TampilTabel(); Pause(); break;
                case "2": TambahTransaksi(); Pause(); break;
                case "3": HapusTransaksi(); Pause(); break;
                case "4": SimpanData(); Pause(); break;
                case "0": return;
                default:
                    Console.WriteLine("Pilihan tidak valid!");
                    Pause();
                    break;
            }
        }
    }
    // --CRUD--
    private void TampilTabel()
    {
        Console.WriteLine("\nID\tKode\tNama Produk\tJumlah\tTotal Harga");
        Console.WriteLine("-------------------------------------------------------------");

        if (data.Count == 0)
        {
            Console.WriteLine("Belum ada data transaksi.");
            return;
        }

        foreach (var t in data)
        {
            Console.WriteLine($"{t.IDTransaksi}\t{t.KodeProduk}\t{t.NamaProduk.PadRight(15)}\t{t.Jumlah}\tRp {t.TotalHarga:N0}");
        }
    }
    private void TambahTransaksi()
    {
        Console.WriteLine("\n==== Tambah Transaksi Baru ====");
        string kode = BacaString("Masukan Kode Produk :");

        // cari produk berdasarkan kode
        var produk = produkManager.GetProdukByKode(kode);
        if (produk == null)
        {
            Console.WriteLine("Produk tidak ditemukan!");
            return;
        }

        Console.WriteLine($"Produk : {produk.Nama} (Stok: {produk.Stok}, Harga : Rp {produk.Harga:N0})");
        int jumlah = BacaInt("Jumlah beli : ");

        if (jumlah <= 0 || jumlah > produk.Stok)
        {
            Console.WriteLine("Jumlah tidak valid atau stok tidak cukup!");
            return;
        }

        // buat transaksi baru
        Transaksi trx = new Transaksi();
        trx.IDTransaksi = $"TRX{DateTime.Now.Ticks % 1000000:D6}";
        trx.KodeProduk = produk.Kode;
        trx.NamaProduk = produk.Nama;
        trx.Jumlah = jumlah;
        trx.TotalHarga = produk.Harga * jumlah;

        // kurangi stok produk
        produkManager.KurangiStok(produk.Kode, jumlah);

        // tambahkan ke list transaksi
        data.Add(trx);
        Changed = true;
        Console.WriteLine($">> Transaksi berhasil ditambahkan. Total : Rp {trx.TotalHarga:N0}");
    }

    private void HapusTransaksi()
    {
        if (data.Count == 0)
        {
            Console.WriteLine("Belum ada data Transaksi.");
            return;
        }

        TampilTabel();
        string id = BacaString("\nMasukan ID transaksi yang ini di hapus :");
        int idx = data.FindIndex(t => t.IDTransaksi.Equals(id, StringComparison.OrdinalIgnoreCase));

        if (idx == -1)
        {
            Console.WriteLine("Transaksi tidak ditemukan!");
            return;
        }

        data.RemoveAt(idx);
        Changed = true;
        Console.WriteLine(">> Transaksi berhasil dihapus!");
    }

    // --FILE HANDLING--
    private List<Transaksi> LoadData()
    {
        if (!File.Exists(filePath)) return new List<Transaksi>();

        try
        {
            var lines = File.ReadAllLines(filePath);
            var list = new List<Transaksi>();
            foreach (var ln in lines)
                if (Transaksi.TryDeserialize(ln, out var t))
                    list.Add(t);
            return list;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Gagal membaca file : {ex.Message}");
            return new List<Transaksi>();
        }
    }

    public void SimpanData()
    {
        try
        {
            using var sw = new StreamWriter(filePath);
            foreach (var t in data)
                sw.WriteLine(t.Serialize());
            Changed = false;
            Console.WriteLine($">> Data Transaksi berhasil disimpan!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Gagal menyimpan file : {ex.Message}");
        }
    }
    // --HELPER--
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

    private void Pause()
    {
        Console.WriteLine("\nTekan tombol apapun untuk melanjutkan...");
        Console.ReadKey();
    }
} // NO CODE BELOW THIS LINE