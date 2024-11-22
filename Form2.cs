using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TutionManager
{
    public partial class frmDatabase : Form
    {
        public frmDatabase()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            try
            {
                LoadStudentData();
                dataGridView1.Columns["StudentID"].ReadOnly = true;

                if (dataGridView1.Columns["Gender"] != null)
                {
                    dataGridView1.Columns.Remove("Gender");
                    DataGridViewComboBoxColumn genderComboBoxColumn = new DataGridViewComboBoxColumn
                    {
                        Name = "Gender",
                        HeaderText = "Gender",
                        DataPropertyName = "Gender",
                        Items = { "Male", "Female" },
                        ValueType = typeof(string),
                        DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox
                    };
                    dataGridView1.Columns.Insert(2, genderComboBoxColumn);
                }
            }
            catch (LiteException liteEx)
            {
                MessageBox.Show($"Database error while loading data: {liteEx.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStudentData()
        {
            try
            {
                using (var db = new LiteDatabase(@"attendance.db"))
                {
                    var studentCollection = db.GetCollection<Student>("students");
                    var studentList = studentCollection.FindAll().ToList();
                    dataGridView1.DataSource = new BindingSource { DataSource = studentList };
                    dataGridView1.AutoResizeColumns();
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }
            catch (LiteException liteEx)
            {
                MessageBox.Show($"Database error while fetching students: {liteEx.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while fetching student data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                using (var db = new LiteDatabase(@"attendance.db"))
                {
                    var studentCollection = db.GetCollection<Student>("students");

                    if (!studentCollection.Exists(Query.All()))
                    {
                        MessageBox.Show("No records in the database.", "No Records", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.IsNewRow) continue;

                        var idCell = row.Cells["StudentID"].Value;
                        if (idCell == null)
                        {
                            MessageBox.Show("Student ID is missing for a row. Cannot update.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            continue;
                        }

                        var student = studentCollection.FindById(idCell as ObjectId);
                        if (student != null)
                        {
                            student.Name = row.Cells["Name"].Value?.ToString() ?? student.Name;
                            student.Address = row.Cells["Address"].Value?.ToString() ?? student.Address;
                            student.Gender = row.Cells["Gender"].Value?.ToString() ?? student.Gender;
                            student.PhoneNo = row.Cells["PhoneNo"].Value?.ToString() ?? student.PhoneNo;

                            studentCollection.Update(student);
                        }
                        else
                        {
                            MessageBox.Show($"Student with ID {idCell} not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }

                MessageBox.Show("Changes saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (LiteException liteEx)
            {
                MessageBox.Show($"Database error while saving data: {liteEx.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteRecord_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select a row to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var studentIdsToDelete = new List<ObjectId>();
                foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                {
                    if (row.IsNewRow) continue;
                    studentIdsToDelete.Add((ObjectId)row.Cells["StudentID"].Value);
                }

                if (studentIdsToDelete.Count == 0)
                {
                    MessageBox.Show("No valid rows selected.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var db = new LiteDatabase(@"attendance.db"))
                {
                    var studentCollection = db.GetCollection<Student>("students");

                    if (!studentCollection.Exists(Query.All()))
                    {
                        MessageBox.Show("No records in the database to delete.", "No Records", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    bool deletionOccurred = false;

                    foreach (var studentId in studentIdsToDelete)
                    {
                        if (studentCollection.Delete(studentId))
                        {
                            deletionOccurred = true;
                            foreach (DataGridViewRow row in dataGridView1.Rows)
                            {
                                if (row.IsNewRow) continue;
                                if (studentId.Equals(row.Cells["StudentID"].Value))
                                {
                                    dataGridView1.Rows.Remove(row);
                                    break;
                                }
                            }
                        }
                    }

                    if (deletionOccurred)
                    {
                        MessageBox.Show("Selected records deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("No records were deleted. Please check if the records exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (LiteException liteEx)
            {
                MessageBox.Show($"Database error while deleting data: {liteEx.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while deleting data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
