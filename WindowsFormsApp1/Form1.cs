using ExcelDataReader;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
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
        DAL dbLogic = new DAL(); 



        public Form1()
        {
            InitializeComponent();
            conn = new MySqlConnection(connectionString);

        }
      

        private void LoadData()
        {
            try
            {

                bindingSource1.DataSource = dbLogic.GetMhs();
                dataGridView1.DataSource = bindingSource1;
                DataGridViewImageColumn fotoColumn = (DataGridViewImageColumn)dataGridView1.Columns["Foto"];
                fotoColumn.ImageLayout = DataGridViewImageCellLayout.Stretch;


                HitungTotal();
                foreach (DataGridViewColumn col in dataGridView1.Columns)
                {
                    Console.WriteLine("Name: " + col.Name + " | DataPropertyName: " + col.DataPropertyName);
                }
                dataGridView1.Enabled = true;
                btnImpDb.Enabled = false;
                btnInsert.Enabled = true;
                btnUpdate.Enabled = true;
                btnDelete.Enabled = true;
                btnCari.Enabled = true;
                btnLoad.Enabled = true;
                btnReset.Enabled = true;
                btnTestInjection.Enabled = true;
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
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
            txtNim.Enabled = true;
            txtNim.Clear();
            txtNama.Clear();
            cmbJK.SelectedIndex = -1;
            txtAlamat.Clear();
            txtKodeProdi.Clear();
            dptTanggalLahir.Value = DateTime.Now;
            fotoMhs.Image = null;
            txtNim.Focus();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dg = MessageBox.Show(
                    "Yakin ingin menghapus data?",
                    "Konfirmasi",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (dg == DialogResult.Yes)
                {
                    dbLogic.DeleteMhs(txtNim.Text);
                    MessageBox.Show("Data mahasiswa berhasil dihapus");
                    ClearForm();
                    btnLoad.PerformClick();
                }
            }
            catch (MySqlException ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("SQL Error : " + ex.Message);
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("General Error : " + ex.Message);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(dbLogic.GetConnectionString()))
                {
                    conn.Open();
                    MessageBox.Show("Koneksi Berhasil");
                }
            }
            catch (MySqlException ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("SQL Error : " + ex.Message);
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("General Error : " + ex.Message);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataRow row = ((DataRowView)bindingSource1[e.RowIndex]).Row;
                txtNim.Text = row[0].ToString();
                txtNama.Text = row[1].ToString();
                cmbJK.Text = row[2].ToString();
                dptTanggalLahir.Value = Convert.ToDateTime(row[3]);
                txtAlamat.Text = row[4].ToString();
                txtKodeProdi.Text = row[6].ToString();

                if (row[5] != DBNull.Value)
                {
                    byte[] imgBytes = (byte[])row[5];
                    using (MemoryStream ms = new MemoryStream(imgBytes))
                    {
                        fotoMhs.Image = Image.FromStream(ms);
                        fotoMhs.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                }
                else
                {
                    fotoMhs.Image = null;
                }

                txtNim.Enabled = false;
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
           LoadData();  
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
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

                byte[] ConvertImageToBytes(PictureBox pb)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        pb.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        return ms.ToArray();
                    }
                }

                byte[] imgBytes = ConvertImageToBytes(fotoMhs);
                dbLogic.InsertMhs(txtNim.Text, txtNama.Text, txtAlamat.Text, cmbJK.Text, dptTanggalLahir.Value.Date, txtKodeProdi.Text, imgBytes);
                MessageBox.Show("Data mahasiswa berhasil ditambahkan");
                ClearForm();
                LoadData();

            }
            catch (MySqlException ex)
            {
                simpanLog("Rollback Insert : " + ex.Message);
                MessageBox.Show("SQL Error : " + ex.Message);
            }
            catch (Exception ex)
            {
                simpanLog("General Error : " + ex.Message);
                MessageBox.Show("General Error : " + ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] ConvertImageToBytes(PictureBox pb)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        pb.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        return ms.ToArray();
                    }
                }
                byte[] imgBytes = ConvertImageToBytes(fotoMhs);
                dbLogic.UpdateMhs(txtNim.Text, txtNama.Text, txtAlamat.Text, cmbJK.Text, dptTanggalLahir.Value.Date, txtKodeProdi.Text, imgBytes);
                MessageBox.Show("Data mahasiswa berhasil diubah");
                ClearForm();
                btnLoad.PerformClick();

            }
            catch (MySqlException ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("SQL Error : " + ex.Message);
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("General Error : " + ex.Message);
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
                dbLogic.resetData();
                MessageBox.Show("Data berhasil direset");
                LoadData();
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

        private void btnTestInjection_Click(object sender, EventArgs e)
        {
            try
            {
                dbLogic.testInject(txtNim.Text);

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
            dbLogic.InsertLog(message);
        }

        private void btnRekap_Click(object sender, EventArgs e)
        {
            Form2 fm2 = new Form2(txtKodeProdi.Text, dptTanggalLahir.Value);
            fm2.Show();
            this.Hide();
        }
        private void HitungTotal()
        {
            try
            {
                int total = (dbLogic.CountMhs().Equals(DBNull.Value)) ? 0 : dbLogic.CountMhs();

                lblCountMhs.Text = "Total Mahasiswa : " + total;

            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }

        private void btnCari_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = dbLogic.GetMhsByNIM(txtNim.Text);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    txtNim.Text = row[0].ToString();
                    txtNama.Text = row[1].ToString();
                    cmbJK.Text = row[2].ToString();
                    dptTanggalLahir.Value = Convert.ToDateTime(row[3]);
                    txtAlamat.Text = row[4].ToString();
                    txtKodeProdi.Text = row[6].ToString();

                    if (row["Foto"] != DBNull.Value)
                    {
                        byte[] imgBytes = (byte[])row["Foto"];
                        using (MemoryStream ms = new MemoryStream(imgBytes))
                        {
                            fotoMhs.Image = Image.FromStream(ms);
                            fotoMhs.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                    }
                    else
                    {
                        fotoMhs.Image = null;
                    }
                }
                bindingSource1.DataSource = dbLogic.GetMhsByNIM(txtNim.Text);
                dataGridView1.DataSource = bindingSource1;
                dataGridView1.Columns["Foto"].Visible = false;

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

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                fotoMhs.Image = Image.FromFile(ofd.FileName);
                fotoMhs.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }

        private void btnImpExcel_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Excel Workbook| *.xlsx" })
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                            {
                                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                                {
                                    UseHeaderRow = true
                                }
                            });
                            DataTable dt = result.Tables[0];
                            dataGridView1.DataSource = dt;
                            dataGridView1.Enabled = false;

                            btnImpDb.Enabled = true;
                            btnInsert.Enabled = false;
                            btnUpdate.Enabled = false;
                            btnDelete.Enabled = false;
                            btnCari.Enabled = false;
                            btnLoad.Enabled = false;
                            btnReset.Enabled = false;
                            btnTestInjection.Enabled = false;
                        }
                    }
                } 
            }
        }

        private void btnImpDb_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = (DataTable)dataGridView1.DataSource;

                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk diimport.");
                    return;
                }

                int sukses = 0;

                foreach (DataRow row in dt.Rows)
                {
                    string nim = row["NIM"].ToString().Trim();
                    string nama = row["Nama"].ToString().Trim();
                    string jk = row["JenisKelamin"].ToString().Trim();
                    string alamat = row["Alamat"].ToString().Trim();
                    string kodeProdi = row["NamaProdi"].ToString().Trim();
                    string fotoPath = row.Table.Columns.Contains("FotoPath")
                        ? row["FotoPath"].ToString().Trim()
                        : string.Empty;

                    if (string.IsNullOrEmpty(nim) || string.IsNullOrEmpty(nama))
                        continue;

                    DateTime tglLahir;
                    if (!DateTime.TryParse(row["TanggalLahir"].ToString(), out tglLahir))
                        continue;
                    byte[] ConvertImageFromPath(string path)
                    {
                        if (string.IsNullOrWhiteSpace(path))
                            return null;

                        if (!File.Exists(path))
                            return null;

                        return File.ReadAllBytes(path);
                    }
                    byte[] fotoBytes = ConvertImageFromPath(fotoPath);

                    dbLogic.InsertMhs(nim, nama, alamat, jk, tglLahir, kodeProdi, fotoBytes);

                    sukses++;
                }

                MessageBox.Show("Data mahasiswa berhasil ditambahkan");
                ClearForm();
                LoadData();
            }
            catch (MySqlException ex)
            {
                simpanLog("Rollback Insert :" + ex.Message);
                MessageBox.Show("SQL Error :" + ex.Message);
            }
            catch (Exception ex)
            {
                simpanLog("General Error :" + ex.Message);
                MessageBox.Show("General Error :" + ex.Message);
            } 
        }
    }
}
