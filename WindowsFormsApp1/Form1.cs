using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        private readonly MySqlConnection conn;
        //private readonly string connectionString =
        //    "Server=localhost;Database=CRUDMahasiswa;Uid=root;Pwd=;SslMode=none;";
        private readonly string connectionString = "Server=localhost;Port=3307;Database=DBAkademi1;Uid=root;Pwd=admin123;";
        //object untuk merepresentasikan konkesi ke MySQL Server


        MySqlDataAdapter da;
        DataTable dtMahasiswa;
        DataSet ds = new DataSet();

        public Form1()
        {
            InitializeComponent();
            conn = new MySqlConnection(connectionString);

        }
      

        private void LoadData()
        {
            try
            {
                // Cek dulu, kalau statusnya tertutup baru kita buka
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                da = new MySqlDataAdapter("SELECT * from vwMahasiswaPublic", conn);

                dtMahasiswa = new DataTable();
                da.Fill(dtMahasiswa);

                bindingSource1.DataSource = dtMahasiswa;
                dataGridView1.DataSource = bindingSource1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            cmbJK.DataSource = new string[] { "L", "P" };

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            bindingNavigator1.BindingSource = bindingSource1;

            LoadData();

        }
        private void ClearForm()
        {
            txtNim.Clear();
            txtNama.Clear();
            cmbJK.SelectedIndex = -1;
            dptTanggalLahir.Value = DateTime.Now;
            txtAlamat.Clear();
            txtKodeProdi.Clear();
            txtNim.Focus();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }

                DialogResult resultConfirm = MessageBox.Show(
                    "Yakin ingin menghapus data?",
                    "Konfirmasi",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (resultConfirm == DialogResult.Yes)
                {
                    string query = "DELETE FROM Mahasiswa WHERE NIM = @NIM";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@NIM", txtNim.Text);

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Data berhasil dihapus");
                        ClearForm();
                        btnLoad.PerformClick();
                    }
                    else
                    {
                        MessageBox.Show("Data tidak ditemukan");
                    }
                }
            }
            catch (MySqlException ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("SQL Error :" + ex.Message);
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("General Error :" + ex.Message);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    MessageBox.Show("Koneksi Berhasil");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Koneksi Gagal : " + ex.Message);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

            txtNim.Text = row.Cells["NIM"].Value.ToString();
            txtNama.Text = row.Cells["nama_mhs"].Value.ToString();
            cmbJK.Text = row.Cells["JenisKelamin"].Value.ToString();

            // --- PERBAIKAN DI SINI ---
            object tglLahir = row.Cells["TanggalLahir"].Value;

            // Cek apakah data kosong (DBNull) atau null
            if (tglLahir != DBNull.Value && tglLahir != null && !string.IsNullOrWhiteSpace(tglLahir.ToString()))
            {
                dptTanggalLahir.Value = Convert.ToDateTime(tglLahir);
            }
            else
            {
                // Jika kosong, set ke tanggal hari ini sebagai default
                dptTanggalLahir.Value = DateTime.Now;
            }
            // -------------------------

            txtAlamat.Text = row.Cells["Alamat"].Value?.ToString(); // Tambahkan '?' untuk aman dari null
            txtKodeProdi.Text = row.Cells["NamaProdi"].Value?.ToString();
            txtNim.Enabled = true;
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
           LoadData();  
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            MySqlTransaction trans = conn.BeginTransaction();
            try
            {
                if (txtNim.Text == "")
                {
                    MessageBox.Show("NIM harus diisi");
                    txtNim.Focus();
                    return;
                }

                if (txtNama.Text == "")
                {
                    MessageBox.Show("Nama harus diisi");
                    txtNama.Focus();
                    return;
                }

                if (cmbJK.Text == "")
                {
                    MessageBox.Show("Jenis Kelamin harus dipilih");
                    cmbJK.Focus();
                    return;
                }

                if (txtKodeProdi.Text == "")
                {
                    MessageBox.Show("Kode Prodi harus diisi");
                    txtKodeProdi.Focus();
                    return;
                }

                MySqlCommand command = new MySqlCommand("sp_InsertMahasiswa", conn);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("p_NIM", txtNim.Text);
                command.Parameters.AddWithValue("p_Nama", txtNama.Text);
                command.Parameters.AddWithValue("p_Alamat", txtAlamat.Text);
                command.Parameters.AddWithValue("p_JenisKelamin", cmbJK.Text);
                command.Parameters.AddWithValue("p_TanggalLahir", dptTanggalLahir.Value.Date);
                command.Parameters.AddWithValue("p_KodeProdi", txtKodeProdi.Text);
                command.Parameters.AddWithValue("p_TanggalDaftar", DateTime.Now); // Tambahkan ini kalau di C# lu kemarin ketinggalan!

                command.ExecuteNonQuery();
                trans.Commit();
                MessageBox.Show("Data mahasiswa berhasil ditambahkan");
                ClearForm();
                LoadData();
            }
            catch (MySqlException ex)
            {
                trans.Rollback();
                simpanLog("Rollback Insert :" + ex.Message);
                MessageBox.Show("SQL Error :" + ex.Message);
            }
            catch (Exception ex)
            {
                trans.Rollback();
                simpanLog("General Error :" + ex.Message);
                MessageBox.Show("General Error :" + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                // Mencari KodeProdi berdasarkan NamaProdi yang diinput di textbox
                string queryProdi = @"Select count(KodeProdi), KodeProdi from Prodi where 
            NamaProdi = @NmProdi Group By KodeProdi;";

                MySqlCommand cmd = new MySqlCommand(queryProdi, conn);
                cmd.Parameters.AddWithValue("@NmProdi", txtKodeProdi.Text);
                int result = Convert.ToInt32(cmd.ExecuteScalar());

                if (result == 0)
                {
                    // Jika Prodi tidak ditemukan, tampilkan daftar prodi yang tersedia
                    string queryNmProdi = @"Select NamaProdi from Prodi";
                    MySqlCommand MySqlcmd = new MySqlCommand(queryNmProdi, conn);
                    MySqlDataReader rd = MySqlcmd.ExecuteReader();

                    string nmProdi = "";
                    while (rd.Read())
                    {
                        nmProdi = rd["NamaProdi"].ToString() + "; ";
                    }
                    rd.Close();
                    MessageBox.Show("Prodi Tidak Ditemukan!\nNama Prodi yang Terdaftar :\n\n" + nmProdi,
                        "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    // Ambil KodeProdi dari hasil pencarian
                    MySqlDataReader reader = cmd.ExecuteReader();
                    string kodeProdi = "";
                    while (reader.Read())
                    {
                        kodeProdi = reader["KodeProdi"].ToString();
                    }
                    reader.Close();

                    // Jalankan perintah Update
                    string query = @"Update Mahasiswa
                set Nama = @Nama,
                    JenisKelamin = @JK,
                    TanggalLahir = @TanggalLahir,
                    Alamat = @Alamat,
                    KodeProdi = @KodeProdi
                where NIM = @NIM";

                    MySqlCommand command = new MySqlCommand(query, conn);

                    command.Parameters.AddWithValue("@NIM", txtNim.Text);
                    command.Parameters.AddWithValue("@Nama", txtNama.Text);
                    command.Parameters.AddWithValue("@Alamat", txtAlamat.Text);
                    command.Parameters.AddWithValue("@JK", cmbJK.Text);
                    command.Parameters.AddWithValue("@TanggalLahir", dptTanggalLahir.Value.Date);
                    command.Parameters.AddWithValue("@KodeProdi", kodeProdi);

                    result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Data mahasiswa berhasil diubah");
                        ClearForm();
                        btnLoad.PerformClick(); // Segarkan tampilan grid
                    }
                    else
                    {
                        MessageBox.Show("Data gagal diubah");
                    }
                }
            }
            catch (MySqlException ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("SQL Error :" + ex.Message);
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("General Error :" + ex.Message);
            }
        }

        private void cmbJK_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbJK.DataSource = new string[] { "L", "P" };

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dataGridView1.CellClick += dataGridView1_CellContentClick;
        }
       
        

        

        private void bindingSource1_CurrentChanged(object sender, EventArgs e)
        {

        }
        private void BindControls()
        {
            txtNim.DataBindings.Clear();
            txtNama.DataBindings.Clear();
            cmbJK.DataBindings.Clear();
            dptTanggalLahir.DataBindings.Clear();
            txtAlamat.DataBindings.Clear();
            txtKodeProdi.DataBindings.Clear();

            txtNim.DataBindings.Add("Text", bindingSource1, "NIM");
            txtNama.DataBindings.Add("Text", bindingSource1, "Nama");
            cmbJK.DataBindings.Add("Text", bindingSource1, "JenisKelamin");
            dptTanggalLahir.DataBindings.Add("Text", bindingSource1, "TanggalLahir");
            txtAlamat.DataBindings.Add("Text", bindingSource1, "Alamat");
            txtKodeProdi.DataBindings.Add("Text", bindingSource1, "NamaProdi");
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            try
            {
                if (conn.State == ConnectionState.Closed) conn.Open();

                // Kosongkan tabel utama
                new MySqlCommand("DELETE FROM mahasiswa", conn).ExecuteNonQuery();

                // Kembalikan data dari backup yang ada di port 3307
                string restoreQuery = "INSERT INTO mahasiswa SELECT * FROM Mahasiswa_Backup";
                new MySqlCommand(restoreQuery, conn).ExecuteNonQuery();

                MessageBox.Show("Data berhasil dikembalikan dari port 3307!");
                LoadData();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal reset: " + ex.Message);
            }
        }

        private void btnTestInjection_Click(object sender, EventArgs e)
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                new MySqlCommand("SET SQL_SAFE_UPDATES = 1;", conn).ExecuteNonQuery();

                // GANTI BARIS QUERY LU JADI KAYAK GINI, BRO:
                string query = "Update mahasiswa set nama_mhs = 'HACKED' where NIM = '" + txtNim.Text + "'";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                LoadData();
            }
            catch (MySqlException ex)
            {
                if (ex.Message.Contains("safe"))
                {
                    simpanLog(ex.Message);
                    MessageBox.Show("SQL Error : Unsafe UPDATE operation not allowed");
                }
                else
                {
                    simpanLog(ex.Message);
                    MessageBox.Show("SQL Error :" + ex.Message);
                }
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("General Error :" + ex.Message);
            }
        }

        private void dptTanggalLahir_ValueChanged(object sender, EventArgs e)
        {

        }
        public void simpanLog(string message)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            MySqlCommand cmd = new MySqlCommand("sp_LogMessage", conn);

            cmd.Parameters.AddWithValue("psn", message);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.ExecuteNonQuery();
        }
    }
}
