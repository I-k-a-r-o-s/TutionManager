using LiteDB;
using System;
using System.Windows.Forms;

namespace TutionManager
{
    public partial class frmRegistration : Form
    {
        public frmRegistration()
        {
            InitializeComponent();
        }
        private void ClearInputFields()
        {
            txtName.Clear();
            txtAddress.Clear();
            txtPhnNo.Clear();
            radbtnMale.Checked = false;
            radbtnFemale.Checked = false;           
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearInputFields();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            try
            {
                string name = txtName.Text.Trim();
                string address = txtAddress.Text.Trim();
                string gender;
                string phone = txtPhnNo.Text.Trim();

                if (string.IsNullOrEmpty(name))
                {
                    MessageBox.Show("Please enter Name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (string.IsNullOrEmpty(address))
                {
                    MessageBox.Show("Please enter address.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (string.IsNullOrEmpty(phone))
                {
                    MessageBox.Show("Please enter phone number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (radbtnMale.Checked)
                {
                    gender = "Male";
                }
                else if (radbtnFemale.Checked)
                {
                    gender = "Female";
                }
                else
                {
                    MessageBox.Show("Please select correct gender.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var student = new Student
                {
                    Name = name,
                    Address = address,
                    Gender = gender,
                    PhoneNo = phone
                };

                using (var db = new LiteDatabase(@"attendance.db"))
                {
                    var studentCollection = db.GetCollection<Student>("students");
                    studentCollection.Insert(student);
                }

                txtConfirmation.Text = $"{txtName.Text} added successfully";
                ClearInputFields();

            }
            catch (LiteException liteEx)
            {
                MessageBox.Show($"Database error: {liteEx.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnDatabase_Click(object sender, EventArgs e)
        {
            var viewForm = new frmDatabase();
            viewForm.Show();
        }
    }

    public class Student
    {
        [BsonId] public object StudentID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public string PhoneNo { get; set; }
    }
}
