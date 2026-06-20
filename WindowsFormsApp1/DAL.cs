using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WindowsFormsApp1
{
   

    internal class DAL
    {
        static string connectionString = string.Format("Server=localhost;Port=3307;Database=DBAkademi1;Uid=root;Pwd=admin123;");

        public string GetConnectionString()
        {
            return connectionString;
        }
        //object untuk merepresentasikan konkesi ke MySQL Server
        MySqlConnection conn = new MySqlConnection(connectionString);
        MySqlDataAdapter da;
        DataTable dtMahasiswa;
        DataTable dtProdi; 


        public int CountMhs()
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            MySqlCommand cmd = new MySqlCommand("sp_CountMahasiswa", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            // ✅ HARUS SAMA DENGAN SP
            MySqlParameter outputParam = new MySqlParameter("p_Total", MySqlDbType.Int32);
            outputParam.Direction = ParameterDirection.Output;

            cmd.Parameters.Add(outputParam);

            cmd.ExecuteNonQuery();

            return Convert.ToInt32(cmd.Parameters["p_Total"].Value);
        } 
     

        public DataTable GetMhs()
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            MySqlCommand cmd = new MySqlCommand("sp_GetMahasiswa", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            da = new MySqlDataAdapter(cmd);

            dtMahasiswa = new DataTable();
            da.Fill(dtMahasiswa);

            return dtMahasiswa; 

        }

        public void InsertMhs(string nim, string nama, string alamat, string jenisKelamin, DateTime tanggalLahir, string kodeProdi, byte[] foto)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            MySqlTransaction trans = conn.BeginTransaction();
            try
            {
                MySqlCommand command = new MySqlCommand("sp_InsertMahasiswa", conn);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("pNIM", nim);
                command.Parameters.AddWithValue("pNama", nama);
                command.Parameters.AddWithValue("pAlamat", alamat);
                command.Parameters.AddWithValue("pTanggalLahir", tanggalLahir);
                command.Parameters.AddWithValue("pJenisKelamin", jenisKelamin);
                command.Parameters.AddWithValue("pNmProdi", kodeProdi);
                command.Parameters.AddWithValue("pFoto", foto);

                command.ExecuteNonQuery();
                trans.Commit();
            }
            catch (Exception ex)
            {
                trans.Rollback();
                MessageBox.Show("Gagal insert data: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        } 

        public void UpdateMhs(string nim, string nama, string alamat, string jenisKelamin, DateTime tanggalLahir, string kodeProdi, byte[] foto)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            MySqlCommand command = new MySqlCommand("sp_UpdateMahasiswa", conn);

            command.Parameters.AddWithValue("pNIM", nim);
            command.Parameters.AddWithValue("pNama", nama);
            command.Parameters.AddWithValue("pAlamat", alamat);
            command.Parameters.AddWithValue("pJenisKelamin", jenisKelamin);
            command.Parameters.AddWithValue("pTanggalLahir", tanggalLahir);
            command.Parameters.AddWithValue("pNmProdi", kodeProdi);
            command.Parameters.AddWithValue("pFoto", foto);

            command.CommandType = CommandType.StoredProcedure;

            command.ExecuteNonQuery();
        }

        public void DeleteMhs(string nim)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            MySqlCommand cmd = new MySqlCommand("sp_DeleteMahasiswa", conn);
            cmd.Parameters.AddWithValue("pNIM", nim);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.ExecuteNonQuery();
        }

        public void resetData()
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            string deleteQuery = "DELETE FROM mahasiswa;";
            MySqlCommand cmdDelete = new MySqlCommand(deleteQuery, conn);
            cmdDelete.ExecuteNonQuery();

            string insertQuery = @"
        INSERT INTO mahasiswa
        SELECT * FROM mahasiswa_backup;";
            MySqlCommand cmdInsert = new MySqlCommand(insertQuery, conn);
            cmdInsert.ExecuteNonQuery();
        }

        public void testInject(string nim)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            new MySqlCommand("SET SQL_SAFE_UPDATES = 1;", conn).ExecuteNonQuery();

            string query = "Update mahasiswa set nama = 'HACKED' where NIM = " + nim;
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.ExecuteNonQuery();
        }

        public DataTable GetMhsByNIM(string nim)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            MySqlCommand cmd = new MySqlCommand("sp_GetMahasiswaByNIM", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("pNIM", nim);
            da = new MySqlDataAdapter(cmd);

            dtMahasiswa = new DataTable();
            da.Fill(dtMahasiswa);

            return dtMahasiswa;
        }

        public void InsertLog(string message)
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


        public DataTable getProdi()
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

            return dtProdi;
        }
        public DataTable getDataRekap(string prodi, DateTime tanggalMasuk)
        {
            MySqlCommand cmd = new MySqlCommand("sp_DashBoardByTahun", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("inTglMsuk", tanggalMasuk.ToString("yyyy"));

            dtMahasiswa = new DataTable();

            if (conn.State == ConnectionState.Closed)
                conn.Open();

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                dtMahasiswa.Load(reader);
            }

            return dtMahasiswa;
        }


        public DataTable getAllDataChart()
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            MySqlCommand cmd = new MySqlCommand("sp_DashBoard", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            da = new MySqlDataAdapter(cmd);
            dtMahasiswa = new DataTable();
            da.Fill(dtMahasiswa);
            return dtMahasiswa;
        }
        public DataTable getDataChartByTahun(DateTime thMasuk)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            MySqlCommand cmd = new MySqlCommand("sp_DashBoardByTahun", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@inTglMsuk", thMasuk.Year);
            da = new MySqlDataAdapter(cmd);
            dtMahasiswa = new DataTable();
            da.Fill(dtMahasiswa);
            return dtMahasiswa;
        }
    }

}
