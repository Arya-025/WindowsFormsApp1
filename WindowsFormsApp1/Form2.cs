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

   
    public partial class Form2 : Form
    {

        static string connectionString = string.Format("Server=localhost;Port=3307;Database=DBAkademi1;Uid=root;Pwd=admin123;");

        //object untuk merepresentasikan konkesi ke MySQL Server
        MySqlConnection conn = new MySqlConnection(connectionString);
        MySqlDataAdapter da;
        DataTable dtMahasiswa;
        DataTable dtProdi;
        



        public Form2(string prodi, DateTime tglmasuk)
        {
            InitializeComponent();

           
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            dtpTanggalMasuk.Format = DateTimePickerFormat.Custom;
            dtpTanggalMasuk.CustomFormat = "yyyy";
            dtpTanggalMasuk.ShowUpDown = true;
            dtpTanggalMasuk.MinDate = new DateTime(2000, 1, 1);
            dtpTanggalMasuk.MaxDate = DateTime.Now;

            cmbProdi.DropDownStyle = ComboBoxStyle.DropDownList;

            btnCetak.Enabled = false;

            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                MySqlCommand cmd = new MySqlCommand("select namaprodi from prodi", conn);
                cmd.CommandType = CommandType.Text;
                dtProdi = new DataTable();
                da = new MySqlDataAdapter(cmd);
                da.Fill(dtProdi);
                cmbProdi.DataSource = dtProdi;
                cmbProdi.DisplayMember = "namaprodi";
                cmbProdi.ValueMember = "namaprodi";

            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                MySqlCommand cmd = new MySqlCommand("sp_Report", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@inProdi", cmbProdi.SelectedValue);
                cmd.Parameters.AddWithValue("@inTglMsuk", dtpTanggalMasuk.Value.Year);

                da = new MySqlDataAdapter(cmd);

                dtMahasiswa = new DataTable();
                da.Fill(dtMahasiswa);

                dataGridView1.DataSource = dtMahasiswa;

                if (dtMahasiswa.Rows.Count > 0)
                {
                    btnCetak.Enabled = true;
                }
                else
                {
                    btnCetak.Enabled = false;
                    MessageBox.Show("Data tidak ditemukan");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        } 

        private void btnCetak_Click(object sender, EventArgs e)
        {
            // Ambil string nama prodi dari .Text secara murni
    string prodiTerpilih = cmbProdi.Text;
    
    // Ambil tahunnya saja (.Year) dari DateTimePicker Form 2
    int tahunTerpilih = dtpTanggalMasuk.Value.Year;

    // Kirim data yang sudah bersih ke Form 3
    Form3 frm3 = new Form3(prodiTerpilih, tahunTerpilih.ToString());
    frm3.Show();
    this.Hide();
        }
    } 

}
