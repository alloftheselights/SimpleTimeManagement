namespace TimeManagement
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();

            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);

            // Set the border style to fixed single to prevent resizing
            FormBorderStyle = FormBorderStyle.FixedSingle;

            // Lock the size by setting both maximum and minimum sizes to the current size
            MaximumSize = new Size(800, 450);
            MinimumSize = new Size(800, 450);

            Name = "Form1";
            Text = "Form1";

            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

    }
}