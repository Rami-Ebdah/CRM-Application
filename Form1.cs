using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace CRMApp
{
    public partial class Form1 : Form
    {
        private static string connectionString = "Server=DESKTOP-MJ8849A\\MSSQLSERVER1;Database=TEST_ODROS;User Id=sa;Password=123456;";
        private bool isCallDateChanged = false;

        public Form1()
        {
            InitializeComponent();

            dtpCallDate.ValueChanged += dtpCallDate_ValueChanged;
            dgvPhones.CellContentClick += dgvPhones_CellContentClick;
            dgvPhones.SelectionChanged += dgvPhones_SelectionChanged;

            StyleForm();
        }

        private void StyleForm()
        {
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10);

            dgvPhones.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgvPhones.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvPhones.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
            dgvPhones.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvPhones.EnableHeadersVisualStyles = false;
            dgvPhones.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 152, 219);
            dgvPhones.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPhones.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            btnLoad.BackColor = Color.FromArgb(52, 152, 219);
            btnLoad.ForeColor = Color.White;
            btnLoad.FlatStyle = FlatStyle.Flat;
            btnLoad.FlatAppearance.BorderSize = 0;

            btnUpdateNote.BackColor = Color.FromArgb(46, 204, 113);
            btnUpdateNote.ForeColor = Color.White;
            btnUpdateNote.FlatStyle = FlatStyle.Flat;
            btnUpdateNote.FlatAppearance.BorderSize = 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            isCallDateChanged = false;
            LoadData();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            string query = "SELECT Id, FullName, PhoneNumber,Location,Sub_Location, Notes, Answered, CallDate FROM Table_1";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvPhones.DataSource = dt;

                   
                    if (!dgvPhones.Columns.Contains("CallButton"))
                    {
                        AddCallButtonColumn();
                        AddMessageColumns();
                    }

                    DisplayStats(dt);
                    CheckTodayCalls();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private void AddCallButtonColumn()
        {
          
            DataGridViewButtonColumn callBtnCol = new DataGridViewButtonColumn();
            callBtnCol.HeaderText = "Call";
            callBtnCol.Name = "CallButton";
            callBtnCol.Text = "Call";
            callBtnCol.UseColumnTextForButtonValue = true;
            callBtnCol.DefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
            callBtnCol.DefaultCellStyle.ForeColor = Color.White;
            callBtnCol.FlatStyle = FlatStyle.Flat;
            dgvPhones.Columns.Add(callBtnCol);

        }


        private void AddMessageColumns()
        {

            DataGridViewButtonColumn MessageBtnCol = new DataGridViewButtonColumn();
            MessageBtnCol.HeaderText = "Message";
            MessageBtnCol.Name = "MessageButton";
            MessageBtnCol.Text = "Message";
            MessageBtnCol.UseColumnTextForButtonValue = true;
            MessageBtnCol.DefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
            MessageBtnCol.DefaultCellStyle.ForeColor = Color.White;
            MessageBtnCol.FlatStyle = FlatStyle.Flat;
            dgvPhones.Columns.Add(MessageBtnCol);

        }

        private void CheckTodayCalls()
        {
            DateTime today = DateTime.Today;

            foreach (DataGridViewRow row in dgvPhones.Rows)
            {
                if (row.IsNewRow) continue; 

                if (row.Cells["CallDate"].Value != null && DateTime.TryParse(row.Cells["CallDate"].Value.ToString(), out DateTime callDate))
                {
                    if (callDate.Date == today)
                    {
                        row.DefaultCellStyle.BackColor = Color.Yellow;

                        
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            if (cell is DataGridViewButtonCell buttonCell)
                            {
                                buttonCell.Style.BackColor = Color.DodgerBlue;
                                buttonCell.Style.ForeColor = Color.White;
                            }
                        }
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.White;

                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            if (cell is DataGridViewButtonCell buttonCell)
                            {
                                buttonCell.Style.BackColor = Color.DodgerBlue;
                                buttonCell.Style.ForeColor = Color.White;
                            }
                        }
                    }
                }
            }
        }




        private void DisplayStats(DataTable dt)
        {
            int total = dt.Rows.Count;
            int answered = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (row["Answered"].ToString() == "Yes")
                    answered++;
            }
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            CheckTodayCalls();
        }

        private void dgvPhones_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPhones.CurrentRow != null)
            {
                txtNotes.Text = dgvPhones.CurrentRow.Cells["Notes"].Value?.ToString();
                string answeredValue = dgvPhones.CurrentRow.Cells["Answered"].Value?.ToString();

                rdoYes.Checked = answeredValue == "Yes";
                rdoNo.Checked = answeredValue == "No";

                if (dgvPhones.CurrentRow.Cells["CallDate"].Value != DBNull.Value)
                    dtpCallDate.Value = Convert.ToDateTime(dgvPhones.CurrentRow.Cells["CallDate"].Value);
                else
                    dtpCallDate.Value = DateTime.Today;

                isCallDateChanged = false;
            }
        }

        private void dtpCallDate_ValueChanged(object sender, EventArgs e)
        {
            isCallDateChanged = true;
        }

        private void dgvPhones_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (dgvPhones.Columns[e.ColumnIndex].Name == "CallButton")
                {
                    string phoneNumber = dgvPhones.Rows[e.RowIndex].Cells["PhoneNumber"].Value?.ToString();

                    if (!string.IsNullOrWhiteSpace(phoneNumber))
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo()
                            {
                                FileName = "dialer.exe",
                                Arguments = $"/d {phoneNumber}",
                                UseShellExecute = true
                            });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Could not initiate call: " + ex.Message);
                        }
                    }
                }
                else if (dgvPhones.Columns[e.ColumnIndex].Name == "MessageButton")
                {
                    string phoneNumber = dgvPhones.Rows[e.RowIndex].Cells["PhoneNumber"].Value?.ToString();

                    if (!string.IsNullOrWhiteSpace(phoneNumber))
                    {
                        
                        
                        if (phoneNumber.StartsWith("0"))
                        {
                            phoneNumber = "962" + phoneNumber.Substring(1); 
                        }

                        string text = Uri.EscapeDataString("  "); 
                        string url = $"https://wa.me/{phoneNumber}?text={text}";

                        try
                        {
                            Process.Start(new ProcessStartInfo()
                            {
                                FileName = url,
                                UseShellExecute = true
                            });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Could not open WhatsApp: " + ex.Message);
                        }
                    }
                }
            }
        }




        private void btnUpdateNote_Click(object sender, EventArgs e)
        {
            if (dgvPhones.CurrentRow != null)
            {
                int id = Convert.ToInt32(dgvPhones.CurrentRow.Cells["Id"].Value);
                List<string> updateFields = new List<string>();
                Dictionary<string, object> parameters = new Dictionary<string, object>();

                string oldNote = dgvPhones.CurrentRow.Cells["Notes"].Value?.ToString() ?? "";
                string newNote = txtNotes.Text;
                if (oldNote != newNote)
                {
                    updateFields.Add("Notes = @Notes");
                    parameters["@Notes"] = newNote;
                }

                string newAnswered = rdoYes.Checked ? "Yes" : rdoNo.Checked ? "No" : null;
                string oldAnswered = dgvPhones.CurrentRow.Cells["Answered"].Value?.ToString();
                if (newAnswered != oldAnswered)
                {
                    updateFields.Add("Answered = @Answered");
                    parameters["@Answered"] = (object)newAnswered ?? DBNull.Value;
                }

                updateFields.Add("CallDate = @CallDate");
                parameters["@CallDate"] = dtpCallDate.Value.Date;

                if (updateFields.Count == 0)
                {
                    MessageBox.Show("No changes to update.");
                    return;
                }

                DialogResult result = MessageBox.Show("Are you sure you want to update this record?", "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes) return;

                string updateQuery = $"UPDATE Table_1 SET {string.Join(", ", updateFields)} WHERE Id = @Id";
                parameters["@Id"] = id;

                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                        {
                            foreach (var param in parameters)
                                cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Record updated successfully!");
                                LoadData();
                                isCallDateChanged = false;
                            }
                            else
                            {
                                MessageBox.Show("Update failed.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating record: " + ex.Message);
                }
            }
        }

        private void btnFilterr_Click(object sender, EventArgs e)
        {
            DateTime startDate = dtpStartDatee.Value.Date;
            DateTime endDate = dtpEndDatee.Value.Date;
            string location = CityCom.SelectedItem?.ToString();

          
            string query = "SELECT Id, FullName, PhoneNumber, Location,Sub_Location, Notes, Answered, CallDate " +
                           "FROM Table_1 " +
                           "WHERE Answered = 'Yes' AND CallDate >= @StartDate AND CallDate <= @EndDate AND Location = @Location";


            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    cmd.Parameters.AddWithValue("@Location", location);
                   
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvPhones.DataSource = dt;

                    DisplayStats(dt);
                    CheckTodayCalls();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error filtering data: " + ex.Message);
            }
        }


    }
}