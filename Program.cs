/*
    Nama    : Samuel Pardamean Pardosi
    NIM     : 2381008
    Pemrograman Framework .NET (PROJECT MID)
    Aplikasi Inventori Toko Sederhana

    File : Program.cs
    -----------------------------------------
    File utama (entry point)
    Program ini mengelola dua entitas utama :
    - Data Produk (barang toko)
    - Data Transaksi (pembelian)
*/

using System;
using System.Data;

class Program
{
    static void Main()
    {
        // inisialisasi objek pengelola produk dan transaksi
        ProdukManager produkManager = new ProdukManager("DataProduk.txt");
        TransaksiManager transaksiManager = new TransaksiManager("DataTransaksi.txt", produkManager);

        while (true)
        {
            Console.Clear();
            Console.WriteLine("==== APLIKASI INVENTORI TOKO SEDERHANA ====");
            Console.WriteLine("1. Kelola Data Produk");
            Console.WriteLine("2. Kelola Data Transaksi");
            Console.WriteLine("0. Keluar");
            Console.Write("Pilih Menu :");
            string? pilihan = Console.ReadLine();

            switch (pilihan)
            {
                case "1":
                    produkManager.MenuProduk();
                    break;
                case "2":
                    transaksiManager.MenuTransaksi();
                    break;
                case "0": // keluar dari app (pastikan user sudah menyimpan)
                    if (produkManager.Changed || transaksiManager.Changed)
                    {
                        Console.Write("Perubahan belum disimpan. Simpan sekarang? (y/n) :");
                        string? simpan = Console.ReadLine()?.Trim().ToLowerInvariant();
                        if (simpan == "y")
                        {
                            produkManager.SimpanData();
                            transaksiManager.SimpanData();
                        }
                    }
                    Console.WriteLine("Terima kasih telah menggunakan apliaksi.");
                    return;
            }
        }
    }

    // fungsi jeda 
    static void Pause()
    {
        Console.WriteLine("\nTekan tombol apapun untuk melanjutkan...");
        Console.ReadKey();
    }
}// NO CODE BELOW THIS LINE