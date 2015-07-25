using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WatiN.Core;

namespace ShowdownBot
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        //Bot info
        //Move to a file or something easier to read/write
        string username = "ultramafic";
        string password = "niggers";
        //Site Info
        string LoginButton = "login";
        string nameField = "username";
        string passwordField = "password";

        Consol c;
        public Form1()
        {
            c = new Consol();
            InitializeComponent();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            StartBot();
        }

        public bool StartBot()
        {

            c.Show();
            c.write("Accessing http://play.pokemonshowdown.com/");
            //OpenSite("http://play.pokemonshowdown.com/");
            return true;
        }

        
       

        private void ocon_Click(object sender, EventArgs e)
        {
            c.Show();
        }
    }
}
