namespace Jetwin.Modules
{
    partial class ReportView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.menuBar = new System.Windows.Forms.Panel();
            this.lblReport = new System.Windows.Forms.Label();
            this.pnlTransactionTable = new System.Windows.Forms.Panel();
            this.transactionDataGrid = new System.Windows.Forms.DataGridView();
            this.pnlContent.SuspendLayout();
            this.menuBar.SuspendLayout();
            this.pnlTransactionTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.transactionDataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlContent
            // 
            this.pnlContent.Controls.Add(this.pnlTransactionTable);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(0, 59);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Size = new System.Drawing.Size(1300, 597);
            this.pnlContent.TabIndex = 16;
            // 
            // menuBar
            // 
            this.menuBar.Controls.Add(this.lblReport);
            this.menuBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.menuBar.Location = new System.Drawing.Point(0, 0);
            this.menuBar.Name = "menuBar";
            this.menuBar.Padding = new System.Windows.Forms.Padding(15, 4, 0, 3);
            this.menuBar.Size = new System.Drawing.Size(1300, 59);
            this.menuBar.TabIndex = 15;
            // 
            // lblReport
            // 
            this.lblReport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblReport.AutoSize = true;
            this.lblReport.Font = new System.Drawing.Font("Cascadia Mono", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblReport.ForeColor = System.Drawing.Color.White;
            this.lblReport.Location = new System.Drawing.Point(33, 13);
            this.lblReport.Name = "lblReport";
            this.lblReport.Size = new System.Drawing.Size(98, 32);
            this.lblReport.TabIndex = 3;
            this.lblReport.Text = "Report";
            // 
            // pnlTransactionTable
            // 
            this.pnlTransactionTable.AutoSize = true;
            this.pnlTransactionTable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.pnlTransactionTable.Controls.Add(this.transactionDataGrid);
            this.pnlTransactionTable.Location = new System.Drawing.Point(137, 116);
            this.pnlTransactionTable.Name = "pnlTransactionTable";
            this.pnlTransactionTable.Padding = new System.Windows.Forms.Padding(10, 0, 10, 10);
            this.pnlTransactionTable.Size = new System.Drawing.Size(786, 202);
            this.pnlTransactionTable.TabIndex = 10;
            // 
            // transactionDataGrid
            // 
            this.transactionDataGrid.AllowUserToAddRows = false;
            this.transactionDataGrid.AllowUserToDeleteRows = false;
            this.transactionDataGrid.AllowUserToResizeRows = false;
            this.transactionDataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.transactionDataGrid.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.transactionDataGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.transactionDataGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.transactionDataGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.transactionDataGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.transactionDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.Silver;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(151)))), ((int)(((byte)(40)))), ((int)(((byte)(90)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.transactionDataGrid.DefaultCellStyle = dataGridViewCellStyle4;
            this.transactionDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.transactionDataGrid.EnableHeadersVisualStyles = false;
            this.transactionDataGrid.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(112)))), ((int)(((byte)(114)))), ((int)(((byte)(116)))));
            this.transactionDataGrid.Location = new System.Drawing.Point(10, 0);
            this.transactionDataGrid.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.transactionDataGrid.Name = "transactionDataGrid";
            this.transactionDataGrid.RowHeadersVisible = false;
            this.transactionDataGrid.RowTemplate.Height = 35;
            this.transactionDataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.transactionDataGrid.Size = new System.Drawing.Size(766, 192);
            this.transactionDataGrid.TabIndex = 7;
            // 
            // frmReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(21)))), ((int)(((byte)(24)))));
            this.ClientSize = new System.Drawing.Size(1300, 656);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.menuBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmReport";
            this.Text = "Report";
            this.pnlContent.ResumeLayout(false);
            this.pnlContent.PerformLayout();
            this.menuBar.ResumeLayout(false);
            this.menuBar.PerformLayout();
            this.pnlTransactionTable.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.transactionDataGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.Panel menuBar;
        private System.Windows.Forms.Label lblReport;
        private System.Windows.Forms.Panel pnlTransactionTable;
        private System.Windows.Forms.DataGridView transactionDataGrid;
    }
}