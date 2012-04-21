﻿namespace NStub.Gui
{
    partial class GeneratorConfig
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.propGrid = new System.Windows.Forms.PropertyGrid();
            this.tbConfig = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lvParameters = new System.Windows.Forms.CheckedListBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(772, 644);
            this.panel1.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.propGrid);
            this.panel2.Controls.Add(this.tbConfig);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 147);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(772, 497);
            this.panel2.TabIndex = 3;
            // 
            // propGrid
            // 
            this.propGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propGrid.Location = new System.Drawing.Point(0, 0);
            this.propGrid.Name = "propGrid";
            this.propGrid.Size = new System.Drawing.Size(772, 312);
            this.propGrid.TabIndex = 2;
            this.propGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.PropGridPropertyValueChanged);
            // 
            // tbConfig
            // 
            this.tbConfig.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tbConfig.Location = new System.Drawing.Point(0, 312);
            this.tbConfig.Multiline = true;
            this.tbConfig.Name = "tbConfig";
            this.tbConfig.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbConfig.Size = new System.Drawing.Size(772, 185);
            this.tbConfig.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lvParameters);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(772, 147);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // lvParameters
            // 
            this.lvParameters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvParameters.Location = new System.Drawing.Point(3, 16);
            this.lvParameters.Name = "lvParameters";
            this.lvParameters.Size = new System.Drawing.Size(766, 124);
            this.lvParameters.TabIndex = 0;
            this.lvParameters.SelectedIndexChanged += new System.EventHandler(this.LvParametersSelectedIndexChanged);
            this.lvParameters.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.LvParametersItemCheck);
            // 
            // GeneratorConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(772, 644);
            this.Controls.Add(this.panel1);
            this.Name = "GeneratorConfig";
            this.Text = "GeneratorConfig";
            this.Load += new System.EventHandler(this.GeneratorConfigLoad);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckedListBox lvParameters;
        private System.Windows.Forms.TextBox tbConfig;
        private System.Windows.Forms.PropertyGrid propGrid;
        private System.Windows.Forms.Panel panel2;
    }
}