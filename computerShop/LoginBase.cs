using System.ComponentModel; // Required for IContainer
using System.Windows.Forms;

namespace computerShop
{
    internal class LoginBase : Form // Ensure it inherits from Form
    {
        private IContainer components = null; // Add this line

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}