using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form3 : Form
    {
        static string connectionString = string.Format("Server=localhost;Port=3307;Database=DBAkademi1;Uid=root;Pwd=admin123;");
        MySqlConnection conn = new MySqlConnection(connectionString);
        MySqlDataAdapter da;
        DataTable dtMahasiswa;
        DataTable dtProdi;
        Mahasiswaado Mahasiswaado = new Mahasiswaado();
        DAL dbLogic = new DAL();

        string _prodi {  get; set; }
        string _tahunMasuk { get; set; }
        public Form3(string prodi, string tahunMasuk)
        {
            InitializeComponent();
            _prodi = prodi;
            _tahunMasuk = tahunMasuk;
            try
            {
                DataTable dtMahasiswa = dbLogic.getDataRekap(prodi, DateTime.Parse(tahunMasuk));

                Mahasiswaado.SetDataSource(dtMahasiswa);
                crystalReportViewer1.ReportSource = Mahasiswaado;
                crystalReportViewer1.Refresh();
            }
            catch (Exception ex)
            {
                //simpanLog(ex.Message);
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }
        private void Form3_Load(object sender, EventArgs e)
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                MySqlCommand cmd = new MySqlCommand("sp_Report", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                // Masukkan parameter filter secara langsung
                cmd.Parameters.AddWithValue("@inProdi", _prodi);
                cmd.Parameters.AddWithValue("@inTglMsuk", _tahunMasuk); // Mengirim string 4 digit (ex: "2026")

                da = new MySqlDataAdapter(cmd);
                dtMahasiswa = new DataTable();
                da.Fill(dtMahasiswa);

                conn.Close();

                // Pasang ke Crystal Report
                Mahasiswaado.SetDataSource(dtMahasiswa);
                crystalReportViewer1.ReportSource = Mahasiswaado;
                crystalReportViewer1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data laporan: " + ex.Message);
            }
        }
    }
   
    }
