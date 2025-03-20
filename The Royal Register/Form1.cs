using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Drawing;
using System.Xml.Linq;
using PdfFont = iTextSharp.text.Font;
using DrawingFont = System.Drawing.Font;
using PdfImage = iTextSharp.text.Image;

namespace The_Royal_Register
{
    public partial class Form1 : Form
    {
        private string connectionString = @"Server=localhost\SQLEXPRESS03;Database=RoyalRegister;Trusted_Connection=True;Encrypt=False";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadGuestList();
        }

        // Check/Add Guest Function and Validation
        private void btnCheck_Click(object sender, EventArgs e)
        {
            string guestName = txtGuestName.Text.Trim();
            string guestIdNumber = txtGuestId.Text.Trim();
            string contactNumber = txtContactNumber.Text.Trim();

            if (string.IsNullOrWhiteSpace(guestName) || string.IsNullOrWhiteSpace(guestIdNumber) || string.IsNullOrWhiteSpace(contactNumber))
            {
                MessageBox.Show("Please enter all required fields (Guest Name, ID, and Contact Number).");
                return;
            }

            // Validate Contact Number (Allow numbers starting with 0)
            if (!System.Text.RegularExpressions.Regex.IsMatch(contactNumber, @"^\d+$"))
            {
                MessageBox.Show("Please enter a valid contact number.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Check if the guest already exists by IdentityNumber
                string query = "SELECT COUNT(*) FROM Guests WHERE IdentityNumber = @IdentityNumber";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdentityNumber", guestIdNumber);
                    int count = (int)cmd.ExecuteScalar();

                    if (count > 0)
                    {
                        lblResult.Text = "A guest with this ID already exists in The Royal Register!";
                    }
                    else
                    {
                        lblResult.Text = "The guest has been added to The Royal Register!";
                        AddGuestToDatabase(guestName, guestIdNumber, contactNumber);
                    }
                }
            }
        }

        // Method to Add Guest (Prevents ID duplication)
        private void AddGuestToDatabase(string guestName, string guestIdNumber, string contactNumber)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Ensure that the textbox data is inserted into the [dbo].[Guests] table on SSMS
                string query = "INSERT INTO Guests (IdentityNumber, FullName, ContactNumber) VALUES (@IdentityNumber, @FullName, @ContactNumber)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdentityNumber", guestIdNumber);
                    cmd.Parameters.AddWithValue("@FullName", guestName);
                    cmd.Parameters.AddWithValue("@ContactNumber", contactNumber);
                    cmd.ExecuteNonQuery();
                }
            }

            LoadGuestList();
        }

        // Button to Remove Guest by ID
        private void btnRemove_Click(object sender, EventArgs e)
        {
            string guestIdNumber = txtGuestId.Text.Trim();

            if (string.IsNullOrWhiteSpace(guestIdNumber))
            {
                MessageBox.Show("Please enter an ID number to remove a guest.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Check if guest exists before attempting deletion
                string checkQuery = "SELECT COUNT(*) FROM Guests WHERE IdentityNumber = @IdentityNumber";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@IdentityNumber", guestIdNumber);
                    int count = (int)checkCmd.ExecuteScalar();

                    if (count == 0)
                    {
                        MessageBox.Show("No guest found with this ID.");
                        return;
                    }
                }

                // Delete the guest
                string deleteQuery = "DELETE FROM Guests WHERE IdentityNumber = @IdentityNumber";
                using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn))
                {
                    deleteCmd.Parameters.AddWithValue("@IdentityNumber", guestIdNumber);
                    int rowsAffected = deleteCmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Guest has been removed from The Royal Register.");
                        LoadGuestList(); // Refresh the guest list
                    }
                    else
                    {
                        MessageBox.Show("Error: Guest could not be removed.");
                    }
                }
            }
        }

        // Load the Guest List into DataGridView
        private void LoadGuestList()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT IdentityNumber, FullName, ContactNumber FROM Guests ORDER BY UserID ASC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    dataGridViewGuests.DataSource = dt;
                }
            }
        }

        // Export Guest List to PDF
        private void ExportGuestListToPdf(string filePath)
        {
            Document doc = new Document();
            try
            {
                PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                doc.Open();

                // Add logo image to PDF
                string logoPath = @"/Users/LavNa/Downloads/RoyalReg.jpg";  // Should work on macOS or Linux                                                                           // Replace with the actual path to your logo
                if (File.Exists(logoPath))
                {
                    PdfImage logo = PdfImage.GetInstance(logoPath);
                    logo.ScaleToFit(100f, 100f); // Resize logo (optional)
                    logo.Alignment = PdfImage.ALIGN_CENTER;  // Center the logo
                    doc.Add(logo); // Add the logo to the document
                }

                // Using PdfFont
                PdfFont titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
                Paragraph title = new Paragraph("The Royal Register - Guest List\n\n", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                doc.Add(title);

                // Table Setup
                PdfPTable table = new PdfPTable(3);
                table.WidthPercentage = 100;
                //table.SetWidths(new float[] {1, 2, 3});
                table.SetWidths(new float[] { 2, 3, 3 });

                // Add Headers
                PdfFont headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
                BaseColor headerColor = new BaseColor(65, 105, 225); // Royal Blue
                PdfPCell cell;

                cell = new PdfPCell(new Phrase("ID Number", headerFont)) { BackgroundColor = headerColor };
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Full Name", headerFont)) { BackgroundColor = headerColor };
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Contact Number", headerFont)) { BackgroundColor = headerColor };
                table.AddCell(cell);

                // Fetch Guest Data
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT IdentityNumber, FullName, ContactNumber FROM Guests ORDER BY UserID ASC";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //table.AddCell(reader["UserID"].ToString());
                                //table.AddCell(reader["IdentityNumber"].ToString());
                                //table.AddCell(reader["FullName"].ToString());
                                //table.AddCell(reader["ContactNumber"].ToString());

                                //Replaced code above due to error handling for NULL values

                                table.AddCell(reader["IdentityNumber"] != DBNull.Value ? reader["IdentityNumber"].ToString() : "N/A");
                                table.AddCell(reader["FullName"] != DBNull.Value ? reader["FullName"].ToString() : "N/A");
                                table.AddCell(reader["ContactNumber"] != DBNull.Value ? reader["ContactNumber"].ToString() : "N/A");

                            }
                        }
                    }
                }

                //Table Added
                doc.Add(table);
                doc.Close();

                MessageBox.Show("Guest list exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting PDF: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Export Button Functionality
        private void btnExportPdf_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "PDF Files|*.pdf";
            saveDialog.Title = "Save Guest List as PDF";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                ExportGuestListToPdf(saveDialog.FileName);
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            txtGuestName.Text = "";
            txtGuestId.Text = "";
            txtContactNumber.Text = "";
        }
    }
}
