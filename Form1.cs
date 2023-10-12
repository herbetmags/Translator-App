using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using TranslatorApp.Models;

namespace TranslatorApp
{
    public class Form1 : Form
	{
		private readonly FormModel _model;

		private readonly List<SelectableItem> _sourceLanguages;

		private readonly List<SelectableItem> _targetLanguages;

		private readonly List<SelectableItem> _fileTypes;

		private IContainer components;

		private OpenFileDialog ofdBrowse;

		private ComboBox cbxSourceLang;

		private ComboBox cbxTargetLang;

		private Label lblSourceLang;

		private Label lblTargetLang;

		private TextBox tbxFilePath;

		private Button btnBrowse;

		private Button btnExtract;

		private ComboBox cbxFileType;

		private Label lblFileType;

		private ProgressBar pbrStatus;

		private Label lblStatus;

		private BindingSource selectableItemBindingSource;

		private Button btnTranslate;

		private DataGridView dgvExcelData;

		private Label lblTotalRecords;

		public Form1()
		{
			Configurations configurations = Program.GetConfigurations();
			this._model = new FormModel();
			this._sourceLanguages = new List<SelectableItem>(
				from x in configurations.Languages
				where x.Enabled
				select x);
			this._targetLanguages = new List<SelectableItem>(
				from x in configurations.Languages
				where x.Enabled
				select x);
			if (this._targetLanguages.Count > 0)
			{
				this._targetLanguages.RemoveAt(0);
			}
			this._fileTypes = new List<SelectableItem>(
				from x in configurations.FileTypes
				where x.Enabled
				select x);
			this.InitializeComponent();
			this.InitializeComponentCustom();
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			this.ofdBrowse.Filter = this._model.FileType;
			this.ofdBrowse.Multiselect = false;
			this.tbxFilePath.Text = (this.ofdBrowse.ShowDialog() == System.Windows.Forms.DialogResult.OK ? this.ofdBrowse.FileName : string.Empty);
		}

		private void btnExtract_Click(object sender, EventArgs e)
		{
			bool flag = true;
			this.UpdateCursor(flag);
			this.ToggleControlStatus(flag);
			bool flag1 = Program.Extract(this._model);
			this.ToggleControlStatus(!flag);
			this.lblTotalRecords.Text = this.GetTotalRecordsText(Program.GetTranslations().Count);
			this.btnTranslate.Enabled = flag1;
			this.UpdateCursor(!flag);
		}

		private void btnTranslate_Click(object sender, EventArgs e)
		{
			bool flag = true;
			this.btnTranslate.Enabled = flag;
			this.UpdateCursor(flag);
			XmlDocument file = Program.GetFile<XmlDocument>(this._model.FilePath);
			System.Windows.Forms.DialogResult dialogResult = this.Translate(file, Program.GetTranslations());
			if (dialogResult != System.Windows.Forms.DialogResult.Cancel)
			{
				if (dialogResult == System.Windows.Forms.DialogResult.None)
				{
					Program.SaveFile(file, this._model.FilePath);
					dialogResult = MessageBox.Show(this, "Translation resource has been translated successfully.\nDo you need more time?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				}
				if (dialogResult == System.Windows.Forms.DialogResult.No)
				{
					base.Close();
				}
			}
			else
			{
				base.Close();
			}
			this.ToggleControlStatus(!flag);
			this.lblTotalRecords.Text = this.GetTotalRecordsText(0);
			this.btnTranslate.Enabled = !flag;
			this.UpdateCursor(!flag);
		}

		private int CalculateProgress(int sum, int total)
		{
			int num = (int)(((decimal)sum / (decimal)total) * this.pbrStatus.Maximum);
			if (num <= this.pbrStatus.Maximum)
			{
				return num;
			}
			return this.pbrStatus.Maximum;
		}

		private void cbxFileType_SelectedIndexChanged(object sender, EventArgs e)
		{
			string name;
			string code;
			SelectableItem selectedValue = (SelectableItem)this.cbxFileType.SelectedValue;
			FormModel formModel = this._model;
			if (selectedValue != null)
			{
				name = selectedValue.Name;
			}
			else
			{
				name = null;
			}
			if (selectedValue != null)
			{
				code = selectedValue.Code;
			}
			else
			{
				code = null;
			}
			formModel.FileType = string.Concat(name, " | ", code);
			this.lblStatus.Text = this.GetStatusText(0);
		}

		private void cbxSourceLang_DataSourceChanged(object sender, EventArgs e)
		{
			this.cbxSourceLang.SelectedIndex = 0;
		}

		private void cbxSourceLang_SelectedIndexChanged(object sender, EventArgs e)
		{
			string code;
			FormModel formModel = this._model;
			SelectableItem selectedValue = (SelectableItem)this.cbxSourceLang.SelectedValue;
			if (selectedValue != null)
			{
				code = selectedValue.Code;
			}
			else
			{
				code = null;
			}
			formModel.SourceLang = code;
			this.lblStatus.Text = this.GetStatusText(0);
		}

		private void cbxTargetLang_DataSourceChanged(object sender, EventArgs e)
		{
			this.cbxTargetLang.SelectedIndex = 0;
		}

		private void cbxTargetLang_SelectedIndexChanged(object sender, EventArgs e)
		{
			string code;
			FormModel formModel = this._model;
			SelectableItem selectedValue = (SelectableItem)this.cbxTargetLang.SelectedValue;
			if (selectedValue != null)
			{
				code = selectedValue.Code;
			}
			else
			{
				code = null;
			}
			formModel.TargetLang = code;
			this.lblStatus.Text = this.GetStatusText(0);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (MessageBox.Show(this, "Do you really want to exit the application?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No)
			{
				e.Cancel = true;
			}
		}

		private string GetStatusText(int percentage)
		{
			return string.Format("{0} % completed", percentage);
		}

		private string GetTitleAndVersion()
		{
			Version version = Program.GetAssemblyName().Version;
			return string.Format("{0} v{1}.{2}", this.Text, version.Major, version.Minor);
		}

		private string GetTotalRecordsText(int totalRecords)
		{
			return string.Format("Total records: {0}", totalRecords);
		}

		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.ofdBrowse = new System.Windows.Forms.OpenFileDialog();
            this.cbxSourceLang = new System.Windows.Forms.ComboBox();
            this.selectableItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.cbxTargetLang = new System.Windows.Forms.ComboBox();
            this.lblSourceLang = new System.Windows.Forms.Label();
            this.lblTargetLang = new System.Windows.Forms.Label();
            this.tbxFilePath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnExtract = new System.Windows.Forms.Button();
            this.cbxFileType = new System.Windows.Forms.ComboBox();
            this.lblFileType = new System.Windows.Forms.Label();
            this.pbrStatus = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnTranslate = new System.Windows.Forms.Button();
            this.dgvExcelData = new System.Windows.Forms.DataGridView();
            this.lblTotalRecords = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.selectableItemBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvExcelData)).BeginInit();
            this.SuspendLayout();
            // 
            // cbxSourceLang
            // 
            this.cbxSourceLang.DataSource = this.selectableItemBindingSource;
            this.cbxSourceLang.DisplayMember = "Name";
            this.cbxSourceLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxSourceLang.FormattingEnabled = true;
            this.cbxSourceLang.Location = new System.Drawing.Point(29, 49);
            this.cbxSourceLang.Name = "cbxSourceLang";
            this.cbxSourceLang.Size = new System.Drawing.Size(121, 24);
            this.cbxSourceLang.TabIndex = 0;
            this.cbxSourceLang.SelectedIndexChanged += new System.EventHandler(this.cbxSourceLang_SelectedIndexChanged);
            this.cbxSourceLang.DataSourceChanged += new System.EventHandler(this.cbxSourceLang_DataSourceChanged);
            // 
            // cbxTargetLang
            // 
            this.cbxTargetLang.DataSource = this.selectableItemBindingSource;
            this.cbxTargetLang.DisplayMember = "Name";
            this.cbxTargetLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxTargetLang.FormattingEnabled = true;
            this.cbxTargetLang.Location = new System.Drawing.Point(219, 49);
            this.cbxTargetLang.Name = "cbxTargetLang";
            this.cbxTargetLang.Size = new System.Drawing.Size(121, 24);
            this.cbxTargetLang.TabIndex = 1;
            this.cbxTargetLang.SelectedIndexChanged += new System.EventHandler(this.cbxTargetLang_SelectedIndexChanged);
            this.cbxTargetLang.DataSourceChanged += new System.EventHandler(this.cbxTargetLang_DataSourceChanged);
            // 
            // lblSourceLang
            // 
            this.lblSourceLang.AutoSize = true;
            this.lblSourceLang.Location = new System.Drawing.Point(26, 30);
            this.lblSourceLang.Name = "lblSourceLang";
            this.lblSourceLang.Size = new System.Drawing.Size(114, 16);
            this.lblSourceLang.TabIndex = 0;
            this.lblSourceLang.Text = "Source Language";
            // 
            // lblTargetLang
            // 
            this.lblTargetLang.AutoSize = true;
            this.lblTargetLang.Location = new System.Drawing.Point(216, 30);
            this.lblTargetLang.Name = "lblTargetLang";
            this.lblTargetLang.Size = new System.Drawing.Size(111, 16);
            this.lblTargetLang.TabIndex = 1;
            this.lblTargetLang.Text = "Target Language";
            // 
            // tbxFilePath
            // 
            this.tbxFilePath.Location = new System.Drawing.Point(29, 98);
            this.tbxFilePath.Name = "tbxFilePath";
            this.tbxFilePath.ReadOnly = true;
            this.tbxFilePath.Size = new System.Drawing.Size(411, 22);
            this.tbxFilePath.TabIndex = 99;
            this.tbxFilePath.TabStop = false;
            this.tbxFilePath.TextChanged += new System.EventHandler(this.tbxFilePath_TextChanged);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(458, 97);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 3;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // btnExtract
            // 
            this.btnExtract.Location = new System.Drawing.Point(119, 142);
            this.btnExtract.Name = "btnExtract";
            this.btnExtract.Size = new System.Drawing.Size(121, 23);
            this.btnExtract.TabIndex = 4;
            this.btnExtract.Text = "Extract";
            this.btnExtract.UseVisualStyleBackColor = true;
            this.btnExtract.Click += new System.EventHandler(this.btnExtract_Click);
            // 
            // cbxFileType
            // 
            this.cbxFileType.DataSource = this.selectableItemBindingSource;
            this.cbxFileType.DisplayMember = "Name";
            this.cbxFileType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxFileType.FormattingEnabled = true;
            this.cbxFileType.Location = new System.Drawing.Point(412, 49);
            this.cbxFileType.Name = "cbxFileType";
            this.cbxFileType.Size = new System.Drawing.Size(121, 24);
            this.cbxFileType.TabIndex = 2;
            this.cbxFileType.SelectedIndexChanged += new System.EventHandler(this.cbxFileType_SelectedIndexChanged);
            // 
            // lblFileType
            // 
            this.lblFileType.AutoSize = true;
            this.lblFileType.Location = new System.Drawing.Point(412, 29);
            this.lblFileType.Name = "lblFileType";
            this.lblFileType.Size = new System.Drawing.Size(64, 16);
            this.lblFileType.TabIndex = 2;
            this.lblFileType.Text = "File Type";
            // 
            // pbrStatus
            // 
            this.pbrStatus.Location = new System.Drawing.Point(29, 223);
            this.pbrStatus.Name = "pbrStatus";
            this.pbrStatus.Size = new System.Drawing.Size(504, 23);
            this.pbrStatus.TabIndex = 99;
            this.pbrStatus.Value = 100;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(444, 204);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(110, 16);
            this.lblStatus.TabIndex = 99;
            this.lblStatus.Text = "100 % completed";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblStatus.TextChanged += new System.EventHandler(this.lblStatus_TextChanged);
            // 
            // btnTranslate
            // 
            this.btnTranslate.Location = new System.Drawing.Point(319, 142);
            this.btnTranslate.Name = "btnTranslate";
            this.btnTranslate.Size = new System.Drawing.Size(121, 23);
            this.btnTranslate.TabIndex = 5;
            this.btnTranslate.Text = "Translate";
            this.btnTranslate.UseVisualStyleBackColor = true;
            this.btnTranslate.Click += new System.EventHandler(this.btnTranslate_Click);
            // 
            // dgvExcelData
            // 
            this.dgvExcelData.AllowUserToAddRows = false;
            this.dgvExcelData.AllowUserToDeleteRows = false;
            this.dgvExcelData.AllowUserToResizeColumns = false;
            this.dgvExcelData.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.Silver;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvExcelData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvExcelData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvExcelData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvExcelData.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvExcelData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvExcelData.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgvExcelData.ImeMode = System.Windows.Forms.ImeMode.On;
            this.dgvExcelData.Location = new System.Drawing.Point(29, 270);
            this.dgvExcelData.Name = "dgvExcelData";
            this.dgvExcelData.ReadOnly = true;
            this.dgvExcelData.RowHeadersVisible = false;
            this.dgvExcelData.RowHeadersWidth = 51;
            this.dgvExcelData.RowTemplate.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvExcelData.RowTemplate.Height = 24;
            this.dgvExcelData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvExcelData.ShowEditingIcon = false;
            this.dgvExcelData.Size = new System.Drawing.Size(504, 0);
            this.dgvExcelData.TabIndex = 6;
            this.dgvExcelData.Visible = false;
            // 
            // lblTotalRecords
            // 
            this.lblTotalRecords.AutoSize = true;
            this.lblTotalRecords.Location = new System.Drawing.Point(26, 204);
            this.lblTotalRecords.Name = "lblTotalRecords";
            this.lblTotalRecords.Size = new System.Drawing.Size(128, 16);
            this.lblTotalRecords.TabIndex = 101;
            this.lblTotalRecords.Text = "Total records: 10000";
            // 
            // Form1
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(566, 280);
            this.Controls.Add(this.lblTotalRecords);
            this.Controls.Add(this.dgvExcelData);
            this.Controls.Add(this.btnTranslate);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.pbrStatus);
            this.Controls.Add(this.lblFileType);
            this.Controls.Add(this.cbxFileType);
            this.Controls.Add(this.btnExtract);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.tbxFilePath);
            this.Controls.Add(this.lblTargetLang);
            this.Controls.Add(this.lblSourceLang);
            this.Controls.Add(this.cbxTargetLang);
            this.Controls.Add(this.cbxSourceLang);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Translator App";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.selectableItemBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvExcelData)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		private void InitializeComponentCustom()
		{
			this.Text = this.GetTitleAndVersion();
			this.cbxSourceLang.DataSource = this._sourceLanguages;
			this.cbxTargetLang.DataSource = this._targetLanguages;
			this.cbxFileType.DataSource = this._fileTypes;
			this.tbxFilePath.Text = null;
			this.btnExtract.Enabled = false;
			this.btnTranslate.Enabled = false;
			this.lblStatus.Text = this.GetStatusText(0);
			this.lblTotalRecords.Text = this.GetTotalRecordsText(0);
		}

		private void lblStatus_TextChanged(object sender, EventArgs e)
		{
			string[] strArrays = this.lblStatus.Text.Split(new string[] { "%" }, StringSplitOptions.RemoveEmptyEntries);
			this.pbrStatus.Value = Convert.ToInt32(strArrays[0]);
		}

		private void tbxFilePath_TextChanged(object sender, EventArgs e)
		{
			this._model.FilePath = this.tbxFilePath.Text;
			bool flag = !string.IsNullOrWhiteSpace(this.tbxFilePath.Text);
			this.btnExtract.Enabled = flag;
			this.btnTranslate.Enabled = false;
			this.lblTotalRecords.Text = this.GetTotalRecordsText(0);
			this.lblStatus.Text = this.GetStatusText(0);
		}

		private void ToggleControlStatus(bool status)
		{
			this.cbxSourceLang.Enabled = !status;
			this.cbxTargetLang.Enabled = !status;
			this.cbxFileType.Enabled = !status;
			this.btnBrowse.Enabled = !status;
			this.btnExtract.Enabled = !status;
			this.lblStatus.Text = (!status ? this.GetStatusText(0) : this.lblStatus.Text);
		}

		private System.Windows.Forms.DialogResult Translate(XmlDocument document, Collection<SdlFilterFrameworkGroup> source)
		{
			int num = 1;
			int num1 = 0;
			Collection<SdlFilterFrameworkGroup> sdlFilterFrameworkGroups = new Collection<SdlFilterFrameworkGroup>(source);
			System.Windows.Forms.DialogResult dialogResult = System.Windows.Forms.DialogResult.None;
			try
			{
				foreach (SdlFilterFrameworkGroup sdlFilterFrameworkGroup in sdlFilterFrameworkGroups)
				{
					Program.UpdateFile(ref document, sdlFilterFrameworkGroup, this._model);
					int num2 = num1 + 1;
					num1 = num2;
					int num3 = num2 * num;
					int num4 = this.CalculateProgress(num3, source.Count);
					this.lblStatus.Text = this.GetStatusText(num4);
				}
			}
			catch
			{
				dialogResult = MessageBox.Show(this, "Unexpected error occurred. Please try again!", "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Hand);
			}
			return dialogResult;
		}

		private void UpdateCursor(bool isProcessing)
		{
			this.Cursor = (isProcessing ? Cursors.WaitCursor : Cursors.Default);
		}
	}
}